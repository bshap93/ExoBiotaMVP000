using DG.Tweening;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class RangedPistolWeapon : RangedToolPrefab
    {
        [SerializeField] GameObject physicalRoot;
        [SerializeField] GameObject slider;
        [SerializeField] GameObject frontEmitter;
        [SerializeField] GameObject cell;
        [SerializeField] GameObject trigger;
        [SerializeField] float cooldownTime = 0.5f;

        [SerializeField] Transform muzzlePosition;
        [SerializeField] PistolToolObject pistolToolObject;


        Vector3 _initialLocalPos;
        bool _readyToFire = true;

        float _timeSinceLastUse;

        void Awake()
        {
            _initialLocalPos = physicalRoot.transform.localPosition;

            AnimController = FindFirstObjectByType<AnimancerRightArmController>();
        }


        void Update()
        {
            if (_timeSinceLastUse < cooldownTime)
                _timeSinceLastUse += Time.deltaTime;
            else
                _readyToFire = true;
        }

        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;
        }

        public void OnUseStarted()
        {
            // Play begin -> during sequence when starting to sample
            if (AnimController != null && AnimController.currentToolAnimationSet != null &&
                AnimController.currentToolAnimationSet.beginUseAnimation != null)
                AnimController.PlayToolUseSequence();
        }

        public override void PerformToolAction()
        {
            if (!_readyToFire) return;

            AnimateRecoil();
            OnUseStarted();

            _readyToFire = false;
            _timeSinceLastUse = 0f;
        }
        void AnimateRecoil()
        {
            physicalRoot.transform.DOKill(); // prevent stacking
            physicalRoot.transform.localPosition = _initialLocalPos;

            physicalRoot.transform.DOPunchPosition(
                new Vector3(0, 0, 0.001f),
                0.15f,
                8,
                0.4f);
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject.defaultReticle;
        }

        public override void ApplyHit()
        {
            Debug.Log("Pistol hit applied");
        }
    }
}
