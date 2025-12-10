using Events;
using Helpers.Events.Gated;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;

namespace Objectives
{
    public class ObjectiveOperationListenerHandler : MonoBehaviour, MMEventListener<ObjectiveEvent>,
        MMEventListener<GatedLevelingEvent>
    {
        [SerializeField] ObjectiveObject objectiveToCompleteWhenGatedLevelingEventOccurs;
        ObjectivesManager _objectivesManager;

        void Awake()
        {
            _objectivesManager = GetComponent<ObjectivesManager>();
        }
        void OnEnable()
        {
            this.MMEventStartListening<ObjectiveEvent>();
            this.MMEventStartListening<GatedLevelingEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ObjectiveEvent>();
            this.MMEventStopListening<GatedLevelingEvent>();
        }
        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                _objectivesManager.CompleteObjective(objectiveToCompleteWhenGatedLevelingEventOccurs.objectiveId);
        }


        public void OnMMEvent(ObjectiveEvent e)
        {
        }
    }
}
