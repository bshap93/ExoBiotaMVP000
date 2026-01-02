using UnityEngine;

namespace SharedUI.Hotbar
{
    [DisallowMultipleComponent]
    public class FPToolHotbar : MonoBehaviour
    {
        // 6 including empty, 5 tools
        [SerializeField] int hotbarSize = 6;
        [SerializeField] HotbarUISlot emptySlot;
        [SerializeField] HotbarUISlot[] toolSlots;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

    }
}
