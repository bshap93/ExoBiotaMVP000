using Manager.DialogueScene;
using Manager.SceneManagers.Dock;
using Michsky.MUIP;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Overview.OverviewMode
{
    [DisallowMultipleComponent]
    public class AnimatedButtonHasNewContent : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("The NPC this button represents (stable id used by dialogue/quests).")]
        [ValueDropdown("GetNPCIdOptions")]
        public string npcId;


        [Header("Optional Location Gating")]
        [Tooltip("Leave empty to always show regardless of dock/location.")]
        [ValueDropdown("GetDockIdOptions")]
        public string requiredDockId; // only animate when player is at this dock

        [Tooltip("Optional: a specific Overview location id the NPC belongs to.")]
        [ValueDropdown("GetLocationIdOptions")]
        public string requiredLocationId;

        [Header("UI")] public AnimatedIconHandler icon; // MUIP animated icon on/near the button

        // cache last visible state
        private bool _isOn;


        private static string[] GetDockIdOptions()
        {
            return DockManager.GetDockIdOptions();
        }

        private static string[] GetNPCIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        private static string[] GetLocationIdOptions()
        {
            return DockManager.GetLocationIdOptions();
        }
    }
}