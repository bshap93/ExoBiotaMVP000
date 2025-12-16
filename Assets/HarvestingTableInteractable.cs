using System;
using UnityEngine;
using Utilities.Interface;

public class HarvestingTableInteractable : MonoBehaviour, IRequiresUniqueID
{
    public string harvestingTableId;

    public string UniqueID => harvestingTableId;
    public void SetUniqueID()
    {
        harvestingTableId = Guid.NewGuid().ToString();
    }
    public bool IsUniqueIDEmpty()
    {
        return string.IsNullOrEmpty(harvestingTableId);
    }
}
