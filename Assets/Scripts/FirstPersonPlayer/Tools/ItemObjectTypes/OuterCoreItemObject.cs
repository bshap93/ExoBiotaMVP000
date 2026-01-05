using System;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "CoreItemObject", menuName = "Scriptable Objects/Items/Core Item Object")]
    public class OuterCoreItemObject : MyBaseItem
    {
        [Serializable]
        public enum CoreObjectValueGrade
        {
            StandardGrade,
            Radiant,
            Stellar,
            Unreasonable,
            MiscExotic
        }

        public enum CoreReactivity
        {
            MostReactive,
            HighlyReactive,
            Reactive,
            Resistant,
            HighlyResistant
        }


        [FormerlySerializedAs("innerObjectValueGrade")] [FormerlySerializedAs("kernelGrade")]
        public CoreObjectValueGrade coreObjectValueGrade = CoreObjectValueGrade.StandardGrade;

        [FormerlySerializedAs("coreGrade")] public CoreReactivity coreReactivity = CoreReactivity.HighlyReactive;

        // TODO most likely just use UniqueIds
        // public List<CoreKernelItemObject> possibleKernelCoreObjects;

        public float dissolveSpeed = 2.0f;

        // For testing purposed, we just do 1 to 1 rather than loot tables
        public InventoryItem innerCoreItem;
        public GameObject innerCoreObjectPicker;

        [Header("Sorting")] public int corePriorityLevel;
    }
}
