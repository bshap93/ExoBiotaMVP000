using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Status;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager.FirstPerson
{
    [Serializable]
    public class DeathInformation
    {
        public PlayerStatsEvent.StatChangeCause causeOfDeath;

        // constructor
        public DeathInformation(PlayerStatsEvent.StatChangeCause causeOfDeath = PlayerStatsEvent.StatChangeCause.Other)
        {
            this.causeOfDeath = causeOfDeath;
        }
    }

    public class PlayerDeathManager : MonoBehaviour, ICoreGameService, MMEventListener<PlayerDeathEvent>
    {
        const string DeathsListKey = "DeathsList";

        [Header("UI")]
        // [SerializeField] private GameObject deathMenuPrefab; // assign DeathScreen.prefab
        [SerializeField]
        DeathMenuController deathMenuController;

        [SerializeField] CanvasGroup deathMenuCanvasGroup;


        [SerializeField] bool autoSave; // checkpoint-only by default

        readonly List<DeathInformation> _deaths = new();
        bool _dirty;

        bool _menuOpen;
        string _savePath;

        public static PlayerDeathManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            // if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
            //     DontDestroyOnLoad(gameObject);
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[PlayerDeathManager] No save file found, forcing initial save...");
                Reset();
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
            ES3.Save(DeathsListKey, _deaths, _savePath);
        }

        public void Load()
        {
            var loadedDeaths = ES3.KeyExists(DeathsListKey, _savePath)
                ? ES3.Load<List<DeathInformation>>(DeathsListKey, _savePath)
                : new List<DeathInformation>();

            _deaths.Clear();
            _deaths.AddRange(loadedDeaths);
        }

        public void Reset()
        {
            _deaths.Clear();
            Save();
            _dirty = false;
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PlayerDeath);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void OnMMEvent(PlayerDeathEvent evt)
        {
            // Record cause if you want (already have Death struct/list)
            _deaths.Add(new DeathInformation(evt.DeathInformation.causeOfDeath));
            MarkDirty(); // buffer it; don't save yet


            SceneManager.LoadScene("DeathScene", LoadSceneMode.Single);
        }

        public bool IsDontDestroyOnLoad()
        {
            return SaveManager.Instance.saveManagersDontDestroyOnLoad;
        }
    }
}
