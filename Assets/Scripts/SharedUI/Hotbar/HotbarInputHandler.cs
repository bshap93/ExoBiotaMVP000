using FirstPersonPlayer;
using UnityEngine;

namespace SharedUI.Hotbar
{
    /// <summary>
    ///     Connects the RewiredFirstPersonInputs to the Hotbar system
    /// </summary>
    [RequireComponent(typeof(RewiredFirstPersonInputs))]
    public class HotbarInputHandler : MonoBehaviour
    {
        [SerializeField] FPHUDHotbars fpHudHotbars;

        RewiredFirstPersonInputs _inputs;

        void Start()
        {
            _inputs = GetComponent<RewiredFirstPersonInputs>();

            if (_inputs == null) Debug.LogError("[HotbarInputHandler] RewiredFirstPersonInputs component not found!");

            if (fpHudHotbars == null)
            {
                fpHudHotbars = FindFirstObjectByType<FPHUDHotbars>();
                if (fpHudHotbars == null) Debug.LogError("[HotbarInputHandler] FPHUDHotbars not found in scene!");
            }
        }

        void Update()
        {
            if (_inputs == null || fpHudHotbars == null) return;

            // Check each hotbar key
            if (_inputs.hotbarFP1)
                fpHudHotbars.HandleHotbarKeyPress(1);
            else if (_inputs.hotbarFP2)
                fpHudHotbars.HandleHotbarKeyPress(2);
            else if (_inputs.hotbarFP3)
                fpHudHotbars.HandleHotbarKeyPress(3);
            else if (_inputs.hotbarFP4)
                fpHudHotbars.HandleHotbarKeyPress(4);
            else if (_inputs.hotbarFP5)
                fpHudHotbars.HandleHotbarKeyPress(5);
            else if (_inputs.hotbarFP6) fpHudHotbars.HandleHotbarKeyPress(6);
        }
    }
}
