using System;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Triggering;
using Manager;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace PhysicsHandlers.Triggers
{
    [DisallowMultipleComponent]
    public class DialogueInitTrigger : MonoBehaviour, IRequiresUniqueID
    {
        public TriggerType triggerType = TriggerType.OnEnter;
        [ValueDropdown("GetStartNodeOptions")] public string startNode;


        [ValueDropdown("GetNpcIdOptions")] [OnValueChanged("OnNpcIdChanged")]
        public string npcId;
        public string uniqueID;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        public bool startDisabled;

        bool _isDisabled;
        bool _isPlayerInTrigger;

        Player _player;
        TriggerColliderManager _triggerColliderManager;

        void Start()
        {
            _player = ReInput.players.GetPlayer(0);

            _triggerColliderManager = TriggerColliderManager.Instance;

            if (_triggerColliderManager == null)
                Debug.LogWarning(
                    "[DialogueInitTrigger] No TriggerColliderManager found in scene. Ensure one exists.");

            _isDisabled = startDisabled;
        }

        void OnTriggerEnter(Collider other)
        {
            if (TutorialManager.Instance == null) return;
            if (_isDisabled) return;
            if (_triggerColliderManager)
                if (!_triggerColliderManager.IsDialogueColliderTriggerable(uniqueID))
                    return;

            if (triggerType == TriggerType.OnEnter)
            {
                if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                    return;

                FirstPersonDialogueEvent.Trigger(
                    FirstPersonDialogueEventType.StartDialogue, npcId, startNode);

                TriggerColliderEvent.Trigger(
                    uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Dialogue);

                startDialogueFeedback?.PlayFeedbacks();
                MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
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

        void OnNpcIdChanged()
        {
            // Clear startNode if it's not valid for the new NPC
            var validNodes = GetStartNodeOptions();
            if (validNodes.Length > 0)
            {
                var isValid = false;
                foreach (var node in validNodes)
                    if (node == startNode)
                    {
                        isValid = true;
                        break;
                    }

                if (!isValid)
                    startNode = string.Empty;
            }
        }

        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        // Instance method that uses the current npcId
        string[] GetStartNodeOptions()
        {
            if (string.IsNullOrEmpty(npcId))
                return new[] { "Select an NPC first" };

            var nodes = DialogueManager.GetNpcStartNodesByNpcId(npcId);
            return nodes != null && nodes.Length > 0
                ? nodes
                : new[] { "No start nodes found" };
        }
    }
}
