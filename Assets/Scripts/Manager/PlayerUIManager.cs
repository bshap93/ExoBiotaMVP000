using Events;
using Helpers.Events;
using Helpers.Events.UI;
using Helpers.Interfaces;
using Helpers.ScriptableObjects;
using MoreMountains.Tools;
using Structs;
using UnityEngine;

namespace Manager
{
    [DefaultExecutionOrder(0)]
    public class PlayerUIManager : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<ModeLoadEvent>,
        MMEventListener<HUDEvent>, ICoreGameService
    {
        public static PlayerUIManager Instance;

        public StatusEffectIconRepository defaultIconRepository;

        public bool uiIsOpen;

        public bool iGUIsOpen;

        public bool modalIsOpen;

        public bool gatedUIIsOpen;
        
        // Persistent variables


        void Awake()
        {
            if (Instance == null)
                Instance = this;
            // if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
            //     DontDestroyOnLoad(gameObject);
            else
                Destroy(gameObject);
        }


        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<ModeLoadEvent>();
            this.MMEventStartListening<HUDEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<ModeLoadEvent>();
            this.MMEventStopListening<HUDEvent>();
        }

        public void OnMMEvent(ModeLoadEvent eventType)
        {
            if (eventType.EventType == ModeLoadEventType.Load)
            {
                Debug.Log("Load Mode");
                if (eventType.ModeName == GameMode.DirigibleFlight)
                {
                }
            }
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            switch (eventType.uiActionType)
            {
                case UIActionType.Open:
                    uiIsOpen = true;
                    if (eventType.uiType == UIType.InGameUI && !modalIsOpen)
                        iGUIsOpen = true;
                    else if (eventType.uiType == UIType.ModalBoxChoice)
                        modalIsOpen = true;
                    else if (eventType.uiType == UIType.HarvestableInteractChoice ||
                             eventType.uiType == UIType.BreakableInteractChoice ||
                             eventType.uiType == UIType.MachineInteractChoice ||
                             eventType.uiType == UIType.WaitWhileInteracting)
                        gatedUIIsOpen = true;

                    break;
                case UIActionType.Close:
                    if (eventType.uiType == UIType.InGameUI)
                        iGUIsOpen = false;
                    else if (eventType.uiType == UIType.ModalBoxChoice)
                        modalIsOpen = false;
                    else if (eventType.uiType == UIType.HarvestableInteractChoice ||
                             eventType.uiType == UIType.BreakableInteractChoice ||
                             eventType.uiType == UIType.MachineInteractChoice ||
                             eventType.uiType == UIType.WaitWhileInteracting)
                        gatedUIIsOpen = false;

                    if (!iGUIsOpen && !modalIsOpen)
                        uiIsOpen = false;

                    break;
                case UIActionType.Update:
                    // Handle any updates if necessary
                    break;
                case UIActionType.Toggle:
                    uiIsOpen = !uiIsOpen;
                    break;
                default:
                    Debug.LogWarning($"Unhandled UIActionType: {eventType.uiActionType}");
                    break;
            }
        }
        public void OnMMEvent(HUDEvent eventType)
        {
            throw new System.NotImplementedException();
        }
        public void Save()
        {
            throw new System.NotImplementedException();
        }
        public void Load()
        {
            throw new System.NotImplementedException();
        }
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
        public void ConditionalSave()
        {
            throw new System.NotImplementedException();
        }
        public void MarkDirty()
        {
            throw new System.NotImplementedException();
        }
        public string GetSaveFilePath()
        {
            throw new System.NotImplementedException();
        }
        public void CommitCheckpointSave()
        {
            throw new System.NotImplementedException();
        }
        public bool HasSavedData()
        {
            throw new System.NotImplementedException();
        }
    }
}
