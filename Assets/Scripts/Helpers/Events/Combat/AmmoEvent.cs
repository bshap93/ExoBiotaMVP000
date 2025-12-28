using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct AmmoEvent
    {
        static AmmoEvent _e;

        public enum AmmoEventType
        {
            AddAmmoItem,
            SpendAmountFromAmmoItem,
            RestoreAmountToAmmoItem,
            RemoveAmmoItem
        }

        public AmmoItem.AmmoType AmmoType;

        public AmmoEventType EventType;

        public float ChargeAmount;

        public int RoundsAmount;

        public static AmmoEvent Trigger(
            AmmoEventType eventType,
            AmmoItem.AmmoType ammoType,
            float chargeAmount = 0f,
            int roundsAmount = 0)
        {
            _e.EventType = eventType;
            _e.AmmoType = ammoType;
            _e.ChargeAmount = chargeAmount;
            _e.RoundsAmount = roundsAmount;
            MMEventManager.TriggerEvent(_e);
            return _e;
        }
    }
}
