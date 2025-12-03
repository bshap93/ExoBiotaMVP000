using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class AssignPatrolWaypointsToBT : MonoBehaviour
    {
        public Blackboard blackboard;

        public List<GameObject> patrolWaypoints;

        void Start()
        {
            Invoke(nameof(Assign), 0.1f);
        }


        void Assign()
        {
            if (blackboard == null)
                blackboard = GetComponent<Blackboard>();

            blackboard.SetVariableValue("patrolWaypoints", patrolWaypoints);
        }
    }
}
