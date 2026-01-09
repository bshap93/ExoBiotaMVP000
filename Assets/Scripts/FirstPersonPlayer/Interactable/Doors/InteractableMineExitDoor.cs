using Helpers.Events;
using Helpers.Events.UI;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class InteractableMineExitDoor : InteractableDoor
    {
        [SerializeField] MMFeedbacks denyEntryFeedbacks;
        // [SerializeField] SpawnInfoEditor spawnInfo;

        // [SerializeField] ObjectiveObject objectiveIfActiveToComplete;

        public string bridgeName;

        public override async void Interact()
        {
            if (!TryOpenWithAccess()) return;
            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            AlertEvent.Trigger(
                AlertReason.UseDoor,
                "Exit the mine and return to the dirigible?", "Use Door", AlertType.ChoiceModal, 0f,
                onConfirm: () =>
                {
                    SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);

                    SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);
                    SaveDataEvent.Trigger();

                    // SpawnEvent.Trigger(
                    //     SpawnEventType.ToMine, sceneToLoad, GameMode.FirstPerson,
                    //     spawnPointId
                    // );
                    //
                    SceneManager.LoadScene(bridgeName);
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
