using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyType",
        menuName = "Scriptable Objects/Character/Creature NPC/Creature Type",
        order = 0)]
    public class CreatureType : ScriptableObject
    {
        public CreatureAttacksProfile attacksProfile;
        public CreatureAnimationSet animationSet;
        [FormerlySerializedAs("enemyName")] public string creatureName;
        [FormerlySerializedAs("vfxSet")] public CreatureEffectsAndFeedbacks effectsAndFeedbacks;
    }
}
