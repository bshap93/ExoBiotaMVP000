using InventoryEngine.InventoryEngine.InventoryEngine.Scripts.Items;
using UnityEditor;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "OreItemObject", menuName = "Scriptable Objects/Items/OreItemObject", order = 0)]
    public class OreItemObject : MyBaseItem 
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (ActionIcon == null)
            {
                // Load once from Resources (put your Pickaxe sprite in a Resources folder)
                ActionIcon = Resources.Load<Sprite>("Images/Pickaxe_Image");
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}