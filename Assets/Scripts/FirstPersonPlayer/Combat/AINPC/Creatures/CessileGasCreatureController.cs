using System;
using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using HighlightPlus;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using OccaSoftware.ResponsiveSmokes.Runtime;
using SharedUI.Interface;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace FirstPersonPlayer.Combat.AINPC.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(CessileCreatureBBSync))]
    public class CessileGasCreatureController : CreatureController, IRequiresUniqueID, IDamageable, IBillboardable,
        IHoverable
    {
        [Header("Scene References")] [Tooltip("InteractiveSmoke child that renders & times the gas cloud.")]
        public InteractiveSmoke smoke;
        public Collider gasAreaCollider;

        [Header("Main Settings")] [SerializeField]
        bool destroyAfterDeath;

        public float detectionRadius;
        [FormerlySerializedAs("lethalRadius")] public float contaminateRadius;
        public float gasReleaseCooldown = 8f;

        [Header("Contamination")] public float contaminationOnEnter = 6f;
        public float contaminationPerSecond = 2f;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks releaseFeedbacks;
        [SerializeField] MMFeedbacks deathFeedbacks;

        [Header("Death Effects")] [SerializeField]
        GameObject deathParticlesPrefab;
        [SerializeField] GameObject deadCreatureModelPrefab;


        public float currentHealth;
        public float maxHealth;

        public bool isDead;
        public string blackboardWasHitKey = "wasHit";

        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits

        [SerializeField] HighlightEffect highlightEffect;

        bool _gasReleased;
        bool _hazardActive; // true while smoke.IsAlive()
        bool _playerInsideInner; // track inner-zone entry for burst

        protected SceneObjectData data;

        protected AnimancerState DeathState;


        protected AnimancerState PuffGasState;

        public bool IsPuffingGas { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            // keep smoke off until released
            if (smoke) smoke.gameObject.SetActive(false);
        }

        void Update()
        {
            if (IsPuffingGas) return;

            if (!IdleState.IsPlaying) animancerComponent.Play(IdleState);

            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                if (creatureType.animationSet.deathAnimation)
                {
                    DeathState = animancerComponent.Play(creatureType.animationSet.deathAnimation);
                    DeathState.Events(this).OnEnd = () => { };
                }

                OnDeath();
            }
        }
        public string GetName()
        {
            return creatureType.creatureName;
        }
        public Sprite GetIcon()
        {
            return creatureType.creatureIcon;
        }
        public string ShortBlurb()
        {
            return creatureType.shortDescription;
        }
        public Sprite GetActionIcon()
        {
            return creatureType.actionIcon;
        }
        public string GetActionText()
        {
            return "Avoid or Destroy";
        }
        public void PlayHitAnimation(AnimationClip value)
        {
            // none for right now
        }
        public void OnDeath()
        {
            CreatureStateEvent.Trigger(
                CreatureStateEventType.SetNewCreatureState, uniqueID,
                CreatureStateManager.CreatureState.ShouldBeDestroyed);

            EnemyDamageEvent.Trigger(0f, currentHealth, maxHealth, DamageEventType.Death, creatureType.creatureName);

            deathFeedbacks?.PlayFeedbacks();
            if (deathParticlesPrefab != null)
                Instantiate(deathParticlesPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            if (destroyAfterDeath)
                Destroy(gameObject, 2f);
        }
        public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        {
            throw new NotImplementedException();
        }
        public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        {
            throw new NotImplementedException();
        }
        public bool OnHoverStart(GameObject go)
        {
            data = new SceneObjectData(
                creatureType.creatureName,
                creatureType.creatureIcon,
                creatureType.shortDescription,
                creatureType.actionIcon,
                GetActionText()
            );

            data.Id = uniqueID;

            BillboardEvent.Trigger(data, BillboardEventType.Show);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (data == null) data = SceneObjectData.Empty();
            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            return true;
        }
        public void StartPuffGas()
        {
            if (IsPuffingGas) return;

            IsPuffingGas = true;

            ReleaseGas();


            PuffGasState = animancerComponent.Play(creatureType.animationSet.attackAnimation);

            PuffGasState.Events(this).OnEnd = () => { FinishPuffGas(); };
        }

        void ReleaseGas()
        {
            _gasReleased = true;
            releaseFeedbacks?.PlayFeedbacks();

            if (smoke)
            {
                smoke.gameObject.SetActive(true);
                smoke.Smoke(); // starts fade-in → active lifetime → fade-out → Cleanup
                StartCoroutine(TrackSmokeLife()); // flips _hazardActive true while smoke.IsAlive()
            }
            else
            {
                // Failsafe: if no smoke reference, still mark hazard active for a short window
                _hazardActive = true;
                StartCoroutine(StopHazardNextFrame());
            }
        }

        IEnumerator StopHazardNextFrame()
        {
            yield return null;
            _hazardActive = false;
        }

        IEnumerator TrackSmokeLife()
        {
            // Wait one frame so InteractiveSmoke.Init() runs
            yield return null;

            // Consider the cloud hazardous as long as InteractiveSmoke reports alive.
            // (Init sets isAlive=true; Cleanup sets isAlive=false). :contentReference[oaicite:1]{index=1}
            _hazardActive = smoke && smoke.IsAlive();
            while (smoke && smoke.IsAlive())
                yield return null;

            _hazardActive = false;
            _playerInsideInner = false;
        }

        public void FinishPuffGas()
        {
            IsPuffingGas = false;
        }
    }
}
