using FirstPersonPlayer.Combat.AINPC.Creatures;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class EnemyHitbox : MonoBehaviour
    {
        public EnemyController owner;
        [SerializeField] AttackUsed attackType;
        bool _active;
        Collider _collider;
        bool _hasHit;

        void Awake()
        {
            _collider = GetComponent<Collider>();
        }
        void OnTriggerEnter(Collider other)
        {
            if (!_active || _hasHit) return;

            if (other.CompareTag("FirstPersonPlayer"))
            {
                owner.OnHitPlayer(other, attackType);
                _hasHit = true;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (!_active || _hasHit) return;

            if (other.CompareTag("FirstPersonPlayer"))
            {
                owner.OnHitPlayer(other, attackType);
                _hasHit = true;
            }
        }

        public void Activate()
        {
            _active = true;
            _hasHit = false;
        }

        public void Deactivate()
        {
            _active = false;
            _hasHit = false;
        }
    }
}
