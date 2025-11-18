using System;
using Animancer;
using Helpers.ScriptableObjects.Animation;
using UnityEngine;

namespace Helpers.AnimancerHelper
{
    public class AnimancerRightArmController : MonoBehaviour
    {
        [Header("References")] public ToolAnimationSet currentToolAnimationSet;
        public AnimancerComponent animancerComponent;

        [Header("Settings")] [Tooltip("Default transition duration for smooth blending")]
        public float defaultTransitionDuration = 0.25f;

        [Tooltip("Transition duration for locomotion changes (idle/walk/run)")]
        public float locomotionTransitionDuration = 0.15f;
        AnimancerState _currentActionState;

        LocomotionState _currentLocoMode = LocomotionState.Idle;

        // Track current states
        AnimancerState _currentLocomotionState;

        void OnValidate()
        {
            // Auto-find AnimancerComponent if not assigned
            if (animancerComponent == null) animancerComponent = GetComponent<AnimancerComponent>();
        }

        /// <summary>
        ///     Call this whenever the tool/animation set changes
        /// </summary>
        public void UpdateAnimationSet()
        {
            if (currentToolAnimationSet == null)
            {
                Debug.LogWarning("No ToolAnimationSet assigned!");
                return;
            }

            // Restart with the new animation set
            // This will smoothly transition to the new idle animation
            LoopIdleAnimation();
        }

        public void SetActionState(AnimancerState state)
        {
            _currentActionState = state;
        }

        public void ClearActionState()
        {
            _currentActionState = null;
        }


        enum LocomotionState
        {
            Idle,
            Walk,
            Run
        }

        #region Locomotion Animations

        /// <summary>
        ///     Play and loop the idle animation
        /// </summary>
        public void LoopIdleAnimation()
        {
            if (currentToolAnimationSet?.idleAnimation == null) return;

            _currentLocoMode = LocomotionState.Idle;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.idleAnimation,
                locomotionTransitionDuration
            );

            // Make sure idle loops
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Play and loop the walk animation
        /// </summary>
        public void LoopWalkAnimation()
        {
            if (currentToolAnimationSet?.walkAnimation == null) return;

            _currentLocoMode = LocomotionState.Walk;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.walkAnimation,
                locomotionTransitionDuration
            );

            // Make walk animation loop
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Play and loop the run animation
        /// </summary>
        public void LoopRunAnimation()
        {
            if (currentToolAnimationSet?.runAnimation == null) return;

            _currentLocoMode = LocomotionState.Run;
            _currentLocomotionState = animancerComponent.Play(
                currentToolAnimationSet.runAnimation,
                locomotionTransitionDuration
            );

            // Make run animation loop
            _currentLocomotionState.Events(this).OnEnd = () => { _currentLocomotionState.Time = 0f; };
        }

        /// <summary>
        ///     Smoothly transition from idle to walk
        /// </summary>
        public void MoveFromIdleToWalk()
        {
            if (_currentLocoMode != LocomotionState.Walk) LoopWalkAnimation();
        }

        /// <summary>
        ///     Smoothly transition back to idle
        /// </summary>
        public void MoveToIdle()
        {
            if (_currentLocoMode != LocomotionState.Idle) LoopIdleAnimation();
        }

        /// <summary>
        ///     Set locomotion based on movement speed
        /// </summary>
        /// <param name="isMoving">Is the player moving?</param>
        /// <param name="isRunning">Is the player running/sprinting?</param>
        public void UpdateLocomotion(bool isMoving, bool isRunning = false)
        {
            if (IsPlayingAction())
                return; // Don't re-trigger walk/run/idle

            if (!isMoving)
                MoveToIdle();
            else if (isRunning)
                LoopRunAnimation();
            else
                LoopWalkAnimation();
        }

        #endregion

        #region Tool Use Animations

        /// <summary>
        ///     Play the tool use sequence: begin -> during (loop) -> end
        /// </summary>
        public void PlayToolUseSequence(Action onComplete = null)
        {
            if (currentToolAnimationSet == null) return;

            // If there's no begin animation, just play during
            if (currentToolAnimationSet.beginUseAnimation == null)
            {
                PlayToolDuringUse();
                return;
            }

            // Play begin animation
            var beginState = animancerComponent.Play(
                currentToolAnimationSet.beginUseAnimation,
                defaultTransitionDuration
                // 0.5f
            );

            // When begin finishes, play the looping "during" animation
            beginState.Events(this).OnEnd = () => { PlayToolDuringUse(); };
        }
        /// <summary>
        ///     Play the looping "during use" animation
        /// </summary>
        void PlayToolDuringUse()
        {
            if (currentToolAnimationSet?.duringUseAnimationLoopable != null)
                _currentActionState = animancerComponent.Play(
                    currentToolAnimationSet.duringUseAnimationLoopable,
                    defaultTransitionDuration
                );
        }

        /// <summary>
        ///     End the tool use and transition back to locomotion
        /// </summary>
        public void EndToolUse(Action onComplete = null)
        {
            if (currentToolAnimationSet?.endUseAnimation == null)
            {
                // No end animation, just return to locomotion
                ReturnToLocomotion();
                onComplete?.Invoke();
                return;
            }

            // Play end animation
            var endState = animancerComponent.Play(
                currentToolAnimationSet.endUseAnimation,
                defaultTransitionDuration
            );

            // When end finishes, return to locomotion
            endState.Events(this).OnEnd = () =>
            {
                ReturnToLocomotion();
                onComplete?.Invoke();
            };
        }

        /// <summary>
        ///     Play a one-shot tool use animation (begin and end together)
        /// </summary>
        public void PlayToolUseOneShot(Action onComplete = null)
        {
            if (currentToolAnimationSet?.beginUseAnimation == null) return;

            var state = animancerComponent.Play(
                currentToolAnimationSet.beginUseAnimation,
                defaultTransitionDuration
            );

            state.Events(this).OnEnd = () =>
            {
                ReturnToLocomotion();
                onComplete?.Invoke();
            };
        }

        #endregion

        #region Equipment Animations

        public void PlayEquipAnimation(Action onComplete = null)
        {
            // If there's no equip animation, just return
        }

        public void PlayUnequipAnimation(Action onComplete = null)
        {
            // If there's no unequip animation, just return
        }

        #endregion

        #region Helper Methods

        /// <summary>
        ///     Return to the appropriate locomotion animation based on current mode
        /// </summary>
        public void ReturnToLocomotion()
        {
            switch (_currentLocoMode)
            {
                case LocomotionState.Idle:
                    LoopIdleAnimation();
                    break;
                case LocomotionState.Walk:
                    LoopWalkAnimation();
                    break;
                case LocomotionState.Run:
                    LoopRunAnimation();
                    break;
            }
        }
        /// <summary>
        ///     Check if currently playing a tool action animation
        /// </summary>
        public bool IsPlayingAction()
        {
            return _currentActionState != null && _currentActionState.IsPlaying;
        }
        /// <summary>
        ///     Stop all animations
        /// </summary>
        public void StopAll()
        {
            animancerComponent.Stop();
            _currentLocomotionState = null;
            _currentActionState = null;
        }

        #endregion
    }
}
