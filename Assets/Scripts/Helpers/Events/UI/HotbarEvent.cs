using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    public struct HotbarEvent
    {
        static HotbarEvent _e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(_e);
        }
    }
}
