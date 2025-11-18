using System;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Domains.Player.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using Inventory;
using Manager;
using Manager.SceneManagers;
using Manager.UI;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    [DisallowMultipleComponent]
    public class BioOrganismBreakableNode : BioOrganismBase, IInteractable,
        MMEventListener<GatedBreakableInteractionEvent>, IGatedInteractable
    {
        [FormerlySerializedAs("gatedInteractionDetails")] [SerializeField]
        GatedBreakableInteractionDetails gatedBreakableInteractionDetails;

        [SerializeField] HatchetBreakable hatchetBreakable;

        [SerializeField] MMFeedbacks loopedInteractionFeedbacks;
        [SerializeField] MMFeedbacks startInteractionFeedbacks;
        bool _hasBeenBroken;
        bool _isProcessingInteraction;
        List<string> _toolsFound;
        protected override void Awake()
        {
            base.Awake();
            // Ensure uniqueID is set FIRST
            // if (string.IsNullOrEmpty(uniqueID)) uniqueID = Guid.NewGuid().ToString();
            // Ensure hatchetBreakable uses the same uniqueID
            if (hatchetBreakable != null)
            {
                if (string.IsNullOrEmpty(hatchetBreakable.uniqueIdForPersistence))
                    hatchetBreakable.uniqueIdForPersistence = uniqueID;
                else
                    // Make sure they match
                    uniqueID = hatchetBreakable.uniqueIdForPersistence;
            }
        }

        void Start()
        {
            // Check synchronously - no coroutine
            if (DestructableManager.Instance != null && DestructableManager.Instance.IsDestroyed(uniqueID))
                Destroy(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening();
        }
        public List<string> HasToolForInteractionInInventory()
        {
            var possibleTools = gatedBreakableInteractionDetails.requiredToolIDs;
            var foundTools = new List<string>();

            var playerInventory =
                GlobalInventoryManager.Instance.playerInventory;

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, playerInventory);
                if (itemInInventory != null) foundTools.Add(toolID);
            }

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, equipmentInventory);
                if (itemInInventory != null && !foundTools.Contains(toolID))
                    foundTools.Add(toolID);
            }

            return foundTools;
        }

        public MyBaseItem GetItemByID(string itemID, MoreMountains.InventoryEngine.Inventory inventory)
        {
            foreach (var item in inventory.Content)
            {
                if (item == null) continue;
                if (item.ItemID == itemID)
                {
                    // Option 1: Cast to MyBaseItem if your inventory actually stores those
                    if (item is MyBaseItem myBaseItem)
                        return myBaseItem;

                    // Option 2 (recommended): Reload the definition from Resources
                    var def = Resources.Load<MyBaseItem>($"Items/{itemID}");
                    if (def != null)
                        return def;

                    Debug.LogWarning($"Item '{itemID}' found in inventory but not in Resources/Items/");
                    return null;
                }
            }

            return null;
        }

        public bool CanInteract(out GatedInteractionManager.ReasonWhyCannotInteract reason)
        {
            var currentStamina = PlayerStatsManager.Instance.CurrentStamina;
            if (currentStamina - gatedBreakableInteractionDetails.staminaCost < 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina;
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina,
                    "You do not have enough stamina to perform this action.",
                    "Not Enough Stamina");

                return false;
            }

            if (!gatedBreakableInteractionDetails.requireTools)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
                return true;
            }

            _toolsFound = HasToolForInteractionInInventory();
            if (_toolsFound.Count == 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool;
                return false;
            }

            reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
            return true;
        }
        public void Interact()
        {
            if (_hasBeenBroken)
            {
                Debug.LogWarning($"BioOrganismBreakableNode [{uniqueID}]: Cannot interact, already broken");
                return;
            }

            if (!CanInteract(out var reason))
            {
                if (reason == GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction, "You need the appropriate axe to destroy this organism.",
                        "Lacking Necessary Tool");
                else if (reason == GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina)
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughStamina, "You do not have enough stamina to perform this action.",
                        "Not Enough Stamina");

                return;
            }

            
            // Equip best tool
            EquipBestTool(gatedBreakableInteractionDetails, _toolsFound);
            // GatedBreakableInteractionEvent.Trigger(
            //     GatedInteractionEventType.TriggerGateUI, gatedBreakableInteractionDetails, uniqueID, _toolsFound);
        }

        public void OnInteractionStart()
        {
            startInteractionFeedbacks?.PlayFeedbacks();
            loopedInteractionFeedbacks?.PlayFeedbacks();
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
        }
        public void OnInteractionEnd(string subjectUniquedID)
        {
            loopedInteractionFeedbacks?.StopFeedbacks();
            if (subjectUniquedID != uniqueID)
            {
                _isProcessingInteraction = false;
                return;
            }

            if (_hasBeenBroken)
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode [{uniqueID}]: Already broken, ignoring duplicate break attempt");

                _isProcessingInteraction = false;
                return;
            }


            if (hatchetBreakable == null)
            {
                hatchetBreakable = GetComponent<HatchetBreakable>();
                if (hatchetBreakable == null)
                    // Debug.LogWarning("BioOrganismBreakableNode: No HatchetBreakable component found.");
                {
                    _isProcessingInteraction = false;
                    return;
                }
            }

            _hasBeenBroken = true;

            // Perform the break action
            if (hatchetBreakable != null)
            {
                hatchetBreakable.BreakInstantly();
                DestructableEvent.Trigger(DestructableEventType.Destroyed, uniqueID, transform);
            }

            _isProcessingInteraction = false;
        }

        public bool CanInteract()
        {
            return CanInteract(out _);
        }
        public void OnMMEvent(GatedBreakableInteractionEvent eventType)
        {
            if (_hasBeenBroken)
                return;

            if (string.IsNullOrWhiteSpace(eventType.SubjectUniqueID) ||
                string.IsNullOrWhiteSpace(uniqueID))
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode: Null or empty uniqueID detected. Event: '{eventType.SubjectUniqueID}', This: '{uniqueID}'");

                return;
            }

            // Use Trim() and case-sensitive comparison to ensure exact match
            if (!eventType.SubjectUniqueID.Trim().Equals(uniqueID.Trim(), StringComparison.Ordinal))
                // This event is for a different object
                return;

            // Guard against re-entry
            if (_isProcessingInteraction && eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode [{uniqueID}]: Already processing, ignoring duplicate CompleteInteraction event");

                return;
            }


            // if (eventType.SubjectUniqueID != uniqueID)
            //     return; // Ignore events for other interactables

            if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                OnInteractionStart();
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                _isProcessingInteraction = true;
                OnInteractionEnd(eventType.SubjectUniqueID);
            }
        }
        
        

        void EquipBestTool(GatedBreakableInteractionDetails details, List<string> toolsFound)
        {
            var toolID = details.GetMostEfficientRequiredToolID(toolsFound);


            var inventory = GlobalInventoryManager.Instance.playerInventory;
            if (inventory == null) return;
            var bestTool = inventory.Content.FirstOrDefault(s => s != null && s.ItemID == toolID);
            var sourceIndex = Array.IndexOf(inventory.Content, bestTool);
            if (bestTool == null) return;

            // scannerItem.Equip("Player1");
            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, bestTool.TargetInventoryName, bestTool, 1, sourceIndex,
                "Player1");
        }

        public override bool OnHoverStart(GameObject go)
        {
            if (!bioOrganismType) return true;

            var recognizable = bioOrganismType.identificationMode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable; // later: OR with analysis progression
            var nameToShow = showKnown ? bioOrganismType.organismName : bioOrganismType.UnknownName;
            var iconToShow = showKnown
                ? bioOrganismType.organismIcon
                : bioOrganismType.organismIcon ?? ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? bioOrganismType.shortDescription : bioOrganismType.UnknownDescription;

            data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                ExaminationManager.Instance?.iconRepository.bioOrganismIcon,
                GetActionText(recognizable)
            )
            {
                Id = bioOrganismType.organismID
            };

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                if (ExaminationManager.Instance != null)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, actionId,
                        string.IsNullOrEmpty(actionText) ? null : actionText,
                        ExaminationManager.Instance.iconRepository.axeIcon);

            return true;
        }


        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Clear Growth";
        }
    }
}
