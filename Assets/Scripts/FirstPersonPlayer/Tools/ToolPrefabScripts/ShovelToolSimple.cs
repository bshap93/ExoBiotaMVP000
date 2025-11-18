using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events.ManagerEvents;
using Manager.Global;
using MoreMountains.Feedbacks;
using TerrainScripts;
using UnityEngine;
using Utilities;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class ShovelToolSimple : MonoBehaviour, IRuntimeTool

    {
        [Header("Digging")] [SerializeField] private LayerMask diggableMask = ~0;

        [SerializeField] private Sprite defaultReticleForTool;

        [SerializeField] private CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicScanner;



        [Tooltip("Allowed texture indices on terrain this shovel can dig")]
        public int[] allowedTerrainTextureIndices;

        [Header("Feedbacks")] public MMFeedbacks diggingFeedbacks;
        public GameObject debrisEffectPrefab;

        [Header("Material Settings")] public Material currentMaterial;

        [Header("Shovel Models")] [SerializeField]
        private GameObject shovelObject;

        [SerializeField] private GameObject shovelGripObject;
        [SerializeField] private GameObject shovelMidObject;

        [SerializeField] private MMFeedbacks equippedFeedbacks;


        [Header("Dig Settings")] public float diggerUsingRange = 5f;
        [Header("Effect Settings")] public float minEffectRadius = 0.3f;
        public float maxEffectRadius = 1.2f;
        public float minEffectOpacity = 5f;
        public float maxEffectOpacity = 150f;
        [SerializeField] protected float miningCooldown = 1f;

        public Camera mainCamera;
        public TerrainBehavior currentTerrainBehavior;
        [SerializeField] protected TerrainLayerDetector terrainLayerDetector;

        protected DiggerMasterRuntime digger;
        protected float lastDigTime = -999f;
        protected RaycastHit lastHit;
        protected PlayerInteraction playerInteraction;

        /* --------- IRuntimeTool --------- */
        public void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            playerInteraction = owner.playerInteraction;
            terrainLayerDetector = owner.terrainLayerDetector;
            if (TerrainManager.Instance != null)
                currentTerrainBehavior = TerrainManager.Instance.currentTerrainController?.terrainBehavior;
            digger = TerrainManager.Instance?.currentDiggerMasterRuntime;
        }

        public void Use()
        {
            PerformToolAction();
        }

        public void Unequip()
        {
        }

        public bool CanInteractWithObject(GameObject target)
        {
            return ((1 << target.layer) & diggableMask) != 0;
        }

        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }

        public bool CanAbortAction()
        {
            return false;
        }

        public MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;

        }

        public CanBeAreaScannedType GetDetectableType()
        {
            return detectableType;
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
            return terrainLayerDetector?.GetTextureIndex(lastHit, out _) ?? -1;
        }

        public void CacheHit(RaycastHit hit)
        {
            lastHit = hit;
        }

        public void SetCurrentMaterial(Material material)
        {
            currentMaterial = material;
            if (shovelObject) shovelObject.GetComponent<Renderer>().material = material;
            if (shovelGripObject) shovelGripObject.GetComponent<Renderer>().material = material;
            if (shovelMidObject) shovelMidObject.GetComponent<Renderer>().material = material;
        }

        /* --------- Core Digging --------- */
        public void PerformToolAction()
        {
            if (Time.time < lastDigTime + miningCooldown) return;

            if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward,
                    out var hit, diggerUsingRange, diggableMask))
                return;

            lastHit = hit;
            lastDigTime = Time.time;

            // Check if it's a DiggerChunk
            if (hit.collider.CompareTag("DiggerChunk"))
                DigChunk(hit);
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) DigTerrain(hit);

            // Effects
            if (debrisEffectPrefab)
            {
                var fx = Instantiate(debrisEffectPrefab, hit.point + hit.normal * 0.1f,
                    Quaternion.LookRotation(-mainCamera.transform.forward));
                Destroy(fx, 2f);
            }

            diggingFeedbacks?.PlayFeedbacks(hit.point);
        }

        /* --------- Helpers --------- */
        private void DigChunk(RaycastHit hit)
        {
            if (digger == null) return;
            var digPos = hit.point + mainCamera.transform.forward * 0.3f;

            // Chunks do not use texture indices
            digger.ModifyAsyncBuffured(
                digPos,
                BrushType.Sphere,
                ActionType.Dig,
                0,
                minEffectOpacity,
                minEffectRadius
            );
        }

        private void DigTerrain(RaycastHit hit)
        {
            if (digger == null || terrainLayerDetector == null) return;

            var detectedTextureIndex = terrainLayerDetector.GetTextureIndex(hit, out _);
            if (!CanInteractWithTextureIndex(detectedTextureIndex)) return;

            // ✅ Ensure we pass the correct index back to Digger
            var finalTextureIndex = ResolveFinalTextureIndex(detectedTextureIndex);

            var digPos = hit.point + mainCamera.transform.forward * 0.3f;

            digger.ModifyAsyncBuffured(
                digPos,
                BrushType.Sphere,
                ActionType.Dig,
                finalTextureIndex,
                minEffectOpacity,
                minEffectRadius
            );
        }

        private int ResolveFinalTextureIndex(int detectedTextureIndex)
        {
            // ✅ For now, just return the detected texture
            // (later you could add depth or override logic here)
            return Mathf.Max(0, detectedTextureIndex);
        }
    }
}