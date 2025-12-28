using System;

namespace PhysicsHandlers.Triggers
{
    [Serializable]
    public enum TriggerType
    {
        OnEnter,
        OnExit,
        Both
    }

    [Serializable]
    public enum TriggerColliderType
    {
        Spontaneous,
        Tutorial,
        Objective,
        Dialogue
    }
}
