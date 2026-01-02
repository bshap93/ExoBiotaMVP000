using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    public struct HotbarEvent
    {
        static HotbarEvent _e;

        public enum HotbarEventType
        {
            AddToHotbar,
            RemoveFromHotbar,
        }
        public HotbarEventType EventType;

        public static void Trigger(HotbarEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
