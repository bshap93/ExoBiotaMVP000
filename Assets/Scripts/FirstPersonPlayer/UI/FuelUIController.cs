using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using ProgressBar = Michsky.MUIP.ProgressBar;

namespace FirstPersonPlayer.UI
{
    public class FuelUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [SerializeField] private ProgressBar fuelRemainingRadial;
        [SerializeField] private TMP_Text fuelPriceText;
        [SerializeField] private TMP_Text fuelRemainingText;
        [SerializeField] private TMP_Text amountToBuyText;
        [SerializeField] private MMFeedbacks buyFuelFeedbacks;
        [SerializeField] private TMP_Text costOfFuelToBuyText;
        private CanvasGroup _canvasGroup;

        private bool _isPaused;
        private Button buyFuelButton;

        public FuelUIController(bool isPaused)
        {
            _isPaused = isPaused;
        }

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            CloseFuelUI();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.FuelConsole)
                if (eventType.uiActionType == UIActionType.Open)
                    OpenFuelUI();
                else if (eventType.uiActionType == UIActionType.Close) CloseFuelUI();
        }

        public void CloseFuelUI()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _isPaused = false;
            // Time.timeScale = 1;

            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }

        public void OpenFuelUI()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _isPaused = true;
            // Time.timeScale = 0;

            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }

        public static void TriggerUpdateFuelUI()
        {
            MyUIEvent.Trigger(UIType.FuelConsole, UIActionType.Update);
        }


        public void UpdateFuelUI(float fuelRemaining, float maxFuelAmount, int fuelPrice, float playerCredits,
            float fuelToBuy, int costOfFuelToBuy)
        {
            fuelRemainingRadial.isOn = false;
            // fuelRemainingRadial.currentPercent = fuelRemaining / maxFuelAmount * 100;
            fuelRemainingRadial.ChangeValue(fuelRemaining / maxFuelAmount * 100);

            Debug.LogWarning("Fuel Remaining: " + fuelRemaining);
            fuelRemainingText.text = $"{fuelRemaining} / {maxFuelAmount} ml";
            fuelPriceText.text = $"{fuelPrice} Credits";
            amountToBuyText.text = $"{fuelToBuy} ml";
            costOfFuelToBuyText.text = $"{costOfFuelToBuy} Credits";
        }
    }
}