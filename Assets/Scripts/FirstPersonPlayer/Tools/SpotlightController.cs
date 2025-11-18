using Domains.Player.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Static;
using UnityEngine;

namespace FirstPersonPlayer.Tools
{
    public class SpotlightController : MonoBehaviour, MMEventListener<PlayerPositionEvent>
    {
        public Light digSpotlight;
        public float spotlightStrengthenDepth = 2f;

        public float initialSpotlightAngle = 30f;
        public float initialSpotlightIntensity;

        public float increasedSpotlightAngle = 45f;
        public float increasedSpotlightIntensity = 1.5f;

        [SerializeField] private MMFeedbacks turnOnFeedback;
        [SerializeField] private MMFeedbacks turnOffFeedback;

        private bool _isSpotlightEnabled;


        private void Update()
        {
            // UpdateLight();
            if (InputService.IsToggleLightPressed())
            {
                _isSpotlightEnabled = !_isSpotlightEnabled;
                UpdateLight();
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(PlayerPositionEvent eventType)
        {
            if (eventType.EventType == PlayerPositionEventType.ReportDepth)
            {
            }
        }

        private void UpdateLight()
        {
            if (digSpotlight != null)
            {
                if (_isSpotlightEnabled)
                {
                    digSpotlight.spotAngle = increasedSpotlightAngle;
                    digSpotlight.intensity = increasedSpotlightIntensity;
                }
                else
                {
                    digSpotlight.spotAngle = initialSpotlightAngle;
                    digSpotlight.intensity = initialSpotlightIntensity;
                }
            }
        }
    }
}