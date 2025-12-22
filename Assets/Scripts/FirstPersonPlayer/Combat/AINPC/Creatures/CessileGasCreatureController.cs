using System.Collections;
using Animancer;
using DG.Tweening;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Helpers.Events.Status;
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

        [SerializeField] GameObject undestroyedModelObject;
        [SerializeField] GameObject destroyedModelObject;

        [Header("Main Settings")] [SerializeField]
        bool destroyAfterDeath;

        public float detectionRadius;
        [FormerlySerializedAs("lethalRadius")] public float contaminateRadius;

        [Header("Contamination")] public float contaminationOnEnter = 6f;
        public float contaminationPerSecond = 2f;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks releaseFeedbacks;


        [Header("Death Effects")] [SerializeField]
        GameObject deathParticlesPrefab;

        // public float currentHealth;
        // public float maxHealth;

        public bool isDead;
        public string blackboardWasHitKey = "wasHit";

        [SerializeField] float attackStartupTime = 0.35f; // wind-up before it hits


        bool _gasReleased;
        bool _hasAppliedBurstContamination; // NEW: track if burst contamination was applied

        Tween _hitTween;
        // bool _hazardActive; // true while smoke.IsAlive()
        bool _playerInsideInner; // track inner-zone entry for burst

        Transform _playerTransform;


        protected SceneObjectData Data;
        protected AnimancerState DeathState;
        protected AnimancerState PuffGasState;

        public bool IsPuffingGas { get; private set; }
        public bool HazardActive { get; set; }

        protected override void Awake()
        {
            base.Awake();
            // keep smoke off until released
            if (smoke) smoke.gameObject.SetActive(false);
        }

        void Update()
        {
            // NEW: Apply contamination while hazard is active and player is in gas area
            if (HazardActive && _playerTransform && gasAreaCollider)
            {
                var playerInGasArea = gasAreaCollider.bounds.Contains(_playerTransform.position);

                if (playerInGasArea)
                {
                    // Apply burst contamination on first entry
                    if (!_hasAppliedBurstContamination)
                    {
                        PlayerStatsEvent.Trigger(
                            PlayerStatsEvent.PlayerStat.CurrentContamination,
                            PlayerStatsEvent.PlayerStatChangeType.Increase,
                            contaminationOnEnter);

                        _hasAppliedBurstContamination = true;
                    }

                    // Apply continuous contamination
                    PlayerStatsEvent.Trigger(
                        PlayerStatsEvent.PlayerStat.CurrentContamination,
                        PlayerStatsEvent.PlayerStatChangeType.Increase,
                        contaminationPerSecond * Time.deltaTime);
                }
            }


            if (HazardActive && (!smoke || !smoke.IsAlive()))
            {
                HazardActive = false;
                _hasAppliedBurstContamination = false;
            }


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
        // NEW: Track player entering/exiting trigger
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer")) return;
            _playerTransform = other.transform;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer")) return;
            if (other.transform == _playerTransform)
            {
                _playerTransform = null;
                _hasAppliedBurstContamination = false;
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
            blackboard.SetVariableValue("isDead", true);
            Debug.Log(creatureType.creatureName + " has died.");

            if (deathFeedbacks != null) deathFeedbacks.enabled = true;
            deathFeedbacks?.PlayFeedbacks();
            // if (deathParticlesPrefab != null)
            //     Instantiate(deathParticlesPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            undestroyedModelObject.SetActive(false);
            destroyedModelObject.SetActive(true);
        }
        // public void ProcessAttackDamage(PlayerToolAttack playerAttack)
        // {
        //     var attributeManager = AttributesManager.Instance;
        //     var damageAmount = playerAttack.rawDamage;
        //     var attackType = playerAttack.attackType;
        //
        //     var isCriticalHit = Random.value <= playerAttack.critChance;
        //
        //     if (attackType == PlayerAttackType.Melee)
        //     {
        //         // Placeholder for strength stat for player
        //         var playerStrength = attributeManager.Strength;
        //         // Provisional damage scaling based on player strength
        //         var playerStrengthMultiplier = 1f + (playerStrength - 1) * 0.5f;
        //         if (isCriticalHit)
        //         {
        //             damageAmount *= playerAttack.critMultiplier;
        //             critDamageFeedbacks?.PlayFeedbacks();
        //         }
        //
        //         damageAmount *= playerStrengthMultiplier;
        //
        //
        //         // StartCoroutine(CooldownAfterWasHit());
        //
        //         if (playerAttack.damageType == MeleeToolPrefab.HitType.Normal)
        //         {
        //             meleeHitFeedbacksBasic?.PlayFeedbacks();
        //             PlayHitTween(t => t.DOPunchPosition(
        //                 new Vector3(creatureType.meleeAttackShakeIntensity, 0f, creatureType.meleeAttackShakeIntensity),
        //                 creatureType.meleeAttackShakeDuration));
        //
        //             blackboard.SetVariableValue("wasHit", true);
        //             Debug.Log("Normal Hit registered on " + creatureType.creatureName);
        //         }
        //         else if (playerAttack.damageType == MeleeToolPrefab.HitType.Heavy)
        //         {
        //             meleeHitFeedbacksHeavy?.PlayFeedbacks();
        //             blackboard.SetVariableValue("wasHitHeavy", true);
        //             Debug.Log("Heavy Hit registered on " + creatureType.creatureName);
        //             PlayHitTween(t => t.DOShakePosition(
        //                 creatureType.meleeAttackShakeDuration,
        //                 new Vector3(
        //                     creatureType.heavyMeleeAttackShakeIntensity, 0f,
        //                     creatureType.heavyMeleeAttackShakeIntensity))); // or still punch
        //         }
        //     }
        //     else if (attackType == PlayerAttackType.Ranged)
        //     {
        //         var playerDexterity = attributeManager.Dexterity;
        //         var playerDexterityMultiplier = 1f + (playerDexterity - 1) * 0.5f;
        //         if (isCriticalHit)
        //         {
        //             damageAmount *= playerAttack.critMultiplier;
        //             critDamageFeedbacks?.PlayFeedbacks();
        //         }
        //
        //         damageAmount *= playerDexterityMultiplier;
        //
        //         if (playerAttack.damageType == MeleeToolPrefab.HitType.Normal)
        //         {
        //             rangedHitFeedbacksBasic?.PlayFeedbacks();
        //             PlayHitTween(t => t.DOPunchPosition(
        //                 new Vector3(
        //                     creatureType.rangedAttackShakeIntensity, 0f, creatureType.rangedAttackShakeIntensity),
        //                 creatureType.rangedAttackShakeDuration));
        //
        //             blackboard.SetVariableValue("wasHit", true);
        //             Debug.Log("Ranged Normal Hit registered on " + creatureType.creatureName);
        //         }
        //         else if (playerAttack.damageType == MeleeToolPrefab.HitType.Heavy)
        //         {
        //             rangedHitFeedbacksHeavy?.PlayFeedbacks();
        //             blackboard.SetVariableValue("wasHitHeavy", true);
        //             Debug.Log("Ranged Heavy Hit registered on " + creatureType.creatureName);
        //             PlayHitTween(t => t.DOShakePosition(
        //                 creatureType.rangedAttackShakeDuration,
        //                 new Vector3(
        //                     creatureType.heavyRangedAttackShakeIntensity, 0f,
        //                     creatureType.heavyRangedAttackShakeIntensity))); // or still punch
        //         }
        //     }
        //
        //     var eventType = isCriticalHit
        //         ? DamageEventType.CriticalHitDamage
        //         : DamageEventType.DealtDamage;
        //
        //     EnemyDamageEvent.Trigger(
        //         currentHealth - damageAmount, currentHealth, maxHealth,
        //         eventType, creatureType.creatureName);
        //
        //     // blackboard.SetVariableValue(blackboardWasHitKey, true);
        //     // Debug.Log("Fallback hit registered on " + creatureType.creatureName);
        //
        //     currentHealth -= damageAmount;
        //     highlightEffect.HitFX();
        // }
        // public void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true)
        // {
        //     if (killPrevious) _hitTween?.Kill();
        //     _hitTween = buildTween(transform);
        // }
        public bool OnHoverStart(GameObject go)
        {
            Data = new SceneObjectData(
                creatureType.creatureName,
                creatureType.creatureIcon,
                creatureType.shortDescription,
                creatureType.actionIcon,
                GetActionText()
            );

            Data.Id = uniqueID;

            BillboardEvent.Trigger(Data, BillboardEventType.Show);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (Data == null) Data = SceneObjectData.Empty();
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            return true;
        }
        public void StartPuffGas()
        {
            if (IsPuffingGas) return;
            if (HazardActive) return; // Don't puff while smoke is still alive

            IsPuffingGas = true;

            ReleaseGas();


            PuffGasState = animancerComponent.Play(creatureType.animationSet.attackAnimation);

            PuffGasState.Events(this).OnEnd = () => { FinishPuffGas(); };
        }

        protected override IEnumerator InitializeAfterCreatureStateManager()
        {
            yield return null;

            var creatureStateManager = CreatureStateManager.Instance;
            if (creatureStateManager != null)
            {
                var creatureState = creatureStateManager.GetCreatureState(uniqueID);
                if (creatureState == CreatureStateManager.CreatureState.None) creatureState = initialCreatureState;

                if (creatureState == CreatureStateManager.CreatureState.HasBeenInitialized)
                {
                    ReLoadCreatureStateData();
                }
                else if (creatureState == CreatureStateManager.CreatureState.ShouldBeDestroyed)
                {
                    isDead = true;
                    blackboard.SetVariableValue("isDead", true);
                    destroyedModelObject.SetActive(true);
                    undestroyedModelObject.SetActive(false);
                }
            }
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
                HazardActive = true;
                StartCoroutine(StopHazardNextFrame());
            }
        }

        IEnumerator StopHazardNextFrame()
        {
            yield return null;
            HazardActive = false;
        }

        IEnumerator TrackSmokeLife()
        {
            // Wait one frame so InteractiveSmoke.Init() runs
            yield return null;

            // Consider the cloud hazardous as long as InteractiveSmoke reports alive.
            // (Init sets isAlive=true; Cleanup sets isAlive=false). :contentReference[oaicite:1]{index=1}
            HazardActive = smoke && smoke.IsAlive();
            while (smoke && smoke.IsAlive())
                yield return null;

            HazardActive = false;
            _playerInsideInner = false;
        }

        public void FinishPuffGas()
        {
            IsPuffingGas = false;
        }
    }
}
