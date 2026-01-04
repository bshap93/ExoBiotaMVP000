using System;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    [Serializable]
    public enum PlayerAttackType
    {
        Melee,
        Ranged
    }


    [CreateAssetMenu(
        fileName = "PlayerToolAttackProfile",
        menuName = "Scriptable Objects/Character/First Person Player/Player Tool Attack",
        order = 0)]
    public class PlayerToolAttack : ScriptableObject
    {
        public string displayName;
        public float baseBlowbackContaminationMultiplier = 1f;
        public float rawDamage;
        public bool causesStunDamage;
        [ShowIf("causesStunDamage")] public float rawStunDamage;
        public float baseEnergyCost;
        [Range(0f, 1f)] public float critChance;
        public float critMultiplier = 1.2f;
        public MeleeToolPrefab.HitType damageType;
        public float rawKnockbackForce;
        public bool causesBleeding;
        [ShowIf("causesBleeding")] [Range(0f, 1f)]
        public float chanceToCauseBleeding;
        public bool causesStagger;
        [ShowIf("causesStagger")] [Range(0f, 1f)]
        public float chanceToCauseStagger;

        public PlayerAttackType attackType;
        public string AttackID => name;
    }
}
