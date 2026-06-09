using System;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Player
{
    public sealed class DemoSimplePlayerController : MonoBehaviour
    {
        public float walkSpeed = 3f;
        public float runSpeed = 6f;
        public float gravity = -20f;
        public bool disableWhenArrested = true;
        public DemoPlayerState playerState;

        private CharacterController characterController;
        private float verticalVelocity;
        private bool warnedInputUnavailable;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerState == null)
            {
                playerState = GetComponent<DemoPlayerState>();
            }
        }

        private void Update()
        {
            if (playerState != null && disableWhenArrested && playerState.IsArrested)
            {
                SetRunning(false);
                return;
            }

            if (!TryReadMovementInput(out Vector2 input, out bool wantsToRun))
            {
                SetRunning(false);
                return;
            }

            Vector3 moveDirection = GetCameraRelativeDirection(input);
            bool hasMovement = moveDirection.sqrMagnitude > 0.001f;
            bool isRunning = hasMovement && wantsToRun;
            float speed = isRunning ? runSpeed : walkSpeed;

            Move(moveDirection * speed);
            SetRunning(isRunning);
        }

        private bool TryReadMovementInput(out Vector2 input, out bool wantsToRun)
        {
            input = Vector2.zero;
            wantsToRun = false;

            try
            {
                float horizontal = 0f;
                float vertical = 0f;

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    horizontal -= 1f;
                }

                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    horizontal += 1f;
                }

                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    vertical -= 1f;
                }

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    vertical += 1f;
                }

                input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
                wantsToRun = Input.GetKey(KeyCode.LeftShift);
                return true;
            }
            catch (InvalidOperationException)
            {
                if (!warnedInputUnavailable)
                {
                    Debug.LogWarning("DemoSimplePlayerController could not read UnityEngine.Input. Enable legacy input support or use another controller.", this);
                    warnedInputUnavailable = true;
                }

                return false;
            }
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector3.zero;
            }

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }

        private void Move(Vector3 horizontalVelocity)
        {
            float deltaTime = Time.deltaTime;

            if (characterController != null)
            {
                if (characterController.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -1f;
                }

                verticalVelocity += gravity * deltaTime;
                Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
                characterController.Move(velocity * deltaTime);
                return;
            }

            transform.position += horizontalVelocity * deltaTime;
        }

        private void SetRunning(bool isRunning)
        {
            if (playerState != null)
            {
                playerState.IsRunning = isRunning;
            }
        }
    }
}
