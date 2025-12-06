using System;
using Animancer;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Interface;

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

        AnimancerState idleState;
        AnimancerState moveState;
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
