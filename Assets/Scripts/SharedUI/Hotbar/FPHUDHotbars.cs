using Helpers.Events.UI;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class FPHUDHotbars : MonoBehaviour, MMEventListener<HotbarEvent>
    {
        [SerializeField] FPToolHotbar fpHudToolHotbar;
        [SerializeField] FPConsumableHotbar fpHudConsumableHotbar;

        void Start()
        {
            ValidateReferences();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(HotbarEvent eventType)
        {
            switch (eventType.EventType)
            {
                case HotbarEvent.HotbarEventType.AddToHotbar:
                    // HotbarManager handles the add logic
                    // UI will be updated via ConsumableHotbarChanged or ToolHotbarChanged events
                    break;

                case HotbarEvent.HotbarEventType.RemoveFromHotbar:
                    // HotbarManager handles the remove logic
                    // UI will be updated via ConsumableHotbarChanged or ToolHotbarChanged events
                    break;

                case HotbarEvent.HotbarEventType.ConsumableHotbarChanged:
                    // This is handled by FPConsumableHotbar directly
                    break;

                case HotbarEvent.HotbarEventType.ToolHotbarChanged:
                    // This is handled by FPToolHotbar directly
                    break;

                case HotbarEvent.HotbarEventType.RefreshAllHotbars:
                    RefreshAll();
                    break;
            }
        }

        void ValidateReferences()
        {
            if (fpHudToolHotbar == null) Debug.LogError("[FPHUDHotbars] fpHudToolHotbar is not assigned!");

            if (fpHudConsumableHotbar == null) Debug.LogError("[FPHUDHotbars] fpHudConsumableHotbar is not assigned!");
        }

        public void RefreshAll()
        {
            if (fpHudToolHotbar != null) fpHudToolHotbar.RefreshAllSlots();

            if (fpHudConsumableHotbar != null) fpHudConsumableHotbar.RefreshAllSlots();
        }

        // Called by input system
        public void HandleHotbarKeyPress(int keyNumber)
        {
            // Keys 1-2: Consumables (array indices 0-1)
            if (keyNumber >= 1 && keyNumber <= 2)
            {
                if (fpHudConsumableHotbar != null) fpHudConsumableHotbar.HandleConsumableKeyPress(keyNumber - 1);
            }
            // Keys 3-6: Tools (key 3 = empty hand = index 0, keys 4-6 = tools = indices 1-3)
            else if (keyNumber >= 3 && keyNumber <= 6)
            {
                if (fpHudToolHotbar != null) fpHudToolHotbar.HandleToolKeyPress(keyNumber - 3);
            }
        }

        public void CycleToolHotbar(int direction)
        {
            if (fpHudToolHotbar != null) fpHudToolHotbar.CycleTools(direction);
        }
    }
}
