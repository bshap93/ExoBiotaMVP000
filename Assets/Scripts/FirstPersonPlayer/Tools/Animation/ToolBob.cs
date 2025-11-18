using Lightbug.CharacterControllerPro.Core;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.Animation
{
    public class ToolBob : MonoBehaviour
    {
        public float swayAmount = 0.01f;
        public float swaySpeed = 6f;
        public float bumpStrength = 0.015f;

        [FormerlySerializedAs("_fpPlayerCharacter")] [SerializeField]
        private CharacterActor fpPlayerCharacter;

        private MMSpringFloat _bobSpring;
        private Vector3 _initialLocalPosition;

        private void Update()
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

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            _initialLocalPosition = transform.localPosition;

            if (fpPlayerCharacter == null)
                fpPlayerCharacter = GetComponentInParent<CharacterActor>();

            if (fpPlayerCharacter == null)
            {
                Debug.LogError("ToolBob: CharacterActor not found in parent hierarchy.");
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