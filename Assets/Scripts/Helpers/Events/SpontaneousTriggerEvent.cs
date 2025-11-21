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
        public int IntParameter;
        public string StringParameter;

        public static void Trigger(string uniqueID, SpontaneousTriggerEventType eventType, int intParameter = -1,
            string stringParameter = null)
        {
            _e.UniqueID = uniqueID;
            _e.EventType = eventType;
            _e.IntParameter = intParameter;
            _e.StringParameter = stringParameter;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
