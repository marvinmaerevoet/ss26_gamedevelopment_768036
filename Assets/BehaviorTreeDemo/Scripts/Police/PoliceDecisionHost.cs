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
            ActiveApproach = BehaviorTreeApproachSelection.CurrentApproach;
            ActiveController = null;

            for (int index = 0; index < controllers.Length; index++)
            {
                PoliceDecisionSlot slot = controllers[index];
                PoliceDecisionController controller = slot?.controller;
                if (controller == null)
                {
                    continue;
                }

                bool isLocal = controller.gameObject == gameObject;
                bool shouldEnable = isLocal && ActiveController == null && slot.approach == ActiveApproach;
                controller.enabled = shouldEnable;

                if (shouldEnable)
                {
                    ActiveController = controller;
                }
            }

            if (ActiveController == null && !warnedMissingController)
            {
                Debug.LogWarning($"PoliceDecisionHost on {name} has no local controller for {ActiveApproach}.", this);
                warnedMissingController = true;
            }
        }
    }
}
