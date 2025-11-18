using Dirigible.Interactable;
using Events;
using Helpers.Events;
using HighlightPlus;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible
{
    [RequireComponent(typeof(HighlightEffect))]
    public class DockHighlightTrigger : MonoBehaviour
    {
        [SerializeField] HighlightEffect highlightEffect;
        [SerializeField] DirigibleDockInteractable dockInteractable;

        [SerializeField] MMFeedbacks enterRangeFeedbacks;
        [SerializeField] MMFeedbacks exitRangeFeedbacks;

        [SerializeField] bool alertEnabled = true;

        void Start()
        {
            if (dockInteractable == null)
                dockInteractable = GetComponent<DirigibleDockInteractable>();

            if (dockInteractable == null) Debug.LogError("NO dock component on gameobject");
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                highlightEffect.highlighted = true;
                if (alertEnabled)
                    AlertEvent.Trigger(
                        AlertReason.InRangeOfDockingStation,
                        "In Range of Docking Station. Approach and press E to initiate docking procedure.",
                        "Docking Station Nearby");

                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show,
                    dockInteractable.actionId);

                enterRangeFeedbacks?.PlayFeedbacks();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                highlightEffect.highlighted = false;
                if (alertEnabled)
                    AlertEvent.Trigger(
                        AlertReason.InRangeOfDockingStation,
                        "Left Range of Docking Station.", "Left Docking Station Range");

                ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, dockInteractable.actionId);

                exitRangeFeedbacks?.PlayFeedbacks();
            }
        }
    }
}
