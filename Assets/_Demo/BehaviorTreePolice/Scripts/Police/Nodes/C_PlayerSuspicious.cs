using Demo.BehaviorTreePolice.BehaviorTree;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class C_PlayerSuspicious : BTCondition
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public C_PlayerSuspicious(PoliceAIContext context) : base("Player Suspicious")
        {
            this.context = context;
        }

        protected override bool Evaluate()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard);

            return blackboard != null && blackboard.PlayerSuspicious;
        }
    }
}
