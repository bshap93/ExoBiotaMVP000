using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using Events;
using FirstPersonPlayer.Tools;
using Gameplay.Events;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using Manager.SceneManagers.Pickable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public class PlayerPropPickup : MonoBehaviour
    {
        public enum ItemStatus
        {
            None,
            Moved,
            Placed
        }

        [Header("Refs")] [SerializeField] public Transform holdPoint;
        [SerializeField] LayerMask pickupMask;

        [Header("UI Help Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionID;
        [SerializeField] string actionText;


        [Header("References")] [SerializeField]
        PlayerInteraction playerInteraction;
        public Rigidbody heldRb;
        [SerializeField] PlayerEquipment rHandEquipment;
        [SerializeField] PlayerEquipment lHandEquipment;

        [Header("Spring Settings")] [SerializeField]
        float spring = 800f;
        [SerializeField] float damper = 60f;


        float _interactionDistance;


        ItemStatus _itemStatus;

        SpringJoint joint;

        RewiredFirstPersonInputs rewired;


        void Update()
        {
            // Fire1 (left mouse) → pick / drop.  Replace with Rewired action if you use Rewired
            if (rewired.pickUpProp)
                if (heldRb == null)
                    TryPickUp();
                else
                    Drop();
        }

        void FixedUpdate()
        {
            if (heldRb)
                // Drag the object toward HoldPoint each physics tick
                joint.connectedAnchor = holdPoint.position;
        }


        void OnEnable()
        {
            rewired = GetComponent<RewiredFirstPersonInputs>();
            if (rewired == null) Debug.LogError("RewiredFirstPersonInputs component not found on PlayerPropPickup.");
            _interactionDistance = playerInteraction.interactionDistance;
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif

        public bool AreBothHandsOccupied()
        {
            var rHandIsOccupied = rHandEquipment.CurrentRuntimeTool != null;
            var lHandIsOccupied = lHandEquipment.CurrentRuntimeTool != null;


            if (rHandIsOccupied && lHandIsOccupied) return true;

            return false;
        }


        void TryPickUp()
        {
            if (!PickableManager.Instance.allowPickingUpPhysicalItems) return;
            if (Camera.main == null) return;
            if (!Physics.Raycast(
                    Camera.main.ViewportPointToRay(Vector3.one * 0.5f),
                    out var hit, playerInteraction.interactionDistance, pickupMask))
                return;

            // if (AreBothHandsOccupied()) return;


            var rb = hit.rigidbody;
            var itemPicker = rb.gameObject.GetComponent<ItemPicker>();
            if (rb == null) return;
            if (itemPicker == null) return;

            rb.isKinematic = false;

            // Build a spring joint at runtime so physics still resolve against environment
            joint = rb.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.zero; // connect to nothing (acts like gravity gun)
            joint.anchor = Vector3.zero;
            joint.spring = spring;
            joint.damper = damper;
            joint.minDistance = 0f;
            joint.maxDistance = 0f;

            // Physics.IgnoreCollision(heldRb.GetComponent<Collider>(),
            //     GetComponent<CharacterController>(), true);

            heldRb = rb;
            heldRb.useGravity = false;
            heldRb.linearDamping = 8f; // snappier feel while held
            _itemStatus = ItemStatus.Moved;

            // PickableEvent.Trigger(PickableEventType.Picked, itemPicker.uniqueID, itemPicker.transform);
            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Show, actionID, "BlockAllNewRequests", additionalInfoText: "to let go");
        }

        void Drop()
        {
            if (heldRb == null) return;

            var droppedRb = heldRb;
            Destroy(joint);

            // Physics.IgnoreCollision(heldRb.GetComponent<Collider>(),
            //     GetComponent<CharacterController>(), false);

            droppedRb.useGravity = true;
            droppedRb.linearDamping = 0f;
            heldRb = null;

            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionID, "UnblockAllRequests");

            StartCoroutine(WaitForRest(droppedRb));
        }

        public void SetItem(GameObject itemToSet)
        {
            if (heldRb != null)
            {
                AlertEvent.Trigger(AlertReason.InvalidAction, "Already holding an item", "Hand Error!");
                return;
            }

            var rb = itemToSet.GetComponent<Rigidbody>();
            var itemPicker = rb.gameObject.GetComponent<ItemPicker>();
            if (rb == null) return;
            if (itemPicker == null) return;

            // Build a spring joint at runtime so physics still resolve against environment
            joint = rb.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = holdPoint.position;
            joint.connectedAnchor = Vector3.zero; // connect to nothing (acts like gravity gun)
            joint.anchor = Vector3.zero;
            joint.spring = spring;
            joint.damper = damper;
            joint.minDistance = 0f;
            joint.maxDistance = 0f;

            heldRb = rb;
            heldRb.useGravity = true;
            heldRb.isKinematic = false;
            heldRb.linearDamping = 8f; // snappier feel while held
            _itemStatus = ItemStatus.Placed;


            // Optional: fire same event as pickup so systems are consistent
            // PickableEvent.Trigger(PickableEventType.Picked, itemPicker.uniqueID, itemPicker.transform);
        }

        IEnumerator WaitForRest(Rigidbody rb)
        {
            const float velocityThreshold = 0.05f;
            const float angularThreshold = 0.05f;
            const float settleTime = 0.5f;

            var timer = 0f;

            while (rb != null)
            {
                // Check if "at rest"
                if (rb.linearVelocity.sqrMagnitude < velocityThreshold * velocityThreshold &&
                    rb.angularVelocity.sqrMagnitude < angularThreshold * angularThreshold)
                {
                    timer += Time.deltaTime;
                    if (timer >= settleTime)
                    {
                        // Trigger your event here
                        Debug.Log($"[PlayerPropPickup] {rb.name} has come to rest.");
                        if (rb.GetComponent<ItemPicker>() != null)
                        {
                            if (_itemStatus == ItemStatus.Moved)
                            {
                                PickableEvent.Trigger(
                                    PickableEventType.MovedItemCameToRest, rb.GetComponent<ItemPicker>().uniqueID,
                                    rb.transform);
                            }
                            else if (_itemStatus == ItemStatus.Placed)
                            {
                                if (rb.GetComponent<ItemPicker>().inventoryItem == null) yield break;
                                var invID = rb.GetComponent<ItemPicker>().inventoryItem.ItemID;
                                PickableEvent.Trigger(
                                    PickableEventType.PlacedItemCameToRest, rb.GetComponent<ItemPicker>().uniqueID,
                                    rb.transform, invID);
                            }
                        }
                        else
                        {
                            Debug.LogError("Unsupported object placed in world, no ItemPicker found.");
                        }

                        rb.isKinematic = true;

                        yield break;
                    }
                }
                else
                {
                    timer = 0f;
                }

                yield return null;
            }
        }
        public bool IsHoldingItem()
        {
            return heldRb != null;
        }
    }
}
