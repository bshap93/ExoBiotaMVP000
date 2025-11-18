using System.Collections;
using Events;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using LevelConstruct.Highlighting;
using MoreMountains.Feedbacks;
using Objectives;
using Objectives.ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable.ItemInteractables
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(HighlightEffectController))]
    [DisallowMultipleComponent]
    public class CommsConsole : ActionConsole, IInteractable
    {
        [SerializeField] string defaultNPCId;
        [FormerlySerializedAs("startNodeOverride")] [SerializeField]
        string defaultStartNode;
        [FormerlySerializedAs("associatedObjectiveIds")] [SerializeField]
        string[] completesObjectives;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        [SerializeField] GameObject rotatingLight;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int continuteActionId;

        [Header("Conditional Dialogue Nodes")] [SerializeField]
        DialogueCondition[] dialogueConditions;

        void Start()
        {
            StartCoroutine(InitializeAfterMachineStateManager());
        }

        public override void Interact()
        {
            if (!CanInteract())
            {
                if (currentConsoleState == ActionConsoleState.Broken)
                    AlertEvent.Trigger(
                        AlertReason.BrokenMachine, "The communications console is broken and cannot be used.",
                        "Comms Console");
                else if (currentConsoleState == ActionConsoleState.LacksPower)
                    AlertEvent.Trigger(
                        AlertReason.MachineLacksPower, "The communications console lacks power and cannot be used.",
                        "Comms Console");

                return;
            }

            foreach (var objId in completesObjectives)
                ObjectiveEvent.Trigger(objId, ObjectiveEventType.ObjectiveCompleted);

            var nodeToUse = GetAppropriateStartNode();
            if (nodeToUse.IsNullOrWhitespace())
                FirstPersonDialogueEvent.Trigger(
                    FirstPersonDialogueEventType.StartDialogue, defaultNPCId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, defaultNPCId, nodeToUse);


            if (completesObjectives.Length > 0)
                foreach (var objId in completesObjectives)
                    ObjectiveEvent.Trigger(objId, ObjectiveEventType.ObjectiveCompleted);

            startDialogueFeedback?.PlayFeedbacks();

            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);
            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Show, continuteActionId, additionalInfoText: " to Continue",
                additionalInstruction: "Instruction");
        }

        public override void OnInteractionStart()
        {
        }

        public override void OnInteractionEnd()
        {
        }

        protected override IEnumerator InitializeAfterMachineStateManager()
        {
            yield return base.InitializeAfterMachineStateManager();

            switch (currentConsoleState)
            {
                case ActionConsoleState.Broken:
                case ActionConsoleState.LacksPower:
                case ActionConsoleState.None:
                    SetConsoleToInactiveState();
                    break;
                case ActionConsoleState.PoweredOn:
                    SetConsoleToActiveState();
                    break;
            }
        }

        string GetAppropriateStartNode()
        {
            var objectivesManager = ObjectivesManager.Instance;
            if (objectivesManager == null)
            {
                Debug.LogWarning("[CommsConsole] ObjectivesManager not found, using default node");
                return defaultStartNode;
            }

            // Check each condition in order
            if (dialogueConditions != null)
                foreach (var condition in dialogueConditions)
                    if (condition.CheckCondition(objectivesManager))
                        return condition.startNode;

            // Fallback to original override
            return defaultStartNode;
        }

        protected override string GetActionText(bool recognizableOnSight)
        {
            return $"{currentConsoleState}";
        }
        public override void SetConsoleToInactiveState()
        {
            if (rotatingLight != null)
                rotatingLight.SetActive(false);

            currentConsoleState = ActionConsoleState.LacksPower;
        }
        public override void SetConsoleToActiveState()
        {
            if (rotatingLight != null)
                rotatingLight.SetActive(true);

            currentConsoleState = ActionConsoleState.PoweredOn;
        }
    }
}
