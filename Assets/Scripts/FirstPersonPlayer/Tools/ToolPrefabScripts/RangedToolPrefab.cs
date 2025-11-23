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
        protected AnimancerRightArmController AnimancerRightArmController;
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
        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            throw new NotImplementedException();
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

        public abstract void PerformToolAction();

        public abstract void ApplyHit();
    }
}
