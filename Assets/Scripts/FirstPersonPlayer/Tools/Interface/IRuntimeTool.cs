using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.Interface
{
    public interface IRuntimeTool
    {
        /// Called right after the prefab is spawned & parented.
        void Initialize(PlayerEquipment owner);

        /// Fire / use the tool.
        void Use();

        /// Called before being destroyed / unequipped.
        void Unequip();

        bool CanInteractWithObject(GameObject colliderGameObject);
        // int GetCurrentTextureIndex();
        // bool CanInteractWithTextureIndex(int terrainIndex);

        Sprite GetReticleForTool(GameObject colliderGameObject);

        bool CanAbortAction();

        MMFeedbacks GetEquipFeedbacks();

        CanBeAreaScannedType GetDetectableType() ;


        //MMFeedbacks GetUseToolFeedbacks();
    }
}
