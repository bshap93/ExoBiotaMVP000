using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyVFXSet",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy VFX Set",
        order = 0)]
    public class CreatureEffectsAndFeedbacks : ScriptableObject
    {
        public GameObject basicHitVFX;
        public GameObject heavyHitVFX;

        public GameObject basicDeathFeedbacks;
        public GameObject basicHitFeedbacks;
        public GameObject heavyHitFeedbacks;
    }
}
