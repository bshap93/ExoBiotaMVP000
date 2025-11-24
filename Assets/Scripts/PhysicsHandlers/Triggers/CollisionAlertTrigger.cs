using Helpers.Events;
using UnityEngine;

namespace PhysicsHandlers.Triggers
{
    public class CollisionAlertTrigger : MonoBehaviour
    {
        [SerializeField] string uniqueID;
        [SerializeField] string alertMessage;
        [SerializeField] string alertTitle;
        [SerializeField] AlertReason alertReason;
        [SerializeField] AlertType alertType = AlertType.Basic;
        [SerializeField] GameObject alertTriggerCollider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
        public void TriggerAlert()
        {
            AlertEvent.Trigger(alertReason, alertMessage, alertTitle, alertType);
            alertTriggerCollider.SetActive(false);
        }
    }
}
