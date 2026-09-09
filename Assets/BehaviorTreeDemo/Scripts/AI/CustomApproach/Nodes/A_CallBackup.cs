using CustomApproachDemo.BehaviorTree;
using UnityEngine;

namespace CustomApproachDemo.Police.Nodes
{
    public sealed class A_CallBackup : BTAction
    {
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool loggedBackup;

        public A_CallBackup(PoliceAIContext context) : base("Call Backup")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, Name, ref warnedMissingContext, ref warnedMissingBlackboard, false);

            if (blackboard == null)
            {
                return BTStatus.Failure;
            }

            blackboard.BackupCalled = true;
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Emergency;

            if (!loggedBackup)
            {
                Debug.Log("Police demo: Backup called.");
                loggedBackup = true;
            }

            return BTStatus.Success;
        }
    }
}
