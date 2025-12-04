using System;
using Helpers.Events;
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
        [FormerlySerializedAs("attributXPText")] [SerializeField]
        TMP_Text xpNeededForNextIncrease;
        [SerializeField] Button increaseButton;
        [SerializeField] Button decreaseButton;

        int _currentPoints;
        int _currentXP;

        public int PendingChanges { get; private set; }
        public AttributeType AttributeType => attributeType;

        public void Initialize(int currentPoints)
        {
            _currentPoints = currentPoints;
            PendingChanges = 0;

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

        public void CommitChanges()
        {
            if (PendingChanges <= 0) return;
            var attributeManager = AttributesManager.Instance;

            switch (attributeType)
            {
                case AttributeType.Strength:
                    attributeManager.Strength += PendingChanges;
                    break;
                case AttributeType.Agility:
                    attributeManager.Agility += PendingChanges;
                    break;
                case AttributeType.Dexterity:
                    attributeManager.Dexterity += PendingChanges;
                    break;
                case AttributeType.MentalToughness:
                    attributeManager.MentalToughness += PendingChanges;
                    break;
                case AttributeType.Exobiotic:
                    attributeManager.Exobiotic += PendingChanges;
                    break;
            }

            attributeManager.ConditionalSave();

            AttributeLevelUpEvent.Trigger(attributeType, _currentPoints + PendingChanges);

            _currentPoints += PendingChanges;
            PendingChanges = 0;

            UpdateDisplay();
            UpdateButtonStates();
        }

        void OnIncreaseButtonClicked()
        {
            // Logic to increase attribute points
            Debug.Log($"Increased {attributeType} points.");
        }

        void OnDecreaseButtonClicked()
        {
            // Logic to decrease attribute points
            Debug.Log($"Decreased {attributeType} points.");
        }
    }
}
