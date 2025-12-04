using Helpers;
using Helpers.Events.Progression;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributePointSetter : MonoBehaviour
{
    [SerializeField] AttributeType attributeType;
    [SerializeField] TMP_Text attributePointText;
    [SerializeField] TMP_Text attributXPText;
    [SerializeField] TMP_Text pendingPointsText;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;

    int _currentPoints;
    int _currentXP;

    public int PendingChanges { get; private set; }
    public AttributeType AttributeType => attributeType;

    public void Initialize(int currentPoints, int currentXP)
    {
        _currentPoints = currentPoints;
        _currentXP = currentXP;
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
        attributXPText.text = _currentXP.ToString();

        if (pendingPointsText != null)
        {
            if (PendingChanges > 0)
            {
                pendingPointsText.text = $"+{PendingChanges}";
                pendingPointsText.gameObject.SetActive(true);
            }

            else
            {
                pendingPointsText.gameObject.SetActive(false);
            }
        }
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
