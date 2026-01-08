using System;
using Helpers.Events.Status;
using UnityEngine;

public class ContaminationZone : MonoBehaviour
{
    [SerializeField] float contaminationPerSecond = 1f;
    void Start()
    {
    }

    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
    }

    void OnTriggerExit(Collider other)
    {
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
            // Apply continuous contamination
            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentContamination,
                PlayerStatsEvent.PlayerStatChangeType.Increase,
                contaminationPerSecond * Time.deltaTime);
    }
}
