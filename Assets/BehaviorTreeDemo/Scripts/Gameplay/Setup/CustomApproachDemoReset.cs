using CustomApproachDemo.Player;
using CustomApproachDemo.Gameplay.Carry;
using CustomApproachDemo.Gameplay.Mission;
using CustomApproachDemo.Gameplay.Arrest;
using CustomApproachDemo.Gameplay.UI;
using CustomApproachDemo.Police;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CustomApproachDemo.Setup
{
    public sealed class CustomApproachDemoReset : MonoBehaviour
    {
        [Header("References")]
        public DemoPlayerState playerState;
        public Transform playerTransform;
        [FormerlySerializedAs("policeRunner")]
        public PoliceDecisionController policeDecisionController;
        public PoliceAIContext policeContext;
        public Transform policeTransform;
        [SerializeField] private PoliceAIContext[] additionalSheriffs = new PoliceAIContext[0];
        private Vector3[] additionalPositions;
        private Quaternion[] additionalRotations;
        private int[] additionalPatrolIndices;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private DemoSimplePlayerController movement;
        [SerializeField] private DemoPlayerCarryController carryController;
        [SerializeField] private DemoCarryable missionCrate;
        [SerializeField] private DemoDeliveryZone delivery;
        [SerializeField] private DemoArrestSequence arrestSequence;
        [SerializeField] private DemoMissionIntroUI introUI;
        [SerializeField] private DemoMissionSuccessUI successUI;
        [SerializeField] private DemoArrestUI arrestUI;
        [SerializeField] private DemoJailReleaseUI releaseUI;

        [Header("Reset Options")]
        public bool resetPlayerPosition = true;
        public bool resetPolicePosition = true;
        public bool resetArrestedState = true;
        public bool resetBlackboard = true;
        public bool resetAgent = true;
        [FormerlySerializedAs("resetTree")]
        public bool resetDecisionState = true;

        private Vector3 initialPlayerPosition;
        private Quaternion initialPlayerRotation;
        private Vector3 initialPolicePosition;
        private Quaternion initialPoliceRotation;

        private bool warnedMissingPlayer;
        private bool warnedMissingPolice;

        private void Start()
        {
            ResolveReferences(false);
            StoreInitialTransforms();
            additionalPositions = new Vector3[additionalSheriffs.Length];
            additionalRotations = new Quaternion[additionalSheriffs.Length];
            additionalPatrolIndices = new int[additionalSheriffs.Length];
            for (int i = 0; i < additionalSheriffs.Length; i++)
            {
                var context = additionalSheriffs[i];
                if (context == null) continue;
                additionalPositions[i] = context.transform.position;
                additionalRotations[i] = context.transform.rotation;
                additionalPatrolIndices[i] = context.PoliceBlackboard.CurrentPatrolIndex;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.rKey.wasPressedThisFrame)
            {
                return;
            }

            ResetDemo();
        }

        public void ResetDemo()
        {
            ResolveReferences(true);

            arrestSequence?.ResetSequence();
            introUI?.ResetUI();
            successUI?.ResetUI();
            arrestUI?.ResetUI();
            releaseUI?.ResetUI();
            carryController?.ResetCarryState();
            missionCrate?.ResetToInitialState();
            delivery?.ResetDeliveryState();

            ResetPlayer();
            ResetPolice();
            ResetBlackboard();

            if (policeContext != null)
            {
                policeContext.lowHealthDemoToggle = false;
                policeContext.SetMovementMode(PoliceMovementMode.Walk);
            }

            if (resetDecisionState && policeDecisionController != null)
            {
                policeDecisionController.ResetDecisionState();
            }

            Debug.Log("Custom Approach Demo reset.", this);
            ResetAdditionalSheriffs();
            introUI?.ShowIntro();
        }

        private void ResolveReferences(bool warnIfMissing)
        {
            if (playerState == null)
            {
                playerState = FindAnyObjectByType<DemoPlayerState>();
            }

            if (policeDecisionController == null)
            {
                policeDecisionController = FindAnyObjectByType<PoliceDecisionController>();
            }

            if (policeContext == null && policeDecisionController != null)
            {
                policeContext = policeDecisionController.GetComponent<PoliceAIContext>();
            }

            if (policeContext == null)
            {
                policeContext = FindAnyObjectByType<PoliceAIContext>();
            }

            if (playerTransform == null && playerState != null)
            {
                playerTransform = playerState.transform;
            }

            if (policeTransform == null && policeContext != null)
            {
                policeTransform = policeContext.transform;
            }

            if (warnIfMissing)
            {
                WarnMissingReferences();
            }
        }

        private void StoreInitialTransforms()
        {
            if (playerTransform != null)
            {
                initialPlayerPosition = playerTransform.position;
                initialPlayerRotation = playerTransform.rotation;
            }

            if (policeTransform != null)
            {
                initialPolicePosition = policeTransform.position;
                initialPoliceRotation = policeTransform.rotation;
            }
        }

        private void ResetPlayer()
        {
            if (playerState != null)
            {
                if (resetArrestedState)
                {
                    playerState.IsArrested = false;
                }

                playerState.IsInRestrictedArea = false;
                playerState.IsRunning = false;
            }

            if (resetPlayerPosition && movement != null && playerSpawn != null)
            {
                movement.TeleportTo(playerSpawn);
            }
            else if (resetPlayerPosition && playerTransform != null)
            {
                CharacterController controller = playerTransform.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                playerTransform.SetPositionAndRotation(initialPlayerPosition, initialPlayerRotation);

                if (controller != null)
                {
                    controller.enabled = true;
                }
            }

            if (playerState != null)
            {
                playerState.ResetSpeedTracking();
            }
        }

        private void ResetPolice()
        {
            if (!resetPolicePosition || policeTransform == null)
            {
                return;
            }

            NavMeshAgent agent = policeContext != null
                ? policeContext.NavMeshAgent
                : policeTransform.GetComponent<NavMeshAgent>();

            if (resetAgent && agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.Warp(initialPolicePosition);
                policeTransform.rotation = initialPoliceRotation;
                return;
            }

            policeTransform.SetPositionAndRotation(initialPolicePosition, initialPoliceRotation);
        }

        private void ResetBlackboard()
        {
            if (!resetBlackboard || policeContext == null || policeContext.PoliceBlackboard == null)
            {
                return;
            }

            ClearBlackboard(policeContext.PoliceBlackboard, 0);
        }

        private static void ClearBlackboard(PoliceBlackboard blackboard, int patrolIndex)
        {
            blackboard.PlayerVisible = false;
            blackboard.PlayerSuspicious = false;
            blackboard.PlayerInArrestRange = false;
            blackboard.HasLastKnownPlayerPosition = false;
            blackboard.BackupCalled = false;
            blackboard.CurrentPatrolIndex = patrolIndex;
            blackboard.CurrentPatrolPoint = null;
            blackboard.LastKnownPlayerPosition = Vector3.zero;
            blackboard.OfficerHealthLow = false;
            blackboard.CurrentBehaviorMode = PoliceBehaviorMode.None;
        }

        private void WarnMissingReferences()
        {
            if (playerState == null && !warnedMissingPlayer)
            {
                Debug.LogWarning("CustomApproachDemoReset could not find a DemoPlayerState.", this);
                warnedMissingPlayer = true;
            }

            if ((policeDecisionController == null || policeContext == null) && !warnedMissingPolice)
            {
                Debug.LogWarning("CustomApproachDemoReset could not find a PoliceDecisionController or PoliceAIContext.", this);
                warnedMissingPolice = true;
            }
        }

        private void ResetAdditionalSheriffs()
        {
            for (int i = 0; i < additionalSheriffs.Length; i++)
            {
                var context = additionalSheriffs[i];
                if (context == null) continue;
                var agent = context.NavMeshAgent;
                if (resetAgent && agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                if (resetPolicePosition)
                {
                    if (resetAgent && agent != null && agent.isOnNavMesh) agent.Warp(additionalPositions[i]);
                    else context.transform.position = additionalPositions[i];
                    context.transform.rotation = additionalRotations[i];
                }
                if (resetBlackboard && context.PoliceBlackboard != null)
                    ClearBlackboard(context.PoliceBlackboard, additionalPatrolIndices[i]);
                context.lowHealthDemoToggle = false;
                context.SetMovementMode(PoliceMovementMode.Walk);
                if (resetDecisionState) context.GetComponent<PoliceDecisionController>()?.ResetDecisionState();
            }
        }
    }
}
