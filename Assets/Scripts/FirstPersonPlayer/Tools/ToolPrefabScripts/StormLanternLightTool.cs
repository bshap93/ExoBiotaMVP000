using System;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class StormLanternLightTool : MonoBehaviour, IRuntimeTool
    {
        [FormerlySerializedAs("LanternMaterial")] [SerializeField]
        Material lanternMaterial; // Material for the lantern light

        [SerializeField] MMFeedbacks switchOnFB; // Feedback for switching on the lantern
        [SerializeField] MMFeedbacks switchOffFB; // Feedback for switching off the lantern


        [SerializeField] Light stormLanternPointLight; // Point light component for the lantern

        [SerializeField] MMFeedbacks stormLanternPointLightFeedback;
        [SerializeField] MMFeedbacks toggleStormLanternFeedback;
        [SerializeField] LightSourceToolItemObject stormLanternLightSourceToolItemObject;

        [SerializeField] MMFeedbacks equippedFeedbacks;


        bool isLanternOn; // State to track if the lantern is on
        LightSourceToolItemObject lightSourceToolItemObject;

        public void Initialize(PlayerEquipment owner)
        {
            if (!(owner is PlayerEquipment))
            {
                Debug.LogError("StormLanternLightTool: Owner is not of type LeftPlayerEquipment.");
                return;
            }

            if (lanternMaterial == null)
            {
                lanternMaterial = GetComponent<Renderer>()?.material;
                if (lanternMaterial == null)
                    Debug.LogError(
                        "StormLanternLightTool: LanternMaterial is not assigned and could not be found on the GameObject.");
            }

            if (stormLanternPointLight == null)
            {
                stormLanternPointLight = GetComponentInChildren<Light>();
                if (stormLanternPointLight == null)
                    Debug.LogError(
                        "StormLanternLightTool: stormLanternPointLight is not assigned and could not be found in children.");
            }

            if (lightSourceToolItemObject == null)
                lightSourceToolItemObject = owner.CurrentToolSo as LightSourceToolItemObject;

            LightEvent.Trigger(lightSourceToolItemObject != null ? LightEventType.TurnOn : LightEventType.TurnOff);
        }

        public void Use()
        {
            // Toggle the lantern light on or off
            isLanternOn = !isLanternOn;
            stormLanternPointLight.enabled = isLanternOn;
            if (isLanternOn)
                switchOnFB?.PlayFeedbacks();
            else
                switchOffFB?.PlayFeedbacks();

            toggleStormLanternFeedback?.PlayFeedbacks();
            LightEvent.Trigger(isLanternOn ? LightEventType.TurnOn : LightEventType.TurnOff);
        }

        public void Unequip()
        {
        }

        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return false;
        }

        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return null;
        }

        public bool CanAbortAction()
        {
            return false;
        }

        public MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }

        public CanBeAreaScannedType GetDetectableType()
        {
            return CanBeAreaScannedType.NotDetectableByScan;
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentTextureIndex()
        {
            return -1;
        }

        public bool CanInteractWithTextureIndex(int terrainIndex)
        {
            return false;
        }
    }
}
