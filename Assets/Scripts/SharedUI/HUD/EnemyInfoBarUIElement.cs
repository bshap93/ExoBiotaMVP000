using DG.Tweening;
using Helpers.Events.Combat;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI.HUD
{
    public class EnemyInfoBarUIElement : MonoBehaviour, MMEventListener<EnemyDamageEvent>
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text enemyNameText;
        [SerializeField] MMProgressBar enemyHealthBar;
        [Header("Feedbacks")] [SerializeField] MMFeedbacks infoBarDeathFeedbacks;
        [SerializeField] MMFeedbacks hitEnemyFeedbacks;
        [SerializeField] MMFeedbacks criticalHitEnemyFeedbacks;
        [Header("Update")] [Tooltip("Minimum absolute change before we push a UI update")] [SerializeField]
        float epsilon = 0.001f;
        [SerializeField] float fadeInOnDamageDuration = 0.1f;
        [SerializeField] float fadeOutOnTimeoutDuration = 0.3f;
        [SerializeField] float visibleDurationAfterDamageDealt = 5f;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        Tween _fadeTween;

        bool _isVisible;

        float _timeSinceLastDamageDealt;

        void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        void Update()
        {
            if (!_isVisible) return;
            _timeSinceLastDamageDealt += Time.deltaTime;

            if (_timeSinceLastDamageDealt >= visibleDurationAfterDamageDealt) FadeOut(fadeOutOnTimeoutDuration);
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(EnemyDamageEvent eventType)
        {
            if (enemyNameText != null)
                enemyNameText.text = eventType.EnemyName;


            if (eventType.EventType == DamageEventType.DealtDamage)
            {
                FadeIn(fadeInOnDamageDuration);
                TryUpdateBar(ref eventType.LastHealth, eventType.CurrentHealth, 0f, eventType.MaxHealth);
                hitEnemyFeedbacks?.PlayFeedbacks();
                _timeSinceLastDamageDealt = 0f;
            }

            if (eventType.EventType == DamageEventType.CriticalHitDamage)
            {
                FadeIn(fadeInOnDamageDuration);
                TryUpdateBar(ref eventType.LastHealth, eventType.CurrentHealth, 0f, eventType.MaxHealth);
                criticalHitEnemyFeedbacks?.PlayFeedbacks();
                _timeSinceLastDamageDealt = 0f;
            }
            else if (eventType.EventType == DamageEventType.Death)
            {
                infoBarDeathFeedbacks?.PlayFeedbacks();
                FadeOut(fadeOutOnTimeoutDuration);
                ResetBar();
            }
            else
            {
                Debug.Log("Nothing for now beyond damage dealt");
            }
        }

        void ResetBar()
        {
        }

        void TryUpdateBar(ref float last, float current, float min, float max)
        {
            if (enemyHealthBar == null) return;
            current = Mathf.Clamp(current, min, max);

            // Only push an update when the source value actually changed
            if (float.IsNaN(last) || Mathf.Abs(current - last) > epsilon)
            {
                // Smooth animated update (MMProgressBar handles the tween)
                enemyHealthBar.UpdateBar(current, min, max);
                last = current;
            }
        }

        public void FadeIn(float duration)
        {
            if (_isVisible && canvasGroup.alpha >= 1) return;
            _fadeTween?.Kill();
            _isVisible = true;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _fadeTween = canvasGroup
                .DOFade(1f, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }

        public void FadeOut(float duration)
        {
            if (!_isVisible) return;
            _fadeTween?.Kill();
            _isVisible = false;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _fadeTween = canvasGroup
                .DOFade(0f, duration)
                .SetEase(Ease.InQuad);
        }
    }
}
