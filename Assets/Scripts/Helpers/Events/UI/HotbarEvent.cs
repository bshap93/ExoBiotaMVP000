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
        public string ItemID;
        public int IndexInInventory;

        public static void Trigger(HotbarEventType eventType, string itemID, int indexInInventory)
        {
            _e.EventType = eventType;
            _e.ItemID = itemID;
            _e.IndexInInventory = indexInInventory;
            MMEventManager.TriggerEvent(_e);        }
    }
}
