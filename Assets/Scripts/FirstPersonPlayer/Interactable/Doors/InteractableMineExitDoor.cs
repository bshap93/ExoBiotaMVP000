using Events;
using Helpers.Events;
using Manager;
using Objectives.ScriptableObjects;
using Overview.OverviewMode.ScriptableObjectDefinitions;
using Structs;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class InteractableMineExitDoor : InteractableDoor
    {
        public DockDefinition dockDefinition;

        [SerializeField] ObjectiveObject objectiveIfActiveToComplete;

        public override async void Interact()
        {
            if (!TryOpenWithAccess()) return;
            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            AlertEvent.Trigger(
                AlertReason.UseDoor,
                "Exit the mine and return to the dirigible?", "Use Door", AlertType.ChoiceModal, 0f,
                onConfirm: () =>
                {
                    var spawnInfo = dockDefinition.spawnInfo.ToSpawnInfo();

                    SpawnEvent.Trigger(
                        SpawnEventType.ToDock, spawnInfo.SceneName, GameMode.Overview,
                        spawnInfo.SpawnPointId,
                        dockDefinition);

                    if (objectiveIfActiveToComplete != null)
                        ObjectiveEvent.Trigger(
                            objectiveIfActiveToComplete.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                },
                onCancel: () => { });
        }
        public override string GetName()
        {
            return "Go to Dock";
        }
        public override Sprite GetIcon()
        {
            return ExaminationManager.Instance.iconRepository.dockIcon;
        }
        public override string ShortBlurb()
        {
            return string.Empty;
        }
        public override Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.doorIcon;
        }
        public override string GetActionText()
        {
            return "Enter";
        }
    }
}
