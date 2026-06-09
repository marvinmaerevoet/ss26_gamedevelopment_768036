using UnityEngine;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class A_MoveToLastKnownPosition : PoliceMovementAction
    {
        public A_MoveToLastKnownPosition(PoliceAIContext context) : base("Move To Last Known Position", context)
        {
        }

        protected override bool TryGetDestination(PoliceBlackboard blackboard, out Vector3 destination)
        {
            if (!blackboard.HasLastKnownPlayerPosition)
            {
                destination = Vector3.zero;
                return false;
            }

            blackboard.CurrentBehaviorName = "Investigate";
            destination = blackboard.LastKnownPlayerPosition;
            return true;
        }
    }
}
