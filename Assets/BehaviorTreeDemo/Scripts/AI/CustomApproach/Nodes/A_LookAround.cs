using BehaviorTreeDemo.Police;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;
namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
{
    public sealed class A_LookAround : BTAction
    {
        private readonly PoliceAIContext context;
        private bool started;
        private bool warnedMissingContext;

        public A_LookAround(PoliceAIContext context) : base("Look Around")
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

            if (!started)
            {
                started = context.BeginLookingAround();
                if (!started)
                {
                    return BTStatus.Failure;
                }
            }

            PoliceInvestigationStatus status = context.UpdateLookingAround();
            switch (status)
            {
                case PoliceInvestigationStatus.Completed:
                    started = false;
                    return BTStatus.Success;
                case PoliceInvestigationStatus.Failed:
                    started = false;
                    return BTStatus.Failure;
                default:
                    return BTStatus.Running;
            }
        }

        public override void Reset()
        {
            base.Reset();
            context?.CancelInvestigation();
            started = false;
        }
    }
}
