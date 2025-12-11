using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Helpers.Events;
using Inventory;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Helpers.StaticHelpers
{
    public static class InventoryHelperCommands
    {
        public static void RemoveDirigibleItem(string itemId)
        {
            var amount = 1;
            var removed = 0;

            var inv = GlobalInventoryManager.Instance;
            if (inv == null)
            {
                Debug.LogWarning("GlobalInventoryManager not found, cannot remove item.");
                return;
            }

            var dirigibleInventoryContent = inv.dirigibleInventory.Content;

            for (var i = 0; i < dirigibleInventoryContent.Length; i++)
            {
                if (removed >= amount) break;
                var item = dirigibleInventoryContent[i];
                if (item == null) continue;
                if (item.ItemID != itemId) continue;
                // inv.playerInventory.RemoveItem(i, 1);
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null,
                    "DirigibleInventory", item, 1, i, inv.playerId);

                removed++;
            }

            AlertEvent.Trigger(AlertReason.ItemsRemoved, $"Removed {removed} x {itemId}", itemId);
        }
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

        public static void RemoveInnerCore(HarvestableInnerObject.InnerObjectValueGrade grade)
        {
            var amount = 1;
            var removed = 0;

            var inv = GlobalInventoryManager.Instance;
            if (inv == null) return;

            var innerCoresInventoryContent = inv.innerCoresInventory.Content;

            for (var i = 0; i < innerCoresInventoryContent.Length; i++)
            {
                if (removed >= amount) break;
                var item = innerCoresInventoryContent[i];
                if (item == null) continue;

                var innerCore = item as HarvestableInnerObject;
                if (innerCore == null) continue;
                if (innerCore.innerObjectValueGrade != grade) continue;

                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Destroy, null,
                    GlobalInventoryManager.InnerCoresInventoryName, item, 1, i, inv.playerId);

                removed++;
            }

            // AlertEvent.Trigger(
            // AlertReason.ItemsRemoved, $"Removed {removed} x {grade} Inner Core", $"InnerCore_{grade}");
        }
    }
}
