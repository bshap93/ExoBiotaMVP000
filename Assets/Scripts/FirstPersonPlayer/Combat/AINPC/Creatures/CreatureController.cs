using System;
using System.Collections;
using Animancer;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using Helpers.Events.NPCs;
using Manager.StateManager;
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
        public CreatureStateManager.CreatureState initialCreatureState;

        // CreatureStateManager.CreatureState _currentCreatureState;

        [SerializeField] protected float secondsBeforeSettingShouldBeDestroyed = 5f;

        [SerializeField] protected bool appearsOnlyOnce;

        [SerializeField] bool doesNotImmediatelyNeedToMove;

        protected AnimancerState IdleState;
        protected AnimancerState MoveState;

        protected virtual void Awake()
        {
            // Pre-load looping animation states
            IdleState = animancerComponent.States.GetOrCreate(creatureType.animationSet.idleAnimation);
            IdleState.Speed = 1f;
            IdleState.Time = 0f;
            IdleState.Events(this).OnEnd = () => { IdleState.Time = 0f; };

            if (doesNotImmediatelyNeedToMove)
                return;

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

        protected virtual IEnumerator InitializeAfterCreatureStateManager()
        {
            yield return null;

            var creatureStateManager = CreatureStateManager.Instance;
            if (creatureStateManager != null)
            {
                var creatureState = creatureStateManager.GetCreatureState(uniqueID);

                Debug.Log("Creating creature state " + creatureState);
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
