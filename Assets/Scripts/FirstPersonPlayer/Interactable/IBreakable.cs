using System.Collections;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public interface IBreakable
    {
        bool CanBeDamagedBy(int toolPower, int strength);
        void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal,
            MeleeToolPrefab.HitType hitType = MeleeToolPrefab.HitType.Normal, PlayerToolAttack attack = null);
        IEnumerator InitializeAfterDestructableManager();
        void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal);
    }
}
