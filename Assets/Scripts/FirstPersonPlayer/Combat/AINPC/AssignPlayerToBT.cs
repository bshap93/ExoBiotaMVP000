using Lightbug.CharacterControllerPro.Core;
using Manager.Global;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class AssignPlayerToBT : MonoBehaviour
    {
        public Blackboard blackboard;

        void Start()
        {
            Invoke(nameof(Assign), 0.1f);
        }

        void Assign()
        {
            if (blackboard == null)
                blackboard = GetComponent<Blackboard>();


            // Get the top-level PlayerRoot
            var root = GameStateManager.Instance.PlayerRoot;
            if (root == null)
            {
                Debug.LogError("No PlayerRoot found!");
                return;
            }

            // Get the first active child (your actual moving pawn)
            Transform movingPawn = null;

            foreach (Transform child in root)
                if (child.gameObject.activeInHierarchy)
                {
                    movingPawn = child;
                    break;
                }

            if (movingPawn == null)
            {
                Debug.LogError("No moving player pawn found under PlayerRoot.");
                return;
            }

            var capsuleScaler = movingPawn.GetComponentInChildren<CharacterGraphicsScaler>();

            // Assign THIS instead of the root
            blackboard.SetVariableValue("playerTransform", movingPawn);
            blackboard.SetVariableValue("capsule", capsuleScaler.gameObject);

            Debug.Log("Assigned moving player pawn: " + movingPawn.name);
        }
    }
}
