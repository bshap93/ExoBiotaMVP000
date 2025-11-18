using System;
using System.Collections.Generic;
using System.Linq;
using creepycat.scifikitvol4;
using CustomAssets.Scripts;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Helpers.Events.Machine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;
using Uween;

namespace FirstPersonPlayer.Interactable.Stateful
{
    public class StatefulElevator : MonoBehaviour, IRequiresUniqueID, MMEventListener<SceneEvent>
    {
        [Serializable]
        public enum ElevatorMovementState
        {
            AtRest,
            MovingUp,
            MovingDown
        }

        [Serializable]
        public enum ElevatorPowerState
        {
            Powered,
            Broken,
            Unpowered
        }

        public LayerMask buttonLayerMask;

        public ElevatorTypeInfo elevatorTypeInfo;


        public RewiredFirstPersonInputs playerInput;

        // [FormerlySerializedAs("EndingPoint")] [FormerlySerializedAs("StartPoint")] [Header("Elevator Travel Setup")]
        // public Transform endingPoint;
        // [FormerlySerializedAs("StartingPoint")] [FormerlySerializedAs("EndPoint")]
        // public Transform startingPoint;
        // Start from the top with index 0 and go down +1 for each floor
        [FormerlySerializedAs("ElevatorPoints")]
        public List<Transform> elevatorPoints;
        [FormerlySerializedAs("TravelTime")] public float travelTime = 10.0f;


        // public AudioClip elevatorSound;

        // public int currentFloorIndex;

        public GameObject elevatorScriptObject;


        public ElevatorState currentState;

        public string elevatorUniqueID;


        public MMFeedbacks clickFeedbacks;
        public MMFeedbacks elevatorTravelFB;
        public MMFeedbacks endTravelFeedbacks;
        public MMFeedbacks startTravelFeedbacks;


        public GameObject frontalBarrier;

        // [FormerlySerializedAs("startAtTop")]
        // [Tooltip("Should the elevator start at the top position (EndPoint) when the scene loads?")]
        // public bool startAtBottom;
        public int startAtIndex;

        readonly EasyTimer movetimer = new();


        // bool _isAtBottom;


        bool moveswitch;

