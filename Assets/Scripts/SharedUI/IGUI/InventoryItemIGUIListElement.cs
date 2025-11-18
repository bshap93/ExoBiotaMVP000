using System;
using Events;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Inventory;
using LevelConstruct.Interactable.ItemInteractables;
using Manager.Global;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities.Static;
using ItemPicker = LevelConstruct.Interactable.ItemInteractables.ItemPicker.ItemPicker;

namespace SharedUI.IGUI
{
    public class InventoryItemIGUIListElement : MonoBehaviour
    {
        public enum ItemType
        {
            Equippable,
            Consumable,
            UseableButNotConsumable,
            Other
        }

        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text itemNameText;
        [SerializeField] ButtonManager infoButton;
        [SerializeField] TMP_Text quantityText;

        [FormerlySerializedAs("useOrEquipButton")] [SerializeField]
        ButtonManager equipButton;
        [SerializeField] ButtonManager useButton;

        [SerializeField] ButtonManager placeButton;

        [SerializeField] ButtonManager moveButton;

        [SerializeField] SlotsIGUIController slotsIGUIController;

        [SerializeField] MMFeedbacks placeObjectFeedbacks;

        MyBaseItem _item;
        // Start is called once before the first execution of Update after the MonoBehaviour is created


        ItemType _itemType;
        int _sourceIndex;

        MoreMountains.InventoryEngine.Inventory _sourceInventory;


        public void Initialize(MoreMountains.InventoryEngine.Inventory source, int index)
        {
            _sourceInventory = source;
            _sourceIndex = index;
            _item = source.Content[index] as MyBaseItem;

            if (_item == null)
            {
                Debug.LogError($"Item at index {index} in inventory {source.name} is not a MyBaseItem.");
                return;
            }

            itemImage.sprite = _item.GetDisplayIcon();
            itemNameText.text = _item.GetDisplayName();

            if (_item.Quantity > 1)
            {
                quantityText.text = $"x{_item.Quantity.ToString()}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }

            infoButton.onClick.AddListener(ShowItemInfo);

            equipButton.onClick.RemoveAllListeners();
            placeButton.onClick.RemoveAllListeners();


            var currentMode = GameStateManager.Instance.CurrentMode;
            if (_item.Equippable)
            {
                equipButton.onClick.AddListener(EquipViaMM);
                useButton.gameObject.SetActive(false);

                if (currentMode == GameMode.FirstPerson &&
                    _item.equippableContext == MyBaseItem.EquippableCtx.FirstPerson)
                {
                }
                else if ((currentMode == GameMode.DirigibleFlight &&
                          _item.equippableContext == MyBaseItem.EquippableCtx.Dirigible) ||
                         (currentMode == GameMode.Overview &&
                          _item.equippableContext == MyBaseItem.EquippableCtx.Dirigible))
                {
                }
                else
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else
            {
                equipButton.gameObject.SetActive(false);
            }

            if (currentMode == GameMode.DirigibleFlight || currentMode == GameMode.Overview)
            {
                moveButton.onClick.AddListener(MoveItem);
                moveButton.gameObject.SetActive(true);
            }
            else
            {
                moveButton.gameObject.SetActive(false);
            }

            if (_item.Usable && !_item.Consumable && !_item.Equippable)
                useButton.onClick.AddListener(UseNonConsumable);
            else if (_item.Usable && _item.Consumable && !_item.Equippable)
                useButton.onClick.AddListener(UseConsumable);
            else useButton.gameObject.SetActive(false);


            SetPlaceButtonActiveIf();
        }
        void SetPlaceButtonActiveIf()
        {
            if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
            {
                placeButton.onClick.AddListener(PlaceItem);
                placeButton.gameObject.SetActive(true);
            }
            else
            {
                placeButton.gameObject.SetActive(false);
            }
        }

        void MoveItem()
        {
            if (_item == null || _sourceInventory == null) return;

            if (_sourceInventory.name == "PlayerMainInventory")
            {
                Debug.Log("Moving item to DirigibleInventory");
                var dirigibleInventory = GlobalInventoryManager.Instance.dirigibleInventory;
                // var newWeight = GlobalInventoryManager.Instance.GetTotalWeightInDirigible();

                if (dirigibleInventory == null)
                {
                    Debug.LogError("No 'DirigibleInventory' for Player1 found.");
                    return;
                }

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.StartMove);

                AlertEvent.Trigger(
                    AlertReason.ItemMoved,
                    $"Moved item '{_item.ItemName}' to dirigible Inventory.", "Item Moved");


                // Make the move
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null, _sourceInventory.name
                    , _item, 1, _sourceIndex,
                    "Player1");

                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null, dirigibleInventory.name, _item, 1, -1,
                    "Player1");

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.FinishMove);
            }
            else if (_sourceInventory.name == "DirigibleInventory")
            {
                Debug.Log("Moving item to PlayerMainInventory");
                var playerInventory = GlobalInventoryManager.Instance.playerInventory;
                if (playerInventory == null)
                {
                    Debug.LogError("No 'PlayerMainInventory' for Player1 found.");
                    return;
                }

                var pInvCurrentWeight = GlobalInventoryManager.Instance.GetWeightOfInventory(playerInventory);
                var pInvMaxWeight = GlobalInventoryManager.Instance.GetMaxWeightOfPlayerCarry();
                var itemWeight = _item.weight * _item.Quantity;

                if (pInvCurrentWeight + itemWeight > pInvMaxWeight)
                {
                    AlertEvent.Trigger(
                        AlertReason.InventoryFull,
                        $"Cannot move item '{_item.ItemName}'. Player inventory weight limit exceeded.",
                        "Inventory Full");

                    return;
                }

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.StartMove);

