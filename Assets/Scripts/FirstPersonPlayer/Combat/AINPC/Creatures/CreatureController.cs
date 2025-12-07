using System;
using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using NodeCanvas.Framework;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [DisallowMultipleComponent]
    public abstract class CreatureController : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;

        [SerializeField] protected Blackboard blackboard;
        [SerializeField] protected AnimancerComponent animancerComponent;
        [SerializeField] protected CreatureType creatureType;

        protected AnimancerState IdleState;
        protected AnimancerState MoveState;
        protected virtual void Awake()
        {
            // Pre-load looping animation states
            IdleState = animancerComponent.States.GetOrCreate(creatureType.animationSet.idleAnimation);
            IdleState.Speed = 1f;
            IdleState.Time = 0f;
            IdleState.Events(this).OnEnd = () => { IdleState.Time = 0f; };

            MoveState = animancerComponent.States.GetOrCreate(creatureType.animationSet.moveAnimation);
            MoveState.Speed = 1f;
            MoveState.Time = 0f;
            MoveState.Events(this).OnEnd = () => { MoveState.Time = 0f; };
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

        public CreatureEffectsAndFeedbacks GetEffectsAndFeedbacks()
        {
            return creatureType.effectsAndFeedbacks;
        }
    }
}
