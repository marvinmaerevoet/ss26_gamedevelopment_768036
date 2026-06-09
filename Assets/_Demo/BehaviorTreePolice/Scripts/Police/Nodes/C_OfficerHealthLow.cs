using Demo.BehaviorTreePolice.BehaviorTree;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class C_OfficerHealthLow : BTCondition
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public C_OfficerHealthLow(PoliceAIContext context) : base("Officer Health Low")
        {
            this.context = context;
        }

        protected override bool Evaluate()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard);

            return blackboard != null && blackboard.OfficerHealthLow;
        }
    }
}
