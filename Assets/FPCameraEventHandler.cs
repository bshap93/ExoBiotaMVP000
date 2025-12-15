using DG.Tweening;
using Helpers.Events.Combat;
using MoreMountains.Tools;
using Unity.Cinemachine;
using UnityEngine;

public class FPCameraEventHandler : MonoBehaviour, MMEventListener<PlayerDamageEvent>
{
    [SerializeField] CinemachineCamera cinemachineCamera;
    // [SerializeField] DOTweenAnimation dOTweenAnimation;

    void OnEnable()
    {
        this.MMEventStartListening();
    }

    void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(PlayerDamageEvent e)
    {
        if (e.HitType == PlayerDamageEvent.HitTypes.CriticalHit)
            ShakeCamera(0.1f, 0.05f);
        // dOTweenAnimation.DORestart();
        else if (e.HitType == PlayerDamageEvent.HitTypes.Normal) ShakeCamera(0.05f, 0.05f);
        // dOTweenAnimation.DORestart();
    }

    void ShakeCamera(float intensity, float duration)
    {
        transform.DOShakePosition(duration, new Vector3(intensity, intensity, intensity))
            .SetEase(Ease.InOutElastic).SetLoops(2, LoopType.Yoyo);
    }
}
