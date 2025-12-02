using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "PlayerToolAttackProfile",
        menuName = "Scriptable Objects/Character/First Person Player/Player Tool Attack Profile",
        order = 0)]
    public class PlayerToolAttackProfile : ScriptableObject
    {
        public PlayerToolAttack basicAttack;
        public PlayerToolAttack heavyAttack;
    }
}
