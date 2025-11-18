using Helpers.Events;
using Manager.Global;
using SceneScripts.Spawn;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using Utilities.Static;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace LevelConstruct.Spawn
{
    [RequireComponent(typeof(SpawnPoint))]
    public class SpawnCheckpoint : MonoBehaviour
    {
#if UNITY_EDITOR
        [ValueDropdown("GetListOfTags")] [SerializeField]
#endif
        string playerPawnTag;
        SpawnPoint _point;


        void Awake()
        {
            _point = GetComponent<SpawnPoint>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(playerPawnTag)) return;
            if (!other.CompareTag(playerPawnTag)) return;

            if (SpawnSystem.CurrentSpawn.SpawnPointId == _point.Id)
                return; // already current → ignore

            var spawnInfo = new SpawnInfo
            {
                SceneName = gameObject.scene.name,
                Mode = GameStateManager.Instance.CurrentMode,
                SpawnPointId = _point.Id
            };


            CheckpointEvent.Trigger(spawnInfo);
            Debug.Log($"Checkpoint reached → attempted save of {spawnInfo}");
        }

#if UNITY_EDITOR
        public static string[] GetListOfTags()
        {
            return InternalEditorUtility.tags;
        }
#endif
    }
}
