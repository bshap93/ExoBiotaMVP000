using Feedbacks.Interface;
using FirstPersonPlayer.Combat.AINPC.Creatures;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Minable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class HatchetToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Hatchet Settings")]
        [Tooltip("Tags this hatchet is allowed to affect (e.g., BioObstacle, Vegetation).")]
        public string[] allowedTags;
        // No cost if swing didn't make contact
        [FormerlySerializedAs("staminaCostPerConnectingSwing")]
        public float baseStaminaCostPerConnectingSwing = 1f;


        [Tooltip("Number of seconds between swings.")]
        public float swingCooldown = 0.6f;


        [Tooltip("Tool power sent to HatchetBreakable (compares to its hardness).")]
        public int hatchetPower = 1;

        [SerializeField] Sprite defaultReticleForTool;


        [FormerlySerializedAs("LastSwingTime")] [SerializeField]
        protected float lastSwingTime = -999f;

        float StaminaCostPerConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return baseStaminaCostPerConnectingSwing;

                var agility = attrMgr.Agility;
                var reduction = agilityReductionFactor * agility; // Example: 0.05
                var finalCost = baseStaminaCostPerConnectingSwing * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        public override void Use()
        {
            if (PlayerMutableStatsManager.Instance.CurrentStamina < StaminaCostPerConnectingSwing)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina, "Not enough stamina to use pickaxe.", "Insufficient Stamina");

                return;
            }

            if (attributesManager == null) attributesManager = AttributesManager.Instance;

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
            if (target.TryGetComponent<IBreakable>(out _)) return true;

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
            // Check if the object has a tag that should show inability reticle
            if (tagsWhichShouldShowInabilityReticle != null)
                foreach (var tagName in tagsWhichShouldShowInabilityReticle)
                    if (colliderGameObject.CompareTag(tagName))
                        return reticleForInabilityToSample;

            // Default to the normal reticle
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
            if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(hatchetPower, hit.point, hit.normal);

                // SpawnFxForConnectingHit(hit.point, hit.normal);
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    StaminaCostPerConnectingSwing);
            }
            else if (go.TryGetComponent<MyOreNode>(out var oreNode))
            {
                // No apply here – ore nodes are for pickaxe only

                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
            else if (go.TryGetComponent<IFleshyObject>(out var fleshyObject))
            {
                hitFleshyFeedbacks?.PlayFeedbacks();
                fleshyObject.MakeJiggle();
            }
            else if (go.CompareTag("DiggerChunk") || go.CompareTag("MainSceneTerrain"))
            {
                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
            else if (go.CompareTag("MiscRigidOrganism"))
            {
                hitRigidOrganismFeedbacks?.PlayFeedbacks();
            }
            else if (go.CompareTag("EnemyNPC"))
            {
                var enemyController = go.GetComponentInParent<EnemyController>();

                if (enemyController == null)
                {
                    Debug.LogWarning("HatchetToolPrefab: Hit enemy NPC but no EnemyController found in parents.");
                    return;
                }

                var playerAttack = DetermineCorrectPlayerToolAttack(AttackDamageType.BasicHit);


                // Spawn VFX with proper cleanup
                var vfx = enemyController.GetEffectsAndFeedbacks().basicHitVFX;
                if (vfx != null)
                {
                    var vfxInstance = Instantiate(vfx, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(vfxInstance, 2f); // Clean up after 2 seconds
                }


                enemyController.ProcessAttackDamage(playerAttack);
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    StaminaCostPerConnectingSwing);
            }
            else
            {
                // No apply here – ore nodes are for pickaxe only

                SpawnFxForIneffectualHit(hit.point, hit.normal);
                hitRockFeedbacks?.PlayFeedbacks();
            }
        }


        public override void PerformToolAction()
        {
            swingCooldown -= agilityCooldownSecondsReducePerPoint * (attributesManager.Agility - 1);
            if (Time.time < lastSwingTime + swingCooldown) return;
            lastSwingTime = Time.time;

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
