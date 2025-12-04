using MoreMountains.Tools;
using SharedUI.Progression;

namespace Helpers.Events
{
    public struct AttributeLevelUpEvent
    {
        static AttributeLevelUpEvent _e;
        public AttributeType AttributeType;
        public int CurrentLevel;
        public static void Trigger(AttributeType attributeType, int currentLevel)
        {
            _e.AttributeType = attributeType;
            _e.CurrentLevel = currentLevel;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
