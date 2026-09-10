using BehaviorTreeDemo.AI.GitAmend.Nodes;
using BehaviorTreeDemo.AI.GitAmend.Runtime;
using BehaviorTreeDemo.Police;
using UnityEngine;

namespace BehaviorTreeDemo.AI.GitAmend
{
    [RequireComponent(typeof(PoliceAIContext))]
    public sealed class GitAmendPoliceDecisionController : PoliceDecisionController
    {
        [Header("Debug")]
        public bool enableEmergencyDemo = true;

        private PoliceAIContext context;
        private PoliceBlackboard blackboard;
        private BehaviourTree tree;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public BehaviourTree Tree => tree;
        public Node.Status LastTreeStatus { get; private set; } = Node.Status.Running;
        public float LastTickTime { get; private set; } = -1f;

        private void Awake()
        {
            EnsureReferences();
            BuildTree();
        }

        private void OnDisable()
        {
            tree?.Reset();
            context?.CancelActiveOperations();
        }

        private void Update()
        {
            EnsureReferences();
            if (context == null || blackboard == null || tree == null)
            {
                return;
            }

            if (!enableEmergencyDemo)
            {
                context.lowHealthDemoToggle = false;
            }

            LastTreeStatus = tree.Process();
            LastTickTime = Time.time;
        }

        public override void ResetDecisionState()
        {
            EnsureReferences();
            if (tree == null)
            {
                BuildTree();
            }

            tree?.Reset();
            LastTreeStatus = Node.Status.Running;
            LastTickTime = -1f;
        }

        private void BuildTree()
        {
            tree = new BehaviourTree("GitAmend Police");
            PrioritySelector root = new PrioritySelector("Police Priority");

            Sequence emergency = new Sequence("Emergency", 500);
            emergency.AddChild(ConditionLeaf("Officer Health Low", () => blackboard != null && blackboard.OfficerHealthLow));

            Selector emergencySelector = new Selector("Emergency Selector");
            Sequence callBackup = new Sequence("Call Backup Sequence");
            callBackup.AddChild(ConditionLeaf("Not Backup Called", () => blackboard != null && !blackboard.BackupCalled));
            callBackup.AddChild(new Leaf("Call Backup", new CallBackupStrategy(context)));
            emergencySelector.AddChild(callBackup);
            emergencySelector.AddChild(new Leaf("Flee To Safe Point", new MoveToSafePointStrategy(context)));
            emergency.AddChild(emergencySelector);

            Sequence arrest = new Sequence("Arrest", 400);
            arrest.AddChild(ConditionLeaf("Player Visible", () => blackboard != null && blackboard.PlayerVisible));
            arrest.AddChild(ConditionLeaf("Player Suspicious", () => blackboard != null && blackboard.PlayerSuspicious));
            arrest.AddChild(ConditionLeaf("Player In Arrest Range", () => blackboard != null && blackboard.PlayerInArrestRange));
            arrest.AddChild(new Leaf("Arrest Player", new ArrestPlayerStrategy(context)));

            Sequence chase = new Sequence("Chase", 300);
            chase.AddChild(ConditionLeaf("Player Visible", () => blackboard != null && blackboard.PlayerVisible));
            chase.AddChild(ConditionLeaf("Player Suspicious", () => blackboard != null && blackboard.PlayerSuspicious));
            Inverter notInArrestRange = new Inverter("Not In Arrest Range");
            notInArrestRange.AddChild(ConditionLeaf("Player In Arrest Range", () => blackboard != null && blackboard.PlayerInArrestRange));
            chase.AddChild(notInArrestRange);
            chase.AddChild(new Leaf("Chase Player", new ChasePlayerStrategy(context)));

            Sequence investigate = new Sequence("Investigate", 200);
            investigate.AddChild(ConditionLeaf(
                "Has Last Known Player Position",
                () => blackboard != null && blackboard.HasLastKnownPlayerPosition));
            investigate.AddChild(new Leaf(
                "Move To Last Known Position",
                new MoveToLastKnownPositionStrategy(context, 5f)));
            investigate.AddChild(new Leaf("Look Around", new LookAroundStrategy(context)));
            investigate.AddChild(new Leaf(
                "Clear Last Known Player Position",
                new ClearLastKnownPlayerPositionStrategy(context)));

            Sequence patrol = new Sequence("Patrol", 100);
            patrol.AddChild(new Leaf("Select Next Patrol Point", new SelectPatrolPointStrategy(context)));
            patrol.AddChild(new Leaf("Move To Patrol Point", new MoveToPatrolPointStrategy(context)));
            patrol.AddChild(new Leaf("Patrol Wait", new WaitStrategy(1f)));

            root.AddChild(emergency);
            root.AddChild(arrest);
            root.AddChild(chase);
            root.AddChild(investigate);
            root.AddChild(patrol);
            tree.AddChild(root);
        }

        private static Leaf ConditionLeaf(string name, System.Func<bool> predicate)
        {
            return new Leaf(name, new Condition(predicate));
        }

        private void EnsureReferences()
        {
            if (context == null)
            {
                context = GetComponent<PoliceAIContext>();
            }

            if (context == null)
            {
                WarnOnce("GitAmendPoliceDecisionController needs a PoliceAIContext.", ref warnedMissingContext);
                return;
            }

            if (blackboard == null)
            {
                blackboard = context.PoliceBlackboard;
            }

            if (blackboard == null)
            {
                WarnOnce("GitAmendPoliceDecisionController needs a PoliceBlackboard.", ref warnedMissingBlackboard);
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
