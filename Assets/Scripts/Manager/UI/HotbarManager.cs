using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events.UI;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.UI
{
    public class HotbarManager : MonoBehaviour, ICoreGameService, MMEventListener<HotbarEvent>
    {
        [SerializeField] int fpToolHotbarSize = 6;
        [SerializeField] int fpConsumableHotbarSize = 2;
        
        BaseTool[] _fpToolHotbarItems;
        ConsumableEffectItem[] _fpConsumableHotbarItems;
        
        int[] _quantityConsumableHotbarItems;
        
        int _currentConsumableHotbarIndex = 0;
        int _currentToolHotbarIndex = 0;
        
        bool _dirty;
        bool _hasLoadedAndApplied;
        string _savePath;
        public bool autoSave;
        
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
            
            if (_fpToolHotbarItems.Length < fpToolHotbarSize)
                ES3.Save("FPToolHotbarItems", _fpToolHotbarItems, path);
            else
            {
                Debug.LogError("Attempted to save: " + _fpToolHotbarItems.Length +
                               " tool hotbar items, but max size is " + fpToolHotbarSize);
            } 
            if (_fpConsumableHotbarItems.Length < fpConsumableHotbarSize)
                ES3.Save("FPConsumableHotbarItems", _fpConsumableHotbarItems, path);
            else
            {
                Debug.LogError("Attempted to save: " + _fpConsumableHotbarItems.Length +
                               " consumable hotbar items, but max size is " + fpConsumableHotbarSize);
            }   
            
            if (_quantityConsumableHotbarItems.Length == _fpConsumableHotbarItems.Length)
                ES3.Save("QuantityConsumableHotbarItems", _quantityConsumableHotbarItems, path);
            else
            {
                Debug.LogError("Attempted to save: " + _quantityConsumableHotbarItems.Length +
                               " consumable quantity hotbar items, but size is " + _fpConsumableHotbarItems.Length);
            }
            if (_currentConsumableHotbarIndex < fpConsumableHotbarSize)
                ES3.Save("CurrentConsumableHotbarIndex", _currentConsumableHotbarIndex, path);
            else
            {
                Debug.LogError("Attempted to save: " + _currentConsumableHotbarIndex +
                               " current consumable hotbar index, but max size is " + fpConsumableHotbarSize);
            }
            if (_currentToolHotbarIndex < fpToolHotbarSize)
                ES3.Save("CurrentToolHotbarIndex", _currentToolHotbarIndex, path);
            else
            {
                Debug.LogError("Attempted to save: " + _currentToolHotbarIndex +
                               " current tool hotbar index, but max size is " + fpToolHotbarSize);
            }

        }
        public void Load()
        {
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
            throw new System.NotImplementedException();
        }
    }
}
