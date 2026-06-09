using Demo.BehaviorTreePolice.BehaviorTree;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class A_LookAtPlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPlayer;

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

            Vector3 direction = blackboard.Player.position - context.Self.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return BTStatus.Success;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            context.Self.rotation = Quaternion.Slerp(
                context.Self.rotation,
                targetRotation,
                Time.deltaTime * 12f);

            return BTStatus.Success;
        }
    }
}
