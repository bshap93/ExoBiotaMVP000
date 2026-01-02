using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Hotbar
{ 
    [DisallowMultipleComponent]
    public class HotbarUISlot : MonoBehaviour
    {
        public enum HotbarSlotType
        {
            Tool,
            Hands,
            EmptySlot,
            Consumable,
        }
        [SerializeField] Image slotBG;
        [SerializeField] Image secondaryBG;
        [SerializeField] Image itemIcon;
        [SerializeField] bool quantityEnabled;
        [ShowIf("quantityEnabled")]
        [SerializeField] TMP_Text quantityText;
        [ShowIf("quantityEnabled")]
        [SerializeField] GameObject quantityBadge;
        [ShowIf("quantityEnabled")]
        [SerializeField] Color moreThanZeroColor;
        [ShowIf("quantityEnabled")]
        [SerializeField] Color zeroColor;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
