using Events;
using Manager.SceneManagers.Dock;
using MoreMountains.Tools;
using UnityEngine;

public class OverviewCameraTarget : MonoBehaviour, MMEventListener<DockingEvent>
{
    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }


    public void OnMMEvent(DockingEvent eventType)
    {
        if (eventType.EventType == DockingEventType.DockAtLocation)
            if (DockManager.Instance != null)
            {
                var dirigibleDockInteractable = DockManager.Instance.currentDockInteractable;
                if (dirigibleDockInteractable != null)
                {
                    transform.position = dirigibleDockInteractable.overviewCameraTarget.position;
                    transform.rotation = dirigibleDockInteractable.overviewCameraTarget.rotation;
                }
            }
    }
}