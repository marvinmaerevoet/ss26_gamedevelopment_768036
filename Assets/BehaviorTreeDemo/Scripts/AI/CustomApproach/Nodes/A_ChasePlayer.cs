using BehaviorTreeDemo.Police;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;
using UnityEngine;

namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
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
                context.StopMovement();
                return BTStatus.Failure;
            }

            if (!blackboard.PlayerSuspicious)
            {
                context.StopMovement();
                return BTStatus.Failure;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Chase;

            if (!blackboard.HasLastKnownPlayerPosition)
            {
                context.StopMovement();
                return BTStatus.Failure;
            }

            if (blackboard.PlayerInArrestRange)
            {
                context.StopMovement();
                return BTStatus.Success;
            }

            context.SetMovementMode(PoliceMovementMode.Run);
            if (!context.TrySetDestination(blackboard.LastKnownPlayerPosition))
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
