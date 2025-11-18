using System.Collections;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using OccaSoftware.ResponsiveSmokes.Runtime;
using SharedUI.Interface;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public class BioOrganismGasEmitterNode : BioOrganismBase
    {
        [Header("Scene References")] [Tooltip("InteractiveSmoke child that renders & times the gas cloud.")]
        public InteractiveSmoke smoke;

        [Tooltip("Outer detection collider (on the root) to trigger the release. Trigger recommended.")]
        public Collider detectionZone;

        [Tooltip("Inner, smaller trigger on the mesh used to contaminate the player while the smoke is alive.")]
        public Collider lethalZone;

        [Header("Contamination")]
        [Tooltip("One-time burst applied when the player FIRST enters the lethalZone while smoke is alive.")]
        public float contaminationOnEnter = 6f;

        [Tooltip("Contamination applied per second while the player REMAINS inside the lethalZone and smoke is alive.")]
        public float contaminationPerSecond = 2f;

        [Header("Trigger Fallback (no Rigidbody required)")]
        [Tooltip(
            "If your triggers aren’t firing (no Rigidbody), enable this to poll the detectionZone bounds each frame.")]
        public bool useOverlapFallback;

        [SerializeField] MMFeedbacks releaseFeedbacks;

        bool _gasReleased;
        bool _hazardActive; // true while smoke.IsAlive()
        bool _playerInsideInner; // track inner-zone entry for burst
        Transform _playerT;

        protected override void Awake()
        {
            base.Awake();
            // keep smoke off until released
            if (smoke) smoke.gameObject.SetActive(false);
        }

        void Update()
        {
            // Optional, robust fallback: release if player stands inside detection bounds
            if (!_gasReleased && useOverlapFallback && detectionZone && TryGetPlayer(out _playerT))
                if (detectionZone.bounds.Contains(_playerT.position))
                    ReleaseGas();

            // While smoke is alive, handle "stay" contamination tick if player is inside lethal zone
            if (_hazardActive && _playerT && lethalZone && lethalZone.bounds.Contains(_playerT.position))
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationPerSecond * Time.deltaTime);

            // Stop hazard the moment the smoke dies (Cleanup ran)
            if (_hazardActive && (!smoke || !smoke.IsAlive()))
            {
                _hazardActive = false;
                _playerInsideInner = false;
            }
        }

        // --- Outer trigger: release gas on player entry ---
        void OnTriggerEnter(Collider other)
        {
            if (_gasReleased) return;
            if (!other.CompareTag("FirstPersonPlayer")) return;

            _playerT = other.transform;
            ReleaseGas();
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.transform == _playerT) _playerT = null;
            _playerInsideInner = false;
        }

        public override bool OnHoverStart(GameObject go)
        {
            if (!bioOrganismType) return true;

            var recognizable = bioOrganismType.identificationMode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable; // later: OR with analysis progression
            var nameToShow = showKnown ? bioOrganismType.organismName : bioOrganismType.UnknownName;
            var iconToShow = showKnown
                ? bioOrganismType.organismIcon
                : bioOrganismType.organismIcon ?? ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? bioOrganismType.shortDescription : string.Empty;

            data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                ExaminationManager.Instance?.iconRepository.bioOrganismIcon,
                GetActionText(recognizable)
            );

            data.Id = bioOrganismType.organismID;

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                if (ExaminationManager.Instance != null)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, actionId,
                        string.IsNullOrEmpty(actionText) ? null : actionText,
                        ExaminationManager.Instance.iconRepository.airSampleIcon);

            return true;
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Toxic Cloud";
        }

        // --- Inner trigger: burst once on entry (only while smoke is alive) ---
        void OnTriggerEnterInner(Collider other)
        {
            if (!_hazardActive) return;
            if (!other.CompareTag("Player")) return;

            // One-time burst on first entry while alive
            if (!_playerInsideInner)
            {
                _playerInsideInner = true;
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    contaminationOnEnter);
            }
        }

        // Helper for child trigger forwarding (see note below)
        public void NotifyInnerEnter(Collider other)
        {
            OnTriggerEnterInner(other);
        }
        public void NotifyInnerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInsideInner = false;
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

        IEnumerator StopHazardNextFrame()
        {
            yield return null;
            _hazardActive = false;
        }
        bool TryGetPlayer(out Transform t)
        {
            if (_playerT)
            {
                t = _playerT;
                return true;
            }

            var go = GameObject.FindGameObjectWithTag("Player");
            t = go ? go.transform : null;
            _playerT = t;
            return t != null;
        }
    }
}
