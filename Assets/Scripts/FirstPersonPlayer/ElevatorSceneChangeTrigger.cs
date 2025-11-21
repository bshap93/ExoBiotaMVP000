using System.Collections.Generic;
using Helpers.Events;
using Manager;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirstPersonPlayer
{
    public class ElevatorSceneChangeTrigger : MonoBehaviour
    {
        [ValueDropdown(nameof(GetSceneNames))] [SerializeField]
        string sceneToLoad;


        [ValueDropdown(nameof(GetSpawnPoints))] [SerializeField]
        string spawnPointId;


        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
            {
                SaveDataEvent.Trigger();
                var spawnInfo = new SpawnInfo
                {
                    SceneName = sceneToLoad,
                    SpawnPointId = spawnPointId,
                    Mode = GameMode.FirstPerson
                };

                // SpawnEvent.Trigger(
                //     SpawnEventType.ToCaverns, spawnInfo.SceneName, GameMode.FirstPerson,
                //     spawnInfo.SpawnPointId
                // );

                SceneManager.LoadScene("BridgeDownChokedCavern");
            }
        }

        static IEnumerable<string> GetSceneNames()
        {
            return PlayerSpawnManager.GetSceneOptions();
        }

        IEnumerable<string> GetSpawnPoints()
        {
            return PlayerSpawnManager.GetSpawnPointIdOptions();
        }
    }
}
