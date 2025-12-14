using Helpers.Events;
using Lightbug.CharacterControllerPro.Core;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.Animation
{
    public class ToolBob : MonoBehaviour, MMEventListener<LoadedManagerEvent>
    {
        public float swayAmount = 0.01f;
        public float swaySpeed = 6f;
        public float bumpStrength = 0.015f;

        [FormerlySerializedAs("_fpPlayerCharacter")] [SerializeField]
        CharacterActor fpPlayerCharacter;

        MMSpringFloat _bobSpring;
        Vector3 _initialLocalPosition;

        void Update()
        {
            if (fpPlayerCharacter == null) return;

            var velocity = fpPlayerCharacter.PlanarVelocity.magnitude;

            // Add bump (optional: replace with footstep trigger)
            if (velocity > 0.1f && Mathf.FloorToInt(Time.time * swaySpeed) % 2 == 0) _bobSpring.Bump(bumpStrength);

            _bobSpring.UpdateSpringValue(Time.deltaTime);

            // Only sway when moving
            var sway = velocity > 0.1f ? Mathf.Sin(Time.time * swaySpeed) * swayAmount : 0f;

            var offset = new Vector3(0f, sway + _bobSpring.CurrentValue, 0f);
            transform.localPosition = _initialLocalPosition + offset;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
                Initialize();
        }

        void Initialize()
        {
            _initialLocalPosition = transform.localPosition;

            if (fpPlayerCharacter == null)
                fpPlayerCharacter = FindFirstObjectByType<CharacterActor>();

            if (fpPlayerCharacter == null)
            {
                Debug.LogWarning("ToolBob: CharacterActor not found in parent hierarchy.");
                return;
            }


            _bobSpring = new MMSpringFloat
            {
                Damping = 0.4f,
                Frequency = 5f
            };

            _bobSpring.SetInitialValue(0f);
        }
    }
}
