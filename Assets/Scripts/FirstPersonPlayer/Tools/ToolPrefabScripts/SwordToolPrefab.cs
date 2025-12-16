using System;
using FirstPersonPlayer.Tools.Interface;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class SwordToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Sword Settings")] [Tooltip("Tags this sword is allowed to affect (e.g., BioObstacle, Vegetation).")]
        public string[] allowedTags;
        public float baseStaminaCostPerConnectingSwing = 1.5f;
        
        [Tooltip("Number of seconds between swings.")]
        public float swingCooldown = 0.8f;

        public int swordPower = 1;
        
        [SerializeField] Sprite defaultReticleForTool;
        
        [SerializeField]
        protected float lastSwingTime = -999f;
        
        float StaminaCostPerNormalConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return baseStaminaCostPerConnectingSwing;

                var agility = attrMgr.Agility;
                var reduction = agilityReductionFactor * agility; // Example: 0.05
                var finalCost = baseStaminaCostPerConnectingSwing * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }
        public override void Initialize(PlayerEquipment owner)
        {
            throw new NotImplementedException();
        }
        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            throw new NotImplementedException();
        }
        public override MMFeedbacks GetEquipFeedbacks()
        {
            throw new NotImplementedException();
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            throw new NotImplementedException();
        }
        public override void ApplyHit()
        {
            throw new NotImplementedException();
        }
        public override void PerformToolAction()
        {
            throw new NotImplementedException();
        }
    }
}
