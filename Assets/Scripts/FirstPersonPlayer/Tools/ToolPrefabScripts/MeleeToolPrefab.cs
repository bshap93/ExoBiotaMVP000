using System;
using System.Collections;
using FirstPersonPlayer.Tools.Interface;
using Helpers.AnimancerHelper;
using Helpers.Events.ManagerEvents;
using Helpers.ScriptableObjects.Animation;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public abstract class MeleeToolPrefab : MonoBehaviour, IRuntimeTool
    {
        [Header("Tool Settings")] public float toolCooldown = 1f;

        public float toolUsingRange = 5f;
        public int toolHardnessPower = 1;

        [Header("Effect Settings")] public float effectRadius = 1f;

        [SerializeField] CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicBioScanner;

        [Header("Swing Animation Settings")] [Tooltip("Use multiple swing animations that alternate?")]
        public bool useMultipleSwings = true;

        [Tooltip("Delay in seconds before hit is applied for Swing 01 animation.")]
        public float swing01HitDelay = 0.2f;

        [Tooltip("Delay in seconds before hit is applied for Swing 02 animation.")]
        public float swing02HitDelay = 0.25f;

        [Tooltip("Delay in seconds before hit is applied for Swing 03 animation (if used).")]
        public float swing03HitDelay = 0.22f;

        [Tooltip("Fallback delay if using beginUseAnimation (legacy mode).")]
        public float defaultHitDelay = 0.2f;


        [Header("FX")] public GameObject impactVfx;

        public MMFeedbacks swingFeedback;

        public GameObject debrisEffectPrefab;
        public MMFeedbacks hitFeedback;

        [Header("References")] public Camera mainCamera;

        [Tooltip("How far the tool can reach.")]
        public float reach = 3.25f;

        [Tooltip("Optional physics mask to limit what raycast can hit.")]
        public LayerMask hitMask = ~0;


        protected AnimancerRightArmController AnimController;

        protected int CurrentSwingIndex; // Track which swing we're on
        protected RaycastHit LastHit;

        // protected float LastTimeOfEffect = -999f;


        public abstract void Initialize(PlayerEquipment owner);

        public virtual void Use()
        {
            PerformToolAction();
        }

        public virtual void Unequip()
        {
        }

        public virtual bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return true;
        }

        public abstract Sprite GetReticleForTool(GameObject colliderGameObject);

        public bool CanAbortAction()
        {
            throw new NotImplementedException();
        }

        public abstract MMFeedbacks GetEquipFeedbacks();
        public abstract MMFeedbacks GetUnequipFeedbacks();
        public CanBeAreaScannedType GetDetectableType()
        {
            return detectableType;
        }

        protected void SpawnFx(Vector3 pos, Vector3 normal)
        {
            if (impactVfx)
            {
                var fx = Instantiate(impactVfx, pos + normal * 0.05f, Quaternion.LookRotation(normal));
                Destroy(fx, 2f);
            }

            if (debrisEffectPrefab)
            {
                var debris = Instantiate(
                    debrisEffectPrefab, pos + normal * 0.05f,
                    Quaternion.LookRotation(-mainCamera.transform.forward));

                Destroy(debris, 2f);
            }
        }

        public virtual void PlaySwingSequence()
        {
            var animSet = AnimController.currentToolAnimationSet;
            AnimationClip swingClip = null;
            AudioClip swingSound = null;
            var hitDelay = defaultHitDelay;

            // Determine which swing to use based on current index
            switch (CurrentSwingIndex)
            {
                case 0:
                    swingClip = animSet.swing01Animation;
                    hitDelay = swing01HitDelay;
                    swingSound = animSet.swing01AudioClip;
                    break;
                case 1:
                    swingClip = animSet.swing02Animation;
                    hitDelay = swing02HitDelay;
                    swingSound = animSet.swing02AudioClip;
                    break;
                case 2:
                    swingClip = animSet.swing03Animation;
                    hitDelay = swing03HitDelay;
                    swingSound = animSet.swing03AudioClip;
                    break;
            }

            // If the selected swing doesn't exist, fall back to swing01
            if (swingClip == null)
            {
                swingClip = animSet.swing01Animation;
                hitDelay = swing01HitDelay;
                CurrentSwingIndex = 0;
            }

            if (swingSound != null) StartCoroutine(PlaySoundAfterDelay(swingSound, hitDelay / 2f));
            // AudioSource.PlayClipAtPoint(swingSound, mainCamera.transform.position);

            if (swingClip != null)
            {
                // Play the specific swing animation
                PlaySwingAnimation(swingClip);

                // Start coroutine with the appropriate delay
                StartCoroutine(ApplyHitAfterDelay(hitDelay));

                // Advance to next swing (with wrap-around)
                AdvanceSwingIndex(animSet);
            }
            else
            {
                // No swing animations available, use legacy mode
                AnimController.PlayToolUseOneShot();
                StartCoroutine(ApplyHitAfterDelay(defaultHitDelay));
            }
        }

        protected IEnumerator PlaySoundAfterDelay(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(clip, mainCamera.transform.position);
        }

        public void PlaySwingAnimation(AnimationClip clip)
        {
            if (AnimController.animancerComponent == null) return;

            var layer = AnimController.animancerComponent.Layers[1];

            // Play swing on Layer 1
            var state = layer.Play(clip, AnimController.defaultTransitionDuration);
            layer.Weight = 1f;

            // Mark this as the active action animation
            AnimController.SetActionState(state);

            // Clear previous events to avoid stacked callbacks
            state.Events(this).Clear();

            // Add a SINGLE end event
            state.Events(this).OnEnd = () =>
            {
                // Disable swing layer
                layer.Weight = 0f;

                // Clear action state so locomotion can resume
                AnimController.ClearActionState();

                // Return to locomotion safely
                AnimController.ReturnToLocomotion();
            };
        }


        protected virtual void AdvanceSwingIndex(ToolAnimationSet animSet)
        {
            // Count how many swing animations are available
            var availableSwings = 0;
            if (animSet.swing01Animation != null) availableSwings = 1;
            if (animSet.swing02Animation != null) availableSwings = 2;
            if (animSet.swing03Animation != null) availableSwings = 3;

            // Advance and wrap around
            CurrentSwingIndex = (CurrentSwingIndex + 1) % Mathf.Max(1, availableSwings);
        }

        protected virtual IEnumerator ApplyHitAfterDelay(float delay)
        {
            // Wait for the specified delay to sync with animation
            yield return new WaitForSeconds(delay);

            // Perform the actual hit detection and application
            ApplyHit();
        }

        public abstract void ApplyHit();

        public abstract void PerformToolAction();
    }
}
