using Events;
using Manager.SceneManagers.Dock;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI
{
    public class AlertNewContentCounterText : MonoBehaviour, MMEventListener<DockingEvent>
    {
        [SerializeField] private TMP_Text text;
        private string _dockId;
        private string _locationId;


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(DockingEvent dockingEvent)
        {
            if (dockingEvent.EventType == DockingEventType.FinishedDocking)
            {
                var events = DockManager.Instance.alertNewContentEvents;
                if (events == null || events.Count == 0)
                {
                    text.text = "0";
                    return;
                }

                var count = 0;
                foreach (var e in events)
                    if (e.LocationId == _locationId && e.DockId == _dockId)
                        count++;

                text.text = count.ToString();
            }
        }

        public void Initialize(string locationId, string dockId)
        {
            _locationId = locationId;
            _dockId = dockId;
        }
    }
}