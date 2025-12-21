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

        [SerializeField] protected MMFeedbacks equipFeedbacks;
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
        }

        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return true;
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
            return equipFeedbacks;
        }
        public CanBeAreaScannedType GetDetectableType()
        {
            throw new NotImplementedException();
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            return equipFeedbacks;
        }
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        public abstract void PerformToolAction();

        public abstract void ApplyHit();
    }
}
