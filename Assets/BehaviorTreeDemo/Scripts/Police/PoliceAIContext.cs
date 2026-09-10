using BehaviorTreeDemo.Gameplay.Player;
using BehaviorTreeDemo.Gameplay.Carry;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeDemo.Police
{
    public sealed class PoliceAIContext : MonoBehaviour
    {
        public const float PerceptionRefreshInterval = 0.1f;

        private enum ArrestPhase
        {
            None,
            Approaching,
            Committed
        }

        [Header("References")]
        public PoliceBlackboard PoliceBlackboard;
        public NavMeshAgent NavMeshAgent;
        public Transform Self;
        public Transform EyePoint;
        public Transform[] PatrolPoints;
        public DemoPlayerState PlayerState;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 3f;
        [SerializeField, Min(0f)] private float runSpeed = 6f;
        [SerializeField, Min(0f)] private float arrestStandDistance = 1.4f;
        [SerializeField, Min(0f)] private float arrestStandTolerance = 0.15f;
        [SerializeField, Min(0f)] private float arrestNavMeshSampleRadius = 1f;
        [SerializeField, Min(0.1f)] private float arrestApproachMaxDuration = 3f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float ArrestStandDistance => arrestStandDistance;
        public PoliceMovementMode CurrentMovementMode { get; private set; } = PoliceMovementMode.Walk;
        public bool IsArrestApproachActive => arrestPhase == ArrestPhase.Approaching;
        public bool IsArrestLatched => arrestPhase != ArrestPhase.None;
        public bool IsArrestCommitted => arrestPhase == ArrestPhase.Committed;
        public float LastPerceptionRefreshTime { get; private set; } = -1f;
        public uint PerceptionRevision { get; private set; }

        [Header("Perception")]
        public float viewDistance = 12f;
        public float viewAngle = 90f;
        public float arrestRange = 2f;
        public LayerMask obstacleMask = Physics.DefaultRaycastLayers;

        [Header("Suspicion")]
        [SerializeField] private DemoPlayerCarryController playerCarryController;
        [SerializeField] private DemoCarryable suspiciousCarryable;
        public bool lowHealthDemoToggle;

        public DemoCarryable SuspiciousCarryable => suspiciousCarryable;

        [Header("Demo")]
        public Transform safePoint;
        public float lookAroundDuration = 2f;
        [SerializeField, Min(0f)] private float faceTurnSpeed = 720f;
        [SerializeField, Min(0f)] private float lookAroundTurnSpeed = 120f;

        public PoliceInvestigationPhase InvestigationPhase { get; private set; }

        private bool warnedMissingPlayerState;
        private bool warnedMissingAgent;
        private bool warnedMissingBlackboard;
        private bool loggedArrest;
        private ArrestPhase arrestPhase;
        private bool facePlayerContinuously;
        private bool manualRotationActive;
        private bool previousAgentUpdateRotation;
        private float investigationStartedAt;
        private bool investigationCompleted;
        private float arrestApproachStartedAt;
        private float nextPerceptionRefreshTime;

        private void Awake()
        {
            EnsureReferences();
            ApplyMovementSpeed();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                InitializePerceptionState();
            }
        }

        private void Update()
        {
            UpdatePerception();

            if (facePlayerContinuously)
            {
                RotateTowardsPlayer(faceTurnSpeed * Time.deltaTime);
            }
            else if (InvestigationPhase == PoliceInvestigationPhase.LookingAround && Self != null)
            {
                if (Time.time - investigationStartedAt >= Mathf.Max(0f, lookAroundDuration))
                {
                    CompleteLookingAround();
                }
                else
                {
                    Self.Rotate(Vector3.up, lookAroundTurnSpeed * Time.deltaTime, Space.World);
                }
            }
        }

        private void Reset()
        {
            EnsureReferences();
            CurrentMovementMode = PoliceMovementMode.Walk;
            ApplyMovementSpeed();
        }

        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            runSpeed = Mathf.Max(0f, runSpeed);
            arrestStandDistance = Mathf.Max(0f, arrestStandDistance);
            arrestStandTolerance = Mathf.Max(0f, arrestStandTolerance);
            arrestNavMeshSampleRadius = Mathf.Max(0f, arrestNavMeshSampleRadius);
            arrestApproachMaxDuration = Mathf.Max(0.1f, arrestApproachMaxDuration);
            faceTurnSpeed = Mathf.Max(0f, faceTurnSpeed);
            lookAroundTurnSpeed = Mathf.Max(0f, lookAroundTurnSpeed);
        }

        public void ResetPerceptionState()
        {
            InitializePerceptionState();
        }

        private void InitializePerceptionState()
        {
            EnsureReferences();
            PoliceBlackboard.PlayerVisible = false;
            PoliceBlackboard.PlayerSuspicious = false;
            PoliceBlackboard.PlayerInArrestRange = false;
            PoliceBlackboard.OfficerHealthLow = false;
            PoliceBlackboard.LastKnownPlayerPosition = Vector3.zero;
            PoliceBlackboard.HasLastKnownPlayerPosition = false;
            LastPerceptionRefreshTime = -1f;
            PerceptionRevision = 0;
            RefreshPerception();
        }

        private void UpdatePerception()
        {
            if (Time.time >= nextPerceptionRefreshTime)
            {
                RefreshPerception();
            }
        }

        private void RefreshPerception()
        {
            EnsureReferences();

            PoliceBlackboard.PlayerVisible = CanSeePlayer();
            PoliceBlackboard.PlayerSuspicious = IsPlayerSuspicious();
            PoliceBlackboard.PlayerInArrestRange = IsPlayerInArrestRange();
            PoliceBlackboard.OfficerHealthLow = lowHealthDemoToggle;

            if (PoliceBlackboard.PlayerVisible &&
                PoliceBlackboard.PlayerSuspicious &&
                PoliceBlackboard.Player != null)
            {
                PoliceBlackboard.LastKnownPlayerPosition = PoliceBlackboard.Player.position;
                PoliceBlackboard.HasLastKnownPlayerPosition = true;
            }

            LastPerceptionRefreshTime = Time.time;
            PerceptionRevision++;
            nextPerceptionRefreshTime = LastPerceptionRefreshTime + PerceptionRefreshInterval;
        }

        private bool CanSeePlayer()
        {
            if (PoliceBlackboard.Player == null)
            {
                return false;
            }

            Vector3 origin = EyePoint.position;
            Vector3 toPlayer = PoliceBlackboard.Player.position - origin;
            float distance = toPlayer.magnitude;

            if (distance > viewDistance)
            {
                return false;
            }

            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            Vector3 direction = toPlayer / distance;
            float angle = Vector3.Angle(EyePoint.forward, direction);

            if (angle > viewAngle * 0.5f)
            {
                return false;
            }

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore);

            Transform player = PoliceBlackboard.Player;
            for (int index = 0; index < hits.Length; index++)
            {
                Transform hitTransform = hits[index].transform;
                if (hitTransform == null || IsPartOf(hitTransform, Self) || IsPartOf(hitTransform, player))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool IsPartOf(Transform candidate, Transform root)
        {
            return root != null && (candidate == root || candidate.IsChildOf(root));
        }

        private bool IsPlayerSuspicious()
        {
            return playerCarryController != null &&
                   suspiciousCarryable != null &&
                   !suspiciousCarryable.IsPickupLocked &&
                   playerCarryController.IsCarrying &&
                   playerCarryController.CurrentCarryable == suspiciousCarryable;
        }

        private bool IsPlayerInArrestRange()
        {
            if (PoliceBlackboard.Player == null)
            {
                return false;
            }

            float distance = Vector3.Distance(Self.position, PoliceBlackboard.Player.position);
            return distance <= arrestRange;
        }

        public void SetMovementMode(PoliceMovementMode mode)
        {
            EnsureMovementReferences();
            CurrentMovementMode = mode;
            ApplyMovementSpeed();
        }

        /// <summary>
        /// Shared movement entry point. Police decision adapters do not manipulate
        /// NavMesh destinations, path resets, velocity, or transform rotation directly.
        /// </summary>
        public bool TrySetDestination(Vector3 destination)
        {
            EnsureMovementReferences();

            if (NavMeshAgent == null)
            {
                WarnMissingAgent();
                return false;
            }

            if (!NavMeshAgent.isOnNavMesh)
            {
                return false;
            }

            NavMeshAgent.isStopped = false;
            if (NavMeshAgent.SetDestination(destination))
            {
                return true;
            }

            StopMovement();
            return false;
        }

        public PoliceMovementStatus GetMovementStatus()
        {
            EnsureMovementReferences();

            if (NavMeshAgent == null)
            {
                WarnMissingAgent();
                return PoliceMovementStatus.Failed;
            }

            if (!NavMeshAgent.isOnNavMesh)
            {
                return PoliceMovementStatus.Failed;
            }

            if (NavMeshAgent.pathPending)
            {
                return PoliceMovementStatus.Running;
            }

            if (NavMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                return PoliceMovementStatus.Failed;
            }

            float arrivedDistance = Mathf.Max(NavMeshAgent.stoppingDistance, 0.05f) + 0.1f;
            if (NavMeshAgent.remainingDistance <= arrivedDistance)
            {
                return !NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude <= 0.05f
                    ? PoliceMovementStatus.Arrived
                    : PoliceMovementStatus.Running;
            }

            return PoliceMovementStatus.Running;
        }

        public bool TryBeginArrest()
        {
            EnsureReferences();

            if (arrestPhase != ArrestPhase.None)
            {
                return true;
            }

            if (PoliceBlackboard == null || PoliceBlackboard.Player == null ||
                PlayerState == null || PlayerState.IsArrested || Self == null || NavMeshAgent == null)
            {
                return false;
            }

            if (!PoliceBlackboard.PlayerSuspicious)
            {
                return false;
            }

            arrestPhase = ArrestPhase.Approaching;
            arrestApproachStartedAt = Time.time;
            PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            StopFacingPlayer();
            CancelInvestigation();
            StopMovement();
            return true;
        }

        public PoliceArrestStatus UpdateArrest()
        {
            EnsureReferences();

            if (arrestPhase == ArrestPhase.None)
            {
                return PoliceArrestStatus.Failed;
            }

            if (PoliceBlackboard == null || PoliceBlackboard.Player == null ||
                PlayerState == null || Self == null || NavMeshAgent == null)
            {
                ResetArrestState();
                return PoliceArrestStatus.Failed;
            }

            if (arrestPhase == ArrestPhase.Committed)
            {
                if (PlayerState.IsArrested)
                {
                    HoldCompletedArrest();
                    return PoliceArrestStatus.Running;
                }

                ResetArrestState();
                return PoliceArrestStatus.Completed;
            }

            // A different officer may have committed before this approach did.
            if (PlayerState.IsArrested)
            {
                ResetArrestState();
                return PoliceArrestStatus.Failed;
            }

            // Delivery or a manual drop can win until this officer commits the arrest.
            // Recheck the current authoritative snapshot so every adapter observes the same input.
            if (!PoliceBlackboard.PlayerSuspicious)
            {
                ResetArrestState();
                return PoliceArrestStatus.Failed;
            }

            if (Time.time - arrestApproachStartedAt >= Mathf.Max(0.1f, arrestApproachMaxDuration))
            {
                ResetArrestState();
                return PoliceArrestStatus.Failed;
            }

            PoliceMovementStatus approachStatus = UpdateArrestApproach();
            if (approachStatus == PoliceMovementStatus.Running)
            {
                return PoliceArrestStatus.Running;
            }

            if (approachStatus == PoliceMovementStatus.Failed)
            {
                ResetArrestState();
                return PoliceArrestStatus.Failed;
            }

            arrestPhase = ArrestPhase.Committed;
            PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            StopMovement();
            FacePlayer();
            PlayerState.IsArrested = true;

            if (!loggedArrest)
            {
                Debug.Log("Police demo: Player arrested.");
                loggedArrest = true;
            }

            return PoliceArrestStatus.Running;
        }

        public void CancelArrest()
        {
            if (arrestPhase == ArrestPhase.None)
            {
                return;
            }

            bool stillOwnsMovement = PoliceBlackboard == null ||
                                     PoliceBlackboard.CurrentBehaviorMode == PoliceBehaviorMode.Arrest;
            ResetArrestState(stillOwnsMovement);
        }

        private PoliceMovementStatus UpdateArrestApproach()
        {
            EnsureReferences();

            if (PoliceBlackboard == null || PoliceBlackboard.Player == null || Self == null || NavMeshAgent == null)
            {
                return PoliceMovementStatus.Failed;
            }

            PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;

            Vector3 toPlayer = PoliceBlackboard.Player.position - Self.position;
            toPlayer.y = 0f;
            float distanceToPlayer = toPlayer.magnitude;

            if (Mathf.Abs(distanceToPlayer - arrestStandDistance) <= arrestStandTolerance)
            {
                StopMovement();
                FacePlayer();
                return PoliceMovementStatus.Arrived;
            }

            if (!TryGetArrestStandPosition(out Vector3 standPosition))
            {
                StopMovement();
                FacePlayer();
                return PoliceMovementStatus.Running;
            }

            SetMovementMode(PoliceMovementMode.Walk);
            if (!TrySetDestination(standPosition))
            {
                return PoliceMovementStatus.Failed;
            }

            PoliceMovementStatus movementStatus = GetMovementStatus();
            if (movementStatus == PoliceMovementStatus.Failed)
            {
                StopMovement();
                FacePlayer();
                return PoliceMovementStatus.Running;
            }

            FacePlayer();
            return PoliceMovementStatus.Running;
        }

        private void HoldCompletedArrest()
        {
            if (PoliceBlackboard != null)
            {
                PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.Arrest;
            }

            StopMovement();
            FacePlayer();
        }

        public void ResetArrestState(bool stopOwnedMovement = true)
        {
            arrestPhase = ArrestPhase.None;
            arrestApproachStartedAt = 0f;
            if (stopOwnedMovement)
            {
                StopMovement();
            }
        }

        public void FacePlayer()
        {
            EnsureReferences();
            if (PoliceBlackboard == null || PoliceBlackboard.Player == null || Self == null)
            {
                return;
            }

            Vector3 direction = PoliceBlackboard.Player.position - Self.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Self.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        public bool BeginFacingPlayer()
        {
            EnsureReferences();
            if (PoliceBlackboard == null || PoliceBlackboard.Player == null || Self == null)
            {
                return false;
            }

            facePlayerContinuously = true;
            AcquireManualRotation();
            return true;
        }

        public void StopFacingPlayer()
        {
            facePlayerContinuously = false;
            ReleaseManualRotationIfUnused();
        }

        public void BeginInvestigationTravel()
        {
            StopFacingPlayer();
            CancelInvestigation();
            InvestigationPhase = PoliceInvestigationPhase.Moving;
        }

        public bool BeginLookingAround()
        {
            EnsureReferences();
            if (Self == null)
            {
                return false;
            }

            StopFacingPlayer();
            StopMovement();
            InvestigationPhase = PoliceInvestigationPhase.LookingAround;
            investigationStartedAt = Time.time;
            investigationCompleted = false;
            AcquireManualRotation();
            return true;
        }

        public PoliceInvestigationStatus UpdateLookingAround()
        {
            if (investigationCompleted)
            {
                investigationCompleted = false;
                return PoliceInvestigationStatus.Completed;
            }

            if (InvestigationPhase != PoliceInvestigationPhase.LookingAround || Self == null)
            {
                return PoliceInvestigationStatus.Failed;
            }

            return PoliceInvestigationStatus.Running;
        }

        public void CancelInvestigation()
        {
            InvestigationPhase = PoliceInvestigationPhase.None;
            investigationStartedAt = 0f;
            investigationCompleted = false;
            ReleaseManualRotationIfUnused();
        }

        public void CancelActiveOperations()
        {
            StopFacingPlayer();
            CancelInvestigation();
            ResetArrestState(false);
            StopMovement();
        }

        public void ResetMovement(Vector3 position, Quaternion rotation, bool resetPosition, bool useAgentWarp)
        {
            EnsureMovementReferences();
            CancelActiveOperations();
            if (!resetPosition || Self == null)
            {
                return;
            }

            if (useAgentWarp && NavMeshAgent != null && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.Warp(position);
                Self.rotation = rotation;
                return;
            }

            Self.SetPositionAndRotation(position, rotation);
        }

        private void CompleteLookingAround()
        {
            InvestigationPhase = PoliceInvestigationPhase.None;
            investigationStartedAt = 0f;
            investigationCompleted = true;
            ReleaseManualRotationIfUnused();
        }

        private void RotateTowardsPlayer(float maxDegreesDelta)
        {
            if (PoliceBlackboard == null || PoliceBlackboard.Player == null || Self == null)
            {
                return;
            }

            Vector3 direction = PoliceBlackboard.Player.position - Self.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Self.rotation = Quaternion.RotateTowards(Self.rotation, targetRotation, Mathf.Max(0f, maxDegreesDelta));
        }

        private void AcquireManualRotation()
        {
            if (manualRotationActive || NavMeshAgent == null)
            {
                return;
            }

            previousAgentUpdateRotation = NavMeshAgent.updateRotation;
            NavMeshAgent.updateRotation = false;
            manualRotationActive = true;
        }

        private void ReleaseManualRotationIfUnused()
        {
            if (!manualRotationActive || facePlayerContinuously ||
                InvestigationPhase == PoliceInvestigationPhase.LookingAround)
            {
                return;
            }

            if (NavMeshAgent != null)
            {
                NavMeshAgent.updateRotation = previousAgentUpdateRotation;
            }

            manualRotationActive = false;
        }

        private bool TryGetArrestStandPosition(out Vector3 standPosition)
        {
            standPosition = Vector3.zero;
            if (PoliceBlackboard == null || PoliceBlackboard.Player == null || Self == null || NavMeshAgent == null)
            {
                return false;
            }

            Transform player = PoliceBlackboard.Player;
            Vector3 awayFromPlayer = Self.position - player.position;
            awayFromPlayer.y = 0f;
            if (awayFromPlayer.sqrMagnitude <= 0.001f)
            {
                awayFromPlayer = -player.forward;
                awayFromPlayer.y = 0f;
            }

            if (awayFromPlayer.sqrMagnitude <= 0.001f)
            {
                awayFromPlayer = Vector3.back;
            }

            Vector3 radialDirection = awayFromPlayer.normalized;
            float[] angularOffsets = { 0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f, 90f, -90f, 180f };
            float bestDistanceError = float.PositiveInfinity;
            bool foundValidPosition = false;

            for (int index = 0; index < angularOffsets.Length; index++)
            {
                Vector3 candidateDirection = Quaternion.AngleAxis(angularOffsets[index], Vector3.up) * radialDirection;
                Vector3 desiredPosition = player.position + candidateDirection * arrestStandDistance;
                if (!NavMesh.SamplePosition(
                        desiredPosition,
                        out NavMeshHit hit,
                        arrestNavMeshSampleRadius,
                        NavMeshAgent.areaMask))
                {
                    continue;
                }

                Vector3 sampledOffset = hit.position - player.position;
                sampledOffset.y = 0f;
                float distanceError = Mathf.Abs(sampledOffset.magnitude - arrestStandDistance);
                if (distanceError > arrestStandTolerance || distanceError >= bestDistanceError)
                {
                    continue;
                }

                bestDistanceError = distanceError;
                standPosition = hit.position;
                foundValidPosition = true;
            }

            return foundValidPosition;
        }

        public bool SelectRandomPatrolPoint()
        {
            EnsureReferences();

            if (PoliceBlackboard == null || PatrolPoints == null || PatrolPoints.Length == 0)
            {
                return false;
            }

            int selectedIndex = -1;
            int fallbackIndex = -1;
            int candidateCount = 0;

            for (int index = 0; index < PatrolPoints.Length; index++)
            {
                Transform patrolPoint = PatrolPoints[index];
                if (patrolPoint == null)
                {
                    continue;
                }

                fallbackIndex = index;

                // Compare references so duplicate slots cannot repeat the current target.
                if (patrolPoint == PoliceBlackboard.CurrentPatrolPoint)
                {
                    continue;
                }

                // Uniform selection over eligible entries without a temporary list.
                candidateCount++;
                if (Random.Range(0, candidateCount) == 0)
                {
                    selectedIndex = index;
                }
            }

            // With only one usable target, reusing it is the only possible choice.
            if (selectedIndex < 0)
            {
                selectedIndex = fallbackIndex;
            }

            if (selectedIndex < 0)
            {
                return false;
            }

            PoliceBlackboard.CurrentPatrolPoint = PatrolPoints[selectedIndex];
            PoliceBlackboard.CurrentPatrolIndex = selectedIndex;
            PoliceBlackboard.CurrentBehaviorMode = PoliceBehaviorMode.Patrol;
            return true;
        }

        public void StopMovement()
        {
            EnsureMovementReferences();

            if (NavMeshAgent == null)
            {
                WarnMissingAgent();
                return;
            }

            if (!NavMeshAgent.isOnNavMesh)
            {
                return;
            }

            NavMeshAgent.ResetPath();
            NavMeshAgent.isStopped = true;
            NavMeshAgent.velocity = Vector3.zero;
        }

        public void ClearLastKnownPlayerPosition()
        {
            EnsureReferences();

            PoliceBlackboard.LastKnownPlayerPosition = Vector3.zero;
            PoliceBlackboard.HasLastKnownPlayerPosition = false;
        }

        private void EnsureReferences()
        {
            EnsureMovementReferences();

            if (EyePoint == null)
            {
                EyePoint = transform;
            }

            if (PoliceBlackboard == null)
            {
                PoliceBlackboard = GetComponent<PoliceBlackboard>();
            }

            if (PoliceBlackboard == null)
            {
                WarnMissingBlackboard();
            }

            TryResolvePlayerState();
            ResolveSuspicionReferences();
        }

        private void EnsureMovementReferences()
        {
            if (Self == null)
            {
                Self = transform;
            }

            if (NavMeshAgent == null)
            {
                NavMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        private void ResolveSuspicionReferences()
        {
            if (playerCarryController != null)
            {
                return;
            }

            if (PlayerState != null)
            {
                playerCarryController = PlayerState.GetComponent<DemoPlayerCarryController>();
            }

            if (playerCarryController == null && PoliceBlackboard != null && PoliceBlackboard.Player != null)
            {
                playerCarryController = PoliceBlackboard.Player.GetComponentInParent<DemoPlayerCarryController>();
            }
        }

        private bool TryResolvePlayerState()
        {
            if (PlayerState == null && PoliceBlackboard != null && PoliceBlackboard.Player != null)
            {
                PlayerState = PoliceBlackboard.Player.GetComponentInParent<DemoPlayerState>();
            }

            if (PlayerState != null)
            {
                if (PoliceBlackboard != null && PoliceBlackboard.Player == null)
                {
                    PoliceBlackboard.Player = PlayerState.transform;
                }

                return true;
            }

            if (!warnedMissingPlayerState)
            {
                Debug.LogWarning("PoliceAIContext has no DemoPlayerState assigned. Perception keeps running, but player suspicion data is unavailable.", this);
                warnedMissingPlayerState = true;
            }

            return false;
        }

        private void WarnMissingAgent()
        {
            if (warnedMissingAgent)
            {
                return;
            }

            Debug.LogWarning("PoliceAIContext has no NavMeshAgent assigned or found on this GameObject.", this);
            warnedMissingAgent = true;
        }

        private void WarnMissingBlackboard()
        {
            if (warnedMissingBlackboard)
            {
                return;
            }

            Debug.LogError("PoliceAIContext needs a PoliceBlackboard on the same GameObject.", this);
            warnedMissingBlackboard = true;
        }

        private void ApplyMovementSpeed()
        {
            if (NavMeshAgent == null)
            {
                WarnMissingAgent();
                return;
            }

            NavMeshAgent.speed = CurrentMovementMode == PoliceMovementMode.Run
                ? runSpeed
                : walkSpeed;
        }
    }
}
