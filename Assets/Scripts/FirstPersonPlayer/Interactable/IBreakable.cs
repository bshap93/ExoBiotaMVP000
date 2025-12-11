using System.Collections;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public interface IBreakable
    {
        bool CanBeDamagedBy(int toolPower, int strength);
        void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal);
        IEnumerator InitializeAfterDestructableManager();
    }
}
