using CustomApproachDemo.BehaviorTree;
namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_SelectNextPatrolPoint : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingPatrolPoints;

        public A_SelectNextPatrolPoint(PoliceAIContext context) : base("Select Next Patrol Point")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a PoliceAIContext.", ref warnedMissingContext);
                return BTStatus.Failure;
            }

            if (!context.SelectRandomPatrolPoint())
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs at least one valid PatrolPoint.", ref warnedMissingPatrolPoints);
                return BTStatus.Failure;
            }

            return BTStatus.Success;
        }
    }
}
