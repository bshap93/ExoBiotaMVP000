using System.Collections.Generic;
using Helpers.Events;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using SharedUI.InputsD;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.Global
{
    public class PauseManager : MonoBehaviour, MMEventListener<PauseEvent>, MMEventListener<InGameTimeActionEvent>
    {
        public static PauseManager Instance;


        [SerializeField] MMFeedbacks pauseFeedback;

        [FormerlySerializedAs("unPauseFeedback")] [SerializeField]
        MMFeedbacks quitUIFeedbacks;

        [FormerlySerializedAs("_defaultInput")] [SerializeField]
        DefaultInput defaultInput;

        [SerializeField] CanvasGroup pauseOverlayCanvasGroup;

        readonly List<AudioSource> _audioSources = new();


        bool IguiOpen { get; set; }
        bool Paused { get; set; }

        void Awake()
        {
            Instance = this;
            Paused = false;
            pauseOverlayCanvasGroup.alpha = 0;
        }

        void Update()
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialOpen()) return;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;
            if (defaultInput.isPausePressed && !PlayerUIManager.Instance.uiIsOpen)
            {
                Paused = !Paused;
                Time.timeScale = Paused ? 0 : 1;
                pauseOverlayCanvasGroup.alpha = Paused ? 1 : 0;
                pauseOverlayCanvasGroup.blocksRaycasts = Paused;
                pauseOverlayCanvasGroup.interactable = Paused;
                Cursor.visible = Paused;
                Cursor.lockState = CursorLockMode.None;
                if (Paused)
                    PauseAudio();
                else
                    UnPauseAudio();


                // SceneEvent.Trigger(SceneEventType.TogglePauseScene);
                pauseFeedback?.PlayFeedbacks();
            }

            if (defaultInput.isIGUITogglePressed)
            {
                IguiOpen = !IguiOpen;
                Time.timeScale = IguiOpen ? 0 : 1;
                // Cursor.lockState = _iguiOpen ? CursorLockMode.None : CursorLockMode.Locked;
                // Cursor.visible = _iguiOpen;
                if (IguiOpen)
                    PauseAudio();
                else
                    UnPauseAudio();
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<PauseEvent>();
            this.MMEventStartListening<InGameTimeActionEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<PauseEvent>();
            this.MMEventStopListening<InGameTimeActionEvent>();
        }
        public void OnMMEvent(InGameTimeActionEvent eventType)
        {
            if (eventType.ActionTypeIG == InGameTimeActionEvent.ActionType.Pause)
            {
                Time.timeScale = 0;
                PauseAudio();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (eventType.ActionTypeIG == InGameTimeActionEvent.ActionType.Resume)
            {
                Time.timeScale = 1;
                UnPauseAudio();
                if (eventType.CurrentGameMode == GameMode.FirstPerson ||
                    eventType.CurrentGameMode == GameMode.DirigibleFlight)
                {
                    Cursor.visible = false;

                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        public void OnMMEvent(PauseEvent eventType)
        {
            if (!Paused && !IguiOpen)
            {
                if (eventType.EventType == PauseEventType.TogglePause)
                {
                    TogglePause();
                }
                else if (eventType.EventType == PauseEventType.PauseOn)
                {
                    Paused = true;
                    Time.timeScale = 0;
                    PauseAudio();
                    // SceneEvent.Trigger(SceneEventType.TogglePauseScene);
                    InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Pause);
                    pauseFeedback?.PlayFeedbacks();
                }
                else if (eventType.EventType == PauseEventType.PauseOff)
                {
                    Paused = false;
                    Time.timeScale = 1;
                    UnPauseAudio();
                    // SceneEvent.Trigger(SceneEventType.TogglePauseScene);
                    InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Resume);
                    quitUIFeedbacks?.PlayFeedbacks();
                }
            }
        }

        public void TogglePause()
        {
            Paused = !Paused;
            Time.timeScale = Paused ? 0 : 1;
            InGameTimeActionEvent.Trigger(
                Paused ? InGameTimeActionEvent.ActionType.Pause : InGameTimeActionEvent.ActionType.Resume);

            pauseOverlayCanvasGroup.alpha = Paused ? 1 : 0;
            if (Paused)
                PauseAudio();
            else
                UnPauseAudio();


            // SceneEvent.Trigger(SceneEventType.TogglePauseScene);
            pauseFeedback?.PlayFeedbacks();
        }

        void UnPauseAudio()
        {
            // foreach (var audioSource in _audioSources)
            //     if (audioSource != null && audioSource != uiButtonAudioSource)
            //         audioSource.UnPause();
            AudioEvent.Trigger(AudioEventType.UnPauseAudio);
        }

        void PauseAudio()
        {
            // _audioSources = new List<AudioSource>(FindObjectsByType<AudioSource>(FindObjectsSortMode.None));
            // foreach (var audioSource in _audioSources)
            //     if (audioSource != null && audioSource != uiButtonAudioSource)
            //         audioSource.Pause();
            AudioEvent.Trigger(AudioEventType.PauseAudio);
        }

        public bool IsPaused()
        {
            return Paused;
        }
    }
}
