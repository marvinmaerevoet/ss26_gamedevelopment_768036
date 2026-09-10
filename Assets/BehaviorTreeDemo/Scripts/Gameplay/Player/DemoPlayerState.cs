using UnityEngine;

namespace BehaviorTreeDemo.Gameplay.Player
{
    public sealed class DemoPlayerState : MonoBehaviour
    {
        public bool IsRunning;
        [UnityEngine.Serialization.FormerlySerializedAs("IsArrested")]
        [SerializeField] private bool isArrested;
        public event System.Action Arrested;
        public bool IsArrested
        {
            get => isArrested;
            set
            {
                if (isArrested == value) return;
                isArrested = value;
                if (value) Arrested?.Invoke();
            }
        }
        public float CurrentSpeed;
        public float runningSpeedThreshold = 3.5f;
        public float maxReasonableFallbackSpeed = 12f;
        public float teleportDistanceThreshold = 3f;

        private Vector3 previousPosition;
        private bool hasPositionSample;
        private int movementReportedFrame = -1;

        private void Awake()
        {
            ResetSpeedTracking();
        }

        private void LateUpdate()
        {
            if (movementReportedFrame == Time.frameCount)
            {
                previousPosition = transform.position;
                hasPositionSample = true;
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

            bool runningFromSpeed = CurrentSpeed > runningSpeedThreshold;

            IsRunning = runningFromSpeed;
            previousPosition = transform.position;
        }

        public void ReportMovement(float movementSpeed, bool isRunning)
        {
            CurrentSpeed = Mathf.Clamp(SanitizeSpeed(movementSpeed), 0f, maxReasonableFallbackSpeed);
            IsRunning = isRunning && CurrentSpeed > 0f;
            movementReportedFrame = Time.frameCount;
        }

        public void ResetSpeedTracking()
        {
            previousPosition = transform.position;
            hasPositionSample = true;
            movementReportedFrame = -1;
            CurrentSpeed = 0f;
            IsRunning = false;
        }

        private static float SanitizeSpeed(float speed)
        {
            return float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0f ? 0f : speed;
        }
    }
}
