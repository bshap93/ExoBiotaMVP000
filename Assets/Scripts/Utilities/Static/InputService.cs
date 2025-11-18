using System;

namespace Static
{
    public static class InputService
    {
        public static bool IsInteractPressed()
        {
            throw new NotImplementedException("InputService.IsInteractPressed is not implemented yet.");
        }

        public static bool IsChangingTools()
        {
            return false;
        }

        public static bool IsPausePressed()
        {
            return false;
        }

        public static int GetWeaponChangeDirection()
        {
            return 0;
        }

        public static float GetLookX()
        {
            return 0f;
        }

        public static float GetLookY()
        {
            return 0f;
        }

        public static bool IsApplyEffectHeld()
        {
            return false;
        }

        public static bool IsToggleLightPressed()
        {
            return false;
        }
    }
}