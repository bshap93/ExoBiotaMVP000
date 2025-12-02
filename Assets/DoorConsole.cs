using FirstPersonPlayer.Interface;
using Helpers.Events;
using Inventory;
using LevelConstruct.Interactable.Door;
using UnityEngine;

public class DoorConsole : MonoBehaviour, IInteractable
{
    [SerializeField] LockedDoor lockedDoor;


    public void Interact()
    {
        if (!CanInteract()) return;
    }

    public bool CanInteract()
    {
        if (!lockedDoor.isLocked)
            return true;

        if (GlobalInventoryManager.Instance.HasKeyForDoor(lockedDoor.keyID))
        {
            lockedDoor.isLocked = false;
            Debug.Log(lockedDoor.keyID);
            return true;
        }

        AlertEvent.Trigger(AlertReason.DoorLocked, "The door is locked. You need a key to open it.");

        return false;
    }
    public void OnInteractionStart()
    {
    }
    public void OnInteractionEnd(string param)
    {
    }

    public bool IsInteractable()
    {
        return true;
    }
    public void OnFocus()
    {
    }
    public void OnUnfocus()
    {
    }
}
