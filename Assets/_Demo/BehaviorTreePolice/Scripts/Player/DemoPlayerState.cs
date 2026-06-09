using System;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Player
{
    public sealed class DemoPlayerState : MonoBehaviour
    {
        public bool IsRunning;
        public bool IsInRestrictedArea;
        public bool IsArrested;
        public float CurrentSpeed;
        public KeyCode runSimulationKey = KeyCode.LeftShift;
        public bool simulateRunningFromKey = true;
        public float runningSpeedThreshold = 3.5f;

        private Vector3 previousPosition;
        private bool warnedInputUnavailable;

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

            try
            {
                return Input.GetKey(runSimulationKey);
            }
            catch (InvalidOperationException)
            {
                if (!warnedInputUnavailable)
                {
                    Debug.LogWarning("DemoPlayerState could not read UnityEngine.Input. Running detection still works through CurrentSpeed.", this);
                    warnedInputUnavailable = true;
                }

                return false;
            }
        }
    }
}
