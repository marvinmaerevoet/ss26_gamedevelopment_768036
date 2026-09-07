using CustomApproachDemo.BehaviorTree;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class C_PlayerInArrestRange : BTCondition
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public C_PlayerInArrestRange(PoliceAIContext context) : base("Player In Arrest Range")
        {
            this.context = context;
        }

        protected override bool Evaluate()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard);

            return blackboard != null && blackboard.PlayerInArrestRange;
        }
    }
}
