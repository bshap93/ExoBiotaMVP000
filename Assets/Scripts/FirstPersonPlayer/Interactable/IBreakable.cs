using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public interface IBreakable
    {
        bool CanBeDamagedBy(int toolPower);
        void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal);
    }
}
