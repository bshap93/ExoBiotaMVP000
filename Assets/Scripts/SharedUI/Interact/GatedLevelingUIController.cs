using FirstPersonPlayer.UI.InventoryListView;
using Helpers.Events;
using Helpers.Events.Gated;
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
    public class GatedLevelingUIController : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<XPEvent>,
        MMEventListener<AttrPendingBuyEvent>
    {
        [SerializeField] InnerCoresDisplay innerCoresDisplay;

        [Header(" Attribute Setters ")] [SerializeField]
        AttributePointSetter dexteritySetter;
        [SerializeField] AttributePointSetter mentalToughnessSetter;
        [SerializeField] AttributePointSetter agilitySetter;
        [SerializeField] AttributePointSetter strengthSetter;
        [Header("Individual UI Elements")] [SerializeField]
        TMP_Text totalUnusedXPText;
        [SerializeField] WaitWhileInteractingOverlay waitOverlay;

        [Header("Feedbacks and Buttons")] [SerializeField]
        MMFeedbacks openFeedbacks;
        [SerializeField] MMFeedbacks addXPFeedbacks;


        [Header(" Buttons ")] [SerializeField] ButtonManager commitButton;
        [SerializeField] ButtonManager cancelButton;

        CanvasGroup _canvasGroup;

        int _currentUnusedXP;

        int _initialAgility;
        int _initialDexterity;
        int _initialMentalToughness;
        int _initialStrength;
        int _pendingNewAgility;
        int _pendingNewDexterity;
        int _pendingNewMentalToughness;
        int _pendingNewStrength;

        int _pendingNewUnusedXP;
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
            this.MMEventStartListening<AttrPendingBuyEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<XPEvent>();
            this.MMEventStopListening<AttrPendingBuyEvent>();
        }
        public void OnMMEvent(AttrPendingBuyEvent eventType)
        {
            var attributeManager = AttributesManager.Instance;
            if (eventType.PendingBuyEventType == PendingBuyEventType.IncreasePendingAttribute)
            {
                if (attributeManager.CurrentUnusedXP -
                    attributeManager.GetXpRequiredForLevel(eventType.AttrLevelTarget) < 0)
                    return;

                _pendingNewUnusedXP = attributeManager.CurrentUnusedXP -
                                      attributeManager.GetXpRequiredForLevel(eventType.AttrLevelTarget);

                switch (eventType.AttributeType)
                {
                    case AttributeType.Dexterity:
                        _pendingNewDexterity = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.MentalToughness:
                        _pendingNewMentalToughness = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.Agility:
                        _pendingNewAgility = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.Strength:
                        _pendingNewStrength = eventType.AttrLevelTarget;
                        break;
                }
            }
            else if (eventType.PendingBuyEventType == PendingBuyEventType.DecreasePendingAttribute)
            {
                switch (eventType.AttributeType)
                {
                    case AttributeType.Dexterity:
                        if (eventType.AttrLevelTarget < _initialDexterity) return;
                        break;
                    case AttributeType.MentalToughness:
                        if (eventType.AttrLevelTarget < _initialMentalToughness) return;
                        break;
                    case AttributeType.Agility:
                        if (eventType.AttrLevelTarget < _initialAgility) return;
                        break;
                    case AttributeType.Strength:
                        if (eventType.AttrLevelTarget < _initialStrength) return;
                        break;
                }

                _pendingNewUnusedXP = attributeManager.CurrentUnusedXP +
                                      attributeManager.GetXpRequiredForLevel(eventType.AttrLevelTarget - 1);


                switch (eventType.AttributeType)
                {
                    case AttributeType.Dexterity:
                        _pendingNewDexterity = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.MentalToughness:
                        _pendingNewMentalToughness = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.Agility:
                        _pendingNewAgility = eventType.AttrLevelTarget;
                        break;
                    case AttributeType.Strength:
                        _pendingNewStrength = eventType.AttrLevelTarget;
                        break;
                }
            }
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

        void Initialize()
        {
            var attributeManager = AttributesManager.Instance;
            if (attributeManager == null)
            {
                Debug.LogError("AttributesManager instance not found!");
                return;
            }

            // Unused XP
            _currentUnusedXP = attributeManager.CurrentUnusedXP;
            totalUnusedXPText.text = _currentUnusedXP.ToString();

            // Inner Cores Display
            innerCoresDisplay.Refresh();

            // Attribute Setters
            dexteritySetter.Initialize(attributeManager.Dexterity);
            mentalToughnessSetter.Initialize(attributeManager.MentalToughness);
            agilitySetter.Initialize(attributeManager.Agility);
            strengthSetter.Initialize(attributeManager.Strength);

            _initialAgility = attributeManager.Agility;
            _initialDexterity = attributeManager.Dexterity;
            _initialMentalToughness = attributeManager.MentalToughness;
            _initialStrength = attributeManager.Strength;

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => { CancelLeveling(); });
            commitButton.onClick.RemoveAllListeners();
            commitButton.onClick.AddListener(CommitChanges);
        }

        void CancelLeveling()
        {
            // Logic to cancel attribute point changes
            Debug.Log("Canceled attribute point changes.");
            MyUIEvent.Trigger(UIType.LevelingUI, UIActionType.Close);
        }

        void CommitChanges()
        {
            // Logic to commit attribute point changes
            Debug.Log("Committed attribute point changes.");
            MyUIEvent.Trigger(UIType.LevelingUI, UIActionType.Close);
            // MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);
            // waitOverlay.Show("Applying Attribute Augments");

            GatedLevelingEvent.Trigger(GatedInteractionEventType.CompleteInteraction);
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
            Initialize();
            // show
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
