using BehaviorTreeDemo.AI.GitAmend.Runtime;
using BehaviorTreeDemo.Police;
using UnityEngine;

namespace BehaviorTreeDemo.AI.GitAmend.Nodes
{
    internal sealed class CallBackupStrategy : IStrategy
    {
        private const string NodeName = "Call Backup";
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool loggedBackup;

        public CallBackupStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, NodeName, ref warnedMissingContext, ref warnedMissingBlackboard);
            if (blackboard == null)
            {
                return Node.Status.Failure;
            }

            blackboard.BackupCalled = true;
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Emergency;

            if (!loggedBackup)
            {
                Debug.Log("Police demo: Backup called.");
                loggedBackup = true;
            }

            return Node.Status.Success;
        }
    }

    internal sealed class MoveToSafePointStrategy : IStrategy
    {
        private const string NodeName = "Flee To Safe Point";
        private readonly PoliceAIContext context;
        private bool destinationSet;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingSafePoint;

        public MoveToSafePointStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, NodeName, ref warnedMissingContext, ref warnedMissingBlackboard);
            if (blackboard == null)
            {
                return Node.Status.Failure;
            }

            if (context.safePoint == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a safePoint.", ref warnedMissingSafePoint);
                return Node.Status.Failure;
            }

            context.SetMovementMode(PoliceMovementMode.Run);
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Emergency;

            if (!destinationSet)
            {
                if (!context.TrySetDestination(context.safePoint.position))
                {
                    return Node.Status.Failure;
                }

                destinationSet = true;
                return Node.Status.Running;
            }

            Node.Status status = PoliceNodeSupport.ToNodeStatus(context.GetMovementStatus());
            if (status != Node.Status.Running)
            {
                destinationSet = false;
            }

            return status;
        }

        public void Reset()
        {
            destinationSet = false;
        }
    }

    internal sealed class ArrestPlayerStrategy : IStrategy
    {
        private const string NodeName = "Arrest Player";
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;

        public ArrestPlayerStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return Node.Status.Failure;
            }

            if (!context.IsArrestLatched && !context.TryBeginArrest())
            {
                return Node.Status.Failure;
            }

            switch (context.UpdateArrest())
            {
                case PoliceArrestStatus.Running:
                    return Node.Status.Running;
                case PoliceArrestStatus.Completed:
                    return Node.Status.Success;
                default:
                    return Node.Status.Failure;
            }
        }

        public void Reset()
        {
            context?.CancelArrest();
        }
    }

    internal sealed class ChasePlayerStrategy : IStrategy
    {
        private const string NodeName = "Chase Player";
        private readonly PoliceAIContext context;
        private bool facingStarted;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPlayer;

        public ChasePlayerStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, NodeName, ref warnedMissingContext, ref warnedMissingBlackboard);
            if (blackboard == null)
            {
                return Node.Status.Failure;
            }

            if (blackboard.Player == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a Player Transform.", ref warnedMissingPlayer);
                return Node.Status.Failure;
            }

            if (!blackboard.PlayerVisible || !blackboard.PlayerSuspicious || !blackboard.HasLastKnownPlayerPosition)
            {
                context.StopMovement();
                StopFacing();
                return Node.Status.Failure;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Chase;

            if (blackboard.PlayerInArrestRange)
            {
                context.StopMovement();
                StopFacing();
                return Node.Status.Success;
            }

            if (!facingStarted)
            {
                facingStarted = context.BeginFacingPlayer();
                if (!facingStarted)
                {
                    return Node.Status.Failure;
                }
            }

            context.SetMovementMode(PoliceMovementMode.Run);
            if (!context.TrySetDestination(blackboard.LastKnownPlayerPosition))
            {
                StopFacing();
                return Node.Status.Failure;
            }

            if (context.GetMovementStatus() == PoliceMovementStatus.Failed)
            {
                StopFacing();
                return Node.Status.Failure;
            }

            return Node.Status.Running;
        }

        public void Reset()
        {
            StopFacing();
        }

        private void StopFacing()
        {
            if (!facingStarted)
            {
                return;
            }

            context.StopFacingPlayer();
            facingStarted = false;
        }
    }

    internal sealed class MoveToLastKnownPositionStrategy : IStrategy
    {
        private const string NodeName = "Move To Last Known Position";
        private readonly PoliceAIContext context;
        private readonly float timeout;
        private bool destinationSet;
        private float startedAt;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;

        public MoveToLastKnownPositionStrategy(PoliceAIContext context, float timeout)
        {
            this.context = context;
            this.timeout = timeout;
        }

        public Node.Status Process()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, NodeName, ref warnedMissingContext, ref warnedMissingBlackboard);
            if (blackboard == null || !blackboard.HasLastKnownPlayerPosition)
            {
                return Node.Status.Failure;
            }

            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Investigate;

            if (!destinationSet)
            {
                context.SetMovementMode(PoliceMovementMode.Walk);
                context.BeginInvestigationTravel();
                if (!context.TrySetDestination(blackboard.LastKnownPlayerPosition))
                {
                    return Node.Status.Failure;
                }

                startedAt = Time.time;
                destinationSet = true;
                return Node.Status.Running;
            }

            if (Time.time - startedAt >= timeout)
            {
                context.StopMovement();
                context.CancelInvestigation();
                destinationSet = false;
                return Node.Status.Failure;
            }

            Node.Status status = PoliceNodeSupport.ToNodeStatus(context.GetMovementStatus());
            if (status != Node.Status.Running)
            {
                destinationSet = false;
            }

            if (status == Node.Status.Failure)
            {
                context.CancelInvestigation();
            }

            return status;
        }

        public void Reset()
        {
            destinationSet = false;
            startedAt = 0f;
            context?.CancelInvestigation();
        }
    }

    internal sealed class LookAroundStrategy : IStrategy
    {
        private const string NodeName = "Look Around";
        private readonly PoliceAIContext context;
        private bool started;
        private bool warnedMissingContext;

        public LookAroundStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return Node.Status.Failure;
            }

            if (!started)
            {
                started = context.BeginLookingAround();
                if (!started)
                {
                    return Node.Status.Failure;
                }
            }

            switch (context.UpdateLookingAround())
            {
                case PoliceInvestigationStatus.Completed:
                    started = false;
                    return Node.Status.Success;
                case PoliceInvestigationStatus.Failed:
                    started = false;
                    return Node.Status.Failure;
                default:
                    return Node.Status.Running;
            }
        }

        public void Reset()
        {
            context?.CancelInvestigation();
            started = false;
        }
    }

    internal sealed class ClearLastKnownPlayerPositionStrategy : IStrategy
    {
        private const string NodeName = "Clear Last Known Player Position";
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;

        public ClearLastKnownPlayerPositionStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return Node.Status.Success;
            }

            context.ClearLastKnownPlayerPosition();
            return Node.Status.Success;
        }
    }

    internal sealed class SelectPatrolPointStrategy : IStrategy
    {
        private const string NodeName = "Select Next Patrol Point";
        private readonly PoliceAIContext context;
        private bool warnedMissingContext;
        private bool warnedMissingPatrolPoints;

        public SelectPatrolPointStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs a PoliceAIContext.", ref warnedMissingContext);
                return Node.Status.Failure;
            }

            if (!context.SelectRandomPatrolPoint())
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs at least one valid PatrolPoint.", ref warnedMissingPatrolPoints);
                return Node.Status.Failure;
            }

            return Node.Status.Success;
        }
    }

    internal sealed class MoveToPatrolPointStrategy : IStrategy
    {
        private const string NodeName = "Move To Patrol Point";
        private readonly PoliceAIContext context;
        private bool destinationSet;
        private int attempts;
        private bool warnedMissingContext;
        private bool warnedMissingBlackboard;
        private bool warnedMissingPatrolPoint;

        public MoveToPatrolPointStrategy(PoliceAIContext context)
        {
            this.context = context;
        }

        public Node.Status Process()
        {
            PoliceBlackboard blackboard = PoliceNodeSupport.GetBlackboard(
                context, NodeName, ref warnedMissingContext, ref warnedMissingBlackboard);
            if (blackboard == null)
            {
                return Node.Status.Failure;
            }

            if (blackboard.CurrentPatrolPoint == null)
            {
                PoliceNodeSupport.WarnOnce($"{NodeName} needs CurrentPatrolPoint.", ref warnedMissingPatrolPoint);
                return Node.Status.Failure;
            }

            context.SetMovementMode(PoliceMovementMode.Walk);
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.Patrol;

            if (!destinationSet)
            {
                if (!context.TrySetDestination(blackboard.CurrentPatrolPoint.position))
                {
                    return HandleFailure();
                }

                destinationSet = true;
                return Node.Status.Running;
            }

            Node.Status status = PoliceNodeSupport.ToNodeStatus(context.GetMovementStatus());
            if (status == Node.Status.Failure)
            {
                return HandleFailure();
            }

            if (status == Node.Status.Success)
            {
                destinationSet = false;
                attempts = 0;
            }

            return status;
        }

        public void Reset()
        {
            destinationSet = false;
            attempts = 0;
        }

        private Node.Status HandleFailure()
        {
            destinationSet = false;
            attempts++;
            if (attempts < 2)
            {
                return Node.Status.Running;
            }

            attempts = 0;
            return Node.Status.Failure;
        }
    }

    internal sealed class WaitStrategy : IStrategy
    {
        private readonly float duration;
        private bool started;
        private float startedAt;

        public WaitStrategy(float duration)
        {
            this.duration = duration;
        }

        public Node.Status Process()
        {
            if (!started)
            {
                started = true;
                startedAt = Time.time;
            }

            if (Time.time - startedAt < duration)
            {
                return Node.Status.Running;
            }

            started = false;
            return Node.Status.Success;
        }

        public void Reset()
        {
            started = false;
            startedAt = 0f;
        }
    }
}
