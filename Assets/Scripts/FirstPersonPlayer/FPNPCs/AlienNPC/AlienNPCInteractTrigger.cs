using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    public class AlienNPCInteractTrigger : MonoBehaviour
    {
        [SerializeField] AlienNPCController alienNPCController;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer"))
            {
                alienNPCController.Interact();
                Debug.Log("Player entered Alien NPC interaction trigger.");
            }
        }

        void OnTriggerExit(Collider other)
        {
        }
    }
}
