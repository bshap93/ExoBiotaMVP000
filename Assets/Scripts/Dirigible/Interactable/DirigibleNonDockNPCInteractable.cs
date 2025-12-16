using Dirigible.Interface;
using Events;
using Lightbug.Utilities;
using Objectives;
using Objectives.ScriptableObjects;
using Overview.NPC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible.Interactable
{
    public class DirigibleNonDockNPCInteractable : MonoBehaviour, IDirigibleInteractable
    {
        [Header("Conditional Dialogue Nodes")] public
            DialogueCondition[] dialogueConditions;
        [SerializeField] NpcDefinition npcDefinition;
        public string defaultStartNode;
        [FormerlySerializedAs("LocationId")] [SerializeField]
        protected string locationId;
        [FormerlySerializedAs("CameraAnchorTransform")]
        public Transform cameraAnchorTransform;
        readonly bool _isInteractable = true;


        public void Interact()
        {
            var nodeToUse = GetAppropriateStartNode();
            if (nodeToUse.IsNullOrWhiteSpace())
                OverviewLocationEvent.Trigger(
                    LocationType.NpcResidence, LocationActionType.Approach, locationId,
                    cameraAnchorTransform, defaultStartNode);
            else
                OverviewLocationEvent.Trigger(
                    LocationType.NpcResidence, LocationActionType.Approach, locationId,
                    cameraAnchorTransform, nodeToUse);
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd()
        {
        }
        public bool CanInteract()
        {
            return IsInteractable();
        }
        public bool IsInteractable()
        {
            return _isInteractable;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public void CompleteObjectiveOnInteract()
        {
        }

        protected string GetAppropriateStartNode()
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
    }
}
