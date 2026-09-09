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
        private bool approachStarted;
        private bool arrestCommitted;

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

            if (arrestCommitted)
            {
                if (playerState.IsArrested)
                {
                    context.HoldCompletedArrest();
                    return BTStatus.Running;
                }

                arrestCommitted = false;
                approachStarted = false;
                context.ResetArrestState();
                return BTStatus.Success;
            }

            // Another officer already owns the active arrest. Do not let this
            // sheriff acquire a second local arrest latch.
            if (playerState.IsArrested)
            {
                approachStarted = false;
                context.ResetArrestState();
                return BTStatus.Failure;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;

            if (!approachStarted)
            {
                context.BeginArrestApproach();
                approachStarted = true;
            }

            PoliceMovementStatus approachStatus = context.UpdateArrestApproach();
            if (approachStatus == PoliceMovementStatus.Running)
            {
                return BTStatus.Running;
            }

            if (approachStatus == PoliceMovementStatus.Failed)
            {
                approachStarted = false;
                return BTStatus.Failure;
            }

            context.StopMovement();
            context.FacePlayer();
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            context.HoldCompletedArrest();
            arrestCommitted = true;
            playerState.IsArrested = true;

            if (!loggedArrest)
            {
                Debug.Log("Police demo: Player arrested.");
                loggedArrest = true;
            }

            return BTStatus.Running;
        }

        public override void Reset()
        {
            base.Reset();
            approachStarted = false;
            arrestCommitted = false;
            context?.ResetArrestState();
        }
    }
}
