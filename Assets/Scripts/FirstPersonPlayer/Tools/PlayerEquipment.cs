using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.UI;
using Helpers.AnimancerHelper;
using Helpers.ScriptableObjects.Animation;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.Tools
{
    public class PlayerEquipment : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public enum Hand
        {
            Right,
            Left
        }

        public bool autoEquipLightSourceInLeftHand;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int ActionId;


        public MoreMountains.InventoryEngine.Inventory equipmentInventory; // Reference to the player's inventory

        [SerializeField] public Hand hand = Hand.Right;
        [SerializeField] string equipmentInventoryName = "EquippedItemInventory"; // set per hand

        [SerializeField] ToolAnimationSet emptyHandAnimationSet;

        public Transform toolAnchor;

        [FormerlySerializedAs("ProgressBarBlue")]
        public ProgressBarBlue progressBarBlue;

        [SerializeField] RewiredFirstPersonInputs rewiredInput;

        public TerrainLayerDetector terrainLayerDetector;

        public PlayerInteraction playerInteraction;

        public AnimancerRightArmController animancerRightArmController;

        public bool emptyHandEnabled;

        BaseTool _currentTool;

        Coroutine _equipInitRoutine;

        float _nextUseTime;

        bool _wasUseButtonHeldLastFrame;


        public static PlayerEquipment InstanceRight { get; private set; }
        public static PlayerEquipment InstanceLeft { get; private set; }

        public BaseTool CurrentToolSo { get; private set; }
        public IRuntimeTool CurrentRuntimeTool { get; private set; }

        public static PlayerEquipment Instance { get; private set; }

        void Awake()
        {
            // Register this instance
            if (hand == Hand.Right)
            {
                InstanceRight = this;
                if (Instance == null) Instance = this; // back-compat: default singleton = Right
            }
            else
            {
                InstanceLeft = this;
            }
        }

        void Start()
        {
        }

        void Update()
        {
            if (rewiredInput != null)
            {
                if (hand == Hand.Right)
                    HandleToolInput(rewiredInput.useEquipped);
                else if (rewiredInput.leftHandToggle && autoEquipLightSourceInLeftHand)
                    if (hand == Hand.Left)
                    {
                        if (CurrentToolSo == null) TryEquipLightSourceTool();
                        HandleToolInput(rewiredInput.leftHandToggle);
                    }
            }
        }


        // void Update()
        // {
        //     if (rewiredInput != null)
        //     {
        //         if (rewiredInput.useEquipped)
        //         {
        //             if (hand == Hand.Right)
        //                 UseCurrentTool();
        //         }
        //         else if (rewiredInput.leftHandToggle && autoEquipLightSourceInLeftHand)
        //         {
        //             if (hand == Hand.Left)
        //             {
        //                 if (CurrentToolSo == null) TryEquipLightSourceTool();
        //                 UseCurrentTool();
        //             }
        //         }
        //     }
        // }
        //

        void OnEnable()
        {
            this.MMEventStartListening();

            // Start a short, frame-by-frame wait for the inventory to actually have a non-null slot
            if (_equipInitRoutine != null) StopCoroutine(_equipInitRoutine);
            _equipInitRoutine = StartCoroutine(WaitForInventoryAndEquip());
        }

        void OnDisable()
        {
            this.MMEventStopListening();
            if (_equipInitRoutine != null)
            {
                StopCoroutine(_equipInitRoutine);
                _equipInitRoutine = null;
            }
        }


        void OnDestroy()
        {
            if (hand == Hand.Right)
            {
                if (InstanceRight == this) InstanceRight = null;
                if (Instance == this) Instance = InstanceRight; // keep compat fallback
            }
            else if (InstanceLeft == this)
            {
                InstanceLeft = null;
            }
        }

        public void OnMMEvent(MMInventoryEvent e)
        {
            if (e.TargetInventoryName != equipmentInventoryName) return;

            switch (e.InventoryEventType)
            {
                case MMInventoryEventType.ItemEquipped:
                    if (e.EventItem is BaseTool tool) EquipTool(tool);
                    break;

                case MMInventoryEventType.ItemUnEquipped:
                    if (e.EventItem is BaseTool) UnequipTool();
                    break;

                case MMInventoryEventType.ItemUsed:
                    if (e.EventItem is BaseTool) UseCurrentTool();
                    break;
                case MMInventoryEventType.Destroy:
                    if (e.EventItem is BaseTool) UnequipTool();
                    break;
            }
        }

        void HandleToolInput(bool useButtonHeld)
        {
            if (CurrentRuntimeTool == null || CurrentToolSo == null)
            {
                _wasUseButtonHeldLastFrame = useButtonHeld;
                return;
            }

            var justPressed = useButtonHeld && !_wasUseButtonHeldLastFrame;
            var justReleased = !useButtonHeld && _wasUseButtonHeldLastFrame;

            if (justPressed)
            {
                if (CurrentToolSo.cooldown > 0f && Time.time < _nextUseTime)
                {
                    _wasUseButtonHeldLastFrame = useButtonHeld;
                    return;
                }

                // Notify tool that use started (for animation control)
                if (CurrentRuntimeTool is IToolAnimationControl animControl) animControl.OnUseStarted();

                if (CurrentToolSo.cooldown > 0f)
                    _nextUseTime = Time.time + CurrentToolSo.cooldown;
            }

            if (useButtonHeld) CurrentRuntimeTool.Use();

            if (justReleased)
                // Notify tool that use stopped (for animation control)
                if (CurrentRuntimeTool is IToolAnimationControl animControl)
                    animControl.OnUseStopped();

            _wasUseButtonHeldLastFrame = useButtonHeld;
        }


        void TryEquipLightSourceTool()
        {
            var sourceInventory = GlobalInventoryManager.Instance.playerInventory;
            if (sourceInventory == null) return;
            foreach (var item in sourceInventory.Content)
                if (item is LightSourceToolItemObject lightSourceToolItemObject && lightSourceToolItemObject.Equippable)
                {
                    // Get index of light source item in source inventory
                    var index = Array.IndexOf(sourceInventory.Content, item);
                    if (item == null || sourceInventory == null) return;

                    sourceInventory.EquipItem(item, index);


                    return;
                }
        }

        IEnumerator WaitForInventoryAndEquip()
        {
            // Use your existing inventory lookup; no refactor beyond deferring.
            equipmentInventory =
                MoreMountains.InventoryEngine.Inventory.FindInventory(equipmentInventoryName, "Player1");

            // Wait until the inventory exists AND has at least one non-null slot.
            // (You mentioned the slot exists but is null immediately after death.)
            var timeoutAt = Time.realtimeSinceStartup + 2f; // safety timeout to avoid infinite wait


            while (equipmentInventory == null
                   || equipmentInventory.Content == null
                   || !equipmentInventory.Content.Any(i => i != null))
            {
                if (Time.realtimeSinceStartup > timeoutAt)
                {
                    _equipInitRoutine = null; // give up quietly if nothing to equip
                    yield break;
                }

                yield return null;
                // Re-check next frame
                equipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(equipmentInventoryName, "Player1");
            }

            // Now run your existing equip attempt exactly as before.
            var equippedItem = GetCurrentlyEquippedItem();
            if (equippedItem != null && equippedItem.Equippable) equippedItem.Equip("Player1");


            _equipInitRoutine = null;
        }

        // Active tool helpers (nice for UI/reticle)
        public static PlayerEquipment GetWithActiveToolOrRight()
        {
            if (InstanceRight?.CurrentRuntimeTool != null) return InstanceRight;
            if (InstanceLeft?.CurrentRuntimeTool != null) return InstanceLeft;
            return InstanceRight ?? InstanceLeft;
        }

        // Example tool-type query
        public static PlayerEquipment GetWithToolType(Type runtimeType)
        {
            if (InstanceRight?.CurrentRuntimeTool != null &&
                runtimeType.IsInstanceOfType(InstanceRight.CurrentRuntimeTool)) return InstanceRight;

            if (InstanceLeft?.CurrentRuntimeTool != null &&
                runtimeType.IsInstanceOfType(InstanceLeft.CurrentRuntimeTool)) return InstanceLeft;

            return null;
        }

        // Helpers for callers that need a specific context
        public static PlayerEquipment Get(Hand h)
        {
            return h == Hand.Right ? InstanceRight : InstanceLeft;
        }

        void EquipTool(BaseTool tool)
        {
            UnequipTool();

            if (tool.FPToolPrefab == null)
            {
                Debug.LogWarning($"[{name}] {tool.name} has no Prefab assigned.");
                return;
            }


            var go = Instantiate(tool.FPToolPrefab, toolAnchor, false);
            CurrentRuntimeTool = go.GetComponent<IRuntimeTool>();
            if (CurrentRuntimeTool == null)
            {
                Debug.LogWarning($"[{name}] {tool.name}'s prefab doesn't implement IRuntimeTool.");
                Destroy(go);
                return;
            }


            CurrentToolSo = tool;


            if (hand == Hand.Right && animancerRightArmController != null)
            {
                animancerRightArmController.gameObject.SetActive(true);

                animancerRightArmController.currentToolAnimationSet = tool.toolAnimationSet;
                animancerRightArmController.UpdateAnimationSet();
            }

            CurrentRuntimeTool.Initialize(this);

            if (CurrentRuntimeTool is IToolAnimationControl animControl) animControl.OnEquipped();

            var fb = CurrentRuntimeTool.GetEquipFeedbacks();
            fb?.PlayFeedbacks();
            _nextUseTime = 0f;
        }

        public void UnequipTool()
        {
            if (CurrentRuntimeTool is MonoBehaviour mb)
            {
                CurrentRuntimeTool.Unequip();
                Destroy(mb.gameObject);
            }

            CurrentRuntimeTool = null;
            if (hand == Hand.Right && animancerRightArmController != null)
            {
                if (emptyHandEnabled)
                {
                    animancerRightArmController.currentToolAnimationSet = emptyHandAnimationSet;
                    animancerRightArmController.UpdateAnimationSet();
                }
                else


                {
                    animancerRightArmController.gameObject.SetActive(false);
                }

                animancerRightArmController.currentToolAnimationSet = null;
            }

            CurrentToolSo = null;
            _nextUseTime = 0f;
        }

        public void UseCurrentTool()
        {
            if (CurrentRuntimeTool == null || CurrentToolSo == null) return;

            if (CurrentToolSo.cooldown > 0f && Time.time < _nextUseTime) return;

            CurrentRuntimeTool.Use();


            if (CurrentToolSo != null && CurrentToolSo.cooldown > 0f)
                _nextUseTime = Time.time + CurrentToolSo.cooldown;
        }

        public InventoryItem GetCurrentlyEquippedItem()
        {
            if (hand == Hand.Right)
                equipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");
            else if (hand == Hand.Left)
                equipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory("LEquipmentInventory", "Player1");

            if (equipmentInventory == null)
            {
                Debug.LogError("Equipment inventory is not assigned.");
                return null;
            }

            // Assuming the first item in the inventory is the equipped item

            var equippedItem = equipmentInventory.Content.FirstOrDefault();
            if (equippedItem != null && (InventoryItem.IsNull(equippedItem) || equippedItem.Quantity <= 0)) return null;

            return equippedItem;
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
    }
}
