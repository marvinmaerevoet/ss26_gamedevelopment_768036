using Demo.BehaviorTreePolice.BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    internal static class PoliceNodeSupport
    {
        public static PoliceBlackboard GetBlackboard(
            PoliceAIContext context,
            string nodeName,
            ref bool warnedMissingContext,
            ref bool warnedMissingBlackboard,
            bool refreshPerception = true)
        {
            if (context == null)
            {
                WarnOnce($"{nodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return null;
            }

            if (refreshPerception)
            {
                context.RefreshPerception();
            }

            if (context.PoliceBlackboard == null)
            {
                WarnOnce($"{nodeName} needs a PoliceBlackboard.", ref warnedMissingBlackboard);
                return null;
            }

            context.PoliceBlackboard.CurrentNodeName = nodeName;
            return context.PoliceBlackboard;
        }

        public static NavMeshAgent GetAgent(
            PoliceAIContext context,
            string nodeName,
            ref bool warnedMissingContext,
            ref bool warnedMissingAgent)
        {
            if (context == null)
            {
                WarnOnce($"{nodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return null;
            }

            if (context.NavMeshAgent == null)
            {
                WarnOnce($"{nodeName} needs a NavMeshAgent.", ref warnedMissingAgent);
                return null;
            }

            if (!context.NavMeshAgent.isOnNavMesh)
            {
                WarnOnce($"{nodeName} NavMeshAgent is not on a NavMesh.", ref warnedMissingAgent);
                return null;
            }

            return context.NavMeshAgent;
        }

        public static BTStatus GetMovementStatus(NavMeshAgent agent)
        {
            if (agent.pathPending)
            {
                return BTStatus.Running;
            }

            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                return BTStatus.Failure;
            }

            float arrivedDistance = Mathf.Max(agent.stoppingDistance, 0.05f) + 0.1f;
            if (agent.remainingDistance <= arrivedDistance)
            {
                return !agent.hasPath || agent.velocity.sqrMagnitude <= 0.05f
                    ? BTStatus.Success
                    : BTStatus.Running;
            }

            return BTStatus.Running;
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
