using System;
using Helpers.ScriptableObjects.Animation;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "BaseTool", menuName = "MoreMountains/InventoryEngine/BaseTool", order = 0)]
    [Serializable]
    public class BaseTool : MyBaseItem
    {
        [Header("Runtime")] public GameObject FPToolPrefab; // must have an IRuntimeTool on it

        [FormerlySerializedAs("Cooldown")] public float cooldown; // optional, leave 0 to ignore

        public ToolAnimationSet toolAnimationSet;
    }
}
