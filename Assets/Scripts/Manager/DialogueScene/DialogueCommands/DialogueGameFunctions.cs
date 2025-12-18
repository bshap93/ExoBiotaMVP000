using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;
using Yarn.Unity;

namespace Manager.DialogueScene.DialogueCommands
{
    public class DialogueGameFunctions : MonoBehaviour, MMEventListener<LoadedManagerEvent>
    {
        [Tooltip("If not assigned, will try DialogueManager.Instance.dialogueRunner")]
        public DialogueRunner dialogueRunner;
        AttributesManager _attributesManager;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            _attributesManager = AttributesManager.Instance;

            if (_attributesManager == null) Debug.LogError("AttributesManager not found in scene.");
        }

        public class AttributFunctions
        {
            [YarnFunction("set_dexterity")]
            public static int SetDexterity(int value)
            {
                return value;
            }
        }
    }
}
