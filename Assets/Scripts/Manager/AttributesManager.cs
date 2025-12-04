using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Helpers.Events;
using Helpers.Events.Progression;
using Helpers.Interfaces;
using Helpers.StaticHelpers;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class AttributesManager : MonoBehaviour, ICoreGameService, MMEventListener<InnerCoreXPEvent>
    {
        const float XpBase = 10f;
        const float XpExponent = 1.5f;
        public bool autoSave;
        // has endurance and agility's traditional 
        // functions been merged into a single stat...for now
        int _agility;

        int _currentUnusedXP;


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
        public int CurrentUnusedXP
        {
            get => _currentUnusedXP;
            set
            {
                _currentUnusedXP = value;
                MarkDirty();
            }
        }

        public int Agility
        {
            get => _agility;
            set
            {
                _agility = value;
                MarkDirty();
            }
        }

        public int Dexterity
        {
            get => _dexterity;
            set
            {
                _dexterity = value;
                MarkDirty();
            }
        }

        public int Exobiotic
        {
            get => _exobiotic;
            set
            {
                _exobiotic = value;
                MarkDirty();
            }
        }

        public int MentalToughness
        {
            get => _mentalToughness;
            set
            {
                _mentalToughness = value;
                MarkDirty();
            }
        }


        public int Strength
        {
            get => _strength;
            set
            {
                _strength = value;
                MarkDirty();
            }
        }


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
            ES3.Save("Strength", _strength, path);
            ES3.Save("Agility", _agility, path);
            ES3.Save("Dexterity", _dexterity, path);
            ES3.Save("MentalToughness", _mentalToughness, path);
            ES3.Save("Exobiotic", _exobiotic, path);
            ES3.Save("CurrentUnusedXP", _currentUnusedXP, path);


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

            if (ES3.KeyExists("CurrentUnusedXP", path))
                _currentUnusedXP = ES3.Load<int>("CurrentUnusedXP", path);
        }
        public void Reset()
        {
            _strength = 1;
            _agility = 1;
            _dexterity = 1;
            _mentalToughness = 1;
            _exobiotic = 1;

            _currentUnusedXP = 0;

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
        public void OnMMEvent(InnerCoreXPEvent eventType)
        {
            if (eventType.EventType == InnerCoreXPEventType.ConvertCoreToXP)
                ConvertCoreToXP(eventType.CoreGrade);
        }


        public int GetXpRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.RoundToInt(XpBase * Mathf.Pow(level, XpExponent));
        }

        public int GetTotalXpForLevel(int targetLevel)
        {
            var total = 0;
            for (var i = 2; i <= targetLevel; i++) total += GetXpRequiredForLevel(i);
            return total;
        }

        void ConvertCoreToXP(
            HarvestableInnerObject.InnerObjectValueGrade coreGrade)
        {
            // remove one core from inventory
            InventoryHelperCommands.RemoveInnerCore(coreGrade);

            // add the XP
            var amount = 0;
            switch (coreGrade)
            {
                case HarvestableInnerObject.InnerObjectValueGrade.StandardGrade:
                    amount = 10;
                    break;
                case HarvestableInnerObject.InnerObjectValueGrade.Radiant:
                    amount = 20;
                    break;
                case HarvestableInnerObject.InnerObjectValueGrade.Stellar:
                    amount = 30;
                    break;
                case HarvestableInnerObject.InnerObjectValueGrade.Unreasonable:
                    amount = 50;
                    break;
                case HarvestableInnerObject.InnerObjectValueGrade.MiscExotic:
                    amount = 0;
                    break;
            }

            _currentUnusedXP += amount;

            XPEvent.Trigger(XPEventType.SetUnusedXP, _currentUnusedXP);

            MarkDirty();
        }
    }
}
