using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events.Combat;
using HighlightPlus;
using Manager;
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

        public bool isDead;


        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits
        [SerializeField] float hitActiveDuration = 0.2f; // active hit window


        [SerializeField] EnemyHitbox hitBoxColliderMouth;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] HighlightEffect highlightEffect;

        [SerializeField] MMFeedbacks meleeHitFeedbacksBasic;
        [SerializeField] MMFeedbacks meleeHitFeedbacksHeavy;

        protected AnimancerState AttackState;

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
                animancerComponent.Play(creatureType.animationSet.deathAnimation);
                navMeshAgent.isStopped = true;
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

            if (attackType == PlayerAttackType.Melee)
            {
                // Placeholder for strength stat for player
                var playerStrength = attributeManager.Strength;
                // Provisional damage scaling based on player strength
                var playerStrengthMultiplier = 1f + (playerStrength - 1) * 0.5f;
                damageAmount *= playerStrengthMultiplier;

                if (playerAttack.damageType == AttackDamageType.BasicHit)
                    meleeHitFeedbacksBasic?.PlayFeedbacks();
                else if (playerAttack.damageType == AttackDamageType.HeavyHit) meleeHitFeedbacksHeavy?.PlayFeedbacks();
            }

            currentHealth -= damageAmount;
            highlightEffect.HitFX();
            Debug.Log("Enemy took " + damageAmount + " damage. Current health: " + currentHealth);
        }
    }
}
