using Overview.NPC;
using UnityEngine;
using Yarn.Unity;

namespace Manager.DialogueScene.DialogueCommands
{
    public partial class DialogueGameCommands
    {
        // Character Avatar animations -----------
        public GameObject characterNPCRoot;
        [YarnCommand("shrug")]
        public void Shrug(string npcId)
        {
            TriggerGesture(npcId, "shrug");
        }

        [YarnCommand("greet")]
        public void Greet(string npcId)
        {
            TriggerGesture(npcId, "greet");
        }

        [YarnCommand("smalltalk")]
        public void SmallTalk(string npcId)
        {
            TriggerGesture(npcId, "smalltalk");
        }

        void TriggerGesture(string npcId, string key)
        {
            // Find NPC by id in the scene
            if (characterNPCRoot == null)
            {
                Debug.LogError($"NPC '{npcId}' not found in scene.");
                return;
            }

            var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

            if (helper == null) return;

            helper.PlayGesture(key);
        }


        void TriggerSound(string npcId, string key)
        {
            if (characterNPCRoot == null)
            {
                Debug.LogError($"NPC '{npcId}' not found in scene.");
                return;
            }

            var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

            if (helper == null) return;

            helper.PlaySound(key);
        }


        [YarnCommand("scoffs")]
        public void Scoffs(string npcId)
        {
            TriggerGesture(npcId, "scoffs");
        }

        [YarnCommand("pleased")]
        public void Pleased(string npcId)
        {
            TriggerGesture(npcId, "pleased");
        }

        // Character Sounds

        [YarnCommand("play_greet_sound")]
        public void PlayGreetSound(string npcId)
        {
            TriggerSound(npcId, "greet");
        }
    }
}
