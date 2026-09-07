using CustomApproachDemo.Player;
using UnityEngine;
using UnityEngine.AI;

namespace CustomApproachDemo.Police
{
    public sealed class PoliceAIContext : MonoBehaviour
    {
        [Header("References")]
        public PoliceBlackboard PoliceBlackboard;
        public NavMeshAgent NavMeshAgent;
        public Transform Self;
        public Transform EyePoint;
        public Transform[] PatrolPoints;
        public DemoPlayerState PlayerState;

        [Header("Perception")]
        public float viewDistance = 12f;
        public float viewAngle = 90f;
        public float arrestRange = 2f;
        public LayerMask obstacleMask;

        [Header("Suspicion")]
        public bool suspiciousIfRunning = true;
        public bool suspiciousIfInRestrictedArea = true;
        public bool lowHealthDemoToggle;

        [Header("Demo")]
        public Transform safePoint;
        public float lookAroundDuration = 2f;

        private bool warnedMissingPlayerState;
        private bool warnedMissingAgent;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Reset()
        {
            EnsureReferences();
        }

        public void RefreshPerception()
        {
            EnsureReferences();

            PoliceBlackboard.PlayerVisible = CanSeePlayer();
            PoliceBlackboard.PlayerSuspicious = IsPlayerSuspicious();
            PoliceBlackboard.PlayerInArrestRange = IsPlayerInArrestRange();
            PoliceBlackboard.OfficerHealthLow = lowHealthDemoToggle;

            if (PoliceBlackboard.PlayerVisible && PoliceBlackboard.Player != null)
            {
                PoliceBlackboard.LastKnownPlayerPosition = PoliceBlackboard.Player.position;
                PoliceBlackboard.HasLastKnownPlayerPosition = true;
            }
        }

        public bool CanSeePlayer()
        {
            EnsureReferences();

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

            return !Physics.Raycast(origin, direction, distance, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        public bool IsPlayerSuspicious()
        {
            EnsureReferences();

            if (!TryResolvePlayerState())
            {
                return false;
            }

            bool runningSuspicious = suspiciousIfRunning && PlayerState.IsRunning;
            bool restrictedAreaSuspicious = suspiciousIfInRestrictedArea && PlayerState.IsInRestrictedArea;

            return runningSuspicious || restrictedAreaSuspicious;
        }

        public bool IsPlayerInArrestRange()
        {
            EnsureReferences();

            if (PoliceBlackboard.Player == null)
            {
                return false;
            }

            float distance = Vector3.Distance(Self.position, PoliceBlackboard.Player.position);
            return distance <= arrestRange;
        }

        public void SetDestination(Vector3 destination)
        {
            EnsureReferences();

            if (NavMeshAgent == null)
            {
                WarnMissingAgent();
                return;
            }

            if (!NavMeshAgent.isOnNavMesh)
            {
                return;
            }

            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(destination);
        }

        public void StopMovement()
        {
            EnsureReferences();

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
        }

        public void ClearLastKnownPlayerPosition()
        {
            EnsureReferences();

            PoliceBlackboard.LastKnownPlayerPosition = Vector3.zero;
            PoliceBlackboard.HasLastKnownPlayerPosition = false;
        }

        private void EnsureReferences()
        {
            if (Self == null)
            {
                Self = transform;
            }

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
                PoliceBlackboard = gameObject.AddComponent<PoliceBlackboard>();
            }

            if (NavMeshAgent == null)
            {
                NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            TryResolvePlayerState();
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
    }
}
