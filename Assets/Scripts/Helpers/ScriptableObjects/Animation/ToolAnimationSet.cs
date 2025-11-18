using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Animation
{
    [CreateAssetMenu(
        fileName = "ToolAnimationSet", menuName = "Scriptable Objects/Animation/ToolAnimationSet")]
    public class ToolAnimationSet : ScriptableObject
    {
        [FormerlySerializedAs("IdleAnimation")]
        public AnimationClip idleAnimation;
        public AnimationClip idleBoredAnimation;

        public AudioClip idleBoredAudioClip;

        [FormerlySerializedAs("WalkAnimation")]
        public AnimationClip walkAnimation;
        [FormerlySerializedAs("RunAnimation")] public AnimationClip runAnimation;

        [Header("Tools with Use Pattern like Sampler")] [FormerlySerializedAs("BeginUseAnimation")]
        public AnimationClip beginUseAnimation;
        [FormerlySerializedAs("DuringUseAnimationLoopable")]
        public AnimationClip duringUseAnimationLoopable;
        [FormerlySerializedAs("EndUseAnimation")]
        public AnimationClip endUseAnimation;

        [Header("Audio for Tools with Use Pattern like Sampler")]
        public AudioClip beginUseAudioClip;
        public AudioClip duringUseLoopAudioClip;
        public AudioClip endUseAudioClip;

        [Header("Tools with Use Pattern like Hatchet")]
        public AnimationClip swing01Animation;
        public AnimationClip swing02Animation;
        public AnimationClip swing03Animation;
        public AnimationClip heavySwingAnimation;

        [Header("Audio for Tools with Use Pattern like Hatchet")]
        public AudioClip swing01AudioClip;
        public AudioClip swing02AudioClip;
        public AudioClip swing03AudioClip;
        public AudioClip heavySwingAudioClip;


        [Header("Equip / Unequip Animations")] public AnimationClip pullOutAnimation;
        public AnimationClip putAwayAnimation;

        [Header("Audio for Equip / Unequip")] public AudioClip pullOutAudioClip;
        public AudioClip putAwayAudioClip;
    }
}
