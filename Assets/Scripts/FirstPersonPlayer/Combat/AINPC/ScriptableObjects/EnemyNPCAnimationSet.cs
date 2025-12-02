using UnityEngine;

namespace AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyNPCAnimationSet",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy NPC Animation Set",
        order = 0)]
    public class EnemyNPCAnimationSet : ScriptableObject
    {
        public AnimationClip idleAnimation;
        public AnimationClip secondaryIdleAnimation;
        public AnimationClip moveAnimation;
        public AnimationClip attackAnimation;
    }
}
