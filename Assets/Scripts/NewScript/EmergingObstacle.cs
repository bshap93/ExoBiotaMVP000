using System;
using DG.Tweening;
using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Utilities.Interface;

namespace NewScript
{
    public class EmergingObstacle : MonoBehaviour, IRequiresUniqueID, MMEventListener<SpontaneousTriggerEvent>
    {
        [SerializeField] string emergeEventID;
        [SerializeField] GameObject childObject;
        [SerializeField] MMFeedbacks emergeFeedbacks;
        [SerializeField] Transform initialPosition;

        void Start()
        {
            if (initialPosition != null)
            {
                childObject.transform.position = initialPosition.position;
                childObject.transform.rotation = initialPosition.rotation;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer")) Emerge();
        }
        public string UniqueID => emergeEventID;
        public void SetUniqueID()
        {
            emergeEventID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(emergeEventID);
        }
        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
        }

        public void Emerge()
        {
            emergeFeedbacks?.PlayFeedbacks();
            childObject.transform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InExpo);
        }
    }
}
