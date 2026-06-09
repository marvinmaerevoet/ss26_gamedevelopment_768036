using Demo.BehaviorTreePolice.BehaviorTree;
using Demo.BehaviorTreePolice.Police.Nodes;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Police
{
    [RequireComponent(typeof(PoliceAIContext))]
    public sealed class PoliceBehaviorTreeRunner : MonoBehaviour
    {
        [Header("Tick")]
        public float tickInterval = 0.1f;
        public bool resetTreeWhenPlayerArrested;

        [Header("Debug")]
        public bool drawDebugGizmos = true;
        public bool enableEmergencyDemo = true;

        private PoliceAIContext context;
        private PoliceBlackboard blackboard;
        private BTNode treeRoot;

        private BTNode emergencyBehavior;
        private BTNode arrestBehavior;
        private BTNode chaseBehavior;
        private BTNode investigateBehavior;
        private BTNode patrolBehavior;

        private float nextTickTime;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool treePausedAfterArrest;

        private BTNode deepestRunningNode;
        private BTNode lastMeaningfulNode;
        private string activeBehaviorName;

        private void Awake()
        {
            EnsureReferences();
            BuildTree();
        }

        private void Update()
        {
            EnsureReferences();

            if (context == null || blackboard == null || treeRoot == null)
            {
                return;
            }

            if (!enableEmergencyDemo)
            {
                context.lowHealthDemoToggle = false;
            }

            context.RefreshPerception();

            if (context.PlayerState != null && context.PlayerState.IsArrested && resetTreeWhenPlayerArrested)
            {
                PauseAfterArrest();
                return;
            }

            treePausedAfterArrest = false;

            if (Time.time < nextTickTime)
            {
                return;
            }

            nextTickTime = Time.time + Mathf.Max(0.01f, tickInterval);
            TickTree();
        }

        private void BuildTree()
        {
            emergencyBehavior = new BTSequence(
                "EmergencyBehavior",
                new BTNode[]
                {
                    new C_OfficerHealthLow(context),
                    new BTSelector(
                        "Emergency Selector",
                        new BTNode[]
                        {
                            new BTSequence(
                                "Call Backup Sequence",
                                new BTNode[]
                                {
                                    new C_NotBackupCalled(context),
                                    new A_CallBackup(context)
                                }),
                            new A_FleeToSafePoint(context)
                        })
                });

            arrestBehavior = new BTSequence(
                "ArrestBehavior",
                new BTNode[]
                {
                    new C_PlayerVisible(context),
                    new C_PlayerSuspicious(context),
                    new C_PlayerInArrestRange(context),
                    new A_ArrestPlayer(context)
                });

            chaseBehavior = new BTSequence(
                "ChaseBehavior",
                new BTNode[]
                {
                    new C_PlayerVisible(context),
                    new C_PlayerSuspicious(context),
                    new BTInverter(
                        "Not In Arrest Range",
                        new C_PlayerInArrestRange(context)),
                    new BTParallel(
                        "Chase Parallel",
                        new BTNode[]
                        {
                            new A_ChasePlayer(context),
                            new A_LookAtPlayer(context)
                        },
                        requiredChildIndex: 0)
                });

            investigateBehavior = new BTSequence(
                "InvestigateBehavior",
                new BTNode[]
                {
                    new C_HasLastKnownPlayerPosition(context),
                    new BTTimeout(
                        "Move To Last Known Position Timeout",
                        new A_MoveToLastKnownPosition(context),
                        5f),
                    new A_LookAround(context),
                    new A_ClearLastKnownPlayerPosition(context)
                });

            patrolBehavior = new BTRepeater(
                "PatrolBehavior",
                new BTSequence(
                    "Patrol Sequence",
                    new BTNode[]
                    {
                        new A_SelectNextPatrolPoint(context),
                        new BTRetry(
                            "Move To Patrol Point Retry",
                            new A_MoveToPatrolPoint(context),
                            2),
                        new BTWaitAction("Patrol Wait", 1f)
                    }));

            treeRoot = new BTSelector(
                "Root Selector",
                new[]
                {
                    emergencyBehavior,
                    arrestBehavior,
                    chaseBehavior,
                    investigateBehavior,
                    patrolBehavior
                },
                rememberRunningChild: false);
        }

        private void TickTree()
        {
            deepestRunningNode = null;
            lastMeaningfulNode = null;
            activeBehaviorName = null;

            BTNode.NodeTicked += OnNodeTicked;

            BTStatus status;
            try
            {
                status = treeRoot.Tick();
            }
            finally
            {
                BTNode.NodeTicked -= OnNodeTicked;
            }

            blackboard.LastTreeStatus = status;

            if (!string.IsNullOrEmpty(activeBehaviorName))
            {
                blackboard.CurrentBehaviorName = activeBehaviorName;
            }

            BTNode debugNode = deepestRunningNode ?? lastMeaningfulNode;
            if (debugNode != null)
            {
                blackboard.CurrentNodeName = debugNode.Name;
            }
        }

        private void OnNodeTicked(BTNode node, BTStatus status)
        {
            if (node == null)
            {
                return;
            }

            if (node == emergencyBehavior)
            {
                activeBehaviorName = "Emergency";
            }
            else if (node == arrestBehavior)
            {
                activeBehaviorName = "Arrest";
            }
            else if (node == chaseBehavior)
            {
                activeBehaviorName = "Chase";
            }
            else if (node == investigateBehavior)
            {
                activeBehaviorName = "Investigate";
            }
            else if (node == patrolBehavior)
            {
                activeBehaviorName = "Patrol";
            }

            if (!IsControlNode(node))
            {
                lastMeaningfulNode = node;
            }

            if (status == BTStatus.Running && deepestRunningNode == null && !IsControlNode(node))
            {
                deepestRunningNode = node;
            }
        }

        private void PauseAfterArrest()
        {
            if (!treePausedAfterArrest)
            {
                treeRoot.Reset();
                treePausedAfterArrest = true;
            }

            blackboard.LastTreeStatus = BTStatus.Success;
            blackboard.CurrentBehaviorName = "Arrested / Paused";
            blackboard.CurrentNodeName = "Player Arrested";
        }

        private void EnsureReferences()
        {
            if (context == null)
            {
                context = GetComponent<PoliceAIContext>();
            }

            if (context == null)
            {
                WarnOnce("PoliceBehaviorTreeRunner needs a PoliceAIContext.", ref warnedMissingContext);
                return;
            }

            if (blackboard == null)
            {
                blackboard = context.PoliceBlackboard;
            }

            if (blackboard == null)
            {
                blackboard = GetComponent<PoliceBlackboard>();
            }

            if (blackboard == null)
            {
                blackboard = gameObject.AddComponent<PoliceBlackboard>();
            }

            context.PoliceBlackboard = blackboard;

            if (blackboard == null)
            {
                WarnOnce("PoliceBehaviorTreeRunner needs a PoliceBlackboard.", ref warnedMissingBlackboard);
            }
        }

        private static bool IsControlNode(BTNode node)
        {
            return node is BTSequence ||
                   node is BTSelector ||
                   node is BTParallel ||
                   node is BTDecorator;
        }

        private static void WarnOnce(string message, ref bool warned)
        {
            if (warned)
            {
                return;
            }

            Debug.LogWarning(message);
            warned = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            PoliceAIContext gizmoContext = context != null
                ? context
                : GetComponent<PoliceAIContext>();

            if (gizmoContext == null)
            {
                return;
            }

            Transform eyePoint = gizmoContext.EyePoint != null
                ? gizmoContext.EyePoint
                : transform;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyePoint.position, gizmoContext.viewDistance);

            Vector3 left = Quaternion.Euler(0f, -gizmoContext.viewAngle * 0.5f, 0f) * eyePoint.forward;
            Vector3 right = Quaternion.Euler(0f, gizmoContext.viewAngle * 0.5f, 0f) * eyePoint.forward;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + left * gizmoContext.viewDistance);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + right * gizmoContext.viewDistance);

            PoliceBlackboard gizmoBlackboard = blackboard != null
                ? blackboard
                : GetComponent<PoliceBlackboard>();

            if (gizmoBlackboard != null && gizmoBlackboard.HasLastKnownPlayerPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(gizmoBlackboard.LastKnownPlayerPosition, 0.5f);
            }
        }
    }
}
