using System;
using Helpers.Events.Progression;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI.Progression
{
    [Serializable]
    public enum AttributeType
    {
        Dexterity,
        MentalToughness,
        Agility,
        Strength,
        Exobiotic
    }

    public class AttributePointSetter : MonoBehaviour
    {
        [SerializeField] AttributeType attributeType;
        [SerializeField] TMP_Text attributePointText;
        [FormerlySerializedAs("xpNeededForNextIncrease")] [FormerlySerializedAs("attributXPText")] [SerializeField]
        TMP_Text xpNeededForNextIncreaseText;
        [SerializeField] Button increaseButton;
        [SerializeField] Button decreaseButton;

        int _currentPoints;
        int _currentXP;
        int _xpNeededForNextIncrease;

        public int PendingChanges { get; private set; }
        public AttributeType AttributeType => attributeType;

        public void Initialize(int currentPoints)
        {
            var attributeManager = AttributesManager.Instance;
            _currentPoints = currentPoints;
            PendingChanges = 0;

            _xpNeededForNextIncrease = attributeManager.GetXpRequiredForLevel(_currentPoints + 1);
            xpNeededForNextIncreaseText.text =
                _xpNeededForNextIncrease.ToString();

            UpdateDisplay();

            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            increaseButton.onClick.AddListener(() => OnIncreaseButtonClicked());
            decreaseButton.onClick.AddListener(() => OnDecreaseButtonClicked());

            UpdateButtonStates();
        }

        void UpdateButtonStates()
        {
            decreaseButton.interactable = PendingChanges > 0;
        }

        void UpdateDisplay()
        {
            attributePointText.text = _currentPoints.ToString();
            // TODO: Calculate XP needed for next increase
            // xpNeededForNextIncrease.text = _currentXP.ToString();
        }

        void OnIncreaseButtonClicked()
        {
            AttrPendingBuyEvent.Trigger(
                attributeType, PendingBuyEventType.IncreasePendingAttribute, _currentPoints + 1);
        }

        void OnDecreaseButtonClicked()
        {
            // Logic to decrease attribute points
            AttrPendingBuyEvent.Trigger(
                attributeType, PendingBuyEventType.DecreasePendingAttribute,
                _currentPoints - 1);
        }
    }
}
