using Helpers.Events;
using MoreMountains.Tools;
using Rewired.Integration.Cinemachine3;
using UnityEngine;

public class DirigibleCameraEventHandler : MonoBehaviour, MMEventListener<MyUIEvent>
{
    RewiredCinemachineInputAxisController _rewiredCinemachineInputAxisController;
    void Awake()
    {
        _rewiredCinemachineInputAxisController = GetComponent<RewiredCinemachineInputAxisController>();
    }

    void OnEnable()
    {
        this.MMEventStartListening();
    }
    void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(MyUIEvent eventType)
    {
        if (eventType.uiActionType == UIActionType.Open)
            _rewiredCinemachineInputAxisController.enabled = false;
        else if (eventType.uiActionType == UIActionType.Close) _rewiredCinemachineInputAxisController.enabled = true;
    }
}
