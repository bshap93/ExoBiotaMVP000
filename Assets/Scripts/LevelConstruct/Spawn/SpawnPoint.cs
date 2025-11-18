using Manager;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

namespace SceneScripts.Spawn
{
    [DisallowMultipleComponent]
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] protected GameMode mode;

        [ValueDropdown("GetSpawnPointIdOptions")]
        public string spawnPointId;

        public string Id
        {
            get => spawnPointId;
            set => spawnPointId = value;
        }

        public GameMode Mode => mode;
        public Transform Xform => transform;

        private static string[] GetSpawnPointIdOptions()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions();
        }
    }
}