using Inventory;
using MoreMountains.InventoryEngine;
using Overview.NPC;
using UnityEngine;
using Yarn.Unity;

public class CustomCommands : MonoBehaviour
{
    // Drag and drop your Dialogue Runner into this variable.
    public DialogueRunner dialogueRunner;
    public GameObject characterNPCRoot;

    public void Awake()
    {
        // Create a new command called 'camera_look', which looks at a target. 
        // Note how we're listing 'GameObject' as the parameter type.
        dialogueRunner.AddCommandHandler(
            "camera_look", // the name of the command
            CameraLookAtTarget // the method to run
        );

        // Inventory Commands

        dialogueRunner.AddCommandHandler<string, int>(
            "give_player_item",
            GivePlayerItem
        );

        // Dialogue Gestures

        dialogueRunner.AddCommandHandler<string, string>(
            "trigger_gesture",
            TriggerGesture
        );

        dialogueRunner.AddCommandHandler<string, string>(
            "switch_idle_animation",
            SwitchIdleLoopingAnimation
        );
    }

    // The method that gets called when '<<camera_look>>' is run.
    void CameraLookAtTarget()
    {
        Debug.LogWarning("Looking at target: ");
    }

    // Inventory Commands

    public void GivePlayerItem(string itemId, int amount = 1)
    {
        Debug.Log($"[Yarn] give_player_item on {name} (instanceID={GetInstanceID()}) x{amount}");

        var inv = GlobalInventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("GlobalInventoryManager not found, cannot give item.");
            return;
        }

        var item = inv.CreateItem(itemId); // SINGLE unit item
        if (item == null)
        {
            Debug.LogWarning($"Item with ID '{itemId}' not found.");
            return;
        }

        MMInventoryEvent.Trigger(
            MMInventoryEventType.Pick, null,
            item.TargetInventoryName, item, amount, 0, inv.playerId);
    }

    // Dialogue Gestures


    public void TriggerGesture(string npcId, string key)
    {
        // Find NPC by id in the scene
        if (characterNPCRoot == null)
        {
            Debug.LogError($"NPC '{npcId}' not found in scene.");
            return;
        }

        var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

        if (helper == null) return;

        helper.PlayGesture(key);
    }

    public void SwitchIdleLoopingAnimation(string npcId, string key)
    {
        // Find NPC by id in the scene
        if (characterNPCRoot == null)
        {
            Debug.LogError($"NPC '{npcId}' not found in scene.");
            return;
        }

        var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

        if (helper == null) return;

        helper.SwitchIdleLoopingAnimation(key);
    }
}
