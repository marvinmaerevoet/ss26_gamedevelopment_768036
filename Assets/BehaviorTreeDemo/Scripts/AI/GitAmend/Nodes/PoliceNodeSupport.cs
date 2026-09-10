using BehaviorTreeDemo.AI.GitAmend.Runtime;
using BehaviorTreeDemo.Police;
using UnityEngine;

namespace BehaviorTreeDemo.AI.GitAmend.Nodes
{
    internal static class PoliceNodeSupport
    {
        public static PoliceBlackboard GetBlackboard(
            PoliceAIContext context,
            string nodeName,
            ref bool warnedMissingContext,
            ref bool warnedMissingBlackboard)
        {
            if (context == null)
            {
                WarnOnce($"{nodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return null;
            }

            if (context.PoliceBlackboard == null)
            {
                WarnOnce($"{nodeName} needs a PoliceBlackboard.", ref warnedMissingBlackboard);
                return null;
            }

            return context.PoliceBlackboard;
        }

        public static Node.Status ToNodeStatus(PoliceMovementStatus status)
        {
            switch (status)
            {
                case PoliceMovementStatus.Arrived:
                    return Node.Status.Success;
                case PoliceMovementStatus.Failed:
                    return Node.Status.Failure;
                default:
                    return Node.Status.Running;
            }
        }

        public static void WarnOnce(string message, ref bool warned)
        {
            if (warned)
            {
                return;
            }

            Debug.LogWarning(message);
            warned = true;
        }
    }
}
