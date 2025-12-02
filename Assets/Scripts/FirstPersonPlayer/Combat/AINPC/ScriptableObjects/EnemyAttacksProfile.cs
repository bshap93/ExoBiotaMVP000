using AINPC.ScriptableObjects;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    public enum AttackUsed
    {
        Primary,
        Secondary
    }

    [CreateAssetMenu(
        fileName = "EnemyAttacksProfile",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy Attacks Profile",
        order = 0)]
    public class EnemyAttacksProfile : ScriptableObject
    {
        public EnemyAttack primaryAttack;
        public EnemyAttack secondaryAttack;
    }
}
