using UnityEngine;

namespace Demo.BehaviorTreePolice.Police
{
    public sealed class PoliceBTGizmos : MonoBehaviour
    {
        public PoliceAIContext Context;

        private void OnDrawGizmosSelected()
        {
            PoliceAIContext gizmoContext = Context != null
                ? Context
                : GetComponent<PoliceAIContext>();

            if (gizmoContext == null)
            {
                return;
            }

            Transform eyePoint = gizmoContext.EyePoint != null
                ? gizmoContext.EyePoint
                : gizmoContext.transform;

            DrawVision(gizmoContext, eyePoint);
            DrawArrestRange(gizmoContext);
            DrawLastKnownPosition(gizmoContext);
            DrawPatrolRoute(gizmoContext);
        }

        private static void DrawVision(PoliceAIContext context, Transform eyePoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyePoint.position, context.viewDistance);

            Vector3 left = Quaternion.Euler(0f, -context.viewAngle * 0.5f, 0f) * eyePoint.forward;
            Vector3 right = Quaternion.Euler(0f, context.viewAngle * 0.5f, 0f) * eyePoint.forward;

            Gizmos.DrawLine(eyePoint.position, eyePoint.position + left * context.viewDistance);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + right * context.viewDistance);
        }

        private static void DrawArrestRange(PoliceAIContext context)
        {
            Transform self = context.Self != null ? context.Self : context.transform;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(self.position, context.arrestRange);
        }

        private static void DrawLastKnownPosition(PoliceAIContext context)
        {
            PoliceBlackboard blackboard = context.PoliceBlackboard;
            if (blackboard == null || !blackboard.HasLastKnownPlayerPosition)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(blackboard.LastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(context.transform.position, blackboard.LastKnownPlayerPosition);
        }

        private static void DrawPatrolRoute(PoliceAIContext context)
        {
            Transform[] patrolPoints = context.PatrolPoints;
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.green;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform current = patrolPoints[i];
                if (current == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(current.position, 0.35f);

                Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (next != null && next != current)
                {
                    Gizmos.DrawLine(current.position, next.position);
                }
            }
        }
    }
}
