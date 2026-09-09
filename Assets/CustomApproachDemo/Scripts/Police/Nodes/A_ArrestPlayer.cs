using CustomApproachDemo.BehaviorTree;
using CustomApproachDemo.Player;
using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_ArrestPlayer : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPlayerState;
        private bool loggedArrest;

        public A_ArrestPlayer(PoliceAIContext context) : base("Arrest Player")
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

            DemoPlayerState playerState = context.PlayerState;
            if (playerState == null && blackboard.Player != null)
            {
                playerState = blackboard.Player.GetComponentInParent<DemoPlayerState>();
            }

            if (playerState == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a DemoPlayerState to arrest.", ref warnedMissingPlayerState);
                return BTStatus.Failure;
            }

            playerState.IsArrested = true;
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            context.StopMovement();

            if (!loggedArrest)
            {
                Debug.Log("Police demo: Player arrested.");
                loggedArrest = true;
            }

            return BTStatus.Success;
        }
    }
}
