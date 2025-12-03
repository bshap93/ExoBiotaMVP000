using Helpers;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class AttributesManager : MonoBehaviour, ICoreGameService, MMEventListener<AttributeEvent>
    {
        public bool autoSave;
        // has endurance and agility's traditional 
        // functions been merged into a single stat...for now
        int _agility;
        // has perception and dexterity's traditional (and possibly thief)
        // functions been merged into a single stat...for now
        int _dexterity;
        bool _dirty;

        // stat for assimilation of exobiota
        int _exobiotic;
        // has intelligence and charisma's traditional 
        // functions been merged into a single stat...for now
        int _mentalToughness;

        string _savePath;
        // just strength as normal
        int _strength;
        public static AttributesManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
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
        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("Strength", _strength, path);
            ES3.Save("Agility", _agility, path);
            ES3.Save("Dexterity", _dexterity, path);
            ES3.Save("MentalToughness", _mentalToughness, path);
            ES3.Save("Exobiotic", _exobiotic, path);
            _dirty = false;
        }
        public void Load()
        {
            var path = GetSaveFilePath();
            if (ES3.KeyExists("Strength", path))
                _strength = ES3.Load<int>("Strength", path);

            if (ES3.KeyExists("Agility", path))
                _agility = ES3.Load<int>("Agility", path);

            if (ES3.KeyExists("Dexterity", path))
                _dexterity = ES3.Load<int>("Dexterity", path);

            if (ES3.KeyExists("MentalToughness", path))
                _mentalToughness = ES3.Load<int>("MentalToughness", path);

            if (ES3.KeyExists("Exobiotic", path))
                _exobiotic = ES3.Load<int>("Exobiotic", path);
        }
        public void Reset()
        {
            _strength = 1;
            _agility = 1;
            _dexterity = 1;
            _mentalToughness = 1;
            _exobiotic = 1;
            MarkDirty();

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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.AttributesSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(AttributeEvent eventType)
        {
            if (eventType.EventType == AttributeEventType.Increase)
                switch (eventType.AttributeType)
                {
                    case AttributeType.Strength:
                        _strength += eventType.Value;
                        break;
                    case AttributeType.Agility:
                        _agility += eventType.Value;
                        break;
                    case AttributeType.Dexterity:
                        _dexterity += eventType.Value;
                        break;
                    case AttributeType.MentalToughness:
                        _mentalToughness += eventType.Value;
                        break;
                    case AttributeType.Exobiotic:
                        _exobiotic += eventType.Value;
                        break;
                }
        }
    }
}
