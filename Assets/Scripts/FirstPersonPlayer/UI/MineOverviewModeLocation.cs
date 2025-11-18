using System;
using Events;
using FirstPersonPlayer.UI.LocationButtonBase;
using Helpers.Events;
using Structs;
using UnityEngine;

namespace FirstPersonPlayer.UI
{
    public class MineOverviewModeLocation : OverviewModeLocationButtons
    {
        public string spawnPointId;

        public string sceneName;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public void Initialize(string spawnPointIdVar, string sceneNameVar, object mineName)
        {
            spawnPointId = spawnPointIdVar;
            sceneName = sceneNameVar;
            locationText.text = mineName.ToString();
            
        }

        public override void Interact()
        {
            if (string.IsNullOrEmpty(spawnPointId) || string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("MineOverviewModeLocation missing spawnPointId/sceneName");
                return;
            }

            if (!RequireAccessPass()) return;

            SpawnEvent.Trigger(SpawnEventType.ToMine, sceneName, GameMode.FirstPerson, spawnPointId);
        }

        public override void ShowCanvasGroup()
        {
            throw new NotImplementedException();
        }

        public override void HideCanvasGroup()
        {
            throw new NotImplementedException();
        }
    }
}
