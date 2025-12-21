using System;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using HighlightPlus;
using MoreMountains.Feedbacks;
using OccaSoftware.ResponsiveSmokes.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(CessileCreatureBBSync))]
    public class CessileGasCreatureController : CreatureController, IRequiresUniqueID, IDamageable
    {
        [Header("Scene References")] [Tooltip("InteractiveSmoke child that renders & times the gas cloud.")]
        public InteractiveSmoke smoke;
        public Collider gasAreaCollider;

        public float detectionRadius;
        [FormerlySerializedAs("lethalRadius")] public float contaminateRadius;

        [Header("Contamination")] public float contaminationOnEnter = 6f;
        public float contaminationPerSecond = 2f;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks releaseFeedbacks;


        public float currentHealth;
        public float maxHealth;

        public bool isDead;
        public string blackboardWasHitKey = "wasHit";

        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits

        [SerializeField] HighlightEffect highlightEffect;

        bool _gasReleased;
        bool _hazardActive; // true while smoke.IsAlive()
        bool _playerInsideInner; // track inner-zone entry for burst


        protected AnimancerState PuffGasState;


        protected override void Awake()
        {
            base.Awake();
            // keep smoke off until released
            if (smoke) smoke.gameObject.SetActive(false);
        }
        public void PlayHitAnimation(AnimationClip value)
        {
            throw new NotImplementedException();
        }
        public void OnDeath()
        {
            throw new NotImplementedException();
        }
        public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            throw new NotImplementedException();
        }
        public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        {
            throw new NotImplementedException();
        }
    }
}
