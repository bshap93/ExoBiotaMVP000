using System.Collections;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class EnergyPistolWeapon : RangedToolPrefab
    {
        public enum EnergyPistolMode
        {
            HeatRay,
            Stun
        }

        [Header("Pistol Components")] [SerializeField]
        GameObject physicalRoot;
        [SerializeField] GameObject slider;
        [SerializeField] GameObject frontEmitter;
        [SerializeField] GameObject cell;
        [SerializeField] GameObject trigger;

        [Header("Shooting Settings")] [SerializeField]
        float cooldownTime = 0.5f;
        [SerializeField] float range = 50f;
        [SerializeField] LayerMask hitMask = ~0;

        [Header("Combat Settings")] [SerializeField]
        PlayerToolAttackProfile attackProfile;
        [SerializeField] bool requiresEnergy = true;

        [Header("Visual Effects")] [SerializeField]
        Transform muzzlePosition;
        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] GameObject hitSparksPrefab;
        [SerializeField] GameObject missSparksPrefab;
        [SerializeField] EnergyPistolMode initialPistolMode;

        [Header("Multi-Beam Settings")] [Tooltip("Number of beams to render (2 or 3 recommended)")] [SerializeField]
        int numberOfBeams = 3;
        [Tooltip("Vertical spacing between beams")] [SerializeField]
        float beamVerticalSpacing = 0.04f;
        [SerializeField] float beamVerticalOffset = -0.04f;


        [SerializeField] LineRenderer[] beamLineRenderers;
        [SerializeField] float beamWidth = 0.03f;
        [SerializeField] float beamDuration = 0.1f;
        [SerializeField] Color beamColor = Color.cyan;

        [Header("Recoil Settings")] [SerializeField]
        float recoilBackComponent = 0.001f;
        [SerializeField] float recoilBackDuration = 0.15f;
        [SerializeField] int recoilBackVibrato = 8;
        [SerializeField] float recoilBackElasticity = 0.4f;
        [SerializeField] float recoilRotationComponent = 2f;


        [Header("Accuracy Settings")] [Tooltip("Maximum spread angle in degrees at Dexterity 1")] [SerializeField]
        float maxSpreadAngle = 5f;
        [Tooltip("Minimum spread angle in degrees at max Dexterity")] [SerializeField]
        float minSpreadAngle = 0.5f;
        [Tooltip("Dexterity level for perfect accuracy (0 spread)")] [SerializeField]
        int perfectAccuracyDexterity = 20;
        [Tooltip("Show debug lines for shot trajectory")] [SerializeField]
        bool debugAccuracy;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks shootFeedbacks;
        [FormerlySerializedAs("hitFeedbacks")] [SerializeField]
        MMFeedbacks nonLocalHitFeedbacks;
        [SerializeField] MMFeedbacks missFeedbacks;

        [Header("Scriptable Object Reference")] [SerializeField]
        PistolToolObject pistolToolObject;
        [SerializeField] float delaySlideAnimation;

        EnergyPistolMode _currentPistolMode;

        Vector3 _initialLocalPos;
        GameObject _muzzleFlashInstance;
        ParticleSystem[] _muzzleParticles;
        bool _readyToFire = true;
        float _timeSinceLastUse;

        float EnergyCostPerHeavyShot
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return attackProfile.heavyAttack.baseEnergyCost;
                var dexterity = attrMgr.Dexterity;
                var reduction = attackProfile.dexterityReductionFactor * (dexterity - 1); // Example: 0.05
                var finalCost = attackProfile.heavyAttack.baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        float EnergyCostPerBasicShot
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return attackProfile.basicAttack.baseEnergyCost;
                var dexterity = attrMgr.Dexterity;
                var reduction = attackProfile.dexterityReductionFactor * (dexterity - 1); // Example: 0.05
                var finalCost = attackProfile.basicAttack.baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        float EnergyCostPerBasicStunShot
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return attackProfile.basicStunAttack.baseEnergyCost;
                var dexterity = attrMgr.Dexterity;
                var reduction = attackProfile.dexterityReductionFactor * (dexterity - 1); // Example: 0.05
                var finalCost = attackProfile.basicStunAttack.baseEnergyCost * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }

        void Awake()
        {
            _initialLocalPos = physicalRoot.transform.localPosition;
            AnimController = FindFirstObjectByType<AnimancerRightArmController>();

            // Setup multiple beam renderers
            SetupBeamRenderers();
            // Setup persistent muzzle flash (Hovl style)
            if (muzzleFlashPrefab != null && muzzlePosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                _muzzleFlashInstance.transform.SetParent(muzzlePosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                // Stop all particles initially
                foreach (var ps in _muzzleParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }

            _currentPistolMode = initialPistolMode;

            // Setup persistent muzzle flash (Hovl style)
            if (muzzleFlashPrefab != null && muzzlePosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                _muzzleFlashInstance.transform.SetParent(muzzlePosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                // Stop all particles initially
                foreach (var ps in _muzzleParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }
        }
        void Update()
        {
            if (_timeSinceLastUse < cooldownTime)
                _timeSinceLastUse += Time.deltaTime;
            else
                _readyToFire = true;
        }

        void OnDestroy()
        {
            // Clean up persistent muzzle flash
            if (_muzzleFlashInstance != null) Destroy(_muzzleFlashInstance);
        }

        public override void Use()
        {
            var attributesManager = AttributesManager.Instance;

            if (PlayerMutableStatsManager.Instance.CurrentStamina < EnergyCostPerBasicShot)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina, "Not enough stamina to use pickaxe.", "Insufficient Stamina");

                return;
            }

            PerformToolAction();
        }

        void SetupBeamRenderers()
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0)
            {
                Debug.LogWarning("No beam LineRenderers assigned. Please assign them in the Inspector.");
                return;
            }

            // Ensure we only use the specified number of beams
            numberOfBeams = Mathf.Min(numberOfBeams, beamLineRenderers.Length);

            for (var i = 0; i < beamLineRenderers.Length; i++)
            {
                var beam = beamLineRenderers[i];
                if (beam != null)
                {
                    beam.enabled = false;
                    beam.startColor = beamColor;
                    beam.endColor = beamColor;
                    beam.startWidth = beamWidth;
                    beam.endWidth = beamWidth * 0.8f;
                    beam.positionCount = 2;

                    // Hide unused beams
                    if (i >= numberOfBeams) beam.gameObject.SetActive(false);
                }
            }
        }

        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;
        }

        public void OnUseStarted()
        {
            if (AnimController != null && AnimController.currentToolAnimationSet != null &&
                AnimController.currentToolAnimationSet.beginUseAnimation != null)
                AnimController.PlayToolUseSequence();
        }

        public override void PerformToolAction()
        {
            if (!_readyToFire) return;


            // Check energy cost
            if (requiresEnergy && !HasSufficientEnergy())
            {
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina,
                    "Not enough energy to fire weapon.",
                    "Insufficient Energy");

                return;
            }


            // Consume energy
            if (requiresEnergy)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina,
                    PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    EnergyCostPerBasicShot);

            // Visual and audio feedback
            AnimateRecoil();
            StartCoroutine(AnimateSlideOutAndBack());
            // AnimateSlideOutAndBack();
            AnimateFrontEmitterOutAndBack();
            OnUseStarted();
            shootFeedbacks?.PlayFeedbacks();

            // Play muzzle flash particles (Hovl style)
            PlayMuzzleFlash();

            // Apply hit after short delay for animation sync
            StartCoroutine(ApplyHitAfterDelay(0.05f));

            _readyToFire = false;
            _timeSinceLastUse = 0f;
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0) return;

            foreach (var ps in _muzzleParticles)
                if (ps != null)
                    ps.Play();
        }

        IEnumerator ApplyHitAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ApplyHit();
        }

        void AnimateRecoil()
        {
            physicalRoot.transform.DOKill();
            physicalRoot.transform.localPosition = _initialLocalPos;
            physicalRoot.transform.DOPunchPosition(
                new Vector3(0, 0, recoilBackComponent),
                recoilBackDuration,
                recoilBackVibrato,
                recoilBackElasticity);

            physicalRoot.transform.DOPunchRotation(
                new Vector3(recoilRotationComponent, 0, 0),
                recoilBackDuration,
                recoilBackVibrato,
                recoilBackElasticity);
        }

        public IEnumerator AnimateSlideOutAndBack()
        {
            if (slider == null) yield break;

            // Wait N seconds to sync with firing animation
            yield return new WaitForSeconds(delaySlideAnimation);

            var originalPos = slider.transform.localPosition;
            var slideOutPos = originalPos + new Vector3(0, 0, 0.2f);

            slider.transform.DOKill();
            slider.transform.localPosition = originalPos;
            slider.transform.DOLocalMove(slideOutPos, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        void AnimateFrontEmitterOutAndBack()
        {
            if (frontEmitter == null) return;

            var originalPos = frontEmitter.transform.localPosition;
            var slideOutPos = originalPos + new Vector3(0, 0, -0.2f);

            frontEmitter.transform.DOKill();
            frontEmitter.transform.localPosition = originalPos;
            frontEmitter.transform.DOLocalMove(slideOutPos, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject.defaultReticle;
        }

        float CalculateSpreadAngle()
        {
            var attributesManager = AttributesManager.Instance;
            if (attributesManager == null) return maxSpreadAngle;

            var dexterity = attributesManager.Dexterity;

            // At Dexterity 1: maxSpreadAngle
            // At perfectAccuracyDexterity: minSpreadAngle
            // Linear interpolation
            var t = Mathf.Clamp01((float)(dexterity - 1) / (perfectAccuracyDexterity - 1));
            var spread = Mathf.Lerp(maxSpreadAngle, minSpreadAngle, t);

            if (debugAccuracy) Debug.Log($"[Pistol] Dex: {dexterity}, Spread: {spread:F2}°");

            return spread;
        }

        Vector3 ApplySpread(Vector3 direction, float spreadAngle)
        {
            // Convert angle to radians
            var spreadRad = spreadAngle * Mathf.Deg2Rad;

            // Random point in a circle (uniform distribution)
            var randomCircle = Random.insideUnitCircle * Mathf.Tan(spreadRad);

            // Create perpendicular vectors to the aim direction
            var right = Vector3.Cross(direction, Vector3.up).normalized;
            if (right.magnitude < 0.1f) // Handle edge case when aiming straight up/down
                right = Vector3.Cross(direction, Vector3.forward).normalized;

            var up = Vector3.Cross(right, direction).normalized;

            // Apply spread
            var spreadDirection = direction + right * randomCircle.x + up * randomCircle.y;
            return spreadDirection.normalized;
        }


        public override void ApplyHit()
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;

            // Calculate spread and apply it
            var spreadAngle = CalculateSpreadAngle();
            var baseDirection = mainCamera.transform.forward;
            var spreadDirection = ApplySpread(baseDirection, spreadAngle);
            var ray = new Ray(mainCamera.transform.position, spreadDirection);

            // Debug visualization
            if (debugAccuracy)
            {
                Debug.DrawRay(mainCamera.transform.position, baseDirection * range, Color.green, 1f);
                Debug.DrawRay(mainCamera.transform.position, spreadDirection * range, Color.red, 1f);
            }

            var didHit = Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore);
            var endPoint = didHit ? hit.point : ray.GetPoint(range);

            // Draw multiple energy beams
            StartCoroutine(DrawMultipleBeams(muzzlePosition.position, endPoint));

            if (didHit)
                ProcessHit(hit);
            else
                missFeedbacks?.PlayFeedbacks();
        }

        IEnumerator DrawMultipleBeams(Vector3 start, Vector3 end)
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0) yield break;

            // Calculate the camera's up vector for vertical offset
            var cameraUp = mainCamera.transform.up;

            // Calculate vertical offset for centering the beams
            var totalHeight = (numberOfBeams - 1) * beamVerticalSpacing;
            var startOffset = totalHeight / 2f;

            // Apply additional downward offset to entire beam array
            var baseOffset = beamVerticalOffset * cameraUp;

            // Draw each beam with vertical offset
            for (var i = 0; i < numberOfBeams && i < beamLineRenderers.Length; i++)
            {
                var beam = beamLineRenderers[i];
                if (beam == null) continue;

                // Calculate vertical offset for this beam
                var verticalOffset = (i * beamVerticalSpacing - startOffset) * cameraUp + baseOffset;

                beam.enabled = true;
                beam.SetPosition(0, start + verticalOffset);
                beam.SetPosition(1, end + verticalOffset);
            }

            yield return new WaitForSeconds(beamDuration);

            // Disable all beams
            for (var i = 0; i < numberOfBeams && i < beamLineRenderers.Length; i++)
                if (beamLineRenderers[i] != null)
                    beamLineRenderers[i].enabled = false;
        }


        void ProcessHit(RaycastHit hit)
        {
            var go = hit.collider.gameObject;

            // Hit enemy NPC
            if (go.CompareTag("EnemyNPC"))
            {
                var creatureController = go.GetComponentInParent<CreatureController>();
                if (creatureController != null)
                {
                    // Spawn hit VFX
                    var vfx = creatureController.GetEffectsAndFeedbacks().basicHitVFX;
                    SpawnHitFX(vfx, hit.point, hit.normal);

                    // Apply damage
                    if (_currentPistolMode == EnergyPistolMode.HeatRay)
                    {
                        var attack = attackProfile?.basicAttack;
                        if (attack != null) creatureController.ProcessAttackDamage(attack);
                    }
                    else if (_currentPistolMode == EnergyPistolMode.Stun)
                    {
                        var attack = attackProfile?.basicStunAttack;
                        if (attack != null) creatureController.ProcessAttackDamage(attack);
                    }


                    nonLocalHitFeedbacks?.PlayFeedbacks();
                    Debug.Log($"Energy pistol hit enemy: {creatureController.name}");
                }
            }
            // Hit breakable object
            else if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                if (_currentPistolMode == EnergyPistolMode.HeatRay) breakable.ApplyHit(1, hit.point, hit.normal);
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                nonLocalHitFeedbacks?.PlayFeedbacks();
            }
            // Hit generic surface
            else
            {
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                nonLocalHitFeedbacks?.PlayFeedbacks();
            }
        }

        void SpawnHitFX(GameObject vfxPrefab, Vector3 position, Vector3 normal)
        {
            if (vfxPrefab == null) return;

            var vfxInstance = Instantiate(vfxPrefab, position, Quaternion.LookRotation(normal));
            var vfxFeedbacks = vfxInstance.GetComponent<MMFeedbacks>();
            vfxFeedbacks?.PlayFeedbacks();
            Destroy(vfxInstance, 2f);
        }

        bool HasSufficientEnergy()
        {
            var statsManager = PlayerMutableStatsManager.Instance;
            if (statsManager == null) return true;

            return statsManager.CurrentStamina >= EnergyCostPerBasicShot;
        }
    }
}
