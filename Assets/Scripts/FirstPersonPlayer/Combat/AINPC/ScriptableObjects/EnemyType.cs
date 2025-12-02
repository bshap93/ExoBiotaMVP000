using AINPC.ScriptableObjects;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyType",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy Type",
        order = 0)]
    public class EnemyType : ScriptableObject
    {
        public EnemyAttacksProfile attacksProfile;
        public EnemyNPCAnimationSet animationSet;
        public string enemyName;
    }
}
