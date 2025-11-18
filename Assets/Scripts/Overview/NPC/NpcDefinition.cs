using System;
using System.Collections.Generic;
using Manager.DialogueScene;
using Manager.SceneManagers.Dock;
using Sirenix.OdinInspector;
using UnityEngine;
using Yarn.Unity;

namespace Overview.NPC
{
    [Serializable]
    public struct StartNodeEntry
    {
        public string locationId; // e.g. "Foreman_MineCamp"
        public string node; // e.g. "foreman_mine_start"
    }

    [Serializable]
    public struct GestureEntry
    {
        public string key; // e.g. "shrug", "scoff", "wave"
        public AnimationClip clip; // NPC-specific animation
    }

    [Serializable]
    public struct SoundEntry
    {
        public string key; // e.g. "greeting", "farewell"
        public AudioClip clip; // NPC-specific sound
    }

    [CreateAssetMenu(fileName = "NpcDefinition", menuName = "Scriptable Objects/NPC/NpcDefinition", order = 1)]
    public class NpcDefinition : ScriptableObject
    {
        public string characterName;
        [ValueDropdown("GetNpcIdOptions")] public string npcId; // also the locationId

        public string startNode; // ONE node only

        [ValueDropdown("GetLocationIdOptions")]
        public string locationId;

        public YarnProject yarnProject;
        public GameObject characterPrefab;

        [FoldoutGroup("Animations")] public List<GestureEntry> gestures = new();

        [FoldoutGroup("Sounds")] public List<SoundEntry> sounds = new();

        public AnimationClip idleClip;

        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        static string[] GetLocationIdOptions()
        {
            return DockManager.GetLocationIdOptions();
        }

        public AnimationClip GetGesture(string key)
        {
            foreach (var g in gestures)
                if (g.key == key)
                    return g.clip;

            return null;
        }
        public AudioClip GetDialogueSound(string key)
        {
            foreach (var s in sounds)
                if (s.key == key)
                    return s.clip;

            return null;
        }
    }
}
