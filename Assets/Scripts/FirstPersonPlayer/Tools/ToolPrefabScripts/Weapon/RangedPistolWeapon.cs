using System;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class RangedPistolWeapon : RangedToolPrefab
    {
        [SerializeField] GameObject slider;
        [SerializeField] GameObject frontEmitter;
        [SerializeField] GameObject cell;
        [SerializeField] GameObject trigger;
        public override void Initialize(PlayerEquipment owner)
        {
            throw new NotImplementedException();
        }
        public override void PerformToolAction()
        {
            throw new NotImplementedException();
        }
        public override void ApplyHit()
        {
            throw new NotImplementedException();
        }
    }
}
