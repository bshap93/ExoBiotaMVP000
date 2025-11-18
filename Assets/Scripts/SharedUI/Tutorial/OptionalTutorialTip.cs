using Helpers.Events;
using Helpers.Events.Tutorial;
using MoreMountains.Tools;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Tutorial
{
    public class OptionalTutorialTip : MonoBehaviour, MMEventListener<MainTutorialBitEvent>
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text tutorialName;
        [SerializeField] Image buttonKeyImage;
        [SerializeField] float holdDuration = 1f;
        [SerializeField] int universalInteractId = 99; // IGUIToggleId from DefaultInput

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        float _currentHoldTime;
        string _currentTutorialId;
        bool _isHolding;
        Player _player;
        bool _tutorialShown;


        void Start()
        {
            Hide();

            // Get the Rewired player (usually player 0)
            _player = ReInput.players.GetPlayer(0);

            // Initialize button image fill (starts full)
            if (buttonKeyImage != null) buttonKeyImage.fillAmount = 1f;
        }


        void Update()
        {
            if (_player == null) return;

            // Check if the button is being held
            var isButtonHeld = _player.GetButton(universalInteractId);

            if (isButtonHeld && !_tutorialShown)
            {
                // Button is being held
                if (!_isHolding)
                {
                    _isHolding = true;
                    _currentHoldTime = 0f;
                }

                _currentHoldTime += Time.deltaTime;

                // Update the fill amount (1 to 0 as time progresses)
                var fillProgress = 1f - _currentHoldTime / holdDuration;
                fillProgress = Mathf.Clamp01(fillProgress);

                if (buttonKeyImage != null) buttonKeyImage.fillAmount = fillProgress;

                // Check if hold duration reached
                if (_currentHoldTime >= holdDuration)
                {
                    ShowTutorial();
                    _tutorialShown = true;
                }
            }
            else
            {
                // Button released or tutorial already shown
                if (_isHolding && !_tutorialShown)
                    // Reset if released before completing hold
                    ResetHold();

                _isHolding = false;
            }
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MainTutorialBitEvent eventType)
        {
            if (eventType.BitEventType == MainTutorialBitEventType.ShowOptionalTutorialBit)
            {
                tutorialName.text = eventType.TutorialName;
                _currentTutorialId = eventType.MainTutID;
                Show();
            }
            else if (eventType.BitEventType == MainTutorialBitEventType.HideOptionalTutorialBit)
            {
                Hide();
                _tutorialShown = false;
            }
        }

        void ResetHold()
        {
            _currentHoldTime = 0f;
            if (buttonKeyImage != null) buttonKeyImage.fillAmount = 1f;
        }

        void ShowTutorial()
        {
            // Trigger your tutorial event here
            if (!string.IsNullOrEmpty(_currentTutorialId))
            {
                MyUIEvent.Trigger(UIType.TutorialWindow, UIActionType.Open);
                MainTutorialBitEvent.Trigger(
                    _currentTutorialId, MainTutorialBitEventType.ShowMainTutBit
                );
            }
        }

        void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            ResetHold();
        }

        void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            ResetHold();
        }
    }
}
