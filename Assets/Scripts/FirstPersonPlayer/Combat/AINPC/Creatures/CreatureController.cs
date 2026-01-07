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
        public enum CreatureState
        {
            Normal,
            Stunned,
            Dead
        }

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

        [SerializeField] protected float secondsBeforeSettingShouldBeDestroyed = 5f;

        [SerializeField] protected bool appearsOnlyOnce;

        [SerializeField] bool doesNotImmediatelyNeedToMove;
        public float currentHealth;
        public bool isStunned;

        public float currentStunDamage;


        [SerializeField] protected HighlightEffect highlightEffect;

        Tween _hitTween;

        Coroutine _stunDecayCoroutine;

        protected AnimancerState IdleState;
        protected AnimancerState MoveState;

        public CreatureState CurrentCreatureState
        {
            get
            {
                if (currentHealth <= 0f) return CreatureState.Dead;
                if (isStunned) return CreatureState.Stunned;
                return CreatureState.Normal;
            }
        }

        public float StunDuration => creatureType.stunCooldownTime;
        public bool IsPlayingCustomAnimation { get; set; }

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

            if (currentStunDamage >= StunThreshold) currentStunDamage = StunThreshold;

            // Check if creature is now stunned
            if (!isStunned && currentStunDamage >= StunThreshold)
            {
                isStunned = true;
                blackboard.SetVariableValue("isStunned", true);
                blackboard.SetVariableValue("wasStunnedAtThreshold", true);
                Debug.Log(creatureType.creatureName + " is now stunned!");

                // Start the stun decay coroutine
                if (_stunDecayCoroutine != null) StopCoroutine(_stunDecayCoroutine);

                _stunDecayCoroutine = StartCoroutine(StunDecayCoroutine());
            }
            else if (isStunned)
            {
                // If already stunned and hit again, stun damage increases (capped), extending stun duration
                Debug.Log(creatureType.creatureName + " stun duration extended!");
            }

            highlightEffect.HitFX();
        }

        IEnumerator StunDecayCoroutine()
        {
            Debug.Log(creatureType.creatureName + " stun decay started");

            // Calculate decay rate: stun threshold should decay to 0 over the stun cooldown time
            var decayRate = StunThreshold / creatureType.stunCooldownTime;

            var timeSinceLastUpdate = 0f;
            var uiUpdateInterval = 0.5f; // Only update UI every 0.1 seconds

            while (currentStunDamage > 0f)
            {
                yield return null; // Still run every frame

                // Decrease damage every frame (smooth)
                currentStunDamage -= decayRate * Time.deltaTime;

                blackboard.SetVariableValue("stunDamage", currentStunDamage);

                // Accumulate time
                timeSinceLastUpdate += Time.deltaTime;

                // Only trigger event periodically OR when reaching zero
                if (timeSinceLastUpdate >= uiUpdateInterval || currentStunDamage <= 0f)
                {
                    EnemyDamageEvent.Trigger(
                        currentStunDamage, currentStunDamage + decayRate * Time.deltaTime, StunThreshold,
                        DamageEventType.DealtDamage, creatureType.creatureName, DamageType.Stun);


                    timeSinceLastUpdate = 0f;
                }
            }

            while (currentStunDamage > 0f)
            {
                yield return null;


                // Clamp to prevent going below zero
                if (currentStunDamage < 0f) currentStunDamage = 0f;
            }

            // Stun damage has reached zero, unstun the creature
            if (isStunned) isStunned = false;
            // blackboard.SetVariableValue("isStunned", false);
            _stunDecayCoroutine = null;
        }

        public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        {
            if (killPrevious) _hitTween?.Kill();
            _hitTween = buildTween(transform);
        }

        public void PlayAnimationClip(AnimationClip clip)
        {
            IsPlayingCustomAnimation = true;
            var state = animancerComponent.Play(clip, 0.2f);
            state.Events(this).OnEnd = () => { IsPlayingCustomAnimation = false; };
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
            // Stop the decay coroutine if it's running
            if (_stunDecayCoroutine != null)
            {
                StopCoroutine(_stunDecayCoroutine);
                _stunDecayCoroutine = null;
            }

            isStunned = false;
            currentStunDamage = 0f;
            blackboard.SetVariableValue("isStunned", false);
            Debug.Log(creatureType.creatureName + " stun state reset (manually)");
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
