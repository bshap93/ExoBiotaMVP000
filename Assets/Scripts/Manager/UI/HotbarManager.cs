using System;
using Helpers.Events.UI;
using Helpers.Interfaces;
using Inventory;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.UI
{
    public class HotbarManager : MonoBehaviour, ICoreGameService, MMEventListener<HotbarEvent>
    {
        [SerializeField] int fpToolHotbarSize = 6;
        [SerializeField] int fpConsumableHotbarSize = 2;

        [FormerlySerializedAs("_inventoryManager")] [SerializeField]
        GlobalInventoryManager inventoryManager;
        public bool autoSave;

        int _currentConsumableHotbarIndex = 0;
        int _currentToolHotbarIndex = 0;

        bool _dirty;
        // Quantity of each consumable item in hotbar
        ItemHotbarData[] _fpConsumableHotbarItems;
        // Quantity not shown in hotbar, but used to answer questions like:
        // If I sell or drop this tool, does the hotbar need to change?
        ItemHotbarData[] _fpToolHotbarItems;
        // bool _hasLoadedAndApplied;
        string _savePath;

        public static HotbarManager Instance { get; private set; }
        void Awake()
        {
            if (Instance == null) Instance = this;
            else
                Destroy(gameObject);
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();

            if (inventoryManager == null) inventoryManager = GlobalInventoryManager.Instance;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void Save()
        {
            var path = GetSaveFilePath();
        }
        public void Load()
        {
            var path = GetSaveFilePath();


            _dirty = false;
            // _hasLoadedAndApplied = true;
        }
        public void Reset()
        {
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.HotbarSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(HotbarEvent eventType)
        {
            if (eventType.EventType == HotbarEvent.HotbarEventType.AddToHotbar)
            {
                var isTool = inventoryManager.IsItemIDaTool(eventType.ItemID);
                var isConsumable = inventoryManager.IsItemIDaConsumableEffectItem(eventType.ItemID);
                // if itemID is for a tool, try add to tool hotbar
                if (isTool)
                    TryAddItemToToolHotbar(eventType.ItemID, eventType.IndexInInventory);
                // if itemID is for a consumable, try add to consumable hotbar, and count the quantity
                else if (isConsumable)
                    TryAddItemToConsumableHotbar(eventType.ItemID, eventType.IndexInInventory);
                else
                    Debug.LogWarning(
                        $"[HotbarManager] Tried to add itemID {eventType.ItemID} to hotbar, but it is neither a tool nor a consumable.");
            }

            void TryAddItemToToolHotbar(string itemID, int indexInInventory)
            {
            }

            void TryAddItemToConsumableHotbar(string itemID, int indexInInventory)
            {
            }
        }

        [Serializable]
        public class ItemHotbarData
        {
            public string itemID;
            public int quantity;
            public int[] inventoryIndices;
        }
    }
}
