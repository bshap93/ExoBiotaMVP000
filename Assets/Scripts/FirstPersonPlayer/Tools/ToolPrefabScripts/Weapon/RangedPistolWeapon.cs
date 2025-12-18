using FirstPersonPlayer.Tools.ItemObjectTypes;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class RangedPistolWeapon : RangedToolPrefab
    {
        [SerializeField] GameObject slider;
        [SerializeField] GameObject frontEmitter;
        [SerializeField] GameObject cell;
        [SerializeField] GameObject trigger;
        [SerializeField] PistolToolObject pistolToolObject;
        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;
        }
        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject.defaultReticle;
        }
        public override void PerformToolAction()
        {
            Debug.Log("Shooting pistol");
        }
        public override void ApplyHit()
        {
            Debug.Log("Pistol hit applied");
        }
    }
}
