using System;
using Animancer;
using UnityEngine;
using Utilities.Interface;

namespace LevelConstruct.Interactable.Door
{
    public class LockedDoor : MonoBehaviour, IRequiresUniqueID
    {
        public bool isLocked;
        public string uniqueID;

        public string keyID;

        [SerializeField] AnimancerComponent animancerComponent;


        [SerializeField] AnimationClip openAnimation;
        [SerializeField] AnimationClip closeAnimation;
        [SerializeField] AnimationClip openedAnimation;

        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
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
        }
    }
}
