using System;
using AINPC;
using AINPC.ScriptableObjects;
using Animancer;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events.Combat;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class EnemyController : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;
        public float currentHealth;
        public float maxHealth;
        public string enemyName;

        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits
        [SerializeField] float hitActiveDuration = 0.2f; // active hit window


        [SerializeField] EnemyHitbox hitBoxColliderMouth;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] AnimancerComponent animancerComponent;
        [SerializeField] Blackboard blackboard;

        [SerializeField] EnemyAttacksProfile attacksProfile;

        [SerializeField] EnemyNPCAnimationSet animationSet;
        AnimancerState attackState;

        AnimancerState idleState;
        AnimancerState moveState;
        public bool IsAttacking { get; private set; }

        void Awake()
        {
            // Pre-load looping animation states
            idleState = animancerComponent.States.GetOrCreate(animationSet.idleAnimation);
            idleState.Speed = 1f;
            idleState.Time = 0f;
            idleState.Events(this).OnEnd = () => { idleState.Time = 0f; };

            moveState = animancerComponent.States.GetOrCreate(animationSet.moveAnimation);
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


            attackState = animancerComponent.Play(animationSet.attackAnimation);


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
                        NPCAttackEvent.Trigger(attacksProfile.primaryAttack);
                        break;
                    case AttackUsed.Secondary:
                        NPCAttackEvent.Trigger(attacksProfile.secondaryAttack);
                        break;
                }
        }
        public object ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            throw new NotImplementedException();
        }
    }
}
