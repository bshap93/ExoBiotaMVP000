using System;
using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    [Serializable]
    public enum PlayerAttackType
    {
        Melee,
        Ranged
    }

    [Serializable]
    public enum AttackDamageType
    {
        BasicHit,
        HeavyHit
    }

    [CreateAssetMenu(
        fileName = "PlayerToolAttackProfile",
        menuName = "Scriptable Objects/Character/First Person Player/Player Tool Attack",
        order = 0)]
    public class PlayerToolAttack : ScriptableObject
    {
        public string displayName;
        public float rawDamage;
        [Range(0f, 1f)] public float critChance;
        public float critMultiplier = 1.2f;
        public AttackDamageType damageType;
        public float rawKnockbackForce;
        public bool causesBleeding;
        // showif
        [Range(0f, 1f)] public float chanceToCauseBleeding;
        public bool causesStagger;
        // showif
        [Range(0f, 1f)] public float chanceToCauseStagger;

        public PlayerAttackType attackType;
        public string AttackID => name;
    }
}
