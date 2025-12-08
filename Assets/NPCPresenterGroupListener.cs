using Helpers.Events.Dialog;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCPresenterGroupListener : MonoBehaviour, MMEventListener<DialoguePresentationEvent>
{
    [Header("Unchanging Texts")]
    // Don't change its font
    [SerializeField]
    TMP_Text characterNameTextAvatar;
    [Header("NPC Side Texts")] [FormerlySerializedAs("CharacterNameText")] [SerializeField]
    TMP_Text characterNameText;
    [SerializeField] TMP_Text npcLineText;
    [SerializeField] TMP_Text lastLineText;


    [SerializeField] TMP_FontAsset modernGalacticFont;
    [SerializeField] TMP_FontAsset sheoliteFont;


    void OnEnable()
    {
        this.MMEventStartListening();
    }

    void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(DialoguePresentationEvent eventType)
    {
        if (eventType.EventType == DialoguePresentationEventType.ChangeFontsOfNPCSide)
        {
        }
    }
}
