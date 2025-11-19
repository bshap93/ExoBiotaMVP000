using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class HatchetToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Hatchet Settings")]
        [Tooltip("Tags this hatchet is allowed to affect (e.g., BioObstacle, Vegetation).")]
        public string[] allowedTags;
        // No cost if swing didn't make contact
        public float staminaCostPerConnectingSwing = 1f;

        [Tooltip("Number of seconds between swings.")]
        public float swingCooldown = 0.6f;


        [Tooltip("Tool power sent to HatchetBreakable (compares to its hardness).")]
        public int hatchetPower = 1;

        [SerializeField] Sprite defaultReticleForTool;


        [SerializeField] MMFeedbacks equipFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;

        protected float LastSwingTime = -999f;

        public override void Use()
        {
            if (PlayerStatsManager.Instance.CurrentStamina < staminaCostPerConnectingSwing)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina, "Not enough stamina to use pickaxe.", "Insufficient Stamina");

                return;
            }

            PerformToolAction();
        }


        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;
        }


        public override void Unequip()
        {
            // no-op for now
        }

        public override bool CanInteractWithObject(GameObject target)
        {
            if (target == null) return false;

            // Component gate
            if (target.TryGetComponent<HatchetBreakable>(out _)) return true;

            // Tag gate
            if (allowedTags != null && allowedTags.Length > 0)
            {
                var t = target.tag;
                for (var i = 0; i < allowedTags.Length; i++)
                    if (!string.IsNullOrEmpty(allowedTags[i]) && t == allowedTags[i])
                        return true;
            }

            return false;
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }

        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equipFeedbacks;
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }


        public override void ApplyHit()
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;

            if (!Physics.Raycast(
                    mainCamera.transform.position, mainCamera.transform.forward,
                    out var hit, reach, hitMask, QueryTriggerInteraction.Ignore))
                return;

            var go = hit.collider.gameObject;

            // First priority: dedicated component
            if (go.TryGetComponent<HatchetBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(hatchetPower, hit.point, hit.normal);

                SpawnFx(hit.point, hit.normal);
                swingFeedback?.PlayFeedbacks(hit.point);
                hitFeedback?.PlayFeedbacks(hit.point);
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    staminaCostPerConnectingSwing);
            }
        }


        public override void PerformToolAction()
        {
            if (Time.time < LastSwingTime + swingCooldown) return;
            LastSwingTime = Time.time;

            if (useMultipleSwings && AnimController.currentToolAnimationSet != null)
            {
                PlaySwingSequence();
            }
            else
            {
                // Fallback to legacy single animation mode
                AnimController.PlayToolUseOneShot();
                StartCoroutine(ApplyHitAfterDelay(defaultHitDelay));
            }
        }


        // Kept to mirror Pickaxe signature – not used by hatchet
        public int GetCurrentTextureIndex()
        {
            return -1;
        }

        public bool CanInteractWithTextureIndex(int terrainIndex)
        {
            return false;
        }
    }
}
