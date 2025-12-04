using FirstPersonPlayer.UI.InventoryListView;
using Helpers.Events;
using Helpers.Events.Progression;
using Manager;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using SharedUI.Progression;
using TMPro;
using UnityEngine;

namespace SharedUI.Interact
{
    public class GatedLevelingUIController : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<XPEvent>
    {
        [SerializeField] InnerCoresDisplay innerCoresDisplay;

        [Header(" Attribute Setters ")] [SerializeField]
        AttributePointSetter dexteritySetter;
        [SerializeField] AttributePointSetter mentalToughnessSetter;
        [SerializeField] AttributePointSetter agilitySetter;
        [SerializeField] AttributePointSetter strengthSetter;
        [Header("Individual UI Elements")] [SerializeField]
        TMP_Text totalUnusedXPText;

        [Header("Feedbacks and Buttons")] [SerializeField]
        MMFeedbacks openFeedbacks;
        [SerializeField] MMFeedbacks addXPFeedbacks;


        [Header(" Buttons ")] [SerializeField] ButtonManager commitButton;
        [SerializeField] ButtonManager cancelButton;

        CanvasGroup _canvasGroup;

        int _currentUnusedXP;
        public int CurrentUnusedXP
        {
            get => _currentUnusedXP;
            set
            {
                _currentUnusedXP = value;
                totalUnusedXPText.text = _currentUnusedXP.ToString();
            }
        }
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // hide
            Hide();
        }

        void Start()
        {
            Initialize();
        }
        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<XPEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<XPEvent>();
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
        public void OnMMEvent(XPEvent eventType)
        {
            if (eventType.EventType == XPEventType.SetUnusedXP)
            {
                if (CurrentUnusedXP > eventType.Amount)
                {
                    // spent XP
                }
                else
                {
                    // gained XP
                    addXPFeedbacks?.PlayFeedbacks();
                }

                CurrentUnusedXP = eventType.Amount;
                totalUnusedXPText.text = CurrentUnusedXP.ToString();
            }
        }

        public void Initialize()
        {
            _currentUnusedXP = 0;
            var attributeManager = AttributesManager.Instance;
            dexteritySetter.Initialize(attributeManager.Dexterity);
            mentalToughnessSetter.Initialize(attributeManager.MentalToughness);
            agilitySetter.Initialize(attributeManager.Agility);
            strengthSetter.Initialize(attributeManager.Strength);

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => { CancelLeveling(); });
            commitButton.onClick.RemoveAllListeners();
            commitButton.onClick.AddListener(CommitChanges);
        }

        void CancelLeveling()
        {
            // Logic to cancel attribute point changes
            Debug.Log("Canceled attribute point changes.");
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
