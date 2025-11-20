using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum SpontaneousTriggerEventType
    {
        Triggered,
        Silenced
    }

    public struct SpontaneousTriggerEvent
    {
        static SpontaneousTriggerEvent _e;

        public string UniqueID;
        public SpontaneousTriggerEventType EventType;

        public static void Trigger(string uniqueID, SpontaneousTriggerEventType eventType)
        {
            _e.UniqueID = uniqueID;
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
