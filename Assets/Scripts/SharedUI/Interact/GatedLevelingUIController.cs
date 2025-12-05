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
        MMEventListener<AttrPendingBuyEvent>, MMEventListener<InnerCoreXPEvent>
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
        [SerializeField] MMFeedbacks commitChangesFeedbacks;


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
            this.MMEventStartListening<InnerCoreXPEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<XPEvent>();
            this.MMEventStopListening<AttrPendingBuyEvent>();
            this.MMEventStopListening<InnerCoreXPEvent>();
        }
        public void OnMMEvent(AttrPendingBuyEvent eventType)
        {
            var attributeManager = AttributesManager.Instance;
            if (eventType.PendingBuyEventType == PendingBuyEventType.IncreasePendingAttribute)
            {
                var xpRequired = attributeManager.GetXpRequiredForLevel(eventType.AttrLevelTarget);
                if (_pendingNewUnusedXP -
                    xpRequired < 0)
                    return;

                _pendingNewUnusedXP = _pendingNewUnusedXP -
                                      xpRequired;

                totalUnusedXPText.text = _pendingNewUnusedXP.ToString();

                switch (eventType.AttributeType)
                {
                    case AttributeType.Dexterity:
                        _pendingNewDexterity = eventType.AttrLevelTarget;
                        dexteritySetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        dexteritySetter.canDecrease = true;
                        break;
                    case AttributeType.MentalToughness:
                        _pendingNewMentalToughness = eventType.AttrLevelTarget;
                        mentalToughnessSetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        mentalToughnessSetter.canDecrease = true;
                        break;
                    case AttributeType.Agility:
                        _pendingNewAgility = eventType.AttrLevelTarget;
                        agilitySetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        agilitySetter.canDecrease = true;
                        break;
                    case AttributeType.Strength:
                        _pendingNewStrength = eventType.AttrLevelTarget;
                        strengthSetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        strengthSetter.canDecrease = true;
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

                _pendingNewUnusedXP += attributeManager.GetXpRequiredForLevel(eventType.AttrLevelTarget - 1);

                totalUnusedXPText.text = _pendingNewUnusedXP.ToString();


                switch (eventType.AttributeType)
                {
                    case AttributeType.Dexterity:
                        _pendingNewDexterity = eventType.AttrLevelTarget;
                        dexteritySetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        dexteritySetter.canDecrease = eventType.AttrLevelTarget >= _initialDexterity;
                        break;
                    case AttributeType.MentalToughness:
                        _pendingNewMentalToughness = eventType.AttrLevelTarget;
                        mentalToughnessSetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        mentalToughnessSetter.canDecrease = eventType.AttrLevelTarget >= _initialMentalToughness;
                        break;
                    case AttributeType.Agility:
                        _pendingNewAgility = eventType.AttrLevelTarget;
                        agilitySetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        agilitySetter.canDecrease = eventType.AttrLevelTarget >= _initialAgility;

                        break;
                    case AttributeType.Strength:
                        _pendingNewStrength = eventType.AttrLevelTarget;
                        strengthSetter.Initialize(eventType.AttrLevelTarget, _pendingNewUnusedXP);
                        strengthSetter.canDecrease = eventType.AttrLevelTarget >= _initialStrength;
                        break;
                }
            }
        }
        public void OnMMEvent(InnerCoreXPEvent eventType)
        {
            if (eventType.EventType == InnerCoreXPEventType.ConvertCoreToXP)
            {
                _currentUnusedXP = _currentUnusedXP +
                                   AttributesManager.Instance.GetXPGainedForCoreGrade(eventType.CoreGrade);

                _pendingNewUnusedXP = _currentUnusedXP;
                RefreshAttrSetters();
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
            _pendingNewUnusedXP = _currentUnusedXP;
            totalUnusedXPText.text = _pendingNewUnusedXP.ToString();

            // Inner Cores Display
            innerCoresDisplay.Refresh();

            RefreshAttrSetters();

            _initialAgility = attributeManager.Agility;
            _initialDexterity = attributeManager.Dexterity;
            _initialMentalToughness = attributeManager.MentalToughness;
            _initialStrength = attributeManager.Strength;

            _pendingNewAgility = _initialAgility;
            _pendingNewDexterity = _initialDexterity;
            _pendingNewMentalToughness = _initialMentalToughness;
            _pendingNewStrength = _initialStrength;

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => { CancelLeveling(); });
            commitButton.onClick.RemoveAllListeners();
            commitButton.onClick.AddListener(CommitChanges);
        }
        void RefreshAttrSetters()
        {
            var attributeManager = AttributesManager.Instance;
            // Attribute Setters
            dexteritySetter.Initialize(attributeManager.Dexterity, _pendingNewUnusedXP);
            mentalToughnessSetter.Initialize(attributeManager.MentalToughness, _pendingNewUnusedXP);
            agilitySetter.Initialize(attributeManager.Agility, _pendingNewUnusedXP);
            strengthSetter.Initialize(attributeManager.Strength, _pendingNewUnusedXP);
        }

        void CancelLeveling()
        {
            // Logic to cancel attribute point changes
            Debug.Log("Canceled attribute point changes.");
            MyUIEvent.Trigger(UIType.LevelingUI, UIActionType.Close);

            _currentUnusedXP = AttributesManager.Instance.CurrentUnusedXP;
            _pendingNewUnusedXP = _currentUnusedXP;
            _initialAgility = AttributesManager.Instance.Agility;
            _pendingNewAgility = _initialAgility;
            _initialDexterity = AttributesManager.Instance.Dexterity;
            _pendingNewDexterity = _initialDexterity;
            _initialMentalToughness = AttributesManager.Instance.MentalToughness;
            _pendingNewMentalToughness = _initialMentalToughness;
            _initialStrength = AttributesManager.Instance.Strength;
            _pendingNewStrength = _initialStrength;
        }

        void CommitChanges()
        {
            // Logic to commit attribute point changes
            Debug.Log("Committed attribute point changes.");
            MyUIEvent.Trigger(UIType.LevelingUI, UIActionType.Close);
            // MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);
            // waitOverlay.Show("Applying Attribute Augments");
            AttributesManager.Instance.ApplyPendingAttributeChanges(
                _pendingNewDexterity,
                _pendingNewMentalToughness,
                _pendingNewAgility,
                _pendingNewStrength);

            AttributesManager.Instance.ApplyPendingUnusedXP(_pendingNewUnusedXP);

            commitChangesFeedbacks?.PlayFeedbacks();

            var newAttrValues = new NewAttributeValues
            {
                dexterity = _pendingNewDexterity,
                mentalToughness = _pendingNewMentalToughness,
                agility = _pendingNewAgility,
                strength = _pendingNewStrength,
                exobiotic = AttributesManager.Instance.Exobiotic
            };

            GatedLevelingEvent.Trigger(GatedInteractionEventType.CompleteInteraction, newAttrValues);
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
