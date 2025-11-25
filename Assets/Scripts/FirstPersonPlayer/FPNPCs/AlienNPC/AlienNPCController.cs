using Animancer;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using Lightbug.Utilities;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.FPNPCs
{
    public enum AlienNPCState
    {
        Hailable,
        InDialogue,
        Unavailable
    }

    public class AlienNPCController : MonoBehaviour, IInteractable
    {
        [SerializeField] AnimancerComponent animancerComponent;
        [FormerlySerializedAs("NPCId")] [ValueDropdown("GetNpcIdOptions")]
        public
            string npcId;

        [SerializeField] string defaultStartNode;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        [SerializeField] AlienNPCState initialState = AlienNPCState.Hailable;
        [SerializeField] bool isInteractable = true;
        protected AlienNPCState CurrentState;
        void Start()
        {
            CurrentState = initialState;
        }
        public void Interact()
        {
            if (!CanInteract()) return;


            var nodeToUse = GetAppropriateDialogueNode();

            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            startDialogueFeedback?.PlayFeedbacks();

            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            if (CurrentState == AlienNPCState.Unavailable) return false;
            if (CurrentState == AlienNPCState.InDialogue) return false;
            if (!isInteractable) return false;
            return true;
        }
        public bool IsInteractable()
        {
            return isInteractable;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }

        protected string GetAppropriateDialogueNode()
        {
            // For now, just return the default start node.
            return defaultStartNode;
        }
        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }
    }
}
