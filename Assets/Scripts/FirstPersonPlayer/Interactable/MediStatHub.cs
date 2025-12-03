using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class MediStatHub : MonoBehaviour, IRequiresUniqueID
    {
        public string uniqueID;
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = System.Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }
    }
}
