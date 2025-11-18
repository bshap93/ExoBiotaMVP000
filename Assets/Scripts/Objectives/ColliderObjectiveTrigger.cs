using Events;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace Objectives
{
    public class ColliderObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] ObjectiveObject objective;

        [SerializeField] TriggerType triggerType = TriggerType.OnEnter;
        [SerializeField] ObjectiveAction[] actions;

        void OnTriggerEnter(Collider other)
        {
            if (triggerType != TriggerType.OnEnter) return;
            if (objective == null)
            {
                Debug.LogWarning("ColliderObjectiveTrigger: No objective assigned.", this);
                return;
            }

            var objectiveId = objective.objectiveId;

            foreach (var action in actions)
                switch (action)
                {
                    case ObjectiveAction.Add:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveAdded);
                        break;
                    case ObjectiveAction.Activate:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveActivated);
                        break;
                    case ObjectiveAction.Complete:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveCompleted);
                        break;
                    case ObjectiveAction.MakeInactive:
                        ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveDeactivated);
                        break;
                }
        }

        void OnTriggerExit(Collider other)
        {
            if (triggerType != TriggerType.OnExit) return;
        }

        enum TriggerType
        {
            OnEnter,
            OnExit
        }

        enum ObjectiveAction
        {
            Add,
            Activate,
            Complete,
            MakeInactive
        }
    }
}
