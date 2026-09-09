using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_FleeToSafePoint : PoliceMovementAction
    {
        private bool warnedMissingSafePoint;

        public A_FleeToSafePoint(PoliceAIContext context) : base("Flee To Safe Point", context)
        {
        }

        protected override bool TryGetDestination(PoliceBlackboard blackboard, out Vector3 destination)
        {
            if (Context.safePoint == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a safePoint.", ref warnedMissingSafePoint);
                destination = Vector3.zero;
                return false;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Emergency;
            destination = Context.safePoint.position;
            return true;
        }
    }
}
