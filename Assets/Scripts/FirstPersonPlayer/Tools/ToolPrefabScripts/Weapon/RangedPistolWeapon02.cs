using System;
using System.Collections;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC.Creatures;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class RangedPistolWeapon02 : RangedToolPrefab
    {
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
        [SerializeField] float energyCostPerShot = 5f;
        [SerializeField] bool requiresEnergy = true;

        [Header("Visual Effects")] [SerializeField]
        Transform muzzlePosition;
        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] GameObject hitEffectPrefab;
        [SerializeField] GameObject missSparksPrefab;

        [Header("Beam Settings")] [SerializeField]
        LineRenderer beamLineRenderer;
        [SerializeField] float beamDuration = 0.1f;
        [SerializeField] Color beamColor = Color.cyan;
        [SerializeField] float beamWidth = 0.05f;

        [Header("Beam Animation (Hovl-style)")] [Tooltip("Animate beam texture scrolling")] [SerializeField]
        bool animateBeamTexture = true;
        [Tooltip("Main texture tiling multiplier per unit distance")] [SerializeField]
        float mainTextureTilingPerUnit = 1f;
        [Tooltip("Noise texture tiling multiplier per unit distance")] [SerializeField]
        float noiseTextureTilingPerUnit = 1f;
        [Tooltip("Offset hit effect slightly along surface normal")] [SerializeField]
        float hitEffectNormalOffset = 0.05f;
        [Tooltip("Use beam rotation for hit effect, or make it look at hit point")] [SerializeField]
        bool useBeamRotationForHitEffect;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks shootFeedbacks;
        [SerializeField] MMFeedbacks hitFeedbacks;
        [SerializeField] MMFeedbacks missFeedbacks;

        [Header("Scriptable Object Reference")] [SerializeField]
        PistolToolObject pistolToolObject;

        [Header("Debug")] [SerializeField] bool debugMode;

        // Track active beam coroutine
        Coroutine _beamCoroutine;
        GameObject _currentHitEffect;
        ParticleSystem[] _hitEffectParticles;

        // State
        Vector3 _initialLocalPos;

        // Particle systems for muzzle flash and hit effects
        ParticleSystem[] _muzzleParticles;
        bool _readyToFire = true;
        float _timeSinceLastUse;

        void Awake()
        {
            if (physicalRoot != null)
                _initialLocalPos = physicalRoot.transform.localPosition;

            AnimController = FindFirstObjectByType<AnimancerRightArmController>();

            SetupBeamRenderer();
            SetupParticleSystems();

            if (debugMode) Debug.Log("[RangedPistol] Awake complete");
        }

        void Update()
        {
            // Cooldown logic
            if (!_readyToFire)
            {
                _timeSinceLastUse += Time.deltaTime;

                if (_timeSinceLastUse >= cooldownTime)
                {
                    _readyToFire = true;
                    if (debugMode) Debug.Log($"[RangedPistol] Ready to fire again after {_timeSinceLastUse:F2}s");
                }
            }
        }

        void OnDisable()
        {
            // Stop beam coroutine if running
            if (_beamCoroutine != null)
            {
                StopCoroutine(_beamCoroutine);
                _beamCoroutine = null;
            }

            // Clean up beam
            if (beamLineRenderer != null) beamLineRenderer.enabled = false;

            // Stop all hit particles
            if (_hitEffectParticles != null)
                foreach (var ps in _hitEffectParticles)
                    if (ps != null && ps.isPlaying)
                        ps.Stop();
        }

        void OnDestroy()
        {
            // Clean up persistent hit effect
            if (_currentHitEffect != null) Destroy(_currentHitEffect);
        }

        void SetupBeamRenderer()
        {
            if (beamLineRenderer == null)
            {
                if (debugMode) Debug.LogWarning("[RangedPistol] No LineRenderer assigned!");
                return;
            }

            beamLineRenderer.enabled = false;
            beamLineRenderer.startColor = beamColor;
            beamLineRenderer.endColor = beamColor;
            beamLineRenderer.startWidth = beamWidth;
            beamLineRenderer.endWidth = beamWidth * 0.5f;
            beamLineRenderer.useWorldSpace = true;

            if (debugMode) Debug.Log("[RangedPistol] Beam renderer setup complete");
        }

        void SetupParticleSystems()
        {
            // Cache muzzle flash particles
            if (muzzleFlashPrefab != null)
            {
                _muzzleParticles = muzzleFlashPrefab.GetComponentsInChildren<ParticleSystem>();
                if (debugMode)
                    Debug.Log($"[RangedPistol] Found {_muzzleParticles?.Length ?? 0} muzzle particle systems");
            }

            // Instantiate persistent hit effect (Hovl style)
            if (hitEffectPrefab != null)
            {
                _currentHitEffect = Instantiate(hitEffectPrefab, Vector3.zero, Quaternion.identity);
                _currentHitEffect.transform.SetParent(transform);
                _hitEffectParticles = _currentHitEffect.GetComponentsInChildren<ParticleSystem>();

                // Stop all particles initially
                if (_hitEffectParticles != null)
                    foreach (var ps in _hitEffectParticles)
                        if (ps != null && ps.isPlaying)
                            ps.Stop();

                if (debugMode)
                    Debug.Log(
                        $"[RangedPistol] Hit effect setup with {_hitEffectParticles?.Length ?? 0} particle systems");
            }
        }

        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;

            if (debugMode) Debug.Log("[RangedPistol] Initialize called");
        }

        public void OnUseStarted()
        {
            if (AnimController != null && AnimController.currentToolAnimationSet != null &&
                AnimController.currentToolAnimationSet.beginUseAnimation != null)
                AnimController.PlayToolUseSequence();
        }

        public override void PerformToolAction()
        {
            if (debugMode)
                Debug.Log(
                    $"[RangedPistol] PerformToolAction called. Ready: {_readyToFire}, TimeSince: {_timeSinceLastUse:F2}");

            if (!_readyToFire)
            {
                if (debugMode) Debug.Log("[RangedPistol] Still on cooldown, ignoring");
                return;
            }

            // Check energy cost
            if (requiresEnergy && !HasSufficientEnergy())
            {
                if (debugMode) Debug.Log("[RangedPistol] Insufficient energy");
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
                    energyCostPerShot);

            // Set cooldown FIRST to prevent double-firing
            _readyToFire = false;
            _timeSinceLastUse = 0f;

            // Visual and audio feedback
            if (physicalRoot != null) AnimateRecoil();
            OnUseStarted();
            shootFeedbacks?.PlayFeedbacks();
            PlayMuzzleFlashParticles();

            // Apply hit
            try
            {
                ApplyHit();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RangedPistol] Error in ApplyHit: {e.Message}\n{e.StackTrace}");
                // Still reset cooldown even on error
                _readyToFire = true;
            }

            if (debugMode) Debug.Log("[RangedPistol] Shot fired successfully");
        }

        void AnimateRecoil()
        {
            physicalRoot.transform.DOKill();
            physicalRoot.transform.localPosition = _initialLocalPos;
            physicalRoot.transform.DOPunchPosition(
                new Vector3(0, 0, 0.001f),
                0.15f,
                8,
                0.4f);
        }

        void PlayMuzzleFlashParticles()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0) return;

            foreach (var ps in _muzzleParticles)
                if (ps != null && !ps.isPlaying)
                    ps.Play();
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject != null ? pistolToolObject.defaultReticle : null;
        }

        public override void ApplyHit()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[RangedPistol] No camera found!");
                return;
            }

            if (muzzlePosition == null)
            {
                Debug.LogError("[RangedPistol] No muzzle position assigned!");
                return;
            }

            var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            var didHit = Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore);

            if (debugMode) Debug.Log($"[RangedPistol] Raycast result: {didHit}");

            if (didHit)
            {
                if (debugMode) Debug.Log($"[RangedPistol] Hit {hit.collider.name} at {hit.point}");

                var distance = Vector3.Distance(muzzlePosition.position, hit.point);
                DrawBeamToPoint(hit.point, distance);
                UpdateHitEffect(hit, true);
                ProcessHit(hit);
            }
            else
            {
                var endPoint = ray.GetPoint(range);
                DrawBeamToPoint(endPoint, range);
                UpdateHitEffect(default, false);
                missFeedbacks?.PlayFeedbacks();
            }
        }

        void DrawBeamToPoint(Vector3 endPoint, float distance)
        {
            if (beamLineRenderer == null) return;

            // Stop previous beam coroutine if running
            if (_beamCoroutine != null) StopCoroutine(_beamCoroutine);

            // Set beam positions
            beamLineRenderer.enabled = true;
            beamLineRenderer.SetPosition(0, muzzlePosition.position);
            beamLineRenderer.SetPosition(1, endPoint);

            // Apply Hovl-style texture tiling
            if (animateBeamTexture && beamLineRenderer.material != null)
            {
                var mainTiling = mainTextureTilingPerUnit * distance;
                var noiseTiling = noiseTextureTilingPerUnit * distance;

                if (beamLineRenderer.material.HasProperty("_MainTex"))
                    beamLineRenderer.material.SetTextureScale("_MainTex", new Vector2(mainTiling, 1f));

                if (beamLineRenderer.material.HasProperty("_Noise"))
                    beamLineRenderer.material.SetTextureScale("_Noise", new Vector2(noiseTiling, 1f));
            }

            // Start new hide coroutine
            _beamCoroutine = StartCoroutine(HideBeamAfterDelay());
        }

        IEnumerator HideBeamAfterDelay()
        {
            yield return new WaitForSeconds(beamDuration);

            if (beamLineRenderer != null) beamLineRenderer.enabled = false;

            _beamCoroutine = null;
        }

        void UpdateHitEffect(RaycastHit hit, bool didHit)
        {
            if (_currentHitEffect == null || _hitEffectParticles == null) return;

            if (didHit)
            {
                _currentHitEffect.transform.position = hit.point + hit.normal * hitEffectNormalOffset;

                if (useBeamRotationForHitEffect)
                    _currentHitEffect.transform.rotation = muzzlePosition.rotation;
                else
                    _currentHitEffect.transform.LookAt(hit.point + hit.normal);

                foreach (var ps in _hitEffectParticles)
                    if (ps != null && !ps.isPlaying)
                        ps.Play();
            }
            else
            {
                foreach (var ps in _hitEffectParticles)
                    if (ps != null && ps.isPlaying)
                        ps.Stop();
            }
        }

        void ProcessHit(RaycastHit hit)
        {
            var go = hit.collider.gameObject;

            // Hit enemy NPC
            if (go.CompareTag("EnemyNPC"))
            {
                var enemyController = go.GetComponentInParent<EnemyController>();
                if (enemyController != null)
                {
                    var attack = attackProfile?.basicAttack;
                    if (attack != null)
                    {
                        enemyController.ProcessAttackDamage(attack);
                        if (debugMode) Debug.Log($"[RangedPistol] Damaged enemy: {enemyController.name}");
                    }

                    // Spawn enemy-specific VFX
                    var enemyVfx = enemyController.GetEffectsAndFeedbacks()?.basicHitVFX;
                    if (enemyVfx != null)
                    {
                        var vfxInstance = Instantiate(enemyVfx, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(vfxInstance, 2f);
                    }

                    hitFeedbacks?.PlayFeedbacks();
                }
            }
            // Hit breakable object
            else if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                breakable.ApplyHit(1, hit.point, hit.normal);
                hitFeedbacks?.PlayFeedbacks();
                if (debugMode) Debug.Log($"[RangedPistol] Hit breakable: {go.name}");
            }
            // Hit generic surface
            else
            {
                hitFeedbacks?.PlayFeedbacks();
                if (debugMode) Debug.Log($"[RangedPistol] Hit surface: {go.name}");
            }
        }

        bool HasSufficientEnergy()
        {
            var statsManager = PlayerMutableStatsManager.Instance;
            if (statsManager == null) return true;

            return statsManager.CurrentStamina >= energyCostPerShot;
        }
    }
}
