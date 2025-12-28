using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(
        fileName = "AmmoItem",
        menuName = "Scriptable Objects/Items/AmmoItem",
        order = 0)]
    public class AmmoItem : MyBaseItem
    {
        public enum AmmoType
        {
            EnergyCell,
            ProjectileClip
        }

        public AmmoType ammoType;
        [ShowIf("ammoType", AmmoType.EnergyCell)]
        public float energyCellChargeMax = 80f;
        [ShowIf("ammoType", AmmoType.ProjectileClip)]
        public int projectileClipRoundsMax = 30;
    }
}
