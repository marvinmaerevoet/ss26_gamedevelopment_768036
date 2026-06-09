using Demo.BehaviorTreePolice.BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class A_ChasePlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingAgent;
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

            blackboard.CurrentBehaviorName = "Chase";
            blackboard.LastKnownPlayerPosition = blackboard.Player.position;
            blackboard.HasLastKnownPlayerPosition = true;

            if (blackboard.PlayerInArrestRange)
            {
                context.StopMovement();
                return BTStatus.Success;
            }

            NavMeshAgent agent = PoliceNodeSupport.GetAgent(
                context, Name, ref warnedMissingContext, ref warnedMissingAgent);

            if (agent == null)
            {
                return BTStatus.Failure;
            }

            context.SetDestination(blackboard.Player.position);

            if (agent.pathPending)
            {
                return BTStatus.Running;
            }

            return agent.pathStatus != NavMeshPathStatus.PathComplete
                ? BTStatus.Failure
                : BTStatus.Running;
        }
    }
}
