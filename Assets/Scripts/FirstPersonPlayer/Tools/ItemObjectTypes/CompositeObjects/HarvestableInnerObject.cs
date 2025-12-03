using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects
{
    [CreateAssetMenu(fileName = "InnerCoreItemObject", menuName = "Scriptable Objects/Items/Inner Core Item Object")]
    public class HarvestableInnerObject : MyBaseItem
    {
        [Serializable]
        public enum InnerObjectValueGrade
        {
            StandardGrade,
            Radiant,
            Stellar,
            Unreasonable,
            MiscExotic
        }


        [FormerlySerializedAs("kernelGrade")]
        public InnerObjectValueGrade innerObjectValueGrade = InnerObjectValueGrade.StandardGrade;
    }
}
