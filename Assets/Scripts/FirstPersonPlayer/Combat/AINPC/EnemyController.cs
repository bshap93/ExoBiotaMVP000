using System;
using AINPC.ScriptableObjects;
using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events.Combat;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(EnemyBlackboardSync))]
    [DisallowMultipleComponent]
    public class EnemyController : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;
        public float currentHealth;
        public float maxHealth;

        // TODO : Replace with ScriptableObject reference
        public EnemyType enemyType;

        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits
        [SerializeField] float hitActiveDuration = 0.2f; // active hit window


        [SerializeField] EnemyHitbox hitBoxColliderMouth;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] AnimancerComponent animancerComponent;
        [SerializeField] Blackboard blackboard;


        AnimancerState attackState;

        AnimancerState idleState;
        AnimancerState moveState;
        public bool IsAttacking { get; private set; }

        void Awake()
        {
            // Pre-load looping animation states
            idleState = animancerComponent.States.GetOrCreate(enemyType.animationSet.idleAnimation);
            idleState.Speed = 1f;
            idleState.Time = 0f;
            idleState.Events(this).OnEnd = () => { idleState.Time = 0f; };

            moveState = animancerComponent.States.GetOrCreate(enemyType.animationSet.moveAnimation);
            moveState.Speed = 1f;
            moveState.Time = 0f;
            moveState.Events(this).OnEnd = () => { moveState.Time = 0f; };
        }
        void Update()
        {
            if (IsAttacking) return;

            var speed = navMeshAgent.velocity.magnitude;

            if (speed < 0.1f)
            {
                if (!idleState.IsPlaying) animancerComponent.Play(idleState);
            }
            else
            {
                if (!moveState.IsPlaying) animancerComponent.Play(moveState);
            }
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

        public void StartAttack()
        {
            if (IsAttacking) return;

            IsAttacking = true;

            hitBoxColliderMouth.Activate();


            attackState = animancerComponent.Play(enemyType.animationSet.attackAnimation);


            attackState.Events(this).OnEnd = () => { FinishAttack(); };
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
                        NPCAttackEvent.Trigger(enemyType.attacksProfile.primaryAttack);
                        break;
                    case AttackUsed.Secondary:
                        NPCAttackEvent.Trigger(enemyType.attacksProfile.secondaryAttack);
                        break;
                }
        }
        public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            var damageAmount = playerAttack.rawDamage;
            var attackType = playerAttack.attackType;

            if (attackType == PlayerAttackType.Melee)
            {
                // Placeholder for strength stat for player
                var playerStrength = 1;
                // Provisional damage scaling based on player strength
                var playerStrengthMultiplier = 1f + (playerStrength - 1) * 0.1f;
                damageAmount *= playerStrengthMultiplier;
            }

            currentHealth -= damageAmount;
            Debug.Log("Enemy took " + damageAmount + " damage. Current health: " + currentHealth);
        }
    }
}
