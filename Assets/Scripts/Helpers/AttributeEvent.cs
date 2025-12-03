using System;
using MoreMountains.Tools;

namespace Helpers
{
    [Serializable]
    public enum AttributeType
    {
        Strength,
        Agility,
        Dexterity,
        MentalToughness,
        Exobiotic
    }

    [Serializable]
    public enum AttributeEventType
    {
        Increase
    }

    public struct AttributeEvent
    {
        static AttributeEvent _e;


        public AttributeType AttributeType;
        public AttributeEventType EventType;
        public int Value;

        public static void Trigger(AttributeType attributeType, AttributeEventType eventType, int value)
        {
            _e.AttributeType = attributeType;
            _e.EventType = eventType;
            _e.Value = value;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
