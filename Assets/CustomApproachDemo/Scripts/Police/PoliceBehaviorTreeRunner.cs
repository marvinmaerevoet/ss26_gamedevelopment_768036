using System.Collections.Generic;
using CustomApproachDemo.BehaviorTree;
using CustomApproachDemo.Police.Nodes;
using UnityEngine;

namespace CustomApproachDemo.Police
{
    [RequireComponent(typeof(PoliceAIContext))]
    public sealed class PoliceBehaviorTreeRunner : PoliceDecisionController
    {
        [Header("Tick")]
        public float tickInterval = 0.1f;
        public bool resetTreeWhenPlayerArrested;

        [Header("Debug")]
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
        private PoliceBehaviorMode? activeBehaviorMode;
        private readonly List<BTNode> lastTickPath = new List<BTNode>();

        public BTNode TreeRoot => treeRoot;
        public BTNode LastTickedNode { get; private set; }
        public IReadOnlyList<BTNode> LastTickPath => lastTickPath;
        public float LastTickTime { get; private set; } = -1f;
        public string CurrentNodeName { get; private set; }
        public BTStatus LastTreeStatus { get; private set; } = BTStatus.Running;

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

        public override void ResetDecisionState()
        {
            ResetTree();
        }

        public void ResetTree()
        {
            EnsureReferences();

            if (treeRoot == null)
            {
                BuildTree();
            }

            treeRoot?.Reset();
            deepestRunningNode = null;
            lastMeaningfulNode = null;
            activeBehaviorMode = null;
            LastTickedNode = null;
            CurrentNodeName = "Reset";
            LastTreeStatus = BTStatus.Running;
            treePausedAfterArrest = false;
            nextTickTime = Time.time;
            LastTickTime = -1f;
            lastTickPath.Clear();

        }

        private void TickTree()
        {
            deepestRunningNode = null;
            lastMeaningfulNode = null;
            activeBehaviorMode = null;
            LastTickedNode = null;

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

            LastTreeStatus = status;
            LastTickTime = Time.time;

            if (activeBehaviorMode.HasValue)
            {
                blackboard.CurrentBehaviorMode = activeBehaviorMode.Value;
            }

            BTNode debugNode = deepestRunningNode ?? lastMeaningfulNode;
            if (debugNode != null)
            {
                CurrentNodeName = debugNode.Name;
                BuildLastTickPath(debugNode);
            }
            else
            {
                lastTickPath.Clear();
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
                activeBehaviorMode = PoliceBehaviorMode.Emergency;
            }
            else if (node == arrestBehavior)
            {
                activeBehaviorMode = PoliceBehaviorMode.Arrest;
            }
            else if (node == chaseBehavior)
            {
                activeBehaviorMode = PoliceBehaviorMode.Chase;
            }
            else if (node == investigateBehavior)
            {
                activeBehaviorMode = PoliceBehaviorMode.Investigate;
            }
            else if (node == patrolBehavior)
            {
                activeBehaviorMode = PoliceBehaviorMode.Patrol;
            }

            if (!IsControlNode(node))
            {
                lastMeaningfulNode = node;
                LastTickedNode = node;
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

            LastTreeStatus = BTStatus.Success;
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            CurrentNodeName = "Player Arrested";
            LastTickedNode = null;
            lastTickPath.Clear();
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

        private void BuildLastTickPath(BTNode node)
        {
            lastTickPath.Clear();

            for (BTNode current = node; current != null; current = current.Parent)
            {
                lastTickPath.Insert(0, current);
            }
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
    }
}
