using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    /// <summary>
    ///     Hotbar event structure for communication between hotbar systems
    /// </summary>
    public struct HotbarEvent
    {
        public enum HotbarEventType
        {
            AddToHotbar,
            RemoveFromHotbar,
            ConsumableHotbarChanged,
            ToolHotbarChanged,
            SelectConsumableSlot,
            SelectToolSlot,
            RefreshAllHotbars
        }

        public HotbarEventType EventType;
        public string ItemID;
        public int IndexInInventory;
        public int SlotIndex;

        /// <summary>
        ///     Initializes a new hotbar event
        /// </summary>
        /// <param name="eventType">The type of hotbar event</param>
        /// <param name="itemID">The item ID (can be null)</param>
        /// <param name="indexOrSlot">Either the inventory index or the hotbar slot index depending on context</param>
        public HotbarEvent(HotbarEventType eventType, string itemID, int indexOrSlot)
        {
            EventType = eventType;
            ItemID = itemID;
            IndexInInventory = indexOrSlot;
            SlotIndex = indexOrSlot;
        }

        static HotbarEvent e;

        public static void Trigger(HotbarEventType eventType, string itemID, int indexOrSlot)
        {
            e.EventType = eventType;
            e.ItemID = itemID;
            e.IndexInInventory = indexOrSlot;
            e.SlotIndex = indexOrSlot;
            MMEventManager.TriggerEvent(e);
        }
    }
}
