using Overview.NPC;
using TMPro;
using UnityEngine;

namespace Overview.UI
{
    public class NPCUIIdentifierPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text npcNameText;


        public void SetInfo(NpcDefinition def)
        {
            if (def.characterName == null)
            {
                Debug.LogError("NPC Definition characterName is null.");
                return;
            }

            npcNameText.text = def.characterName;
        }
    }
}
