using UnityEngine;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class FPConsumableHotbar : MonoBehaviour
    {
        [SerializeField] int hotbarSize = 2;
        [SerializeField] HotbarUISlot[] consumableSlots;
        
    }
}
