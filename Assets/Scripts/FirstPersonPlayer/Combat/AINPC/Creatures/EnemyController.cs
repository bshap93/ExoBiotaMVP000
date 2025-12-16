using System;
using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using HighlightPlus;
using Manager;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(EnemyBlackboardSync))]
    [DisallowMultipleComponent]
    public class EnemyController : CreatureController, IRequiresUniqueID
    {
        public float currentHealth;
        public float maxHealth;

        public bool destroyAfterDeath = true;

        public bool isDead;

        public string blackboardWasHitKey = "wasHit";


        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits
        [SerializeField] float hitActiveDuration = 0.2f; // active hit window


        [SerializeField] EnemyHitbox hitBoxColliderMouth;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] HighlightEffect highlightEffect;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks meleeHitFeedbacksBasic;
        [SerializeField] MMFeedbacks meleeHitFeedbacksHeavy;
        [SerializeField] MMFeedbacks critDamageFeedbacks;

        [SerializeField] GameObject deathParticlesPrefab;
        [SerializeField] MMFeedbacks deathFeedbacks;

        [SerializeField] MMFeedbacks movementLoopFeedbacks;

        Tween _hitTween;

        protected AnimancerState AttackState;
        protected AnimancerState DeathState;
        protected AnimancerState HitState;

        public bool IsAttacking { get; private set; }


        void Update()
        {
            if (IsAttacking) return;

            var speed = navMeshAgent.velocity.magnitude;

            if (speed < 0.1f)
            {
                if (!IdleState.IsPlaying) animancerComponent.Play(IdleState);
            }
            else
            {
                if (!MoveState.IsPlaying) animancerComponent.Play(MoveState);
            }

            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                DeathState = animancerComponent.Play(creatureType.animationSet.deathAnimation);
                DeathState.Events(this).OnEnd = () => { Destroy(gameObject); };

                OnDeath();
            }
        }

        void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        {
            if (killPrevious) _hitTween?.Kill();
            _hitTween = buildTween(transform);
        }


        public void StartAttack()
        {
            if (IsAttacking) return;

            IsAttacking = true;

            hitBoxColliderMouth.Activate();


            AttackState = animancerComponent.Play(creatureType.animationSet.attackAnimation);


            AttackState.Events(this).OnEnd = () => { FinishAttack(); };
        }

        public void FinishAttack()
        {
            IsAttacking = false;
            hitBoxColliderMouth.Deactivate();
        }
        public void OnHitPlayer(Collider other, AttackUsed attackUsed)
        {
            if (other.CompareTag("FirstPersonPlayer"))
                switch (attackUsed)
                {
                    case AttackUsed.Primary:
                        NPCAttackEvent.Trigger(creatureType.attacksProfile.primaryAttack);
                        break;
                    case AttackUsed.Secondary:
                        NPCAttackEvent.Trigger(creatureType.attacksProfile.secondaryAttack);
                        break;
                }
        }
        IEnumerator CooldownAfterWasHit()
        {
            // blackboard.SetVariableValue(blackboardWasHitKey, true);
            yield return new WaitForSeconds(creatureType.wasHitCooldownTime);
            blackboard.SetVariableValue(blackboardWasHitKey, false);
        }
        public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            var attributeManager = AttributesManager.Instance;
            var damageAmount = playerAttack.rawDamage;
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
                    critDamageFeedbacks?.PlayFeedbacks();
                }

                damageAmount *= playerStrengthMultiplier;

                blackboard.SetVariableValue("wasHit", true);
                // StartCoroutine(CooldownAfterWasHit());

                if (playerAttack.damageType == AttackDamageType.BasicHit)
                {
                    meleeHitFeedbacksBasic?.PlayFeedbacks();
                    PlayHitTween(t => t.DOPunchPosition(
                        new Vector3(creatureType.meleeAttackShakeIntensity, 0f, creatureType.meleeAttackShakeIntensity),
                        creatureType.meleeAttackShakeDuration));
                }
                else if (playerAttack.damageType == AttackDamageType.HeavyHit)
                {
                    meleeHitFeedbacksHeavy?.PlayFeedbacks();
                    PlayHitTween(t => t.DOShakePosition(
                        creatureType.meleeAttackShakeDuration,
                        new Vector3(
                            creatureType.heavyMeleeAttackShakeIntensity, 0f,
                            creatureType.heavyMeleeAttackShakeIntensity))); // or still punch
                }
            }

            var eventType = isCriticalHit
                ? DamageEventType.CriticalHitDamage
                : DamageEventType.DealtDamage;

            EnemyDamageEvent.Trigger(
                currentHealth - damageAmount, currentHealth, maxHealth,
                eventType, creatureType.creatureName);

            blackboard.SetVariableValue(blackboardWasHitKey, true);

            currentHealth -= damageAmount;
            highlightEffect.HitFX();
            Debug.Log("Enemy took " + damageAmount + " damage. Current health: " + currentHealth);
        }

        protected virtual void OnDeath()
        {
            navMeshAgent.isStopped = true;
            CreatureStateEvent.Trigger(
                CreatureStateEventType.SetNewCreatureState, uniqueID,
                CreatureStateManager.CreatureState.ShouldBeDestroyed);

            movementLoopFeedbacks?.StopFeedbacks();

            EnemyDamageEvent.Trigger(
                0f, currentHealth, maxHealth,
                DamageEventType.Death, creatureType.creatureName);

            deathFeedbacks?.PlayFeedbacks();
            if (deathParticlesPrefab != null)
                Instantiate(deathParticlesPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            if (destroyAfterDeath)
                Destroy(gameObject, 2f);
        }
        public void PlayHitAnimation(AnimationClip value)
        {
            HitState = animancerComponent.Play(value);

            HitState.Events(this).OnEnd = () => { };
        }
    }
}