        // Get components
        void Start()
        {
            if (IsAtBottom())
                elevatorScriptObject.transform.localPosition =
                    elevatorPoints[elevatorTypeInfo.numberOfFloors - 1].localPosition;
            else
                elevatorScriptObject.transform.localPosition = elevatorPoints[startAtIndex].localPosition;

            // if (startAtBottom)
            //     elevatorScriptObject.transform.localPosition = startingPoint.transform.localPosition;
            // else
            //     elevatorScriptObject.transform.localPosition = endingPoint.transform.localPosition;

            // IsAtBottom = !startAtBottom;
        }
        void FixedUpdate()
        {
            if (movetimer.IsDone)
            {
                if (moveswitch)
                {
                    //Debug.Log("Time A off");
                    moveswitch = false;
                    endTravelFeedbacks?.PlayFeedbacks();
                    elevatorTravelFB?.StopFeedbacks();
                }

                frontalBarrier.SetActive(false);
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public string UniqueID => elevatorUniqueID;
        public void SetUniqueID()
        {
            elevatorUniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(elevatorUniqueID);
        }

        public void OnMMEvent(SceneEvent eventType)
        {
            if (eventType.EventType == SceneEventType.PlayerPawnLoaded) Initialize();
        }

        bool IsAtBottom()
        {
            return currentState.currentFloor >= elevatorTypeInfo.numberOfFloors - 1;
        }

        bool IsAtTop()
        {
            return currentState.currentFloor <= 0;
        }

        void ElevatorGoDown(int indexOfDestination)
        {
            if (!currentState.accessibleFloors.Contains(indexOfDestination))
            {
                AlertEvent.Trigger(
                    AlertReason.ElevatorIssue, "The selected floor is not accessible from this elevator.",
                    "Elevator Issue");

                return;
            }

            var floorsToTravel = indexOfDestination - currentState.currentFloor;
            var destinationPoint = elevatorPoints[indexOfDestination];
            clickFeedbacks?.PlayFeedbacks();
            startTravelFeedbacks?.PlayFeedbacks();
            elevatorTravelFB?.PlayFeedbacks();

            frontalBarrier.SetActive(true);

            //Debug.Log("Button Elevator Clicked");
            TweenXYZ.Add(
                    elevatorScriptObject.transform.gameObject, travelTime * floorsToTravel,
                    destinationPoint.transform.localPosition)
                .EaseInOutSine();

            movetimer.SetNewDuration(travelTime * floorsToTravel);

            currentState.currentFloor = indexOfDestination;

            ElevatorStateEvent.Trigger(
                UniqueID, currentState, ElevatorStateEventType.SetNewFloorState);


            // _isAtBottom = false;
            moveswitch = true;
        }

        void ElevatorGoUp(int indexOfDestination)
        {
            if (!currentState.accessibleFloors.Contains(indexOfDestination))
            {
                AlertEvent.Trigger(
                    AlertReason.ElevatorIssue, "The selected floor is not accessible from this elevator.",
                    "Elevator Issue");

                return;
            }

            var floorsToTravel = currentState.currentFloor - indexOfDestination;
            var destinationPoint = elevatorPoints[indexOfDestination];
            clickFeedbacks?.PlayFeedbacks();
            startTravelFeedbacks?.PlayFeedbacks();
            elevatorTravelFB?.PlayFeedbacks();

            frontalBarrier.SetActive(true);

            //Debug.Log("Button Elevator Clicked");
            TweenXYZ.Add(
                    elevatorScriptObject.transform.gameObject, travelTime * floorsToTravel,
                    destinationPoint.transform.localPosition)
                .EaseInOutCubic();

            movetimer.SetNewDuration(travelTime * floorsToTravel);

            currentState.currentFloor = indexOfDestination;

            ElevatorStateEvent.Trigger(
                UniqueID, currentState, ElevatorStateEventType.SetNewFloorState);


            // _isAtBottom = true;
            moveswitch = true;
        }

        public void OnButtonClick(ButtonClickAnim buttonClickAnim)
        {
            var buttonType = buttonClickAnim.buttonType;

            switch (buttonType)
            {
                case ButtonClickAnim.ElevatorButtonType.CallToTop:
                    if (!IsAtTop())
                    {
                        ElevatorGoUp(0);
                        Debug.Log("IsAtTop false");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the top.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.CallToBottom:
                    if (!IsAtBottom())
                    {
                        ElevatorGoDown(elevatorTypeInfo.numberOfFloors - 1);
                        Debug.Log("IsAtTop true");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.ElevatorGoUp:
                    if (!IsAtTop())
                    {
                        ElevatorGoUp(currentState.currentFloor - 1);
                        Debug.Log("IsAtTop false");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the top.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.ElevatorGoDown:
                    if (!IsAtBottom())
                    {
                        ElevatorGoDown(currentState.currentFloor + 1);
                        Debug.Log("IsAtTop true");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
                            "Elevator Issue");
                    }

                    break;
                default:
                    AlertEvent.Trigger(
                        AlertReason.ElevatorIssue, "Unknown elevator button type.", "Elevator Issue");

                    break;
            }
            // if (!moveswitch)
            // {
            //     if (playerInput == null) return;
            //
            //     if (playerInput.interact)
            //         // Get the gameobject clicked
            //         if (Camera.main != null)
            //         {
            //             var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //             RaycastHit hit;
            //
            //             // If something clicked
            //             if (Physics.Raycast(ray, out hit, Mathf.Infinity, buttonLayerMask))
            //             {
            //                 var button = hit.transform.GetComponent<ButtonClickAnim>();
            //                 if (button == null)
            //                 {
            //                     Debug.LogError("ButtonClickAnim component not found on the clicked object.");
            //                     return;
            //                 }
            //
            //                 var buttonType = button.buttonType;
            //
            //                 switch (buttonType)
            //                 {
            //                     case ButtonClickAnim.ElevatorButtonType.CallToTop:
            //                         if (!IsAtTop())
            //                         {
            //                             ElevatorGoUp(0);
            //                             Debug.Log("IsAtTop false");
            //                         }
            //                         else
            //                         {
            //                             AlertEvent.Trigger(
            //                                 AlertReason.ElevatorIssue, "Elevator is already at the top.",
            //                                 "Elevator Issue");
            //                         }
            //
            //                         break;
            //                     case ButtonClickAnim.ElevatorButtonType.CallToBottom:
            //                         if (!IsAtBottom())
            //                         {
            //                             ElevatorGoDown(elevatorTypeInfo.numberOfFloors - 1);
            //                             Debug.Log("IsAtTop true");
            //                         }
            //                         else
            //                         {
            //                             AlertEvent.Trigger(
            //                                 AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
            //                                 "Elevator Issue");
            //                         }
            //
            //                         break;
            //                     case ButtonClickAnim.ElevatorButtonType.ElevatorGoUp:
            //                         if (!IsAtTop())
            //                         {
            //                             ElevatorGoUp(currentState.currentFloor - 1);
            //                             Debug.Log("IsAtTop false");
            //                         }
            //                         else
            //                         {
            //                             AlertEvent.Trigger(
            //                                 AlertReason.ElevatorIssue, "Elevator is already at the top.",
            //                                 "Elevator Issue");
            //                         }
            //
            //                         break;
            //                     case ButtonClickAnim.ElevatorButtonType.ElevatorGoDown:
            //                         if (!IsAtBottom())
            //                         {
            //                             ElevatorGoDown(currentState.currentFloor + 1);
            //                             Debug.Log("IsAtTop true");
            //                         }
            //                         else
            //                         {
            //                             AlertEvent.Trigger(
            //                                 AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
            //                                 "Elevator Issue");
            //                         }
            //
            //                         break;
            //                     default:
            //                         AlertEvent.Trigger(
            //                             AlertReason.ElevatorIssue, "Unknown elevator button type.", "Elevator Issue");
            //
            //                         break;
            //                 }
            //             }
            //         }
            // }
            // else
            // {
            //     // Only play elevator sound if game is not paused
            //     if (!PauseManager.Instance.IsPaused())
            //     {
            //     }
            // }
        }
        void Initialize()
        {
            playerInput = FindFirstObjectByType<RewiredFirstPersonInputs>();

            if (playerInput == null) Debug.LogError("No RewiredFirstPersonInputs found in scene.");
        }


        [Serializable]
        public class ElevatorState
        {
            [FormerlySerializedAs("position")] public ElevatorMovementState movementState;
            public ElevatorPowerState powerState;
            public int currentFloor;
            public int[] accessibleFloors;
        }
    }
}
