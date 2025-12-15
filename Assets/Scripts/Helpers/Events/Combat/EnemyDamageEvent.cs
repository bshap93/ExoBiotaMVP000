using System;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    [Serializable]
    public enum DamageEventType
    {
        DealtDamage,
        CriticalHitDamage,
        Missed,
        Healed,
        Blocked,
        Death
    }


    public struct EnemyDamageEvent
    {
        static EnemyDamageEvent _e;

        public float CurrentHealth;
        public float LastHealth;
        public float MaxHealth;
        public DamageEventType EventType;
        public string EnemyName;

        public static void Trigger(float currentHealth, float lastHealth, float maxHealth, DamageEventType eventType,
            string enemyName)
        {
            _e.CurrentHealth = currentHealth;
            _e.MaxHealth = maxHealth;
            _e.EventType = eventType;
            _e.LastHealth = lastHealth;
            _e.EnemyName = enemyName;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
