using System;
using System.Collections.Generic;
using Events;
using Helpers.Events;
using Helpers.Events.Tutorial;
using Helpers.ScriptableObjects.Tutorial;
using Manager;
using Objectives.ScriptableObjects;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace SharedUI.Tutorial
{
    public class ColliderTutorialTrigger : MonoBehaviour, IRequiresUniqueID
    {
        public enum TutorialType
        {
            MainTutorialBit,
            ControlPromptSequence,
            None
        }
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int ActionId;

        public bool OfferOptionalTutorialBit;

        [SerializeField] ObjectiveObject objectiveToStartOnBoop;


        [FormerlySerializedAs("tutorialBitID")] [SerializeField]
        MainTutBitWindowArgs tutorialBit;
        [SerializeField] TriggerType triggerType = TriggerType.OnEnter;
        [SerializeField] TutorialType tutorialType;

        public string uniqueID;

        public string prePromptTextOverride;
        public string postPromptTextOverride;

        public bool canBeBooped;
        public bool objectiveToBecomeActive = true;

        bool _isActionButtonPressed;
        bool _isPlayerInTrigger;

        Player _player;

        void Start()
        {
            _player = ReInput.players.GetPlayer(0);
        }

        void Update()
        {
            if (_player == null) return;

            if (canBeBooped && _isPlayerInTrigger)
            {
                var isButtonPressed = _player.GetButton(ActionId);
                if (isButtonPressed)
                {
                    ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, ActionId);
                    canBeBooped = false;
                    if (tutorialBit != null)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID,
                            MainTutorialBitEventType.ClearTutorialColliderTrigger, tutorialBit.tutBitName);

                    if (objectiveToStartOnBoop != null)
                    {
                        ObjectiveEvent.Trigger(
                            objectiveToStartOnBoop.objectiveId,
                            ObjectiveEventType.ObjectiveAdded);

                        if (objectiveToBecomeActive)
                            ObjectiveEvent.Trigger(
                                objectiveToStartOnBoop.objectiveId,
                                ObjectiveEventType.ObjectiveActivated);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (TutorialManager.Instance == null) return;
            if (!TutorialManager.Instance.AreTutorialsEnabled()) return;
            if (tutorialType == TutorialType.MainTutorialBit)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = true;
                    if (triggerType != TriggerType.OnEnter) return;
                    if (string.IsNullOrEmpty(tutorialBit.mainTutID))
                    {
                        Debug.LogWarning("ColliderTutorialTrigger: No tutorialBitID assigned.", this);
                        return;
                    }

                    if (TutorialManager.Instance == null) return;

                    if (TutorialManager.Instance.IsTutorialBitComplete(tutorialBit.mainTutID)) return;

                    MainTutorialBitEvent.Trigger(tutorialBit.mainTutID, MainTutorialBitEventType.ShowMainTutBit);
                    MyUIEvent.Trigger(UIType.TutorialWindow, UIActionType.Open);
                }
            }
            else if (tutorialType == TutorialType.ControlPromptSequence)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = true;
                    if (!string.IsNullOrEmpty(prePromptTextOverride) && !string.IsNullOrEmpty(postPromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, prePromptTextOverride, null, postPromptTextOverride);
                    else if (!string.IsNullOrEmpty(prePromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, prePromptTextOverride);
                    else if (!string.IsNullOrEmpty(postPromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, null, null, postPromptTextOverride);
                    else
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId);

                    if (OfferOptionalTutorialBit)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID, MainTutorialBitEventType.ShowOptionalTutorialBit,
                            tutorialBit.tutBitName);
                }
            }
            else
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player")) _isPlayerInTrigger = true;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (tutorialType == TutorialType.ControlPromptSequence)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = false;
                    ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, ActionId);
                    if (OfferOptionalTutorialBit)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID, MainTutorialBitEventType.HideOptionalTutorialBit);
                }
            }
            else
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player")) _isPlayerInTrigger = false;
            }
        }

        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif

        enum TriggerType
        {
            OnEnter,
            OnExit,
            Both
        }
    }
}
