using System;
using Events;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Inventory;
using LevelConstruct.Highlighting;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using Objectives.ScriptableObjects;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class KeyCrateBoxInteractable : MonoBehaviour, IInteractable, IRequiresUniqueID, IHoverable, IBillboardable
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public string uniqueID;
        [SerializeField] KeyItemObject keyItem;
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Sprite icon;
        [SerializeField] HighlightEffectController effectController;

        [Header("Items inside the crate box")] [SerializeField]
        bool hasOtherItems;
        [ShowIf("hasOtherItems")] [SerializeField]
        MyBaseItem[] items;

        [SerializeField] bool givesMoney;
        [ShowIf("givesMoney")] [SerializeField]
        int moneyAmount;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks getKeyItemFeedback;
        [SerializeField] MMFeedbacks alreadyGotKeyFeedback;

        [SerializeField] GameObject holoScreenMesh;
        [SerializeField] int screenlayer09Index = 5;
        
        [Header("Objective Options")]
        [SerializeField] InteractableObjectiveModifier.ObjectiveActionType objectiveActionType;
        [SerializeField] ObjectiveObject attachedObjective;

        bool _hasBeenOpened;

        public string GetName()
        {
            return "Key Box";
        }
        public Sprite GetIcon()
        {
            return icon;
        }
        public string ShortBlurb()
        {
            if (!hasOtherItems)
                return "Contains a virtual key.";

            return "Contains a key and $" + items.Length + " other items.";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.pushIcon;
        }
        public string GetActionText()
        {
            return "Open";
        }
        public bool OnHoverStart(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public void Interact()
        {
            if (!_hasBeenOpened)

            {
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null,
                    keyItem.TargetInventoryName, keyItem, 1, 0, GlobalInventoryManager.Instance.playerId);

                getKeyItemFeedback?.PlayFeedbacks();
                
                PerformObjectiveAction();
            }
            else
            {
                alreadyGotKeyFeedback?.PlayFeedbacks();
            }

            // TODO: Add other items to inventory if hasOtherItems is true.

            _hasBeenOpened = true;
            
            effectController.SetSecondaryStateHighlightColor();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            return true;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

        public void SetHoloScreenTint(Color newColor, int index = 2)
        {
            var rendererVar = holoScreenMesh.GetComponent<MeshRenderer>();
            var mats = rendererVar.materials;

            var newMat = new Material(mats[index]); // clone instance-safe
            newMat.SetColor(BaseColor, newColor);

            //  enhance emissive glow
            // if (newMat.HasProperty("_EmissionColor"))
            // {
            //     newMat.EnableKeyword("_EMISSION");
            //     newMat.SetColor("_EmissionColor", newColor * 1.5f);
            // }

            mats[index] = newMat;
            rendererVar.materials = mats;
        }

        public void PerformObjectiveAction()
        {
            var objective = attachedObjective;
            var objectiveAction = objectiveActionType;
            if (objective == null)
            {
                return;
            }
            
            switch (objectiveAction)
            {
                case InteractableObjectiveModifier.ObjectiveActionType.Add:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveAdded);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Activate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveActivated);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Complete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Deactivate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeactivated) ;
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Delete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeleted);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
