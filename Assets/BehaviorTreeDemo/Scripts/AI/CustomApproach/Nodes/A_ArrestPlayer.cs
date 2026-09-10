using BehaviorTreeDemo.Police;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;

namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
{
    public sealed class A_ArrestPlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;

        public A_ArrestPlayer(PoliceAIContext context) : base("Arrest Player")
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

            if (!context.IsArrestLatched && !context.TryBeginArrest())
            {
                return BTStatus.Failure;
            }

            switch (context.UpdateArrest())
            {
                case PoliceArrestStatus.Running:
                    return BTStatus.Running;
                case PoliceArrestStatus.Completed:
                    return BTStatus.Success;
                default:
                    return BTStatus.Failure;
            }
        }

        public override void Reset()
        {
            base.Reset();
            context?.CancelArrest();
        }
    }
}
