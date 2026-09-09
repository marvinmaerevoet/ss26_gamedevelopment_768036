using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomApproachDemo.Gameplay.Carry
{
    [DisallowMultipleComponent]
    public sealed class DemoPlayerCarryController : MonoBehaviour
    {
        private const int MaxCarryableHits = 12;
        private const string DefaultCarryAnchorName = "CarryAnchor";

        [Header("References")]
        [SerializeField] private Transform carryAnchor;

        [Header("Input")]
        [SerializeField] private Key interactKey = Key.E;

        [Header("Pickup")]
        [SerializeField, Min(0.1f)] private float pickupRange = 2f;
        [SerializeField] private Vector3 pickupOriginOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private LayerMask pickupMask = ~0;

        [Header("Drop")]
        [SerializeField, Min(0.2f)] private float dropDistance = 1.25f;
        [SerializeField, Min(0f)] private float dropProbeHeight = 1.5f;
        [SerializeField, Min(0.1f)] private float dropProbeDistance = 3f;
        [SerializeField] private LayerMask dropGroundMask = ~0;

        private readonly Collider[] carryableHits = new Collider[MaxCarryableHits];
        private DemoCarryable currentCarryable;

        public bool IsCarrying => currentCarryable != null;
        public DemoCarryable CurrentCarryable => currentCarryable;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            pickupRange = Mathf.Max(0.1f, pickupRange);
            dropDistance = Mathf.Max(0.2f, dropDistance);
            dropProbeHeight = Mathf.Max(0f, dropProbeHeight);
            dropProbeDistance = Mathf.Max(0.1f, dropProbeDistance);
            ResolveReferences();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[interactKey].wasPressedThisFrame)
            {
                return;
            }

            if (IsCarrying)
            {
                DropCurrent();
                return;
            }

            TryPickupNearest();
        }

        public bool TryPickupNearest()
        {
            ResolveReferences();

            if (carryAnchor == null || IsCarrying)
            {
                return false;
            }

            DemoCarryable nearest = FindNearestCarryable();
            if (nearest == null)
            {
                return false;
            }

            currentCarryable = nearest;
            currentCarryable.BeginCarry(carryAnchor);
            return true;
        }

        public void DropCurrent()
        {
            if (currentCarryable == null)
            {
                return;
            }

            Vector3 dropPosition = ResolveDropPosition(currentCarryable);
            Quaternion dropRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            currentCarryable.EndCarry(dropPosition, dropRotation);
            currentCarryable = null;
        }

        private void ResolveReferences()
        {
            if (carryAnchor != null)
            {
                return;
            }

            Transform foundAnchor = transform.Find(DefaultCarryAnchorName);
            if (foundAnchor != null)
            {
                carryAnchor = foundAnchor;
            }
        }

        public bool PlaceCurrentCarryable(DemoCarryable expected, Transform destination)
        {
            if (expected == null || destination == null || currentCarryable != expected || !expected.IsCarried)
                return false;

            expected.EndCarry(destination.position, destination.rotation);
            currentCarryable = null;
            return true;
        }

        public void ResetCarryable(DemoCarryable carryable, Transform target)
        {
            if (carryable == null || target == null) return;
            if (currentCarryable == carryable) currentCarryable = null;
            carryable.ResetTo(target);
        }

        private DemoCarryable FindNearestCarryable()
        {
            Vector3 origin = transform.position + pickupOriginOffset;
            int hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                pickupRange,
                carryableHits,
                pickupMask,
                QueryTriggerInteraction.Ignore);

            DemoCarryable nearest = null;
            float nearestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = carryableHits[i];
                carryableHits[i] = null;

                if (hit == null)
                {
                    continue;
                }

                DemoCarryable carryable = hit.GetComponentInParent<DemoCarryable>();
                if (carryable == null || carryable.IsCarried || carryable.IsPickupLocked)
                {
                    continue;
                }

                float distanceSqr = (carryable.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearest = carryable;
                    nearestDistanceSqr = distanceSqr;
                }
            }

            return nearest;
        }

        private Vector3 ResolveDropPosition(DemoCarryable carryable)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            Vector3 dropPosition = transform.position + forward * dropDistance;
            Vector3 probeOrigin = dropPosition + Vector3.up * dropProbeHeight;

            if (Physics.Raycast(
                probeOrigin,
                Vector3.down,
                out RaycastHit hit,
                dropProbeHeight + dropProbeDistance,
                dropGroundMask,
                QueryTriggerInteraction.Ignore))
            {
                dropPosition.y = hit.point.y + carryable.DropGroundOffset;
            }

            return dropPosition;
        }
    }
}
