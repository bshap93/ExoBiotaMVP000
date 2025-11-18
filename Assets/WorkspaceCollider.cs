using System.Collections.Generic;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public class WorkspaceCollider : MonoBehaviour
{
    [SerializeField] MMFeedbacks enterIntendedItemFeedbacks;

    readonly HashSet<string> _itemPickerUniqueIDs = new();

    void OnTriggerEnter(Collider other)
    {
        foreach (var varTag in tagsCheckedFor)
            if (other.CompareTag(varTag))
            {
                var uniqueID = other.GetComponent<ItemPicker>().uniqueID;
                if (_itemPickerUniqueIDs.Contains(uniqueID)) return;
                enterIntendedItemFeedbacks?.PlayFeedbacks();
                _itemPickerUniqueIDs.Add(uniqueID);
            }
    }


#if UNITY_EDITOR
    List<string> GetAllTags()
    {
        var tags = new List<string>();
        foreach (var tag in InternalEditorUtility.tags) tags.Add(tag);
        return tags;
    }

    [ValueDropdown(nameof(GetAllTags))]
#endif
    [SerializeField]
    List<string> tagsCheckedFor;
}
