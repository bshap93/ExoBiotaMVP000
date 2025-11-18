using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.FeedbackControllers
{
    public class DeathFeedbackController : MonoBehaviour, IFeedbackController,
        MMEventListener<PlayerDeathEvent>
    {
        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void OnMMEvent(PlayerDeathEvent eventType)
        {
        }
    }
}