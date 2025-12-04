using FirstPersonPlayer.UI.InventoryListView;
using Helpers.Events;
using Manager;
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
            Hide();
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
                    Show();
                else if (eventType.uiActionType == UIActionType.Close) Hide();
            }
        }

        public void Initialize()
        {
            var attributeManager = AttributesManager.Instance;
            dexteritySetter.Initialize(attributeManager.Dexterity, attributeManager.DexterityXp);
            mentalToughnessSetter.Initialize(attributeManager.MentalToughness, attributeManager.MentalToughnessXp);
            agilitySetter.Initialize(attributeManager.Agility, attributeManager.AgilityXp);
            strengthSetter.Initialize(attributeManager.Strength, attributeManager.StrengthXp);

            commitButton.onClick.AddListener(CommitChanges);
        }

        void CommitChanges()
        {
            // Logic to commit attribute point changes
            Debug.Log("Committed attribute point changes.");
        }

        void Hide()
        {
            // hide
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        void Show()
        {
            innerCoresDisplay.Refresh();
            // show
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
