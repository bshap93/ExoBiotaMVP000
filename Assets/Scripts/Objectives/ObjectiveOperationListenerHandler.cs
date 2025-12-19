using System;
using Animancer;
using Events;
using Helpers.Events.Gated;
using Helpers.Events.Status;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objectives
{
    public class ObjectiveOperationListenerHandler : MonoBehaviour, MMEventListener<ObjectiveEvent>,
        MMEventListener<GatedLevelingEvent>, MMEventListener<PlayerStatsEvent>
    {
        [SerializeField] ObjectiveObject objectiveToCompleteWhenGatedLevelingEventOccurs;
        [SerializeField] ObjectiveObject objectiveToCompleteWhenDecontaminationTakesPlace;

        public ActionOnObjectiveOperation[] ActionOnObjectiveOperations;
        ObjectivesManager _objectivesManager;

        void Awake()
        {
            _objectivesManager = GetComponent<ObjectivesManager>();
        }
        void OnEnable()
        {
            this.MMEventStartListening<ObjectiveEvent>();
            this.MMEventStartListening<GatedLevelingEvent>();
            this.MMEventStartListening<PlayerStatsEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ObjectiveEvent>();
            this.MMEventStopListening<GatedLevelingEvent>();
            this.MMEventStopListening<PlayerStatsEvent>();
        }
        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                _objectivesManager.CompleteObjective(objectiveToCompleteWhenGatedLevelingEventOccurs.objectiveId);
        }


        public void OnMMEvent(ObjectiveEvent e)
        {
        }
        public void OnMMEvent(PlayerStatsEvent eventType)
        {
            if (eventType.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease &&
                eventType.StatType == PlayerStatsEvent.PlayerStat.CurrentContamination)
                _objectivesManager.CompleteObjective(objectiveToCompleteWhenDecontaminationTakesPlace.objectiveId);
        }

        [Serializable]
        public class ActionOnObjectiveOperation
        {
            [FormerlySerializedAs("ActionToPerform")]
            public UnityEvent actionToPerform;
            [FormerlySerializedAs("ObjectiveIdToActUpon")]
            public string objectiveIdToActUpon;
            [FormerlySerializedAs("TriggerEvent")]
            public ObjectiveObject.TriggersOnObjectiveLifecycleEvent triggerEvent;
        }
    }
}
