using Animancer;
using Helpers.Events;
using Objectives;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.Collider
{
    public class ColliderSpontaneousEventTrigger : MonoBehaviour
    {
        public UnityEvent sceneAction;
        [FormerlySerializedAs("TargetUniqueID")]
        public string targetUniqueID;
        public ColliderObjectiveTrigger.TriggerType triggerType = ColliderObjectiveTrigger.TriggerType.OnEnter;
        [FormerlySerializedAs("SpontaneousTriggerEventType")]
        public SpontaneousTriggerEventType spontaneousTriggerEventType;

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (triggerType != ColliderObjectiveTrigger.TriggerType.OnEnter &&
                triggerType != ColliderObjectiveTrigger.TriggerType.Both) return;

            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                return;

            if (string.IsNullOrEmpty(targetUniqueID))
                return;

            SpontaneousTriggerEvent.Trigger(targetUniqueID, spontaneousTriggerEventType);
            if (sceneAction != null)
                sceneAction.Invoke();
        }
    }
}
