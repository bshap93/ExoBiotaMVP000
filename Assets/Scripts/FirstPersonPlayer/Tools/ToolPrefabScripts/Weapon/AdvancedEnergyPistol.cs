using System;
using System.Collections;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class AdvancedEnergyPistol : RangedToolPrefab
    {
        [Header("Pistol Components")] [SerializeField]
        GameObject physicalRoot;
        [SerializeField] GameObject energyCell;
        [SerializeField] Transform muzzlePosition;

        [Header("Shooting Settings")] [SerializeField]
        float normalCooldown = 0.5f;
        [SerializeField] float chargedCooldown = 1.5f;
        [SerializeField] float range = 50f;
        [SerializeField] LayerMask hitMask = ~0;
        [SerializeField] bool allowChargeShots = true;

        [Header("Charge Settings")] [SerializeField]
        float timeToFullCharge = 1.5f;
        [SerializeField] float chargeEnergyDrainPerSecond = 2f;

        [Header("Combat Settings")] [SerializeField]
        PlayerToolAttackProfile attackProfile;
        [SerializeField] float energyCostPerNormalShot = 5f;
        [SerializeField] float energyCostPerChargedShot = 15f;
        [SerializeField] bool requiresEnergy = true;

        [Header("Visual Effects")] [SerializeField]
        GameObject muzzleFlashPrefab;
        [SerializeField] GameObject chargedMuzzleFlashPrefab;
        [SerializeField] GameObject hitSparksPrefab;
        [SerializeField] GameObject chargedHitEffectPrefab;
        [SerializeField] LineRenderer beamLineRenderer;
        [SerializeField] float beamDuration = 0.1f;
        [SerializeField] Color normalBeamColor = Color.cyan;
        [SerializeField] Color chargedBeamColor = Color.magenta;
        [SerializeField] ParticleSystem chargeParticles;

        [Header("Audio")] [SerializeField] AudioClip shootSound;
        [SerializeField] AudioClip chargeSound;
        [SerializeField] AudioClip chargedShootSound;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks shootFeedbacks;
        [SerializeField] MMFeedbacks chargedShootFeedbacks;
        [SerializeField] MMFeedbacks hitFeedbacks;

        [Header("Scriptable Object Reference")] [SerializeField]
        PistolToolObject pistolToolObject;

        [Header("Debug")] [SerializeField] bool debugMode;
        Coroutine _beamCoroutine;
        AudioSource _chargeAudioSource;
        float _chargeTime;
        float _currentCooldown;

        // State tracking
        Vector3 _initialLocalPos;
        bool _isCharging;
        bool _isFullyCharged;
        bool _readyToFire = true;
        float _timeSinceLastUse;

        void Awake()
        {
            if (physicalRoot != null)
                _initialLocalPos = physicalRoot.transform.localPosition;

            AnimController = FindFirstObjectByType<AnimancerRightArmController>();

            SetupBeamRenderer();
            SetupAudio();

            if (chargeParticles != null)
                chargeParticles.Stop();

            _currentCooldown = normalCooldown;

            if (debugMode) Debug.Log("[AdvancedPistol] Awake complete");
        }

        void Update()
        {
            UpdateCooldown();
            UpdateChargeVisuals();
        }

        void OnDisable()
        {
            // Stop beam coroutine
            if (_beamCoroutine != null)
            {
                StopCoroutine(_beamCoroutine);
                _beamCoroutine = null;
            }

            // Clean up beam
            if (beamLineRenderer != null) beamLineRenderer.enabled = false;

            // Reset charging state
            ResetCharge();
        }

        void UpdateCooldown()
        {
            if (!_readyToFire)
            {
                _timeSinceLastUse += Time.deltaTime;

                if (_timeSinceLastUse >= _currentCooldown)
                {
                    _readyToFire = true;
                    if (debugMode) Debug.Log($"[AdvancedPistol] Ready to fire again after {_timeSinceLastUse:F2}s");
                }
            }
        }

        void UpdateChargeVisuals()
        {
            if (!_isCharging) return;

            // Update charge particles intensity
            if (chargeParticles != null)
            {
                var emission = chargeParticles.emission;
                emission.rateOverTime = Mathf.Lerp(10f, 50f, _chargeTime / timeToFullCharge);
            }

            // Pulse energy cell material
            if (energyCell != null)
            {
                var renderer = energyCell.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    var pulseFactor = Mathf.PingPong(Time.time * 3f, 1f);
                    var intensity = Mathf.Lerp(1f, 3f, _chargeTime / timeToFullCharge);

                    if (renderer.material.HasProperty("_EmissionColor"))
                        renderer.material.SetColor("_EmissionColor", chargedBeamColor * intensity * pulseFactor);
                }
            }
        }

        void SetupBeamRenderer()
        {
            if (beamLineRenderer != null)
            {
                beamLineRenderer.enabled = false;
                beamLineRenderer.startColor = normalBeamColor;
                beamLineRenderer.endColor = normalBeamColor;
                beamLineRenderer.startWidth = 0.05f;
                beamLineRenderer.endWidth = 0.02f;
                beamLineRenderer.useWorldSpace = true;
            }
        }

        void SetupAudio()
        {
            if (chargeSound != null)
            {
                _chargeAudioSource = gameObject.AddComponent<AudioSource>();
                _chargeAudioSource.clip = chargeSound;
                _chargeAudioSource.loop = true;
                _chargeAudioSource.volume = 0.3f;
                _chargeAudioSource.playOnAwake = false;
            }
        }

        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerRightArmController;

            if (debugMode) Debug.Log("[AdvancedPistol] Initialize called");
        }

        public override void PerformToolAction()
        {
            if (debugMode)
                Debug.Log($"[AdvancedPistol] PerformToolAction. Ready: {_readyToFire}, Charged: {_isFullyCharged}");

            if (!_readyToFire)
            {
                if (debugMode) Debug.Log("[AdvancedPistol] Still on cooldown");
                return;
            }

            if (_isCharging && _isFullyCharged)
                FireChargedShot();
            else
                FireNormalShot();

            ResetCharge();
        }

        void FireNormalShot()
        {
            if (debugMode) Debug.Log("[AdvancedPistol] Firing normal shot");

            if (requiresEnergy && !HasSufficientEnergy(energyCostPerNormalShot))
            {
                ShowInsufficientEnergyAlert();
                return;
            }

            // Set cooldown FIRST
            _readyToFire = false;
            _timeSinceLastUse = 0f;
            _currentCooldown = normalCooldown;

            ConsumeEnergy(energyCostPerNormalShot);

            if (physicalRoot != null) AnimateRecoil(1f);
            shootFeedbacks?.PlayFeedbacks();
            SpawnMuzzleFlash(muzzleFlashPrefab);
            PlayShootSound(shootSound);

            try
            {
                ApplyHit(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdvancedPistol] Error in ApplyHit: {e.Message}\n{e.StackTrace}");
                _readyToFire = true; // Reset on error
            }
        }

        void FireChargedShot()
        {
            if (debugMode) Debug.Log("[AdvancedPistol] Firing charged shot");

            if (requiresEnergy && !HasSufficientEnergy(energyCostPerChargedShot))
            {
                ShowInsufficientEnergyAlert();
                return;
            }

            // Set cooldown FIRST
            _readyToFire = false;
            _timeSinceLastUse = 0f;
            _currentCooldown = chargedCooldown;

            ConsumeEnergy(energyCostPerChargedShot);

            if (physicalRoot != null) AnimateRecoil(2f);
            chargedShootFeedbacks?.PlayFeedbacks();
            SpawnMuzzleFlash(chargedMuzzleFlashPrefab);
            PlayShootSound(chargedShootSound);

            try
            {
                ApplyHit(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdvancedPistol] Error in ApplyHit: {e.Message}\n{e.StackTrace}");
                _readyToFire = true; // Reset on error
            }
        }

        public void StartCharging()
        {
            if (!allowChargeShots || !_readyToFire) return;

            if (debugMode) Debug.Log("[AdvancedPistol] Start charging");

            _isCharging = true;
            _chargeTime = 0f;

            if (chargeParticles != null)
                chargeParticles.Play();

            if (_chargeAudioSource != null)
                _chargeAudioSource.Play();

            ChargeToolEvent.Trigger(ChargeToolEventType.Start);
        }

        public void UpdateCharging()
        {
            if (!_isCharging) return;

            _chargeTime += Time.deltaTime;

            // Drain energy while charging
            if (requiresEnergy)
            {
                var energyDrain = chargeEnergyDrainPerSecond * Time.deltaTime;
                if (!HasSufficientEnergy(energyDrain))
                {
                    ResetCharge();
                    ShowInsufficientEnergyAlert();
                    return;
                }

                ConsumeEnergy(energyDrain);
            }

            // Check if fully charged
            if (_chargeTime >= timeToFullCharge && !_isFullyCharged)
            {
                _isFullyCharged = true;
                OnFullyCharged();
            }

            ChargeToolEvent.Trigger(ChargeToolEventType.Update, _chargeTime / timeToFullCharge);
        }

        void OnFullyCharged()
        {
            if (debugMode) Debug.Log("[AdvancedPistol] Fully charged!");

            if (chargeParticles != null)
            {
                var main = chargeParticles.main;
                main.startColor = chargedBeamColor;
            }
        }

        void ResetCharge()
        {
            _isCharging = false;
            _isFullyCharged = false;
            _chargeTime = 0f;

            if (chargeParticles != null)
                chargeParticles.Stop();

            if (_chargeAudioSource != null)
                _chargeAudioSource.Stop();

            ChargeToolEvent.Trigger(ChargeToolEventType.Release);
        }

        public override void ApplyHit()
        {
            ApplyHit(false);
        }

        void ApplyHit(bool isCharged)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || muzzlePosition == null)
            {
                Debug.LogError("[AdvancedPistol] Missing camera or muzzle position!");
                return;
            }

            var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            var didHit = Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore);

            if (debugMode) Debug.Log($"[AdvancedPistol] Raycast hit: {didHit}");

            var endPoint = didHit ? hit.point : ray.GetPoint(range);
            var color = isCharged ? chargedBeamColor : normalBeamColor;
            var width = isCharged ? 0.1f : 0.05f;

            DrawBeam(muzzlePosition.position, endPoint, color, width);

            if (didHit) ProcessHit(hit, isCharged);
        }

        void ProcessHit(RaycastHit hit, bool isCharged)
        {
            var go = hit.collider.gameObject;

            // Hit enemy NPC
            if (go.CompareTag("EnemyNPC"))
            {
                var enemyController = go.GetComponentInParent<EnemyController>();
                if (enemyController != null)
                {
                    var attack = isCharged
                        ? attackProfile?.heavyAttack
                        : attackProfile?.basicAttack;

                    if (attack != null)
                    {
                        enemyController.ProcessAttackDamage(attack);

                        var vfxPrefab = isCharged ? chargedHitEffectPrefab : hitSparksPrefab;
                        if (vfxPrefab != null) SpawnHitVFX(vfxPrefab, hit.point, hit.normal);

                        hitFeedbacks?.PlayFeedbacks();

                        if (debugMode)
                            Debug.Log($"[AdvancedPistol] Hit enemy with {(isCharged ? "charged" : "normal")} shot");
                    }
                }
            }
            // Hit breakable object
            else if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                var hitType = isCharged ? MeleeToolPrefab.HitType.Heavy : MeleeToolPrefab.HitType.Normal;
                var power = isCharged ? 3 : 1;
                breakable.ApplyHit(power, hit.point, hit.normal, hitType);

                SpawnHitVFX(hitSparksPrefab, hit.point, hit.normal);
                hitFeedbacks?.PlayFeedbacks();
            }
            // Generic surface
            else
            {
                SpawnHitVFX(hitSparksPrefab, hit.point, hit.normal);
                hitFeedbacks?.PlayFeedbacks();
            }
        }

        void AnimateRecoil(float intensity)
        {
            physicalRoot.transform.DOKill();
            physicalRoot.transform.localPosition = _initialLocalPos;
            physicalRoot.transform.DOPunchPosition(
                new Vector3(0, 0, 0.001f * intensity),
                0.15f,
                8,
                0.4f);
        }

        void SpawnMuzzleFlash(GameObject flashPrefab)
        {
            if (flashPrefab != null && muzzlePosition != null)
            {
                var flash = Instantiate(flashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                Destroy(flash, 1f);
            }
        }

        void PlayShootSound(AudioClip clip)
        {
            if (clip != null && mainCamera != null) AudioSource.PlayClipAtPoint(clip, mainCamera.transform.position);
        }

        void DrawBeam(Vector3 start, Vector3 end, Color color, float width)
        {
            if (beamLineRenderer == null) return;

            // Stop previous coroutine
            if (_beamCoroutine != null) StopCoroutine(_beamCoroutine);

            beamLineRenderer.startColor = color;
            beamLineRenderer.endColor = color;
            beamLineRenderer.startWidth = width;
            beamLineRenderer.endWidth = width * 0.5f;

            beamLineRenderer.enabled = true;
            beamLineRenderer.SetPosition(0, start);
            beamLineRenderer.SetPosition(1, end);

            _beamCoroutine = StartCoroutine(HideBeamAfterDelay());
        }

        IEnumerator HideBeamAfterDelay()
        {
            yield return new WaitForSeconds(beamDuration);

            if (beamLineRenderer != null) beamLineRenderer.enabled = false;

            _beamCoroutine = null;
        }

        void SpawnHitVFX(GameObject vfxPrefab, Vector3 position, Vector3 normal)
        {
            if (vfxPrefab == null) return;

            var vfxInstance = Instantiate(vfxPrefab, position, Quaternion.LookRotation(normal));
            Destroy(vfxInstance, 2f);
        }

        bool HasSufficientEnergy(float cost)
        {
            var statsManager = PlayerMutableStatsManager.Instance;
            if (statsManager == null) return true;

            return statsManager.CurrentStamina >= cost;
        }

        void ConsumeEnergy(float cost)
        {
            if (!requiresEnergy) return;

            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentStamina,
                PlayerStatsEvent.PlayerStatChangeType.Decrease,
                cost);
        }

        void ShowInsufficientEnergyAlert()
        {
            AlertEvent.Trigger(
                AlertReason.NotEnoughStamina,
                "Not enough energy to fire weapon.",
                "Insufficient Energy");
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject != null ? pistolToolObject.defaultReticle : null;
        }

        public void HandleChargeInput(bool isHeld, bool justPressed, bool justReleased)
        {
            if (justPressed)
                StartCharging();
            else if (isHeld && _isCharging)
                UpdateCharging();
            else if (justReleased && _isCharging) PerformToolAction();
        }
    }
}
