using Helpers.Events;
using Manager;
using Rewired;
using UnityEngine;

namespace FirstPersonPlayer
{
    public class RewiredFirstPersonInputs : MonoBehaviour
    {
        public enum InputActions
        {
            MoveForwardBackward,
            MoveLeftRight,
            Jump,
            Interact,
            UseEquipped,
            Crouch,
            ScrollBetweenTools,
            LookY,
            LookX,
            InteractHeld,
            JumpHeld,
            NoOp,
            Sprint,
            SprintStart,
            SprintStop,
            PickUpProp,
            Pause,
            LeftHandToggle,
            PickablePick
        }

        [Header("Character Input Values")] public Vector2 move;


        public Vector2 look;
        public float scrollBetweenTools;
        public bool jump;
        public bool jumpHeld;
        // Sprint
        public bool sprint;
        public bool sprintStart;
        public bool sprintStop;


        public bool analogMovement;
        public bool interact;
        public bool interactHeld;
        public bool crouch;
        public bool useEquipped;
        public bool pickUpProp;
        public bool pause;
        public bool leftHandToggle;
        public bool pickablePick;

        Player _rewiredPlayer;

        void Start()
        {
            _rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        void Update()
        {
            if (_rewiredPlayer == null) return;

            // Read movement input
            move = new Vector2(
                _rewiredPlayer.GetAxis("Move Horizontal"),
                _rewiredPlayer.GetAxis("Move Vertical")
            );

            Debug.DrawRay(
                transform.position,
                transform.forward * move.y + transform.right * move.x,
                Color.cyan, 0.01f, false);

            // Read look input
            look = new Vector2(
                _rewiredPlayer.GetAxis("Look X"),
                _rewiredPlayer.GetAxis("Look Y")
            );

            // Trigger stamina events when sprint state changes
            // if (sprintStart)
            //     StaminaAffectorEvent.Trigger(
            //         StaminaAffectorEventType.StaminaDrainActivityStarted,
            //         PlayerStatsManager.Instance.SprintStaminaDrainPerSecond);
            //
            // if (sprintStop) StaminaAffectorEvent.Trigger(StaminaAffectorEventType.StaminaDrainActivityStopped, 0f);

            // Read button inputs
            jump = _rewiredPlayer.GetButton("Jump");
            // Sprint
            sprint = _rewiredPlayer.GetButton("Sprint");
            sprintStart = _rewiredPlayer.GetButtonDown("Sprint");
            sprintStop = _rewiredPlayer.GetButtonUp("Sprint");

            interact = _rewiredPlayer.GetButtonDown("Interact");
            interactHeld = _rewiredPlayer.GetButton("Interact");
            crouch = _rewiredPlayer.GetButton("Crouch");
            pickUpProp = _rewiredPlayer.GetButtonDown("PickUpProp");
            useEquipped = _rewiredPlayer.GetButton("UseEquipped");
            leftHandToggle = _rewiredPlayer.GetButtonDown("LeftHandToggle");
            pickablePick = _rewiredPlayer.GetButtonDown("PickablePick");
            scrollBetweenTools = _rewiredPlayer.GetAxisDelta("ScrollTools");

            if (sprintStart)
                StaminaAffectorEvent.Trigger(
                    StaminaAffectorEventType.StaminaDrainActivityStarted,
                    PlayerStatsManager.Instance.SprintStaminaDrainPerSecond);

            if (sprintStop)
                StaminaAffectorEvent.Trigger(
                    StaminaAffectorEventType.StaminaDrainActivityStopped, 0f);
        }

        public bool GetButtonInput(InputActions input)
        {
            switch (input)
            {
                case InputActions.Jump:
                    return jump;
                case InputActions.Interact:
                    return interact;
                case InputActions.UseEquipped:
                    return useEquipped;

                case InputActions.Crouch:
                    return crouch;

                case InputActions.Sprint:
                    return sprint;

                case InputActions.SprintStart:
                    return sprintStart;

                case InputActions.SprintStop:
                    return sprintStop;

                case InputActions.InteractHeld:
                    return interactHeld;
                case InputActions.JumpHeld:
                    return jumpHeld;
                case InputActions.PickUpProp:
                    return pickUpProp;


                case InputActions.Pause:
                    return pause;
                case InputActions.LeftHandToggle:
                    return leftHandToggle;
                case InputActions.PickablePick:
                    return pickablePick;


                default:
                    return false;
            }
        }

        public float GetAxisInput(InputActions action)
        {
            switch (action)
            {
                case InputActions.MoveForwardBackward:
                    return move.y;
                case InputActions.MoveLeftRight:
                    return move.x;
                case InputActions.LookY:
                    return look.y;
                case InputActions.LookX:
                    return look.x;
                case InputActions.ScrollBetweenTools:
                    return scrollBetweenTools;
                default:
                    return 0f; // Default value if action ID is not recognized
            }
        }
    }
}
