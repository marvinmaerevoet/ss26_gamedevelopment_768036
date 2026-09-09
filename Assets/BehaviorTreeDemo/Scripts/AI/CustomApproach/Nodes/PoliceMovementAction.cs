using CustomApproachDemo.BehaviorTree;
using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public abstract class PoliceMovementAction : BTAction
    {
        protected readonly PoliceAIContext Context;

        private bool destinationSet;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        protected PoliceMovementAction(string name, PoliceAIContext context) : base(name)
        {
            Context = context;
        }

        protected override BTStatus Execute()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                Context, Name, ref warnedMissingContext, ref warnedMissingBlackboard, false);

            if (blackboard == null)
            {
                return BTStatus.Failure;
            }

            if (!destinationSet)
            {
                if (!TryGetDestination(blackboard, out Vector3 destination))
                {
                    return BTStatus.Failure;
                }

                if (!Context.TrySetDestination(destination))
                {
                    return BTStatus.Failure;
                }

                destinationSet = true;
                return BTStatus.Running;
            }

            BTStatus status = PoliceNodeSupport.ToBTStatus(Context.GetMovementStatus());

            if (status != BTStatus.Running)
            {
                destinationSet = false;
            }

            return status;
        }

        public override void Reset()
        {
            base.Reset();
            destinationSet = false;
        }

        protected abstract bool TryGetDestination(PoliceBlackboard blackboard, out Vector3 destination);
    }
}
