using Helpers.Events;
using Inventory;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Helpers.StaticHelpers
{
    public static class InventoryHelperCommands
    {
        public static void RemovePlayerItem(string itemId)
        {
            var amount = 1;
            var removed = 0;

            var inv = GlobalInventoryManager.Instance;
            if (inv == null)
            {
                Debug.LogWarning("GlobalInventoryManager not found, cannot remove item.");
                return;
            }

            var playerInventoryContent = inv.playerInventory.Content;

            for (var i = 0; i < playerInventoryContent.Length; i++)
            {
                if (removed >= amount) break;
                var item = playerInventoryContent[i];
                if (item == null) continue;
                if (item.ItemID != itemId) continue;
                // inv.playerInventory.RemoveItem(i, 1);
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null,
                    "PlayerMainInventory", item, 1, i, inv.playerId);

                removed++;
            }

            AlertEvent.Trigger(AlertReason.ItemsRemoved, $"Removed {removed} x {itemId}", itemId);
        }
    }
}
