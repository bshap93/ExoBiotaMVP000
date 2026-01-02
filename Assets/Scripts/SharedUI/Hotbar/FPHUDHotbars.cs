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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
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

                    break;
                case HotbarEvent.HotbarEventType.RemoveFromHotbar:
                    

                    break;
            }
            
        }
    }
}
