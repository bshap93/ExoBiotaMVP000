using AINPC.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct NPCAttackEvent
    {
        static NPCAttackEvent _e;

        public EnemyAttack Attack;


        public static void Trigger(EnemyAttack attack)
        {
            _e.Attack = attack;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
