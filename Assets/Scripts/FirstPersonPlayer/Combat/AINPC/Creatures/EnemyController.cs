using Animancer;
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

                if (playerAttack.damageType == AttackDamageType.BasicHit)
                    meleeHitFeedbacksBasic?.PlayFeedbacks();
                else if (playerAttack.damageType == AttackDamageType.HeavyHit) meleeHitFeedbacksHeavy?.PlayFeedbacks();
            }

            var eventType = isCriticalHit
                ? DamageEventType.CriticalHitDamage
                : DamageEventType.DealtDamage;

            EnemyDamageEvent.Trigger(
                currentHealth - damageAmount, currentHealth, maxHealth,
                eventType, creatureType.creatureName);

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
    }
}
