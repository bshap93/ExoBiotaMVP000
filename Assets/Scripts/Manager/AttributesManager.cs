using Helpers;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class AttributesManager : MonoBehaviour, ICoreGameService, MMEventListener<AttributeEvent>
    {
        public void Save()
        {
            throw new System.NotImplementedException();
        }
        public void Load()
        {
            throw new System.NotImplementedException();
        }
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
        public void ConditionalSave()
        {
            throw new System.NotImplementedException();
        }
        public void MarkDirty()
        {
            throw new System.NotImplementedException();
        }
        public string GetSaveFilePath()
        {
            throw new System.NotImplementedException();
        }
        public void CommitCheckpointSave()
        {
            throw new System.NotImplementedException();
        }
        public bool HasSavedData()
        {
            throw new System.NotImplementedException();
        }
        public void OnMMEvent(AttributeEvent eventType)
        {
            throw new System.NotImplementedException();
        }
    }
}
