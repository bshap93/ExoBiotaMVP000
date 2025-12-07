using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyNPCAnimationSet",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy NPC Animation Set",
        order = 0)]
    public class CreatureAnimationSet : ScriptableObject
    {
        public AnimationClip idleAnimation;
        public AnimationClip secondaryIdleAnimation;
        public AnimationClip moveAnimation;
        public AnimationClip attackAnimation;
        public AnimationClip deathAnimation;
    }
}
