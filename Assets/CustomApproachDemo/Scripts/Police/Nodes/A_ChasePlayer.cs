using CustomApproachDemo.BehaviorTree;
using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_ChasePlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPlayer;

        public A_ChasePlayer(PoliceAIContext context) : base("Chase Player")
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

            if (!blackboard.PlayerVisible)
            {
                return BTStatus.Failure;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Chase;
            blackboard.LastKnownPlayerPosition = blackboard.Player.position;
            blackboard.HasLastKnownPlayerPosition = true;

            if (blackboard.PlayerInArrestRange)
            {
                context.StopMovement();
                return BTStatus.Success;
            }

            if (!context.TrySetDestination(blackboard.Player.position))
            {
                return BTStatus.Failure;
            }

            // Reaching the current chase destination does not finish Chase;
            // only entering arrest range completes this action.
            return context.GetMovementStatus() == PoliceMovementStatus.Failed
                ? BTStatus.Failure
                : BTStatus.Running;
        }
    }
}
