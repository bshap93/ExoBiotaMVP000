using System;
using FirstPersonPlayer.Interface;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    [DisallowMultipleComponent]
    public class HarvestingTableInteractable : MonoBehaviour, IRequiresUniqueID, IInteractable
    {
        [SerializeField] float interactionDistance = 3f;
        public string harvestingTableId;
        public void Interact()
        {
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
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public string UniqueID => harvestingTableId;
        public void SetUniqueID()
        {
            harvestingTableId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(harvestingTableId);
        }
    }
}
