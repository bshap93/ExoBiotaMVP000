using System;
using Animancer;
using FirstPersonPlayer.Interface;
using Inventory;
using UnityEngine;
using Utilities.Interface;

namespace LevelConstruct.Interactable.Door
{
    public class LockedDoor : MonoBehaviour, IRequiresUniqueID, IInteractable
    {
        public bool isLocked;
        public string uniqueID;

        [SerializeField] AnimancerComponent animancerComponent;


        [SerializeField] AnimationClip openAnimation;
        [SerializeField] AnimationClip closeAnimation;
        [SerializeField] AnimationClip openedAnimation;
        public void Interact()
        {
            throw new NotImplementedException();
        }
        public void OnInteractionStart()
        {
            throw new NotImplementedException();
        }
        public void OnInteractionEnd(string param)
        {
            throw new NotImplementedException();
        }
        public bool CanInteract()
        {
            if (!isLocked)
                return true;

            if (GlobalInventoryManager.Instance.HasKeyForDoor(uniqueID))
            {
                isLocked = false;
                return true;
            }

            return false;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
            throw new NotImplementedException();
        }
        public void OnUnfocus()
        {
            throw new NotImplementedException();
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
