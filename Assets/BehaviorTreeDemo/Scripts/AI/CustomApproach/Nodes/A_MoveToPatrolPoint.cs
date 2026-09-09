using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_MoveToPatrolPoint : PoliceMovementAction
    {
        private bool warnedMissingPatrolPoint;

        public A_MoveToPatrolPoint(PoliceAIContext context) : base("Move To Patrol Point", context)
        {
        }

        protected override bool TryGetDestination(PoliceBlackboard blackboard, out Vector3 destination)
        {
            if (blackboard.CurrentPatrolPoint == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs CurrentPatrolPoint.", ref warnedMissingPatrolPoint);
                destination = Vector3.zero;
                return false;
            }

            Context.SetMovementMode(PoliceMovementMode.Walk);
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Patrol;
            destination = blackboard.CurrentPatrolPoint.position;
            return true;
        }
    }
}