                AlertEvent.Trigger(
                    AlertReason.ItemMoved,
                    $"Moved item '{_item.ItemName}' to Player Inventory.", "Item Moved");

                // Make the move
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null, _sourceInventory.name
                    , _item, 1, _sourceIndex,
                    "Player1");

                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null, playerInventory.name, _item, 1, -1,
                    "Player1");

                ItemTransactionEvent.Trigger(
                    ItemTransactionEventType.FinishMove);
            }
        }

        void PlaceItem()
        {
            var propsItemHold = FindFirstObjectByType<PlayerPropPickup>();
            if (propsItemHold == null)
            {
                Debug.LogError("No PlayerPropPickup found in scene.");
                return;
            }

            if (_item.isQuestItem)
            {
                AlertEvent.Trigger(
                    AlertReason.CannotPlaceQuestItem, "Cannot remove a quest item by placing it.",
                    "Cannot Remove Quest Item");

                return;
            }


            if (propsItemHold.heldRb == null)
            {
                if (propsItemHold.AreBothHandsOccupied())
                    UnequipViaMM();
                // PlayerInteraction.Instance.RightHandEquipment.UnequipTool();

                var playerInventory = GlobalInventoryManager.Instance.playerInventory;
                var prefab = _item.Prefab;
                playerInventory?.RemoveItemByID(_item.ItemID, 1);
                var holdPoint = propsItemHold.holdPoint;
                var instance = Instantiate(prefab, holdPoint.position, holdPoint.rotation);
                placeObjectFeedbacks?.PlayFeedbacks();
                var itemPicker = instance.GetComponent<ItemPicker>();
                var statefulItemPicker = instance.GetComponent<IStatefulItemPicker>();
                if (statefulItemPicker != null) statefulItemPicker.SetStateToDefault();
                itemPicker.uniqueID = Guid.NewGuid().ToString();
                propsItemHold.SetItem(instance);
                MyUIEvent.Trigger(UIType.InGameUI, UIActionType.Close);
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, 67, "BlockAllNewRequests");
            }
            else
            {
                AlertEvent.Trigger(
                    AlertReason.HoldingItemAlready, "Cannot place item, already holding one.", "Holding an Item");
            }
        }

        void DropItem()
        {
            if (_item == null || _sourceInventory == null) return;
            // Remove one quantity of the item from the source inventory
            _sourceInventory.DropItem(_item, _sourceIndex);
            Debug.Log($"Dropped item: {_item.ItemName} from {_sourceInventory.name}[{_sourceIndex}]");
        }


        void EquipViaMM()
        {
            if (_item == null || _sourceInventory == null) return;

            // Make sure target equipment inventory exists & has room
            var equipInv = MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");
            if (equipInv == null)
            {
                Debug.LogError("No 'EquippedItemInventory' for Player1 found.");
                return;
            }

            // Ensure capacity >= 1
            if (equipInv.Content == null || equipInv.Content.Length < 1)
            {
                Debug.LogError(
                    $"Equipment inventory '{equipInv.name}' has size {equipInv.Content?.Length ?? 0}. Set it to >= 1.");

                return;
            }


            // This is the important bit: use the inventory + index, NOT item.Equip()
            // _sourceInventory.EquipItem(_item, _sourceIndex);
            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, _item.TargetInventoryName, _item, 1, _sourceIndex, "Player1");

            Debug.Log($"Equip attempt '{_item.ItemName}' from '{_sourceInventory.name}[{_sourceIndex}]'");
        }

        void UnequipViaMM()
        {
            if (_item == null || _sourceInventory == null) return;

            // Make sure target equipment inventory exists & has room
            var equipInv = MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");
            if (equipInv == null)
            {
                Debug.LogError("No 'EquippedItemInventory' for Player1 found.");
                return;
            }

            // Ensure capacity >= 1
            if (equipInv.Content == null || equipInv.Content.Length < 1)
            {
                Debug.LogError(
                    $"Equipment inventory '{equipInv.name}' has size {equipInv.Content?.Length ?? 0}. Set it to >= 1.");

                return;
            }

            MMInventoryEvent.Trigger(
                MMInventoryEventType.UnEquipRequest, null, _item.TargetEquipmentInventoryName, _item, 1, _sourceIndex,
                "Player1");
        }


        void UseConsumable()
        {
            if (_item == null) return;

            // ask the item to use itself for Player1.
            _item.Use("Player1");

            // Optionally, remove the item from the inventory after use
            _sourceInventory.RemoveItem(_sourceIndex, _item.Quantity);
        }

        void UseNonConsumable()
        {
            if (_item == null) return;

            // ask the item to use itself for Player1.
            _item.Use("Player1");

            Debug.Log($"Used non-consumable item: {_item.ItemName}");
        }


        void ShowItemInfo()
        {
            InventoryEvent.Trigger(InventoryEventType.ShowItem, null, _item);
        }
    }
}
