using System;
using UnityEngine;

namespace AINPC.ScriptableObjects
{
    [Serializable]
    public enum NPCAttackType
    {
        Melee,
        Ranged,
        ContaminantPOE
    }

    [CreateAssetMenu(
        fileName = "EnemyAttack",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy Attack",
        order = 0)]
    public class EnemyAttack : ScriptableObject
    {
        public float rawDamage;
        // Amount that an attack ignores armor. 
        // [Range(0f, 1f)] public float armorPenetration;
        public float contaminationAmount;
        [Range(0f, 1f)] public float critChance;
        public float critMultiplier;
        public float knockbackForce;

        public NPCAttackType attackType;
    }
}
