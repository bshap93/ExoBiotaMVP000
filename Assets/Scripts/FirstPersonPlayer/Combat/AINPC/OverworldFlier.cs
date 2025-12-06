using System;
using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.Combat.AINPC
{
    [RequireComponent(typeof(Blackboard))]
    [RequireComponent(typeof(AnimancerComponent))]
    [DisallowMultipleComponent]
    public class OverworldFlierController : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] AnimancerComponent animancerComponent;
        [SerializeField] Blackboard blackboard;

        public EnemyType enemyType;

        [SerializeField] MMFeedbacks creatureCallFeedbacks;

        [SerializeField] float minCallDelay = 10f;
        [SerializeField] float maxCallDelay = 15f;

        AnimancerState _idleState;
        AnimancerState _moveState;

        float _nextCallTime;

        void Awake()
        {
            // Pre-load looping animation states
            _idleState = animancerComponent.States.GetOrCreate(enemyType.animationSet.idleAnimation);
            _idleState.Speed = 1f;
            _idleState.Time = 0f;
            _idleState.Events(this).OnEnd = () => { _idleState.Time = 0f; };

            _moveState = animancerComponent.States.GetOrCreate(enemyType.animationSet.moveAnimation);
            _moveState.Speed = 1f;
            _moveState.Time = 0f;
            _moveState.Events(this).OnEnd = () => { _moveState.Time = 0f; };
        }

        void Start()
        {
            // Screech immediately on spawn
            creatureCallFeedbacks?.PlayFeedbacks();
            ScheduleNextCall();
        }

        void Update()
        {
            if (!_idleState.IsPlaying)
                animancerComponent.Play(_idleState);

            if (Time.time >= _nextCallTime)
            {
                creatureCallFeedbacks?.PlayFeedbacks();
                ScheduleNextCall();
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

        void ScheduleNextCall()
        {
            _nextCallTime = Time.time + Random.Range(minCallDelay, maxCallDelay);
        }
    }
}
