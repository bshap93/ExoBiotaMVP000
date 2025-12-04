using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum InnerCoreXPEventType
    {
        ConvertCoreToXP
    }

    public struct InnerCoreXPEvent
    {
        static InnerCoreXPEvent _e;
        public HarvestableInnerObject.InnerObjectValueGrade CoreGrade;
        public InnerCoreXPEventType EventType;
        public static void Trigger(InnerCoreXPEventType eventType,
            HarvestableInnerObject.InnerObjectValueGrade coreGrade)
        {
            _e.EventType = eventType;
            _e.CoreGrade = coreGrade;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
