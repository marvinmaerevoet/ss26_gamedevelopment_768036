using BehaviorTreeDemo.Police;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;
namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
{
    public sealed class A_LookAtPlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPlayer;
        private bool facingStarted;

        public A_LookAtPlayer(PoliceAIContext context) : base("Look At Player")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard);

            if (blackboard == null)
            {
                return BTStatus.Failure;
            }

            if (blackboard.Player == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a Player Transform.", ref warnedMissingPlayer);
                return BTStatus.Failure;
            }

            facingStarted = context.BeginFacingPlayer();
            return facingStarted ? BTStatus.Running : BTStatus.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            if (facingStarted)
            {
                context.StopFacingPlayer();
                facingStarted = false;
            }
        }
    }
}
