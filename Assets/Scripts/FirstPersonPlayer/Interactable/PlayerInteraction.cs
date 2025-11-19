// [WIP] Placeholder for revised First Person Interaction-Inventory system	

using System.Collections;
using System.Collections.Generic;
using Digger.Modules.Core.Sources;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using FirstPersonPlayer.UI;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Tools;
using SharedUI;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace FirstPersonPlayer.Interactable
{
    public class PlayerInteraction : MonoBehaviour, MMEventListener<PlayerInteractionEvent>
    {
        public float interactionDistance = 2f; // How far the player can interact
        public LayerMask interactableLayer; // Only detect objects in this layer
        public LayerMask terrainLayer; // Only detect objects in this layer
        public LayerMask obstacleLayer; // New: layers that block interaction (e.g., walls, rocks)
        public float controlHelpReminderDuration = 2f; // Duration to show control help reminder

        GameObject _currentlyHoveredObject;

        [FormerlySerializedAs("RightHandEquipment")] [SerializeField]
        PlayerEquipment rightHandEquipment;
        [SerializeField] PlayerEquipment leftHandEquipment;
        public PlayerEquipment RightHandEquipment => rightHandEquipment;

        public PlayerEquipment LeftHandEquipment => leftHandEquipment;

        public static PlayerInteraction Instance { get; private set; }

        public PlayerPropPickup propPickup;

        bool _holdingItem;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();

            // End hover if we're currently hovering something
            if (_currentlyHoveredObject != null)
            {
                var hoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                hoverable?.OnHoverEnd(_currentlyHoveredObject);
                _currentlyHoveredObject = null;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }


        [SerializeField] RewiredFirstPersonInputs rewiredInput;

        public CinemachineCamera playerCamera; // Reference to the player’s camera

        [FormerlySerializedAs("LightReminderActionId")] [FormerlySerializedAs("ActionId")]
#if UNITY_EDITORj
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int lightReminderActionId;

        [Header("Reticle")] public ReticleController reticleController;

        public LayerMask playerLayerMask;


        public TerrainLayerDetector forwardTerrainLayerDetector;


        void Start()
        {
            FindFirstObjectByType<DiggerMaster>();
            // Find the TextureDetector in the scene


            if (reticleController == null) reticleController = FindFirstObjectByType<ReticleController>();

            if (rewiredInput == null) rewiredInput = GetComponent<RewiredFirstPersonInputs>();

            ControlsHelpEvent.Trigger(ControlHelpEventType.Show, lightReminderActionId);

            StartCoroutine(WaitAndDisableControlHelp(controlHelpReminderDuration));
        }

        void Update()
        {
            PerformRaycastCheck(); // ✅ Single raycast for both interactables and diggable terrain


            if (rewiredInput.interact) // Press E to interact
                PerformInteraction();

            if (rewiredInput.pickablePick)
                PerformPickablePick();


            if (rewiredInput.dropPropOrHold)
            {
                _holdingItem = propPickup != null && propPickup.IsHoldingItem();
                if (_holdingItem)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, 67, "BlockAllNewRequests");
                else
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Hide, 67, "UnblockAllNewRequests");
            }
        }
        void PerformPickablePick()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var interactMask = interactableLayer & ~playerLayerMask;
            var terrMask = terrainLayer & ~playerLayerMask;
            var obstacleMask = obstacleLayer & ~playerLayerMask; // Add obstacle mask

            // Check for obstacles first
            RaycastHit obstacleHit;
            var obstacleBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out obstacleHit, interactionDistance, obstacleMask);

            // Check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrMask);

            // Check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactMask);

            if (hitInteractable &&
                (!obstacleBlocking || interactableHit.distance < obstacleHit.distance) &&
                (!terrainBlocking || interactableHit.distance < terrainHit.distance))
            {
                var itemPicker = interactableHit.collider.GetComponent<ItemPicker>();
                if (itemPicker != null)
                    itemPicker.PickupItemDirect();
            }
        }

        IEnumerator WaitAndDisableControlHelp(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, lightReminderActionId);
        }

        void PerformInteraction()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var interactMask = interactableLayer & ~playerLayerMask;
            var terrMask = terrainLayer & ~playerLayerMask;
            var obstacleMask = obstacleLayer & ~playerLayerMask; // Add obstacle mask

            // Check for obstacles first
            RaycastHit obstacleHit;
            var obstacleBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out obstacleHit, interactionDistance, obstacleMask);

            // Check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrMask);

            // Check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactMask);

            // Only interact if:
            // 1. We hit an interactable AND
            // 2. No obstacles are blocking AND
            // 3. Either there's no terrain blocking OR the interactable is closer than the terrain
            if (hitInteractable &&
                (!obstacleBlocking || interactableHit.distance < obstacleHit.distance) &&
                (!terrainBlocking || interactableHit.distance < terrainHit.distance))
            {
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }


        void PerformRaycastCheck()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var terrMask = terrainLayer & ~playerLayerMask;
            var interactMask = interactableLayer & ~playerLayerMask;

            // Combined raycast check
            RaycastHit terrainHit;
            var terrainBlocking =
                Physics.Raycast(rayOrigin, rayDirection, out terrainHit, interactionDistance, terrMask);

            if (terrainBlocking) forwardTerrainLayerDetector?.UpdateFromHit(terrainHit);

            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance,
                interactMask);

            // Determine the hit to process
            RaycastHit? actualHit = null;
            var isTerrainBlocking = false;

            if (terrainBlocking && hitInteractable)
            {
                if (terrainHit.distance < interactableHit.distance)
                    isTerrainBlocking = true;
                else
                    actualHit = interactableHit;
            }
            else if (hitInteractable)
            {
                actualHit = interactableHit;
            }

            // NEW: Handle hover state for IHoverable objects
            HandleHoverState(actualHit, isTerrainBlocking);


            // Update reticle through controller
            reticleController.UpdateReticle(actualHit, isTerrainBlocking);

            // If the current tool is ShovelToolSimple, update its hit
            var peWithShovel = PlayerEquipment.GetWithToolType(typeof(ShovelToolSimple));
            if (peWithShovel?.CurrentRuntimeTool is ShovelToolSimple shovel && actualHit.HasValue)
                shovel.CacheHit(actualHit.Value);
        }

        void HandleHoverState(RaycastHit? hit, bool isBlocked)
        {
            GameObject hitObject = null;

            // Only consider the object if it's not blocked
            if (hit.HasValue && !isBlocked) hitObject = hit.Value.collider.gameObject;

            // Check if we're hovering over a new object
            if (hitObject != _currentlyHoveredObject)
            {
                // End hover on previous object
                if (_currentlyHoveredObject != null)
                {
                    var previousHoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                    previousHoverable?.OnHoverEnd(_currentlyHoveredObject);
                }

                // Start hover on new object
                if (hitObject != null)
                {
                    var newHoverable = hitObject.GetComponent<IHoverable>();
                    if (newHoverable != null) newHoverable.OnHoverStart(hitObject);
                }

                _currentlyHoveredObject = hitObject;
            }
            // Continue hovering over same object
            else if (_currentlyHoveredObject != null)
            {
                var hoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                hoverable?.OnHoverStay(_currentlyHoveredObject);
            }
        }

        public int GetGroundTextureIndex()
        {
            var origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 2f, terrainLayer & ~playerLayerMask))
            {
                forwardTerrainLayerDetector?.UpdateFromHit(hit);
                return forwardTerrainLayerDetector?.textureIndex ?? -1;
            }

            return -1;
        }

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif
        public void OnMMEvent(PlayerInteractionEvent eventType)
        {
            if (eventType.EventType == PlayerInteractionEventType.Interacted) PerformInteraction();
        }
    }
}
