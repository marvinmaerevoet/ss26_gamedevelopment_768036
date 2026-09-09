using System;
using CustomApproachDemo.Gameplay.Carry;
using UnityEngine;

namespace CustomApproachDemo.Gameplay.Mission
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class DemoDeliveryZone : MonoBehaviour
    {
        [SerializeField] private DemoCarryable expectedCarryable;
        [SerializeField] private DemoPlayerCarryController playerCarryController;
        [SerializeField] private Transform deliveredCrateAnchor;

        public bool IsDelivered { get; private set; }
        public event Action Delivered;
        public void ResetDeliveryState() => IsDelivered = false;

        private void OnTriggerEnter(Collider other)
        {
            TryDeliver(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDeliver(other);
        }

        private void TryDeliver(Collider other)
        {
            if (IsDelivered || playerCarryController == null || expectedCarryable == null ||
                deliveredCrateAnchor == null ||
                other.GetComponentInParent<DemoPlayerCarryController>() != playerCarryController ||
                playerCarryController.CurrentCarryable != expectedCarryable)
                return;

            if (!playerCarryController.PlaceCurrentCarryable(expectedCarryable, deliveredCrateAnchor))
                return;

            expectedCarryable.LockPickupAtCurrentPosition();
            IsDelivered = true;
            Delivered?.Invoke();
        }
    }
}
