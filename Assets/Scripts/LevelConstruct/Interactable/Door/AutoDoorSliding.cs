using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace LevelConstruct.Interactable.Door
{
    public class AutoDoorSliding : MonoBehaviour
    {
        [SerializeField] GameObject rightDoor;
        [SerializeField] GameObject leftDoor;
        [SerializeField] Vector3 rightDoorOpenPosition;
        [SerializeField] Vector3 leftDoorOpenPosition;
        [SerializeField] Vector3 rightDoorClosedPosition;
        [SerializeField] Vector3 leftDoorClosedPosition;
        [SerializeField] float openCloseDuration = 1f;
        [SerializeField] MMFeedbacks doorOpenFeedbacks;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void OpenDoor()
        {
            // DoTween
            rightDoor.transform.DOLocalMoveX(rightDoorOpenPosition.x, openCloseDuration);
            leftDoor.transform.DOLocalMoveX(leftDoorOpenPosition.x, openCloseDuration);
        }

        public void CloseDoor()
        {
            // DoTween
            rightDoor.transform.DOMove(rightDoorClosedPosition, openCloseDuration);
            leftDoor.transform.DOMove(leftDoorClosedPosition, openCloseDuration);
        }
    }
}
