using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.BehaviorTreePolice.Player
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

        private Vector3 previousPosition;

        private void Awake()
        {
            previousPosition = transform.position;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (deltaTime > Mathf.Epsilon)
            {
                CurrentSpeed = Vector3.Distance(transform.position, previousPosition) / deltaTime;
            }

            bool runningFromKey = simulateRunningFromKey && IsRunSimulationKeyPressed();
            bool runningFromSpeed = CurrentSpeed > runningSpeedThreshold;

            IsRunning = runningFromKey || runningFromSpeed;
            previousPosition = transform.position;
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
    }
}
