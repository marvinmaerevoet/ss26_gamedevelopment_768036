using System;
using UnityEngine;

namespace BehaviorTreeDemo.Police
{
    [Serializable]
    public sealed class PoliceDecisionSlot
    {
        public BehaviorTreeApproach approach;
        public PoliceDecisionController controller;
    }

    [DefaultExecutionOrder(-9000)]
    public sealed class PoliceDecisionHost : PoliceDecisionController
    {
        [SerializeField] private PoliceDecisionSlot[] controllers = Array.Empty<PoliceDecisionSlot>();

        private bool warnedMissingController;

        public BehaviorTreeApproach ActiveApproach { get; private set; }
        public PoliceDecisionController ActiveController { get; private set; }

        private void Awake()
        {
            ApplySelection();
        }

        private void OnEnable()
        {
            ApplySelection();
        }

        public override void ResetDecisionState()
        {
            if (ActiveController == null)
            {
                ApplySelection();
            }

            ActiveController?.ResetDecisionState();
        }

        private void ApplySelection()
        {
            BehaviorTreeApproach requestedApproach = BehaviorTreeApproachSelection.CurrentApproach;
            ActiveApproach = requestedApproach;
            ActiveController = FindLocalController(requestedApproach);

            PoliceDecisionController[] localControllers = GetComponents<PoliceDecisionController>();

            // Stop every other local decision layer before the selected one starts.
            // This also catches stale or accidentally unregistered controllers.
            for (int index = 0; index < localControllers.Length; index++)
            {
                PoliceDecisionController controller = localControllers[index];
                if (controller == null || controller == this || controller == ActiveController)
                {
                    continue;
                }

                controller.enabled = false;
            }

            if (ActiveController != null)
            {
                ActiveController.enabled = true;
                warnedMissingController = false;
                return;
            }

            if (!warnedMissingController)
            {
                Debug.LogWarning($"PoliceDecisionHost on {name} has no local controller for {ActiveApproach}. No decision layer will be activated.", this);
                warnedMissingController = true;
            }
        }

        private PoliceDecisionController FindLocalController(BehaviorTreeApproach approach)
        {
            for (int index = 0; index < controllers.Length; index++)
            {
                PoliceDecisionSlot slot = controllers[index];
                PoliceDecisionController controller = slot?.controller;
                if (controller != null && controller.gameObject == gameObject && slot.approach == approach)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}
