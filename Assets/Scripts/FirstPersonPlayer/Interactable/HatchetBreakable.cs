using System;
using Helpers.Events.Domains.Player.Events;
using HighlightPlus;
using MoreMountains.Feedbacks;
using RayFire;
using UnityEngine;
using Utilities.Interface;

// for DestructableEvent (optional, matches your ore node usage)

namespace FirstPersonPlayer.Interactable
{
    [DisallowMultipleComponent]
    public class HatchetBreakable : MonoBehaviour, IRequiresUniqueID, IBreakable
    {
        [Header("Break Settings")] [Tooltip("How many successful hatchet hits until this is destroyed.")]
        public int hitsToBreak = 2;

        [Tooltip("Minimum tool power required to count as a successful hit.")]
        public int hardness = 1;

        [Tooltip("If set, destroy this root instead of just this component's GameObject.")]
        public GameObject destroyRoot;

        [Tooltip("If true, Destroy() the object. If false, just disable renderers/colliders.")]
        public bool destroyGameObject = true;

        [Header("FX")] public MMFeedbacks onHitFeedbacks;

        public MMFeedbacks onBreakFeedbacks;
        public GameObject hitParticles;
        public GameObject breakParticles;

        [Header("Persistence (optional)")]
        [Tooltip("If provided, we will trigger DestructableEvent.Destroyed with this ID when broken.")]
        public string uniqueIdForPersistence;

        HighlightEffect _highlight; // cache HighlightEffect if present

        int _hitCount;
        bool _isBroken; // Prevent multiple breaks


        RayfireRigid _rf;

        void Awake()
        {
            _highlight = GetComponent<HighlightEffect>();
            _rf = GetComponent<RayfireRigid>();

            if (string.IsNullOrEmpty(uniqueIdForPersistence))
                uniqueIdForPersistence = Guid.NewGuid().ToString();


            _rf.demolitionEvent.LocalEvent += OnDemolished;
        }

        public bool CanBeDamagedBy(int toolPower)
        {
            return toolPower >= hardness;
        }


        public void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal)
        {
            ApplyHatchetHit(toolPower, hitPoint, hitNormal);
        }


        public string UniqueID => uniqueIdForPersistence;
        public void SetUniqueID()
        {
            if (string.IsNullOrEmpty(uniqueIdForPersistence))
                uniqueIdForPersistence = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueIdForPersistence);
        }


        void OnDemolished(RayfireRigid demolished)
        {
            if (demolished.HasFragments)
                foreach (var frag in demolished.fragments)
                    frag.gameObject.layer = LayerMask.NameToLayer("Debris");
        }

        void ApplyHatchetHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal)
        {
            // Prevent breaking if already broken
            if (_isBroken)
            {
                Debug.LogWarning($"HatchetBreakable [{uniqueIdForPersistence}]: Already broken, ignoring hit");
                return;
            }

            if (!CanBeDamagedBy(toolPower))
            {
                PlayHitFx(hitPoint, hitNormal, true);
                return;
            }

            _hitCount++;
            PlayHitFx(hitPoint, hitNormal);

            if (_hitCount < hitsToBreak) return;

            _isBroken = true;

            // break FX
            onBreakFeedbacks?.PlayFeedbacks(transform.position);
            if (breakParticles)
            {
                var fx2 = Instantiate(breakParticles, transform.position, Quaternion.identity);
                Destroy(fx2, 3f);
            }

            if (!string.IsNullOrEmpty(uniqueIdForPersistence))
                DestructableEvent.Trigger(DestructableEventType.Destroyed, uniqueIdForPersistence, transform);

            var root = destroyRoot != null ? destroyRoot : gameObject;
            if (destroyGameObject)
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                if (_rf != null)
                    _rf.Demolish();
                else
                    Destroy(root, 0.05f);
            }
            else
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                enabled = false;
            }
        }

        public void BreakInstantly()
        {
            if (_isBroken)
            {
                Debug.LogWarning(
                    $"HatchetBreakable [{uniqueIdForPersistence}]: Already broken, ignoring BreakInstantly call");

                return;
            }

            // Skip the incremental hits; just perform the full break logic
            _hitCount = hitsToBreak;
            ApplyHatchetHit(hardness, transform.position, transform.up);
        }

        void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal, bool softFail = false)
        {
            onHitFeedbacks?.PlayFeedbacks(transform.position);

            if (hitParticles)
            {
                var fx = Instantiate(hitParticles, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(fx, 2f);
            }

            // Highlight Plus Hit FX
            if (_highlight != null) _highlight.HitFX(); // plays the configured hit effect
        }
    }
}
