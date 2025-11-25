using Animancer;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs
{
    public class AlienNPCController : MonoBehaviour
    {
        [SerializeField] AnimancerComponent animancerComponent;
        [SerializeField] string NPCId;
        [SerializeField] string defaultStartNode;
        [SerializeField] MMFeedbacks startDialogueFeedback;
    }
}
