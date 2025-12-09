using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable.Door
{
    public class OnEnterTriggerCallArbitraryEvent : MonoBehaviour
    {
        [FormerlySerializedAs("OnTriggerEnterEvent")]
        public UnityEvent onTriggerEnterEvent;
        void OnTriggerEnter(Collider other)
        {
            onTriggerEnterEvent?.Invoke();
        }
    }
}
