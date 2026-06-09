using System.Collections.Generic;
using Demo.BehaviorTreePolice.Player;
using Demo.BehaviorTreePolice.Police;
using UnityEngine;
using UnityEngine.AI;

namespace Demo.BehaviorTreePolice.Polish
{
    public sealed class DemoBasicAnimationDriver : MonoBehaviour
    {
        public Animator animator;
        public NavMeshAgent agent;
        public DemoPlayerState playerState;
        public PoliceBlackboard blackboard;

        public float walkingVisualSpeed = 1.5f;
        public float runningVisualSpeed = 4f;
        public float movingThreshold = 0.1f;
        public bool forceRootMotionOff = true;
        public bool logMissingParametersOnce;

        private readonly HashSet<string> warnedMissingParameters = new HashSet<string>();

        private void Awake()
        {
            ResolveReferences();
            ApplyAnimatorOptions();
        }

        private void Update()
        {
            ResolveReferences();
            ApplyAnimatorOptions();

            if (animator == null)
            {
                return;
            }

            float speed = CalculateSpeed();
            bool chasing = HasBehavior("Chase");
            bool investigating = HasBehavior("Investigate");
            bool arrested = (playerState != null && playerState.IsArrested) || HasBehavior("Arrest");
            bool emergency = HasBehavior("Emergency") || (blackboard != null && blackboard.OfficerHealthLow);
            bool running = (playerState != null && playerState.IsRunning) || chasing;

            SetFloat("Speed", speed);
            SetBool("IsMoving", speed > movingThreshold);
            SetBool("IsRunning", running);
            SetBool("IsChasing", chasing);
            SetBool("IsInvestigating", investigating);
            SetBool("IsArrested", arrested);
            SetBool("IsEmergency", emergency);
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            if (agent == null)
            {
                agent = GetComponentInParent<NavMeshAgent>();
            }

            if (playerState == null)
            {
                playerState = GetComponent<DemoPlayerState>();
            }

            if (playerState == null)
            {
                playerState = GetComponentInParent<DemoPlayerState>();
            }

            if (blackboard == null)
            {
                blackboard = GetComponent<PoliceBlackboard>();
            }

            if (blackboard == null)
            {
                blackboard = GetComponentInParent<PoliceBlackboard>();
            }
        }

        private void ApplyAnimatorOptions()
        {
            if (animator != null && forceRootMotionOff)
            {
                animator.applyRootMotion = false;
            }
        }

        private float CalculateSpeed()
        {
            float speed = 0f;

            if (agent != null)
            {
                speed = agent.velocity.magnitude;
            }
            else if (playerState != null && playerState.CurrentSpeed > movingThreshold)
            {
                speed = playerState.IsRunning ? runningVisualSpeed : walkingVisualSpeed;
            }

            if (float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0f)
            {
                speed = 0f;
            }

            return Mathf.Clamp(speed, 0f, 6f);
        }

        private bool HasBehavior(string token)
        {
            return blackboard != null &&
                   !string.IsNullOrEmpty(blackboard.CurrentBehaviorName) &&
                   blackboard.CurrentBehaviorName.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SetFloat(string parameterName, float value)
        {
            if (HasParameter(parameterName, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        private void SetBool(string parameterName, bool value)
        {
            if (HasParameter(parameterName, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(parameterName, value);
            }
        }

        private bool HasParameter(string parameterName, AnimatorControllerParameterType expectedType)
        {
            if (animator == null)
            {
                return false;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == expectedType)
                {
                    return true;
                }
            }

            WarnMissingParameter(parameterName, expectedType);
            return false;
        }

        private void WarnMissingParameter(string parameterName, AnimatorControllerParameterType expectedType)
        {
            if (!logMissingParametersOnce || !warnedMissingParameters.Add(parameterName))
            {
                return;
            }

            Debug.LogWarning($"Animator on {name} is missing parameter '{parameterName}' of type {expectedType}.", this);
        }
    }
}
