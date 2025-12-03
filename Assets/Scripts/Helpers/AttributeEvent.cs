using System;
using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
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
        public HarvestableInnerObject.InnerObjectValueGrade Grade;

        public static void Trigger(AttributeType attributeType, AttributeEventType eventType,
            HarvestableInnerObject.InnerObjectValueGrade grade)
        {
            _e.AttributeType = attributeType;
            _e.EventType = eventType;
            _e.Grade = grade;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
