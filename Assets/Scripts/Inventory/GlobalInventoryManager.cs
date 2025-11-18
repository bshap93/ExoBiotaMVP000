using System;
using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Interfaces;
using Inventory.ScriptableObjects;
using Manager;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Inventory
{
    public class GlobalInventoryManager : MonoBehaviour, ICoreGameService
    {
        public enum EquippableType
        {
            LHand,
            RHand,
            Back,
            NotEquippable,
            Unknown
        }

        public enum InventoryWithWeightLimit
        {
            PlayerMainInventory,
            DirigibleInventory
        }


        public const string PlayerInventoryName = "PlayerMainInventory";
        public const string LeftHandEquipmentInventoryName = "LEquipmentInventory";
        public const string RightHandEquipmentInventoryName = "EquippedItemInventory";
        public const string BackEquipmentInventoryName = "BackEquippedItemInv";
        public const string DirigibleInventoryName = "DirigibleInventory";
        public const string DirigibleScannerInventoryName = "DirigMainScannerInventory";


        static string _savePath;
        static string _currentSceneName;

        [Header("Saves")] [SerializeField] bool autoSave; // <— NEW

        [Header("Player Main Inventory")] [FormerlySerializedAs("PlayerInventory")]
        public MoreMountains.InventoryEngine.Inventory playerInventory;

        [Header("First Person Slot Inventories")]
        public MoreMountains.InventoryEngine.Inventory lEquipmentInventory;

        public MoreMountains.InventoryEngine.Inventory equipmentInventory;
        public MoreMountains.InventoryEngine.Inventory backEquipmentInventory;

        [Header("Dirigible Inventory")] public MoreMountains.InventoryEngine.Inventory dirigibleInventory;

        [FormerlySerializedAs("dirigibleEquipmentInventory")] [Header("Dirigible Slot Inventories")]
        public MoreMountains.InventoryEngine.Inventory dirigibleScannerSlot;

        [Header("Default Items")] public DefaultInventoryDefinition playerStartingItems;
        public DefaultInventoryDefinition lEquipmentStartingItems;
        public DefaultInventoryDefinition equipmentStartingItems;
        public DefaultInventoryDefinition dirigibleStartingItems;
        public DefaultInventoryDefinition backEquipmentStartingItems;
        public DefaultInventoryDefinition dirigibleScannerStartingItems;

        [FormerlySerializedAs("intitialPlayerFPMaxWeight")] [Header("Initial Weight Limits")] [SerializeField]
        float initialPlayerFPMaxWeight;
        [FormerlySerializedAs("maxDirigibleWeight")] [SerializeField]
        float initialMaxDirigibleWeight;


        [FormerlySerializedAs("PlayerId")] public string playerId = "Player1";

        [Header("Equippable Types")] public EquippableTypesDatatable equippableTypesTable;

        public InventoryDatabase InventoryDatabaseVariable;
        float _currentDirigibleWeight;
        float _currentPlayerFPWeight;
        bool _dirty; // <— NEW
        float _maxDirigibleWeight;

        float _maxPlayerFPWeight;

        public Dictionary<string, EquippableType> ItemEquippableTypesDictionary;

        public static GlobalInventoryManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _currentSceneName = SceneManager.GetActiveScene().name;

            // Ensure defaults are set even before Load/Reset
            _maxPlayerFPWeight = initialPlayerFPMaxWeight;
            _maxDirigibleWeight = initialMaxDirigibleWeight;

            if (equippableTypesTable != null)
                ItemEquippableTypesDictionary = equippableTypesTable.ToDictionary();
            else
                Debug.LogWarning("EquippableTypesDatatable is not assigned in GlobalInventoryManager.");
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _savePath = GetSaveFilePath();
        }


        public void Save()
        {
            SaveGlobalInventories();
            ES3.Save("CurrentFPWeightCarried", _currentPlayerFPWeight, _savePath);
            ES3.Save("MaxFPWeight", _maxPlayerFPWeight, _savePath);
            ES3.Save("CurrentDirigibleWeightCarried", _currentDirigibleWeight, _savePath);
            ES3.Save("MaxDirigibleWeight", _maxDirigibleWeight, _savePath);
            _dirty = false; // <— NEW
        }

        public void Load()
        {
            LoadGlobalInventories();

            LoadWeightValues();

            _dirty = false; // <— NEW
        }

        public void Reset()
        {
            ResetGlobalInventories();
            PopulateInventoriesFromDefaults();
            _maxDirigibleWeight = initialMaxDirigibleWeight;
            _maxPlayerFPWeight = initialPlayerFPMaxWeight;
            _currentDirigibleWeight = 0f;
            _currentPlayerFPWeight = 0f;
            _dirty = true;
            ConditionalSave();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.GlobalInventorySave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }
        void LoadWeightValues()
        {
            _savePath = GetSaveFilePath();
            // If no saved inventory exists, populate from defaults
            if (!HasSavedData())
            {
                // Set current player fp weight to default
                _currentPlayerFPWeight = 0f;
                _maxPlayerFPWeight = initialPlayerFPMaxWeight;
                _currentDirigibleWeight = 0f;
                _maxDirigibleWeight = initialMaxDirigibleWeight;
            }

            if (ES3.KeyExists("CurrentFPWeightCarried", _savePath))
                _currentPlayerFPWeight = ES3.Load<float>("CurrentFPWeightCarried", _savePath, 0);

            if (ES3.KeyExists("MaxFPWeight", _savePath))
                _maxPlayerFPWeight = ES3.Load("MaxFPWeight", _savePath, initialPlayerFPMaxWeight);

            if (ES3.KeyExists("CurrentDirigibleWeightCarried", _savePath))
                _currentDirigibleWeight = ES3.Load<float>("CurrentDirigibleWeightCarried", _savePath, 0);

            if (ES3.KeyExists("MaxDirigibleWeight", _savePath))
                _maxDirigibleWeight = ES3.Load("MaxDirigibleWeight", _savePath, initialMaxDirigibleWeight);
        }

        public float GetDirigibleMaxWeight()
        {
            return _maxDirigibleWeight;
        }
        public float GetPlayerMaxWeight()
        {
            return _maxPlayerFPWeight;
        }

        public float GetWeightOfPlayerMainPlusEquipped()
        {
            var totalWeight = 0f;
            totalWeight += GetWeightOfInventoryItems(playerInventory);
            totalWeight += GetWeightOfInventoryItems(equipmentInventory);
            totalWeight += GetWeightOfInventoryItems(lEquipmentInventory);
            totalWeight += GetWeightOfInventoryItems(backEquipmentInventory);
            return totalWeight;
        }

        public float GetTotalWeightInDirigible()
        {
            var totalWeight = 0f;
            totalWeight += GetWeightOfInventoryItems(dirigibleInventory);
            totalWeight += GetWeightOfInventoryItems(dirigibleScannerSlot);
            totalWeight += GetWeightOfPlayerMainPlusEquipped();
            return totalWeight;
        }

        float GetWeightOfInventoryItems(MoreMountains.InventoryEngine.Inventory inventory)
        {
            var totalWeight = 0f;
            foreach (var item in inventory.Content)
            {
                var myBaseItem = item as MyBaseItem;
                if (myBaseItem == null) continue;
                var itemWeight = myBaseItem.weight * item.Quantity;
                totalWeight += itemWeight;
            }


            return totalWeight;
        }

        public bool IsDontDestroyOnLoad()
        {
            return SaveManager.Instance.saveManagersDontDestroyOnLoad;
        }

        public void AddItemTo(MoreMountains.InventoryEngine.Inventory inv, InventoryItem item, int quantity)
        {
            if (inv == null || item == null) return;
            inv.AddItem(item, Math.Max(1, quantity));
            MarkDirty(); // <— NEW
            ConditionalSave(); // <— NEW
        }


        void PopulateInventoriesFromDefaults()
        {
            PopulateInventory(playerInventory, playerStartingItems);
            PopulateInventory(equipmentInventory, equipmentStartingItems);
            PopulateInventory(lEquipmentInventory, lEquipmentStartingItems);
            PopulateInventory(dirigibleInventory, dirigibleStartingItems);
            PopulateInventory(backEquipmentInventory, backEquipmentStartingItems);
            PopulateInventory(dirigibleScannerSlot, dirigibleScannerStartingItems);
        }

        static void PopulateInventory(MoreMountains.InventoryEngine.Inventory inv,
            DefaultInventoryDefinition def)
        {
            if (inv == null || def == null) return;

            var size = Mathf.Max(def.inventorySize, def.defaultItems.Length);
            inv.ResizeArray(size); // ensure enough slots
            inv.EmptyInventory(); // start clean

            foreach (var item in def.defaultItems)
            {
                if (item == null) continue;
                inv.AddItem(
                    item.Copy(), // never add the SO instance itself
                    Math.Max(1, item.Quantity)); // 1 if Quantity not set at runtime
            }
        }

        public void SaveGlobalInventories()
        {
            SaveOne(playerInventory);
            SaveOne(equipmentInventory);
            SaveOne(lEquipmentInventory);
            SaveOne(dirigibleInventory);
            SaveOne(backEquipmentInventory);
            SaveOne(dirigibleScannerSlot);
        }

        public void ResetGlobalInventories()
        {
            ResetOne(playerInventory);
            ResetOne(equipmentInventory);
            ResetOne(lEquipmentInventory);
            ResetOne(dirigibleInventory);
            ResetOne(backEquipmentInventory);
            ResetOne(dirigibleScannerSlot);
        }

        public void LoadGlobalInventories()
        {
            LoadOne(playerInventory);
            LoadOne(equipmentInventory);
            LoadOne(lEquipmentInventory);
            LoadOne(dirigibleInventory);
            LoadOne(backEquipmentInventory);
            LoadOne(dirigibleScannerSlot);
        }

        static void SaveOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
                inv.SaveInventory();
        }

        static void LoadOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
                inv.LoadSavedInventory();
        }

        static void ResetOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
            {
                inv.ResetSavedInventory();
                inv.EmptyInventory();
            }
        }

        public EquippableType GetEquippableType(InventoryItem itemVar)
        {
            if (itemVar == null) throw new ArgumentNullException(nameof(itemVar), "Item cannot be null");

            if (equippableTypesTable.entries.Exists(e => e.ItemID == itemVar.ItemID))
            {
                // Find the entry in the equippable types table
                var entry = equippableTypesTable.entries.Find(e => e.ItemID == itemVar.ItemID);
                return entry.Type;
            }


            // Default to NotEquippable if not found
            return EquippableType.Unknown;
        }

        public InventoryItem CreateItem(string itemId, int amount = 1)
        {
            if (InventoryDatabaseVariable == null)
            {
                Debug.LogError("InventoryDatabase not assigned in GlobalInventoryManager!");
                return null;
            }

            // Always return a single-unit instance; let AddItem decide how many to add.
            return InventoryDatabaseVariable.CreateItem(itemId);
        }
        public float GetMaxWeightOfPlayerCarry()
        {
            return _maxPlayerFPWeight;
        }

        public float GetMaxWeightOfDirigibleCarry()
        {
            return _maxDirigibleWeight;
        }
        public float GetWeightOfInventory(MoreMountains.InventoryEngine.Inventory inventory)
        {
            if (inventory == null) return 0f;
            return GetWeightOfInventoryItems(inventory);
        }
    }
}
