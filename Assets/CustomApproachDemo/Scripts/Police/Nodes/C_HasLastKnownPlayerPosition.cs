using CustomApproachDemo.BehaviorTree;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class C_HasLastKnownPlayerPosition : BTCondition
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public C_HasLastKnownPlayerPosition(PoliceAIContext context) : base("Has Last Known Player Position")
        {
            this.context = context;
        }

        protected override bool Evaluate()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard);

            return blackboard != null && blackboard.HasLastKnownPlayerPosition;
        }
    }
}
