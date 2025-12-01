using AINPC.ScriptableObjects;
using Animancer;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace AINPC
{
    public class EnemyController : MonoBehaviour
    {
        public float currentHealth;
        public float maxHealth;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] AnimancerComponent animancerComponent;
        [SerializeField] Blackboard blackboard;

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

        public void StartAttack()
        {
            if (IsAttacking) return;

            IsAttacking = true;

            attackState = animancerComponent.Play(animationSet.attackAnimation);

            attackState.Events(this).OnEnd = () => { FinishAttack(); };

            // After animation finishes:
            // call FinishAttack(),
            // probably using Animancer events OR animation length timer.
        }

        public void FinishAttack()
        {
            IsAttacking = false;
        }
    }
}
