using System;
using Helpers.Events.Combat;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class AmmoManager : MonoBehaviour, ICoreGameService, MMEventListener<AmmoEvent>
    {
        [SerializeField] MoreMountains.InventoryEngine.Inventory ammoInventory;
        bool _dirty;
        bool _hasLoadedAndApplied;
        string _savePath;
        public static AmmoManager Instance { get; private set; }

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
            throw new NotImplementedException();
        }
        public void Load()
        {
            throw new NotImplementedException();
        }
        public void Reset()
        {
            throw new NotImplementedException();
        }
        public void ConditionalSave()
        {
            throw new NotImplementedException();
        }
        public void MarkDirty()
        {
            throw new NotImplementedException();
        }
        public string GetSaveFilePath()
        {
            throw new NotImplementedException();
        }
        public void CommitCheckpointSave()
        {
            throw new NotImplementedException();
        }
        public bool HasSavedData()
        {
            throw new NotImplementedException();
        }
        public void OnMMEvent(AmmoEvent eventType)
        {
            throw new NotImplementedException();
        }
    }
}
