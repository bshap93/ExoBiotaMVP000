using System.Collections.Generic;
using Dirigible.Input;
using Events;
using FirstPersonPlayer;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Lightbug.CharacterControllerPro.Core;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable
{
    public class LadderInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] MMFeedbacks climbLadderFeedbacks;
        [SerializeField] string actionMessage = "Climb the Ladder?";
        [SerializeField] string actionTitle = "Ladder Climb";

#if UNITY_EDITOR
        [FormerlySerializedAs("ActionId")] [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] float triggerCooldown = 6f;
        CharacterActor _characterActor;
        bool _initialized;
        float _lastTriggerTime;
        GameObject _player01;
        TeleportPlayer _teleportPlayer;

        void OnTriggerEnter(Collider other)
        {
            if (Time.time - _lastTriggerTime < triggerCooldown)
                return;

            _lastTriggerTime = Time.time;
            InitiateClimb();
        }


        public void Interact()
        {
            InitiateClimb();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }


        public bool CanInteract()
        {
            return true;
        }


        public bool IsInteractable()
        {
            return true;
        }

        public void OnFocus()
        {
            ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);
        }

        public void OnUnfocus()
        {
        }
        public void OnInteractionEnd()
        {
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif
        void InitiateClimb()
        {
            AlertEvent.Trigger(
                AlertReason.GateInteractable,
                actionMessage,
                actionTitle,
                AlertType.ChoiceModal,
                0f,
                onConfirm: () =>
                {
                    Initialize();
                    if (_teleportPlayer == null)
                    {
                        Debug.LogError("LadderInteractable: No TeleportPlayer component found.");
                        return;
                    }

                    climbLadderFeedbacks?.PlayFeedbacks();

                    _teleportPlayer.Teleport(_characterActor);
                    MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
                },
                onCancel: () => { }
            );
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Initialize()
        {
            _characterActor = FindFirstObjectByType<CharacterActor>();
            _teleportPlayer = GetComponent<TeleportPlayer>();
            if (_teleportPlayer == null)
                Debug.LogError("LadderInteractable: No TeleportPlayer component found on the ladder.");

            if (_player01 == null) return;

            _player01 = _characterActor.gameObject;
        }
    }
}
