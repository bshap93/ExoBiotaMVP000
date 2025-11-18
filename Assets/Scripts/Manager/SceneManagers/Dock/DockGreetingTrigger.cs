using Events;
using Manager.DialogueScene;
using Manager.SceneManagers.Dock;
using MoreMountains.Tools;
using Overview.NPC;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.SceneManagers.Global.Dock
{
    /// Attaclh to the Core scene (lives across modes)
    public class DockGreetingTrigger : MonoBehaviour, MMEventListener<DockingEvent>
    {
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private NpcDatabase npcDatabase;

        [ValueDropdown("GetNpcGreeterIdOptions")] [SerializeField]
        private string npcGreeterId;

        [FormerlySerializedAs("DockId")] [ValueDropdown("GetDockIdOptions")]
        public string dockId;

        public void OnEnable()
        {
            this.MMEventStartListening();
        }

        public void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(DockingEvent e)
        {
            if (e.EventType != DockingEventType.DockAtLocation) return;
            if (e.DockDefinition.dockId != dockId) return;

            // 1. build $clancyGreeted, $adaGreeted, …
            var varKey = BuildGreetingFlag(npcGreeterId);

            // 2. skip if already greeted
            var storage = dialogueManager.variableStorage;
            if (storage.TryGetValue<bool>(varKey, out var greeted) && greeted)
                return;

            OverviewLocationEvent.Trigger(LocationType.Any,
                LocationActionType.BeApproached, null, null);

            // 3. open the dialogue
            if (npcDatabase.TryGet(npcGreeterId, out var def))
                dialogueManager.Open(def, autoClose: true);
        }

        private static string[] GetNpcGreeterIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        private static string[] GetDockIdOptions()
        {
            return DockManager.GetDockIdOptions();
        }

        private static string BuildGreetingFlag(string npcId)
        {
            // UpperCamelName  -> lowerCamelGreeted
            if (string.IsNullOrEmpty(npcId))
                return "$greeted";

            var lowerCamel = char.ToLowerInvariant(npcId[0]) + npcId.Substring(1) + "Greeted";
            return $"${lowerCamel}";
        }
    }
}