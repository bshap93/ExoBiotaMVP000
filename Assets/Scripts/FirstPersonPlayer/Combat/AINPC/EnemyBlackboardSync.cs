using FirstPersonPlayer.Combat.AINPC.Creatures;
using NodeCanvas.Framework;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class EnemyBlackboardSync : MonoBehaviour
    {
        Blackboard _bb;
        EnemyController _enemy;

        void Awake()
        {
            _enemy = GetComponent<EnemyController>();
            _bb = GetComponent<Blackboard>();
        }

        void Update()
        {
            // Keep blackboard values synchronized
            _bb.SetVariableValue("currentHealth", _enemy.currentHealth);
            _bb.SetVariableValue("maxHealth", _enemy.MaxHealth);
        }
    }
}
