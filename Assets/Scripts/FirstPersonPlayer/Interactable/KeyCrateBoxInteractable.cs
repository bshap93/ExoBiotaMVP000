using System;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Inventory;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class KeyCrateBoxInteractable : MonoBehaviour, IInteractable, IRequiresUniqueID, IHoverable, IBillboardable
    {
        public string uniqueID;
        [SerializeField] KeyItemObject keyItem;
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Sprite icon;

        [Header("Items inside the crate box")] [SerializeField]
        bool hasOtherItems;
        [ShowIf("hasOtherItems")] [SerializeField]
        MyBaseItem[] items;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks getKeyItemFeedback;
        [SerializeField] MMFeedbacks alreadyGotKeyFeedback;

        bool _hasBeenOpened;
        void Start()
        {
        }

        public string GetName()
        {
            return "Key Box";
        }
        public Sprite GetIcon()
        {
            return icon;
        }
        public string ShortBlurb()
        {
            if (!hasOtherItems)
                return "Contains a virtual key.";

            return "Contains a key and $" + items.Length + " other items.";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.pushIcon;
        }
        public string GetActionText()
        {
            return "Open";
        }
        public bool OnHoverStart(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public void Interact()
        {
            if (!_hasBeenOpened)

            {
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null,
                    keyItem.TargetInventoryName, keyItem, 1, 0, GlobalInventoryManager.Instance.playerId);

                getKeyItemFeedback?.PlayFeedbacks();
            }
            else
            {
                alreadyGotKeyFeedback?.PlayFeedbacks();
            }

            // TODO: Add other items to inventory if hasOtherItems is true.

            _hasBeenOpened = true;
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            return true;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
