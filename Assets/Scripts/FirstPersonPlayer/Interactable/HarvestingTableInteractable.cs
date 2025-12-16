using System;
using FirstPersonPlayer.Interface;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class HarvestingTableInteractable : MonoBehaviour, IRequiresUniqueID, IInteractable
    {
        public string harvestingTableId;
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
            throw new NotImplementedException();
        }
        public bool IsInteractable()
        {
            throw new NotImplementedException();
        }
        public void OnFocus()
        {
            throw new NotImplementedException();
        }
        public void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        public float GetInteractionDistance()
        {
            throw new NotImplementedException();
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
