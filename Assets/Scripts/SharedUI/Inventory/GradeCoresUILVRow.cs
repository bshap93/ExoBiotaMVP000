using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI.Inventory
{
    public class GradeCoresUILVRow : MonoBehaviour
    {
        [SerializeField] Image coreImage;
        [SerializeField] TMP_Text coreNameText;
        [SerializeField] TMP_Text coreQuantityText;

        [FormerlySerializedAs("onIncreaseQuantityF")]
        [FormerlySerializedAs("onChangeQuantityFeedbacks")]
        [SerializeField]
        MMFeedbacks onIncreaseQuantityFeedbacks;
        [SerializeField] MMFeedbacks onDecreaseQuantityFeedbacks;


        int _currentQuantity;


        public void Initialize(HarvestableInnerObject.InnerObjectValueGrade grade, int quantity)
        {
            if (quantity > _currentQuantity) onIncreaseQuantityFeedbacks?.PlayFeedbacks();
            else if (quantity < _currentQuantity) onDecreaseQuantityFeedbacks?.PlayFeedbacks();

            _currentQuantity = quantity;
            coreQuantityText.text = _currentQuantity.ToString();
        }
    }
}
