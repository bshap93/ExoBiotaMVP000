using System;
using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Helpers.Events.Status;
using HighlightPlus;
using Manager;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;
using Utilities.Interface;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [DisallowMultipleComponent]
    public abstract class CreatureController : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;

        [SerializeField] protected Blackboard blackboard;
        [SerializeField] protected AnimancerComponent animancerComponent;
        [SerializeField] public CreatureType creatureType;
        public CreatureStateManager.CreatureState initialCreatureState;
        [Header("Feedbacks")] [SerializeField] protected MMFeedbacks deathFeedbacks;

        [SerializeField] protected MMFeedbacks critDamageFeedbacks;
        [SerializeField] protected MMFeedbacks meleeHitFeedbacksBasic;
        [SerializeField] protected MMFeedbacks meleeHitFeedbacksHeavy;
        [SerializeField] protected MMFeedbacks rangedHitFeedbacksBasic;
        [SerializeField] protected MMFeedbacks rangedHitFeedbacksHeavy;


        // CreatureStateManager.CreatureState _currentCreatureState;

        [SerializeField] protected float secondsBeforeSettingShouldBeDestroyed = 5f;

        [SerializeField] protected bool appearsOnlyOnce;

        [SerializeField] bool doesNotImmediatelyNeedToMove;
        public float currentHealth;
        public bool isStunned;

        public float currentStunDamage;

        [SerializeField] protected HighlightEffect highlightEffect;

        Tween _hitTween;

        protected AnimancerState IdleState;
        protected AnimancerState MoveState;

        public float MaxHealth => creatureType.maxHealth;
        public float StunThreshold => creatureType.stunTreshold;

        protected virtual void Awake()
        {
            // Pre-load looping animation states
            IdleState = animancerComponent.States.GetOrCreate(creatureType.animationSet.idleAnimation);
            IdleState.Speed = 1f;
            IdleState.Time = 0f;
            IdleState.Events(this).OnEnd = () => { IdleState.Time = 0f; };

            if (doesNotImmediatelyNeedToMove)
                return;

            if (creatureType.animationSet.moveAnimation == null)
            {
                Debug.Log(
                    $"CreatureType {creatureType.name} does not have a move animation assigned in its AnimationSet.");

                return;
            }

            MoveState = animancerComponent.States.GetOrCreate(creatureType.animationSet.moveAnimation);
            MoveState.Speed = 1f;
            MoveState.Time = 0f;
            MoveState.Events(this).OnEnd = () => { MoveState.Time = 0f; };
        }

        protected virtual void Start()
        {
            StartCoroutine(InitializeAfterCreatureStateManager());
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
        public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            var attributeManager = AttributesManager.Instance;
            var damageAmount = playerAttack.rawDamage;
            var stunAmount = playerAttack.rawStunDamage;
            var attackType = playerAttack.attackType;

            var isCriticalHit = Random.value <= playerAttack.critChance;

            if (attackType == PlayerAttackType.Melee)
            {
                // Placeholder for strength stat for player
                var playerStrength = attributeManager.Strength;
                // Provisional damage scaling based on player strength
                var playerStrengthMultiplier = 1f + (playerStrength - 1) * 0.5f;
                if (isCriticalHit)
                {
                    damageAmount *= playerAttack.critMultiplier;
                    stunAmount *= playerAttack.critMultiplier;
                    critDamageFeedbacks?.PlayFeedbacks();
                }

                damageAmount *= playerStrengthMultiplier;
                stunAmount *= playerStrengthMultiplier;

                // Melee contamination
                var contaminationAmt = creatureType.baseBlowbackContaminationAmt *
                                       playerAttack.baseBlowbackContaminationMultiplier;

                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationAmt);

                // StartCoroutine(CooldownAfterWasHit());

                if (playerAttack.damageType == MeleeToolPrefab.HitType.Normal)
                {
                    meleeHitFeedbacksBasic?.PlayFeedbacks();
                    PlayHitTween(t => t.DOPunchPosition(
                        new Vector3(creatureType.meleeAttackShakeIntensity, 0f, creatureType.meleeAttackShakeIntensity),
                        creatureType.meleeAttackShakeDuration));

                    blackboard.SetVariableValue("wasHit", true);
                    Debug.Log("Normal Hit registered on " + creatureType.creatureName);
                }
                else if (playerAttack.damageType == MeleeToolPrefab.HitType.Heavy)
                {
                    meleeHitFeedbacksHeavy?.PlayFeedbacks();
                    blackboard.SetVariableValue("wasHitHeavy", true);
                    Debug.Log("Heavy Hit registered on " + creatureType.creatureName);
                    PlayHitTween(t => t.DOShakePosition(
                        creatureType.meleeAttackShakeDuration,
                        new Vector3(
                            creatureType.heavyMeleeAttackShakeIntensity, 0f,
                            creatureType.heavyMeleeAttackShakeIntensity))); // or still punch
                }
            }
            else if (attackType == PlayerAttackType.Ranged)
            {
                var playerDexterity = attributeManager.Dexterity;
                var playerDexterityMultiplier = 1f + (playerDexterity - 1) * 0.5f;
                if (isCriticalHit)
                {
                    damageAmount *= playerAttack.critMultiplier;
                    stunAmount *= playerAttack.critMultiplier;
                    critDamageFeedbacks?.PlayFeedbacks();
                }

                damageAmount *= playerDexterityMultiplier;
                stunAmount *= playerDexterityMultiplier;

                if (playerAttack.damageType == MeleeToolPrefab.HitType.Normal)
                {
                    rangedHitFeedbacksBasic?.PlayFeedbacks();
                    PlayHitTween(t => t.DOPunchPosition(
                        new Vector3(
                            creatureType.rangedAttackShakeIntensity, 0f, creatureType.rangedAttackShakeIntensity),
                        creatureType.rangedAttackShakeDuration));

                    blackboard.SetVariableValue("wasHit", true);
                    Debug.Log("Ranged Normal Hit registered on " + creatureType.creatureName);
                }
                else if (playerAttack.damageType == MeleeToolPrefab.HitType.Heavy)
                {
                    rangedHitFeedbacksHeavy?.PlayFeedbacks();
                    blackboard.SetVariableValue("wasHitHeavy", true);
                    Debug.Log("Ranged Heavy Hit registered on " + creatureType.creatureName);
                    PlayHitTween(t => t.DOShakePosition(
                        creatureType.rangedAttackShakeDuration,
                        new Vector3(
                            creatureType.heavyRangedAttackShakeIntensity, 0f,
                            creatureType.heavyRangedAttackShakeIntensity))); // or still punch
                }
            }

            var eventType = isCriticalHit
                ? DamageEventType.CriticalHitDamage
                : DamageEventType.DealtDamage;

            EnemyDamageEvent.Trigger(
                currentHealth - damageAmount, currentHealth, creatureType.maxHealth,
                eventType, creatureType.creatureName, DamageType.Health);

            EnemyDamageEvent.Trigger(
                currentStunDamage + stunAmount, currentStunDamage, creatureType.stunTreshold,
                DamageEventType.DealtDamage, creatureType.creatureName, DamageType.Stun);

            // blackboard.SetVariableValue(blackboardWasHitKey, true);
            // Debug.Log("Fallback hit registered on " + creatureType.creatureName);

            currentHealth -= damageAmount;
            currentStunDamage += stunAmount;

            // Check if creature is now stunned
            if (!isStunned && currentStunDamage >= StunThreshold)
            {
                isStunned = true;
                blackboard.SetVariableValue("isStunned", true);
                Debug.Log(creatureType.creatureName + " is now stunned!");
            }

            highlightEffect.HitFX();
        }

        public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        {
            if (killPrevious) _hitTween?.Kill();
            _hitTween = buildTween(transform);
        }

        protected virtual IEnumerator InitializeAfterCreatureStateManager()
        {
            yield return null;

            var creatureStateManager = CreatureStateManager.Instance;
            if (creatureStateManager != null)
            {
                var creatureState = creatureStateManager.GetCreatureState(uniqueID);

                if (creatureState == CreatureStateManager.CreatureState.None) creatureState = initialCreatureState;

                if (creatureState == CreatureStateManager.CreatureState.HasBeenInitialized)
                {
                    ReLoadCreatureStateData();
                }
                else if (creatureState == CreatureStateManager.CreatureState.ShouldBeDestroyed)
                {
                    Debug.Log("Destroying creature on load");
                    Destroy(gameObject);
                }
            }

            if (appearsOnlyOnce)
            {
                // Wait x seconds 
                yield return new WaitForSeconds(secondsBeforeSettingShouldBeDestroyed);
                CreatureStateEvent.Trigger(
                    CreatureStateEventType.SetNewCreatureState, uniqueID,
                    CreatureStateManager.CreatureState.ShouldBeDestroyed);
            }
        }

        public void ResetStunState()
        {
            isStunned = false;
            currentStunDamage = 0f;
            blackboard.SetVariableValue("isStunned", false);
            Debug.Log(creatureType.creatureName + " stun state reset");
        }


        public CreatureEffectsAndFeedbacks GetEffectsAndFeedbacks()
        {
            return creatureType.effectsAndFeedbacks;
        }

        protected virtual void ReLoadCreatureStateData()
        {
            Debug.Log("Loading creature state data");
        }
    }
}
