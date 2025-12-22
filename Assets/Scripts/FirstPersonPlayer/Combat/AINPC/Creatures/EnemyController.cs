using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(EnemyBlackboardSync))]
    [DisallowMultipleComponent]
    public class EnemyController : CreatureController, IRequiresUniqueID, IDamageable
    {
        // public float currentHealth;
        // public float maxHealth;

        public bool destroyAfterDeath = true;

        public bool isDead;

        public string blackboardWasHitKey = "wasHit";


        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits
        [SerializeField] float hitActiveDuration = 0.2f; // active hit window


        [SerializeField] EnemyHitbox hitBoxColliderMouth;

        [SerializeField] NavMeshAgent navMeshAgent;
        // [SerializeField] HighlightEffect highlightEffect;

        // [Header("Feedbacks")] [SerializeField] MMFeedbacks meleeHitFeedbacksBasic;
        // [SerializeField] MMFeedbacks meleeHitFeedbacksHeavy;
        // [SerializeField] MMFeedbacks critDamageFeedbacks;

        // [SerializeField] MMFeedbacks rangedHitFeedbacksBasic;
        // [SerializeField] MMFeedbacks rangedHitFeedbacksHeavy;

        [SerializeField] GameObject deathParticlesPrefab;
        // [SerializeField] MMFeedbacks deathFeedbacks;

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


        public virtual void OnDeath()
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

        // public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        // {
        //     if (killPrevious) _hitTween?.Kill();
        //     _hitTween = buildTween(transform);
        // }


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
    }
}
