using BehaviorTreeDemo.Police;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;
using UnityEngine;

namespace BehaviorTreeDemo.AI.CustomApproach.Nodes
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

        public static BTStatus ToBTStatus(PoliceMovementStatus status)
        {
            switch (status)
            {
                case PoliceMovementStatus.Arrived:
                    return BTStatus.Success;
                case PoliceMovementStatus.Failed:
                    return BTStatus.Failure;
                default:
                    return BTStatus.Running;
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
