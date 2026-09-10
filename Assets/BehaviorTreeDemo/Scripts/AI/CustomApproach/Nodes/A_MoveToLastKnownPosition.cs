using BehaviorTreeDemo.Police;
using UnityEngine;

namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
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

            Context.SetMovementMode(PoliceMovementMode.Walk);
            Context.BeginInvestigationTravel();
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Investigate;
            destination = blackboard.LastKnownPlayerPosition;
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            Context?.CancelInvestigation();
        }
    }
}
