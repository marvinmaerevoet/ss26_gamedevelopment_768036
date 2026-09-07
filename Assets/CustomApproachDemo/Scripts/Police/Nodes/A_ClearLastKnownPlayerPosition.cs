using CustomApproachDemo.BehaviorTree;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_ClearLastKnownPlayerPosition : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;

        public A_ClearLastKnownPlayerPosition(PoliceAIContext context) : base("Clear Last Known Player Position")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a PoliceAIContext.", ref warnedMissingContext);
                return BTStatus.Success;
            }

            context.ClearLastKnownPlayerPosition();
            return BTStatus.Success;
        }
    }
}
