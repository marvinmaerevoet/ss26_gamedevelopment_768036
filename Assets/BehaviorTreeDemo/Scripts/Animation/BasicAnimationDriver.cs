using System.Collections.Generic;
using BehaviorTreeDemo.Gameplay.Carry;
using BehaviorTreeDemo.Gameplay.Player;
using BehaviorTreeDemo.Police;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeDemo.Animation {
    public sealed class BasicAnimationDriver : MonoBehaviour {
        public Animator animator;
        public NavMeshAgent agent;
        public DemoPlayerState playerState;
        public PoliceBlackboard blackboard;
        public PoliceAIContext policeContext;

        public float walkingVisualSpeed = 1.5f;
        public float runningVisualSpeed = 4f;
        public float playerIdleDeadzone = 0.25f;
        public float agentIdleDeadzone = 0.05f;
        public float speedDampTime = 0.08f;
        public bool forceRootMotionOff = true;
        public bool logMissingParametersOnce;
        public bool snapSpeedToZeroWhenIdle = true;

        [Header("Debug")]
        public float debugSourceSpeed;
        public float debugAnimatorSpeed;
        public bool debugIsMoving;
        public bool debugIsRunning;
        private readonly HashSet<string> warnedMissingParameters = new HashSet<string>();
        private DemoPlayerCarryController carryController;

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

            PoliceBehaviorMode behaviorMode = blackboard != null
                ? blackboard.CurrentBehaviorMode
                : PoliceBehaviorMode.None;
            bool isPolice = blackboard != null || policeContext != null;
            bool localArrestLatched = policeContext != null && policeContext.IsArrestLatched;
            bool chasing = !localArrestLatched && behaviorMode == PoliceBehaviorMode.Chase;
            bool investigating = !localArrestLatched &&
                policeContext != null &&
                policeContext.InvestigationPhase == PoliceInvestigationPhase.LookingAround;
            bool emergency = !localArrestLatched &&
                (behaviorMode == PoliceBehaviorMode.Emergency || (blackboard != null && blackboard.OfficerHealthLow));

            AnimationValues values = CalculateAnimationValues(chasing);
            bool playerArrested = playerState != null && playerState.IsArrested;
            bool arrestApproachActive = policeContext != null && policeContext.IsArrestApproachActive;
            bool sheriffArrestPose = localArrestLatched && !arrestApproachActive && !values.IsMoving;
            bool arrested = isPolice ? sheriffArrestPose : playerArrested;

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

            if(policeContext == null) {
                policeContext = GetComponent<PoliceAIContext>();
            }

            if(policeContext == null) {
                policeContext = GetComponentInParent<PoliceAIContext>();
            }

            if(playerState != null && carryController == null) {
                carryController = playerState.GetComponent<DemoPlayerCarryController>();
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

            // Sheriff / NPC: use NavMeshAgent velocity.
            if(agent != null) {
                sourceSpeed = SanitizeSpeed(agent.velocity.magnitude);

                if(sourceSpeed > agentIdleDeadzone) {
                    speed = Mathf.Clamp(sourceSpeed, 0f, 6f);
                    isMoving = true;
                    isRunning = chasing;
                }
            }
            // Player: use the authoritative movement state reported by the player controller.
            else if(playerState != null) {
                bool isCarrying = carryController != null && carryController.IsCarrying;
                sourceSpeed = SanitizeSpeed(playerState.CurrentSpeed);
                isMoving = sourceSpeed > playerIdleDeadzone;

                if(isMoving) {
                    isRunning = playerState.IsRunning && !isCarrying;
                    speed = isRunning ? runningVisualSpeed : walkingVisualSpeed;
                } else {
                    speed = 0f;
                    isRunning = false;
                }
            }

            speed = SanitizeSpeed(speed);
            speed = isMoving ? Mathf.Clamp(speed, 0f, 6f) : 0f;

            return new AnimationValues(sourceSpeed, speed, isMoving, isRunning);
        }

        private static float SanitizeSpeed(float speed) {
            return float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0f ? 0f : speed;
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

        public bool TriggerReleaseAnimation() {
            ResolveReferences();
            if(!HasParameter("ReleaseStretch", AnimatorControllerParameterType.Trigger)) {
                return false;
            }

            animator.ResetTrigger("ReleaseStretch");
            animator.SetTrigger("ReleaseStretch");
            return true;
        }

        public void ResetReleaseAnimationTrigger() {
            ResolveReferences();
            if(HasParameter("ReleaseStretch", AnimatorControllerParameterType.Trigger)) {
                animator.ResetTrigger("ReleaseStretch");
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
