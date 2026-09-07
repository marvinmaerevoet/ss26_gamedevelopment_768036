using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomApproachDemo.Player
{
    public sealed class DemoPlayerState : MonoBehaviour
    {
        public bool IsRunning;
        public bool IsInRestrictedArea;
        public bool IsArrested;
        public float CurrentSpeed;
        public Key runSimulationKey = Key.LeftShift;
        public bool simulateRunningFromKey = true;
        public float runningSpeedThreshold = 3.5f;
        public float maxReasonableFallbackSpeed = 12f;
        public float teleportDistanceThreshold = 3f;

        private Vector3 previousPosition;
        private bool hasPositionSample;
        private bool movementReportedThisFrame;
        private bool reportedIsRunning;

        private void Awake()
        {
            ResetSpeedTracking();
        }

        private void Update()
        {
            if (movementReportedThisFrame)
            {
                IsRunning = reportedIsRunning;
                previousPosition = transform.position;
                hasPositionSample = true;
                movementReportedThisFrame = false;
                return;
            }

            float deltaTime = Time.deltaTime;

            if (!hasPositionSample)
            {
                previousPosition = transform.position;
                hasPositionSample = true;
                CurrentSpeed = 0f;
            }
            else if (deltaTime > 0.0001f)
            {
                float distance = Vector3.Distance(transform.position, previousPosition);
                if (distance > teleportDistanceThreshold)
                {
                    CurrentSpeed = 0f;
                }
                else
                {
                    CurrentSpeed = Mathf.Clamp(distance / deltaTime, 0f, maxReasonableFallbackSpeed);
                }
            }

            bool runningFromKey = simulateRunningFromKey && IsRunSimulationKeyPressed();
            bool runningFromSpeed = CurrentSpeed > runningSpeedThreshold;

            IsRunning = runningFromKey || runningFromSpeed;
            previousPosition = transform.position;
        }

        public void ReportMovement(float movementSpeed, bool isRunning)
        {
            CurrentSpeed = Mathf.Clamp(SanitizeSpeed(movementSpeed), 0f, maxReasonableFallbackSpeed);
            reportedIsRunning = isRunning && CurrentSpeed > 0f;
            IsRunning = reportedIsRunning;
            movementReportedThisFrame = true;
        }

        public void ResetSpeedTracking()
        {
            previousPosition = transform.position;
            hasPositionSample = true;
            movementReportedThisFrame = false;
            reportedIsRunning = false;
            CurrentSpeed = 0f;
            IsRunning = false;
        }

        private bool IsRunSimulationKeyPressed()
        {
            if (!simulateRunningFromKey)
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyboard[runSimulationKey].isPressed;
        }

        private static float SanitizeSpeed(float speed)
        {
            return float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0f ? 0f : speed;
        }
    }
}
