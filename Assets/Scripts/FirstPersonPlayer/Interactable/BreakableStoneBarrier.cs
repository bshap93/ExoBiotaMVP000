using System;
using HighlightPlus;
using Manager;
using MoreMountains.Feedbacks;
using RayFire;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class BreakableStoneBarrier : MonoBehaviour, IRequiresUniqueID, IBreakable
    {
        [SerializeField] RayfireRigid rayfireRigid;
        [SerializeField] int strengthNeededToBreak = 2;
        public string uniqueId;

        [Tooltip("If set, destroy this root instead of just this component's GameObject.")]
        public GameObject destroyRoot;
        HighlightEffect _highlightEffect;
        
        public MMFeedbacks onBreakFeedbacks;


        void Awake()
        {
            _highlightEffect = GetComponent<HighlightEffect>();

            if (rayfireRigid == null)
                rayfireRigid = GetComponent<RayfireRigid>();

            rayfireRigid.demolitionEvent.LocalEvent += OnDemolished;
        }


        public bool CanBeDamagedBy(int toolPower, int strength)
        {
            var attrMgr = AttributesManager.Instance;
            return attrMgr != null && attrMgr.Strength >= strengthNeededToBreak;
        }
        public void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal)
        {
            var attrMgr = AttributesManager.Instance;
            var root = destroyRoot != null ? destroyRoot : gameObject;

            if (CanBeDamagedBy(toolPower, attrMgr.Strength))
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                if (rayfireRigid != null)
                    rayfireRigid.Demolish();
                else
                    Destroy(root, 0.05f);
            }
        }
        public string UniqueID =>
            uniqueId;
        public void SetUniqueID()
        {
            uniqueId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueId);
        }


        void OnDemolished(RayfireRigid demolished)
        {
            if (demolished.HasFragments)
                foreach (var frag in demolished.fragments)
                    frag.gameObject.layer = LayerMask.NameToLayer("Debris");
        }
    }
}
