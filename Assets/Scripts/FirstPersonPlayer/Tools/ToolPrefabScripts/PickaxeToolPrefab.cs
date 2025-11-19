using System.Linq;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Helpers.Events.Status;
using LevelConstruct.Highlighting;
using Manager;
using Manager.Global;
using MoreMountains.Feedbacks;
using UnityEngine;
using Utilities;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class PickaxeToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Mining Settings")] public float miningCooldown = 1f;

        public int hardnessCanBreak = 1;
        public float highlighingRange;

        // No cost if swing didn't make contact
        public float staminaCostPerConnectingSwing = 1f;

        [SerializeField] Sprite defaultReticleForTool;


        // Not in abstract base class
        public float effectOpacity = 10f;

        // Not in abstract base class
        [Header("Allowed Textures")] public int[] allowedTerrainTextureIndices;

        [SerializeField] MMFeedbacks equippedFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;


        [SerializeField] GameObject unequippedEffectPrefab;
        [SerializeField] bool showHighlights = true;

        [Header("Layer Settings")] [SerializeField]
        LayerMask minableLayers = -1; // Default: all layers


        // Not in abstract base class
        public TerrainLayerDetector terrainLayerDetector;

        [Tooltip("Tool power sent to breakables (compares to their hardness).")]
        public int pickaxePower = 1;
        float _checkObjectsCooldown;

        DiggerMasterRuntime _digger;
        bool _hasValidHit;

        RaycastHit _pendingHit;

        protected float LastSwingTime = -999f;


        void Update()
        {
            if (showHighlights)
            {
                _checkObjectsCooldown -= Time.deltaTime;
                if (_checkObjectsCooldown <= 0f)
                {
                    foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                                 .OfType<IMinable>())
                    {
                        var controller = (minable as Component)?.GetComponent<HighlightEffectController>();
                        if (controller == null) continue;

                        //Check if the object is on an allowed layer
                        if (!IsOnAllowedLayer(controller.gameObject)) continue;

                        var inRange = IsMinableWithinHighlightingRange(controller.gameObject);
                        controller.SetHighlighted(inRange);
                        controller.SetTargetVisible(inRange);
                    }

                    _checkObjectsCooldown = 0.25f;
                }
            }
        }


        void OnEnable()
        {
            foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IMinable>())
            {
                var controller = (minable as Component)?.GetComponent<HighlightEffectController>();

                if (controller != null && IsMinableWithinHighlightingRange(controller.gameObject))
                    //if (controller != null)
                {
                    controller.SetHighlighted(true);
                    controller.SetTargetVisible(true);
                }
            }
        }


        /* --------- IRuntimeTool --------- */
        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            terrainLayerDetector = owner.terrainLayerDetector;
            _digger = TerrainManager.Instance?.currentDiggerMasterRuntime;
            AnimController = owner.animancerRightArmController;
        }

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

        public override void Unequip()
        {
            foreach (var minable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IMinable>())
            {
                var controller = (minable as Component)?.GetComponent<HighlightEffectController>();
                if (controller != null)
                {
                    controller.SetHighlighted(false);
                    controller.SetTargetVisible(false);
                }
            }
        }

        public override bool CanInteractWithObject(GameObject target)
        {
            // ✅ 1. Check if it's an ore/minable object
            var minable = target.GetComponent<IMinable>();
            if (minable != null) return true;

            var breakableBio = target.GetComponent<IBreakable>();
            if (breakableBio != null) return true;

            return false;
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }

        /* --------- Core Mining --------- */
        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }


        bool IsOnAllowedLayer(GameObject obj)
        {
            return (minableLayers.value & (1 << obj.layer)) != 0;
        }

        bool IsMinableWithinHighlightingRange(GameObject minableObj)
        {
            if (Camera.main != null)
            {
                var camerTransform = Camera.main.transform;
                var minableTransform = minableObj.transform;

                var sqrDistance = (camerTransform.position - minableTransform.position).sqrMagnitude;

                return sqrDistance <= highlighingRange * highlighingRange;
            }

            return false;
        }

        public bool CanInteractWithTextureIndex(int index)
        {
            if (index < 0) return false;
            if (allowedTerrainTextureIndices == null || allowedTerrainTextureIndices.Length == 0) return true;
            foreach (var allowed in allowedTerrainTextureIndices)
                if (index == allowed)
                    return true;

            return false;
        }

        public int GetCurrentTextureIndex()
        {
            return terrainLayerDetector?.GetTextureIndex(LastHit, out _) ?? -1;
        }

        public override void ApplyHit()
        {
            // Use the stored hit from when button was pressed
            if (!_hasValidHit) return;

            var hit = _pendingHit; // ← Use stored data
            var go = hit.collider.gameObject;

            // First priority: dedicated component
            if (go.TryGetComponent<HatchetBreakable>(out var breakable))
            {
                // hardness/HP handled inside component
                breakable.ApplyHit(pickaxePower, hit.point, hit.normal);

                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    staminaCostPerConnectingSwing);

                SpawnFx(hit.point, hit.normal);
                swingFeedback?.PlayFeedbacks(hit.point);
                hitFeedback?.PlayFeedbacks(hit.point);
                return;
            }

            var minable = go.GetComponent<IMinable>();
            if (minable != null)
                if (minable.GetHardness() <= hardnessCanBreak)
                {
                    minable.MinableMineHit();
                    PlayerStatsEvent.Trigger(
                        PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                        staminaCostPerConnectingSwing);
                }
        }
        public override void PerformToolAction()
        {
            // Check cooldown first
            if (Time.time < LastSwingTime + miningCooldown) return;

            // IMMEDIATELY raycast to capture target
            _hasValidHit = Physics.Raycast(
                mainCamera.transform.position,
                mainCamera.transform.forward,
                out _pendingHit, // ← Store result here
                reach,
                hitMask,
                QueryTriggerInteraction.Ignore
            );

            // Validate target BEFORE starting animation
            if (_hasValidHit)
            {
                var targetGo = _pendingHit.collider.gameObject;
                if (!CanInteractWithObject(targetGo)) return; // Not valid - don't waste swing
            }

            LastSwingTime = Time.time;
            PlaySwingSequence();

            // if (Time.time < LastSwingTime + miningCooldown) return;

            // if (useMultipleSwings && AnimController.currentToolAnimationSet != null)
            // {
            //     PlaySwingSequence();
            // }
            // else
            // {
            //     // Fallback to legacy single animation mode
            //     AnimController.PlayToolUseOneShot();
            //     StartCoroutine(ApplyHitAfterDelay(defaultHitDelay));
            // }

            // PlayerInteractionEvent.Trigger(PlayerInteractionEventType.Interacted);
        }


        /* --------- Helpers --------- */
        void SpawnEffects(Vector3 pos, Vector3 normal)
        {
            if (debrisEffectPrefab != null)
            {
                var fx = Instantiate(
                    debrisEffectPrefab, pos + normal * 0.1f,
                    Quaternion.LookRotation(-mainCamera.transform.forward));

                Destroy(fx, 2f);
            }
        }
    }
}
