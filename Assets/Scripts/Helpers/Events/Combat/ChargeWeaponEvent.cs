using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum ChargeWeaponEventType
    {
        StartCharging,
        CancelCharging,
        ChargeRelease,
        UpdateCharge
    }

    public struct ChargeWeaponEvent
    {
        static ChargeWeaponEvent _e;
        public float ChargeAmount;
        public ChargeWeaponEventType EventType;
        public static void Trigger(float charge01, ChargeWeaponEventType chargeWeaponEventType)
        {
            _e.ChargeAmount = charge01;
            _e.EventType = chargeWeaponEventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
