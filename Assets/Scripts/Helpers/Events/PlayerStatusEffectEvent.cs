using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct PlayerStatusEffectEvent
    {
        static PlayerStatusEffectEvent _e;

        public enum StatusEffectEventType
        {
            Apply,
            Remove,
            RemoveAllFromCatalog
        }

        public enum DirectionOfEvent
        {
            Inbound,
            Outbound
        }

        public StatusEffectEventType Type;

        public string EffectID;
        public string CatalogID;
        public DirectionOfEvent Direction;

        public static void Trigger(StatusEffectEventType type, string effectID, string catalogID,
            DirectionOfEvent direction)
        {
            _e.Type = type;
            _e.EffectID = effectID;
            _e.CatalogID = catalogID;
            _e.Direction = direction;


            MMEventManager.TriggerEvent(_e);
        }
    }
}
