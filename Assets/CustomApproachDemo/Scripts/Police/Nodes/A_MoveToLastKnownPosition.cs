using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
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

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Investigate;
            destination = blackboard.LastKnownPlayerPosition;
            return true;
        }
    }
}
