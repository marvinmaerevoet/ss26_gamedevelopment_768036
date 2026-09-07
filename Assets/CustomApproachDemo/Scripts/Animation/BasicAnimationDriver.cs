using System.Collections.Generic;
using CustomApproachDemo.Player;
using CustomApproachDemo.Police;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace CustomApproachDemo.Animation {
    public sealed class BasicAnimationDriver : MonoBehaviour {
        public Animator animator;
        public NavMeshAgent agent;
        public DemoPlayerState playerState;
        public PoliceBlackboard blackboard;

        public float walkingVisualSpeed = 1.5f;
        public float runningVisualSpeed = 4f;
        public float movingThreshold = 0.1f;
        public float playerIdleDeadzone = 0.25f;
        public float agentIdleDeadzone = 0.05f;
        public float speedDampTime = 0.08f;
        public bool forceRootMotionOff = true;
        public bool logMissingParametersOnce;
        public bool snapSpeedToZeroWhenIdle = true;

        [Header("Player Input")]
        public bool useInputForPlayerMovement = true;

        [Header("Debug")]
        public float debugSourceSpeed;
        public float debugAnimatorSpeed;
        public bool debugIsMoving;
        public bool debugIsRunning;
        public bool debugInputMoving;
        public bool debugInputRunning;
        public bool debugUsedInputForPlayer;

        private readonly HashSet<string> warnedMissingParameters = new HashSet<string>();

        private void Awake() {
            ResolveReferences();
            ApplyAnimatorOptions();
        }

        private void Update() {
            ResolveReferences();
            ApplyAnimatorOptions();

            if(animator == null) {
                return;
            }

            bool chasing = HasBehavior("Chase");
            bool investigating = HasBehavior("Investigate");
            bool arrested = (playerState != null && playerState.IsArrested) || HasBehavior("Arrest");
            bool emergency = HasBehavior("Emergency") || (blackboard != null && blackboard.OfficerHealthLow);

            AnimationValues values = CalculateAnimationValues(chasing);

            debugSourceSpeed = values.SourceSpeed;
            debugAnimatorSpeed = values.Speed;
            debugIsMoving = values.IsMoving;
            debugIsRunning = values.IsRunning;

            SetSpeed(values.Speed);
            SetBool("IsMoving", values.IsMoving);
            SetBool("IsRunning", values.IsRunning);
            SetBool("IsChasing", chasing);
            SetBool("IsInvestigating", investigating);
            SetBool("IsArrested", arrested);
            SetBool("IsEmergency", emergency);
        }

        private void ResolveReferences() {
            if(animator == null) {
                animator = GetComponentInChildren<Animator>();
            }

            if(agent == null) {
                agent = GetComponent<NavMeshAgent>();
            }

            if(agent == null) {
                agent = GetComponentInParent<NavMeshAgent>();
            }

            if(playerState == null) {
                playerState = GetComponent<DemoPlayerState>();
            }

            if(playerState == null) {
                playerState = GetComponentInParent<DemoPlayerState>();
            }

            if(blackboard == null) {
                blackboard = GetComponent<PoliceBlackboard>();
            }

            if(blackboard == null) {
                blackboard = GetComponentInParent<PoliceBlackboard>();
            }
        }

        private void ApplyAnimatorOptions() {
            if(animator != null && forceRootMotionOff) {
                animator.applyRootMotion = false;
            }
        }

        private AnimationValues CalculateAnimationValues(bool chasing) {
            float sourceSpeed = 0f;
            float speed = 0f;
            bool isMoving = false;
            bool isRunning = false;

            debugInputMoving = false;
            debugInputRunning = false;
            debugUsedInputForPlayer = false;

            // Sheriff / NPC: use NavMeshAgent velocity.
            if(agent != null) {
                sourceSpeed = SanitizeSpeed(agent.velocity.magnitude);

                if(sourceSpeed > agentIdleDeadzone) {
                    speed = Mathf.Clamp(sourceSpeed, 0f, 6f);
                    isMoving = true;
                    isRunning = chasing;
                }
            }
            // Player: prefer direct input, because CurrentSpeed can contain tiny residual values.
            else if(playerState != null) {
                if(useInputForPlayerMovement && TryReadPlayerInput(out bool inputMoving, out bool inputRunning)) {
                    debugInputMoving = inputMoving;
                    debugInputRunning = inputRunning;
                    debugUsedInputForPlayer = true;

                    isMoving = inputMoving;
                    isRunning = inputMoving && inputRunning;

                    if(isMoving) {
                        speed = isRunning ? runningVisualSpeed : walkingVisualSpeed;
                        sourceSpeed = speed;
                    } else {
                        speed = 0f;
                        sourceSpeed = 0f;
                    }
                } else {
                    sourceSpeed = SanitizeSpeed(playerState.CurrentSpeed);
                    isMoving = sourceSpeed > playerIdleDeadzone;

                    if(isMoving) {
                        isRunning = playerState.IsRunning;
                        speed = isRunning ? runningVisualSpeed : walkingVisualSpeed;
                    } else {
                        speed = 0f;
                        isRunning = false;
                    }
                }
            }

            speed = SanitizeSpeed(speed);
            speed = isMoving ? Mathf.Clamp(speed, 0f, 6f) : 0f;

            return new AnimationValues(sourceSpeed, speed, isMoving, isRunning);
        }

        private static bool TryReadPlayerInput(out bool moving, out bool running) {
            moving = false;
            running = false;

            Keyboard keyboard = Keyboard.current;
            if(keyboard == null) {
                return false;
            }

            moving =
                keyboard.wKey.isPressed ||
                keyboard.aKey.isPressed ||
                keyboard.sKey.isPressed ||
                keyboard.dKey.isPressed ||
                keyboard.upArrowKey.isPressed ||
                keyboard.downArrowKey.isPressed ||
                keyboard.leftArrowKey.isPressed ||
                keyboard.rightArrowKey.isPressed;

            running =
                keyboard.leftShiftKey.isPressed ||
                keyboard.rightShiftKey.isPressed;

            return true;
        }

        private static float SanitizeSpeed(float speed) {
            return float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0f ? 0f : speed;
        }

        private bool HasBehavior(string token) {
            return blackboard != null &&
                   !string.IsNullOrEmpty(blackboard.CurrentBehaviorName) &&
                   blackboard.CurrentBehaviorName.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SetSpeed(float value) {
            if(!HasParameter("Speed", AnimatorControllerParameterType.Float)) {
                return;
            }

            if(snapSpeedToZeroWhenIdle && value <= 0f) {
                animator.SetFloat("Speed", 0f);
                debugAnimatorSpeed = 0f;
                return;
            }

            if(speedDampTime > 0f) {
                animator.SetFloat("Speed", value, speedDampTime, Time.deltaTime);
            } else {
                animator.SetFloat("Speed", value);
            }

            debugAnimatorSpeed = animator.GetFloat("Speed");
        }

        private void SetBool(string parameterName, bool value) {
            if(HasParameter(parameterName, AnimatorControllerParameterType.Bool)) {
                animator.SetBool(parameterName, value);
            }
        }

        private bool HasParameter(string parameterName, AnimatorControllerParameterType expectedType) {
            if(animator == null) {
                return false;
            }

            foreach(AnimatorControllerParameter parameter in animator.parameters) {
                if(parameter.name == parameterName && parameter.type == expectedType) {
                    return true;
                }
            }

            WarnMissingParameter(parameterName, expectedType);
            return false;
        }

        private void WarnMissingParameter(string parameterName, AnimatorControllerParameterType expectedType) {
            if(!logMissingParametersOnce || !warnedMissingParameters.Add(parameterName)) {
                return;
            }

            Debug.LogWarning($"Animator on {name} is missing parameter '{parameterName}' of type {expectedType}.", this);
        }

        private readonly struct AnimationValues {
            public readonly float SourceSpeed;
            public readonly float Speed;
            public readonly bool IsMoving;
            public readonly bool IsRunning;

            public AnimationValues(float sourceSpeed, float speed, bool isMoving, bool isRunning) {
                SourceSpeed = sourceSpeed;
                Speed = speed;
                IsMoving = isMoving;
                IsRunning = isRunning;
            }
        }
    }
}
