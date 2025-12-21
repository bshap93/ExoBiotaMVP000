using FirstPersonPlayer.Combat.AINPC.Creatures;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class CessileCreatureBBSync : MonoBehaviour
    {
        Blackboard _bb;
        CessileGasCreatureController _creatureController;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _creatureController = GetComponent<CessileGasCreatureController>();
            _bb = GetComponent<Blackboard>();
        }

        void Start()
        {
            _bb.SetVariableValue("maxHealth", _creatureController.maxHealth);
            _bb.SetVariableValue("detectionRadius", _creatureController.detectionRadius);
        }

        // Update is called once per frame
        void Update()
        {
            // Keep blackboard values synchronized
            _bb.SetVariableValue("currentHealth", _creatureController.currentHealth);
        }
    }
}
