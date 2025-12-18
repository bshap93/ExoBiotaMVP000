using System;
using FirstPersonPlayer.Tools.Interface;
using Helpers.AnimancerHelper;
using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public abstract class RangedToolPrefab : MonoBehaviour, IRuntimeTool
    {
        [SerializeField] protected bool toolIsUsedOnRelease;

        [Header("References")] public Camera mainCamera;
        protected AnimancerRightArmController AnimancerRightArmController;
        protected AnimancerRightArmController AnimController;
        protected RaycastHit LastHit;


        public abstract void Initialize(PlayerEquipment owner);
        public void Use()
        {
            PerformToolAction();
        }
        public void Unequip()
        {
            throw new NotImplementedException();
        }
        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            throw new NotImplementedException();
        }
        public abstract Sprite GetReticleForTool(GameObject colliderGameObject);

        public bool ToolIsUsedOnRelease()
        {
            return toolIsUsedOnRelease;
        }
        public bool CanAbortAction()
        {
            throw new NotImplementedException();
        }
        public MMFeedbacks GetEquipFeedbacks()
        {
            throw new NotImplementedException();
        }
        public CanBeAreaScannedType GetDetectableType()
        {
            throw new NotImplementedException();
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            throw new NotImplementedException();
        }
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        public abstract void PerformToolAction();

        public abstract void ApplyHit();
    }
}
