using Animancer;
using Helpers.Events;
using Helpers.Events.Triggering;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.Collider
{
    public class ColliderSpontaneousEventTrigger : MonoBehaviour
    {
        public UnityEvent sceneAction;
        [FormerlySerializedAs("TargetUniqueID")]
        public string targetUniqueID;
        // public ColliderObjectiveTrigger.TriggerType triggerType = ColliderObjectiveTrigger.TriggerType.OnEnter;
        [FormerlySerializedAs("SpontaneousTriggerEventType")]
        public SpontaneousTriggerEventType spontaneousTriggerEventType;
        public bool setNotTriggerableOnExit;
        public bool setNotTriggerableOnEnter;
        TriggerColliderManager _triggerColliderManager;

        void Start()
        {
            _triggerColliderManager = TriggerColliderManager.Instance;
            if (_triggerColliderManager == null)
                Debug.LogWarning(
                    "[ColliderSpontaneousEventTrigger] No TriggerColliderManager found in scene. Ensure one exists.");
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            // if (triggerType != ColliderObjectiveTrigger.TriggerType.OnEnter &&
            //     triggerType != ColliderObjectiveTrigger.TriggerType.Both) return;


            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                return;

            if (!_triggerColliderManager.IsSpontaneousColliderTriggerable(targetUniqueID))
                return;

            if (string.IsNullOrEmpty(targetUniqueID))
                return;

            if (setNotTriggerableOnEnter)
                TriggerColliderEvent.Trigger(
                    targetUniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Spontaneous);

            SpontaneousTriggerEvent.Trigger(targetUniqueID, spontaneousTriggerEventType);
            if (sceneAction != null)
                sceneAction.Invoke();
        }

        void OnTriggerExit(UnityEngine.Collider other)
        {
            // if (triggerType != ColliderObjectiveTrigger.TriggerType.OnExit &&
            //     triggerType != ColliderObjectiveTrigger.TriggerType.Both) return;

            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                return;

            if (string.IsNullOrEmpty(targetUniqueID))
                return;

            if (setNotTriggerableOnExit)
                TriggerColliderEvent.Trigger(
                    targetUniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Spontaneous);
        }
    }
}
