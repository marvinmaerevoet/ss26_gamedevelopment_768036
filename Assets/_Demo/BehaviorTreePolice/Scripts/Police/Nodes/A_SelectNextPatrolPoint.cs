using Demo.BehaviorTreePolice.BehaviorTree;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class A_SelectNextPatrolPoint : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPatrolPoints;

        public A_SelectNextPatrolPoint(PoliceAIContext context) : base("Select Next Patrol Point")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard, false);

            if (blackboard == null)
            {
                return BTStatus.Failure;
            }

            if (context.PatrolPoints == null || context.PatrolPoints.Length == 0)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs PatrolPoints.", ref warnedMissingPatrolPoints);
                return BTStatus.Failure;
            }

            int startIndex = blackboard.CurrentPatrolPoint == null
                ? Mathf.Clamp(blackboard.CurrentPatrolIndex, 0, context.PatrolPoints.Length - 1)
                : (blackboard.CurrentPatrolIndex + 1) % context.PatrolPoints.Length;

            for (int offset = 0; offset < context.PatrolPoints.Length; offset++)
            {
                int index = (startIndex + offset) % context.PatrolPoints.Length;
                Transform patrolPoint = context.PatrolPoints[index];

                if (patrolPoint == null)
                {
                    continue;
                }

                blackboard.CurrentPatrolPoint = patrolPoint;
                blackboard.CurrentPatrolIndex = index;
                blackboard.CurrentBehaviorName = "Patrol";
                return BTStatus.Success;
            }

            PoliceNodeSupport.WarnOnce($"{Name} found only null PatrolPoints.", ref warnedMissingPatrolPoints);
            return BTStatus.Failure;
        }
    }
}
