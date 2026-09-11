using BehaviorTreeDemo.Police;
using Unity.Behavior;
using UnityEngine;

namespace BehaviorTreeDemo.AI.UnityBehavior
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PoliceAIContext), typeof(BehaviorGraphAgent))]
    public sealed class UnityBehaviorPoliceDecisionController : PoliceDecisionController
    {
        [SerializeField] private BehaviorGraphAgent behaviorAgent;
        [SerializeField] private PoliceAIContext context;

        public BehaviorGraphAgent BehaviorAgent => behaviorAgent;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (behaviorAgent == null)
            {
                return;
            }

            behaviorAgent.enabled = true;
            if (Application.isPlaying && behaviorAgent.Graph != null)
            {
                behaviorAgent.Start();
            }
        }

        private void OnDisable()
        {
            if (behaviorAgent != null)
            {
                behaviorAgent.End();
                behaviorAgent.enabled = false;
            }

            StopOwnedOperations();
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        public override void ResetDecisionState()
        {
            EnsureReferences();
            StopOwnedOperations();

            if (behaviorAgent != null && behaviorAgent.Graph != null && behaviorAgent.isActiveAndEnabled)
            {
                behaviorAgent.Restart();
            }
        }

        private void StopOwnedOperations()
        {
            if (context == null)
            {
                return;
            }

            context.CancelActiveOperations();
            context.SetMovementMode(PoliceMovementMode.Walk);

            if (context.PoliceBlackboard != null)
            {
                context.PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.None;
            }
        }

        private void EnsureReferences()
        {
            if (behaviorAgent == null)
            {
                behaviorAgent = GetComponent<BehaviorGraphAgent>();
            }

            if (context == null)
            {
                context = GetComponent<PoliceAIContext>();
            }
        }
    }
}
