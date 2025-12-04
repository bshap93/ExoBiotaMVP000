using FirstPersonPlayer.UI.InventoryListView;
using Helpers.Events;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Interact
{
    public class GatedLevelingUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [SerializeField] InnerCoresDisplay innerCoresDisplay;

        [SerializeField] AttributePointSetter dexteritySetter;
        [SerializeField] AttributePointSetter mentalToughnessSetter;
        [SerializeField] AttributePointSetter agilitySetter;
        [SerializeField] AttributePointSetter strengthSetter;


        [SerializeField] ButtonManager commitButton;

        CanvasGroup _canvasGroup;
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // hide
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.LevelingUI)
            {
                if (eventType.uiActionType == UIActionType.Open)
                {
                    // show
                    _canvasGroup.alpha = 1;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    // hide
                    _canvasGroup.alpha = 0;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
}
