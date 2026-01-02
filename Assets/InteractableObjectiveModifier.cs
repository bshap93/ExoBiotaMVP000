using Events;
using FirstPersonPlayer.Interface;
using Objectives.ScriptableObjects;
using UnityEngine;

public class InteractableObjectiveModifier : MonoBehaviour, IInteractable
{
    [SerializeField] ObjectiveObject objective;
    [SerializeField]  ObjectiveActionType objectiveAction;
    public enum ObjectiveActionType
    {
        Add,
        Activate,
        Complete,
        Deactivate,
        Delete
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Interact()
    {
        switch (objectiveAction)
        {
            case ObjectiveActionType.Add:
                ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveAdded);
                break;
            case ObjectiveActionType.Activate:
                ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveActivated);
                break;
            case ObjectiveActionType.Complete:
                ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                break;
            case ObjectiveActionType.Deactivate:
                ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeactivated) ;
                break;
            case ObjectiveActionType.Delete:
                ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeleted);
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
    public void OnInteractionStart()
    {
        throw new System.NotImplementedException();
    }
    public void OnInteractionEnd(string param)
    {
        throw new System.NotImplementedException();
    }
    public bool CanInteract()
    {
        throw new System.NotImplementedException();
    }
    public bool IsInteractable()
    {
        throw new System.NotImplementedException();
    }
    public void OnFocus()
    {
        throw new System.NotImplementedException();
    }
    public void OnUnfocus()
    {
        throw new System.NotImplementedException();
    }
    public float GetInteractionDistance()
    {
        throw new System.NotImplementedException();
    }
}
