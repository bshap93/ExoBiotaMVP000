using System;
using System.Collections.Generic;
using Helpers.Events.Status;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.Status.Scriptable
{
    [Serializable]
    public struct StatsChange
    {
        [FormerlySerializedAs("Stat")] public PlayerStatsEvent.PlayerStat statType;
        public PlayerStatsEvent.PlayerStatChangeType changeType;
        public float amount;
        [Range(0f, 1f)] public float percent;
        public Sprite icon;
        public bool isPositive;
    }

    [CreateAssetMenu(fileName = "StatusEffect", menuName = "Scriptable Objects/Character/StatusEffect", order = 1)]
    public class StatusEffect : ScriptableObject
    {
        [FormerlySerializedAs("EffectID")] public string effectID;
        [FormerlySerializedAs("EffectName")] public string effectName;
        [FormerlySerializedAs("Description")] [TextArea(1, 4)]
        public string description;
        public Sprite effectIcon;
        [FormerlySerializedAs("stats")] [FormerlySerializedAs("Stats")]
        public List<StatsChange> statsChanges = new();

        [Header("Visual Effects")] public bool distortion;
        public bool floaters;

        [Range(0f, 1f)] public float riskOfDeath;

        [Header("Perceptual Effects")] public bool intrusiveThoughts;

        [Header("Removal Settings")] public bool removableViaDecontaminationTank = true;
    }
}
