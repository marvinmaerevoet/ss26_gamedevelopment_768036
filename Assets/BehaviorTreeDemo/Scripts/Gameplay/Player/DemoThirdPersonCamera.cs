using UnityEngine;
using UnityEngine.InputSystem;

namespace BehaviorTreeDemo.Gameplay.Player
{
    public sealed class DemoThirdPersonCamera : MonoBehaviour
    {
        private const int MaxCollisionHits = 16;

        public Transform target;
        public Vector3 targetOffset = new Vector3(0f, 1.4f, 0f);
        public float distance = 5f;
        public float minDistance = 2f;
        public float maxDistance = 10f;
        public float height = 2f;
        public float mouseSensitivity = 120f;
        public float zoomSensitivity = 2f;
        public float minPitch = -20f;
        public float maxPitch = 65f;
        public bool rotateOnlyWhileRightMouseHeld;
        public bool lockCursorOnPlay = true;
        public bool followInLateUpdate = true;
        public LayerMask collisionMask = Physics.DefaultRaycastLayers;
        public float collisionRadius = 0.2f;

        private readonly RaycastHit[] collisionHits = new RaycastHit[MaxCollisionHits];
        private float yaw;
        private float pitch;
        private bool warnedMissingTarget;

        private void Start()
        {
            ResolveTarget();

            Vector3 eulerAngles = transform.eulerAngles;
            yaw = eulerAngles.y;
            pitch = NormalizePitch(eulerAngles.x);

            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (lockCursorOnPlay)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (!followInLateUpdate)
            {
                UpdateCamera();
            }
        }

        private void LateUpdate()
        {
            if (followInLateUpdate)
            {
                UpdateCamera();
            }
        }

        private void UpdateCamera()
        {
            ResolveTarget();

            if (target == null)
            {
                return;
            }

            ReadMouseInput();

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 lookPoint = target.position + targetOffset;
            Vector3 desiredPosition = lookPoint + Vector3.up * height - rotation * Vector3.forward * distance;
            Vector3 finalPosition = ResolveCollision(lookPoint, desiredPosition);

            transform.SetPositionAndRotation(finalPosition, rotation);
            transform.LookAt(lookPoint);
        }

        private void ResolveTarget()
        {
            if (target != null)
            {
                return;
            }

            if (!warnedMissingTarget)
            {
                Debug.LogError("DemoThirdPersonCamera needs an assigned target reference.", this);
                warnedMissingTarget = true;
            }
        }

        private void ReadMouseInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            bool canRotate = !rotateOnlyWhileRightMouseHeld || mouse.rightButton.isPressed;
            if (canRotate)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity * Time.unscaledDeltaTime;
                pitch -= delta.y * mouseSensitivity * Time.unscaledDeltaTime;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Vector2 scroll = mouse.scroll.ReadValue();
            if (Mathf.Abs(scroll.y) > Mathf.Epsilon)
            {
                float scrollSteps = scroll.y / 120f;
                distance = Mathf.Clamp(distance - scrollSteps * zoomSensitivity, minDistance, maxDistance);
            }
        }

        private Vector3 ResolveCollision(Vector3 lookPoint, Vector3 desiredPosition)
        {
            if (collisionMask.value == 0)
            {
                return desiredPosition;
            }

            Vector3 toCamera = desiredPosition - lookPoint;
            float desiredDistance = toCamera.magnitude;

            if (desiredDistance <= Mathf.Epsilon)
            {
                return desiredPosition;
            }

            Vector3 direction = toCamera / desiredDistance;
            float radius = Mathf.Max(0.01f, collisionRadius);

            int hitCount = Physics.SphereCastNonAlloc(
                    lookPoint,
                    radius,
                    direction,
                    collisionHits,
                    desiredDistance,
                    collisionMask,
                    QueryTriggerInteraction.Ignore);

            float nearestHitDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = collisionHits[index];
                Transform hitTransform = hit.transform;
                if (hitTransform == null ||
                    (target != null && (hitTransform == target || hitTransform.IsChildOf(target))))
                {
                    continue;
                }

                nearestHitDistance = Mathf.Min(nearestHitDistance, hit.distance);
            }

            if (!float.IsPositiveInfinity(nearestHitDistance))
            {
                return lookPoint + direction * Mathf.Max(0f, nearestHitDistance - radius);
            }

            return desiredPosition;
        }

        private static float NormalizePitch(float value)
        {
            return value > 180f ? value - 360f : value;
        }
    }
}
