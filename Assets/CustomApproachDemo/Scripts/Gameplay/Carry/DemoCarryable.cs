using UnityEngine;

namespace CustomApproachDemo.Gameplay.Carry
{
    [DisallowMultipleComponent]
    public sealed class DemoCarryable : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody carriedRigidbody;
        [SerializeField] private Collider[] carriedColliders;

        [Header("Carry Pose")]
        [SerializeField] private Vector3 carriedLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 carriedLocalEulerAngles = Vector3.zero;

        [Header("Physics")]
        [SerializeField] private bool disableCollidersWhileCarried = true;
        [SerializeField] private bool disableRigidbodyCollisionsWhileCarried = true;
        [SerializeField] private float dropGroundOffset = 0.03f;

        private Transform originalParent;
        private Transform activeCarryAnchor;
        private RigidbodyInterpolation originalInterpolation;
        private CollisionDetectionMode originalCollisionDetectionMode;
        private bool wasKinematic;
        private bool usedGravity;
        private bool detectedCollisions;
        private bool[] colliderEnabledStates;
        private Transform resetParent;
        private RigidbodyInterpolation resetInterpolation;
        private CollisionDetectionMode resetCollisionMode;

        public bool IsCarried { get; private set; }
        public bool IsPickupLocked { get; private set; }
        public Rigidbody Rigidbody => carriedRigidbody;
        public float DropGroundOffset => Mathf.Max(0f, dropGroundOffset);

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
            resetParent = transform.parent;
            if (carriedRigidbody != null)
            {
                resetInterpolation = carriedRigidbody.interpolation;
                resetCollisionMode = carriedRigidbody.collisionDetectionMode;
            }
        }

        // The owning carry controller must release its reference before calling this.
        public void ResetTo(Transform target)
        {
            if (target == null) return;
            ResolveReferences();
            if (IsCarried) EndCarry(target.position, target.rotation);
            activeCarryAnchor = null;
            IsCarried = false;
            IsPickupLocked = false;
            transform.SetParent(resetParent, true);
            if (carriedRigidbody != null)
            {
                carriedRigidbody.interpolation = RigidbodyInterpolation.None;
                carriedRigidbody.detectCollisions = false;
            }
            transform.SetPositionAndRotation(target.position, target.rotation);
            if (carriedRigidbody != null)
            {
                carriedRigidbody.position = target.position;
                carriedRigidbody.rotation = target.rotation;
                carriedRigidbody.isKinematic = false;
                carriedRigidbody.linearVelocity = Vector3.zero;
                carriedRigidbody.angularVelocity = Vector3.zero;
                carriedRigidbody.useGravity = true;
                carriedRigidbody.collisionDetectionMode = resetCollisionMode;
                carriedRigidbody.detectCollisions = true;
                carriedRigidbody.interpolation = resetInterpolation;
            }
            SetCollidersEnabled(true);
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void BeginCarry(Transform carryAnchor)
        {
            if (carryAnchor == null || IsCarried || IsPickupLocked)
            {
                return;
            }

            ResolveReferences();
            StorePhysicsState();

            originalParent = transform.parent;
            activeCarryAnchor = carryAnchor;

            if (carriedRigidbody != null)
            {
                carriedRigidbody.interpolation = RigidbodyInterpolation.None;
                carriedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                if (!carriedRigidbody.isKinematic)
                {
                    carriedRigidbody.linearVelocity = Vector3.zero;
                    carriedRigidbody.angularVelocity = Vector3.zero;
                }
                carriedRigidbody.useGravity = false;
                carriedRigidbody.isKinematic = true;

                if (disableRigidbodyCollisionsWhileCarried)
                {
                    carriedRigidbody.detectCollisions = false;
                }
            }

            SetCollidersEnabled(!disableCollidersWhileCarried);
            transform.SetParent(carryAnchor, false);
            IsCarried = true;
            SnapToCarryAnchor();
        }

        private void LateUpdate()
        {
            if (IsCarried && activeCarryAnchor != null)
            {
                // Player movement runs in Update. Apply the exact pose afterwards,
                // without physics interpolation or a time-dependent follow speed.
                SnapToCarryAnchor();
            }
        }

        private void SnapToCarryAnchor()
        {
            transform.localPosition = carriedLocalPosition;
            transform.localRotation = Quaternion.Euler(carriedLocalEulerAngles);
        }

        public void EndCarry(Vector3 dropPosition, Quaternion dropRotation)
        {
            if (!IsCarried)
            {
                return;
            }

            transform.SetParent(originalParent, true);
            transform.SetPositionAndRotation(dropPosition, dropRotation);

            // Seed the physics pose before restoring simulation/interpolation so
            // it cannot resume from the last pre-pickup physics position.
            if (carriedRigidbody != null)
            {
                carriedRigidbody.position = dropPosition;
                carriedRigidbody.rotation = dropRotation;
            }

            RestorePhysicsState();
            IsCarried = false;
            activeCarryAnchor = null;
        }

        private void ResolveReferences()
        {
            if (carriedRigidbody == null)
            {
                carriedRigidbody = GetComponent<Rigidbody>();
            }

            if (carriedColliders == null || carriedColliders.Length == 0)
            {
                carriedColliders = GetComponentsInChildren<Collider>();
            }
        }

        public void LockPickupAtCurrentPosition()
        {
            if (IsCarried) return;
            IsPickupLocked = true;
            if (carriedRigidbody == null) return;
            carriedRigidbody.interpolation = RigidbodyInterpolation.None;
            carriedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (!carriedRigidbody.isKinematic)
            {
                carriedRigidbody.linearVelocity = Vector3.zero;
                carriedRigidbody.angularVelocity = Vector3.zero;
            }
            carriedRigidbody.useGravity = false;
            carriedRigidbody.isKinematic = true;
        }

        private void StorePhysicsState()
        {
            if (carriedRigidbody != null)
            {
                wasKinematic = carriedRigidbody.isKinematic;
                usedGravity = carriedRigidbody.useGravity;
                detectedCollisions = carriedRigidbody.detectCollisions;
                originalInterpolation = carriedRigidbody.interpolation;
                originalCollisionDetectionMode = carriedRigidbody.collisionDetectionMode;
            }

            colliderEnabledStates = new bool[carriedColliders != null ? carriedColliders.Length : 0];

            for (int i = 0; i < colliderEnabledStates.Length; i++)
            {
                colliderEnabledStates[i] = carriedColliders[i] != null && carriedColliders[i].enabled;
            }
        }

        private void RestorePhysicsState()
        {
            RestoreColliders();

            if (carriedRigidbody == null)
            {
                return;
            }

            carriedRigidbody.isKinematic = wasKinematic;
            carriedRigidbody.useGravity = usedGravity;
            carriedRigidbody.detectCollisions = detectedCollisions;
            carriedRigidbody.collisionDetectionMode = originalCollisionDetectionMode;
            if (!wasKinematic)
            {
                carriedRigidbody.linearVelocity = Vector3.zero;
                carriedRigidbody.angularVelocity = Vector3.zero;
            }
            carriedRigidbody.interpolation = originalInterpolation;
        }

        private void SetCollidersEnabled(bool enabled)
        {
            if (carriedColliders == null)
            {
                return;
            }

            foreach (Collider carriedCollider in carriedColliders)
            {
                if (carriedCollider != null)
                {
                    carriedCollider.enabled = enabled;
                }
            }
        }

        private void RestoreColliders()
        {
            if (carriedColliders == null || colliderEnabledStates == null)
            {
                return;
            }

            int count = Mathf.Min(carriedColliders.Length, colliderEnabledStates.Length);
            for (int i = 0; i < count; i++)
            {
                if (carriedColliders[i] != null)
                {
                    carriedColliders[i].enabled = colliderEnabledStates[i];
                }
            }
        }
    }
}
