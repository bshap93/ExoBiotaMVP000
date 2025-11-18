using Overview.OverviewMode.ScriptableObjectDefinitions;
using SceneScripts.Spawn;
using UnityEngine.Serialization;

namespace Spawn
{
    public class DockSpawnPoint : SpawnPoint
    {
        [FormerlySerializedAs("DockDefinition")]
        public DockDefinition dockDefinition;
    }
}