using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects
{
    [CreateAssetMenu(fileName = "InnerCoreItemObject", menuName = "Scriptable Objects/Items/Inner Core Item Object")]
    public class HarvestableInnerObject : MyBaseItem
    {
        public enum InnerObjectValueGrade
        {
            Common,
            Respectable
        }


        [FormerlySerializedAs("kernelGrade")]
        public InnerObjectValueGrade innerObjectValueGrade = InnerObjectValueGrade.Common;
    }
}
