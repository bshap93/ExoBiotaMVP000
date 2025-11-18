using Animancer;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Overview.NPC
{
    [RequireComponent(typeof(AnimancerComponent))]
    public class NPCCharacterAnimancerHelper : MonoBehaviour
    {
        [SerializeField] AnimancerComponent _animancer;
        [SerializeField] AnimationClip idleClip;

        [SerializeField] AnimationClip handGesture02;
        [SerializeField] NpcDefinition npcDefinition;

        public MMF_Player dialogueSoundFeedbackPlayer;

        AnimancerState _idleState;


        void Start()
        {
            if (npcDefinition != null && npcDefinition.idleClip != null)
                _idleState = _animancer.Play(npcDefinition.idleClip);
        }


        public void PlayGesture(string key)
        {
            if (npcDefinition == null)
            {
                Debug.LogWarning($"[{name}] No NPCDefinition assigned.");
                return;
            }

            var clip = npcDefinition.GetGesture(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{npcDefinition.characterName}] has no gesture for '{key}'.");
                return;
            }

            var state = _animancer.Play(clip);
            state.Events(_animancer).OnEnd = () =>
            {
                if (_idleState != null)
                    _animancer.Play(_idleState);
            };
        }
        public void PlaySound(string key)
        {
            if (dialogueSoundFeedbackPlayer == null)
            {
                Debug.LogWarning($"[{name}] No dialogueSoundFeedbackPlayer assigned.");
                return;
            }

            var clip = npcDefinition.GetDialogueSound(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{npcDefinition.characterName}] has no dialogue sound for '{key}'.");
                return;
            }

            var mmfSound = dialogueSoundFeedbackPlayer.FeedbacksList[0] as MMF_MMSoundManagerSound;
            if (mmfSound == null)
            {
                Debug.LogWarning($"[{name}] dialogueSoundFeedbackPlayer has no MMF_MMSoundManagerSound feedback.");
                return;
            }

            mmfSound.Sfx = clip;

            dialogueSoundFeedbackPlayer?.PlayFeedbacks();
        }
    }
}
