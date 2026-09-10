using BehaviorTreeDemo.Gameplay.Player;
using BehaviorTreeDemo.Gameplay.Carry;
using BehaviorTreeDemo.Gameplay.Mission;
using BehaviorTreeDemo.Gameplay.Arrest;
using BehaviorTreeDemo.UI;
using BehaviorTreeDemo.Police;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace BehaviorTreeDemo.Gameplay.Setup
{
    public sealed class DemoReset : MonoBehaviour
    {
        [Header("References")]
        public DemoPlayerState playerState;
        public Transform playerTransform;
        [FormerlySerializedAs("policeRunner")]
        public PoliceDecisionController policeDecisionController;
        public PoliceAIContext policeContext;
        public Transform policeTransform;
        [SerializeField] private PoliceAIContext[] additionalSheriffs = new PoliceAIContext[0];
        [SerializeField] private PoliceDecisionController[] additionalDecisionControllers = new PoliceDecisionController[0];
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
        private int initialPolicePatrolIndex;

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
            ResetAllSheriffs();

            Debug.Log("Behavior Tree Demo reset.", this);
            introUI?.ShowIntro();
        }

        private void ResolveReferences(bool warnIfMissing)
        {
            if (policeContext == null && policeDecisionController != null)
            {
                policeContext = policeDecisionController.GetComponent<PoliceAIContext>();
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

            if (policeContext != null && policeContext.PoliceBlackboard != null)
            {
                initialPolicePatrolIndex = policeContext.PoliceBlackboard.CurrentPatrolIndex;
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

        private void ResetAllSheriffs()
        {
            ResetSheriff(
                policeContext,
                policeTransform,
                initialPolicePosition,
                initialPoliceRotation,
                initialPolicePatrolIndex,
                policeDecisionController);

            for (int i = 0; i < additionalSheriffs.Length; i++)
            {
                PoliceAIContext context = additionalSheriffs[i];
                if (context == null || context == policeContext)
                {
                    continue;
                }

                ResetSheriff(
                    context,
                    context.transform,
                    additionalPositions[i],
                    additionalRotations[i],
                    additionalPatrolIndices[i],
                    i < additionalDecisionControllers.Length
                        ? additionalDecisionControllers[i]
                        : null);
            }
        }

        private void ResetSheriff(
            PoliceAIContext context,
            Transform sheriffTransform,
            Vector3 initialPosition,
            Quaternion initialRotation,
            int initialPatrolIndex,
            PoliceDecisionController decisionController)
        {
            if (context != null)
            {
                context.ResetMovement(initialPosition, initialRotation, resetPolicePosition, resetAgent);
            }
            else if (resetPolicePosition && sheriffTransform != null)
            {
                NavMeshAgent agent = sheriffTransform.GetComponent<NavMeshAgent>();
                if (resetAgent && agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    agent.Warp(initialPosition);
                    sheriffTransform.rotation = initialRotation;
                }
                else
                {
                    sheriffTransform.SetPositionAndRotation(initialPosition, initialRotation);
                }
            }

            if (resetBlackboard && context != null && context.PoliceBlackboard != null)
            {
                ClearBlackboard(context.PoliceBlackboard, initialPatrolIndex);
            }

            if (context != null)
            {
                context.lowHealthDemoToggle = false;
                context.SetMovementMode(PoliceMovementMode.Walk);
                context.ResetPerceptionState();
            }

            if (resetDecisionState)
            {
                decisionController?.ResetDecisionState();
            }
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
                Debug.LogWarning("DemoReset needs an assigned DemoPlayerState reference.", this);
                warnedMissingPlayer = true;
            }

            if ((policeDecisionController == null || policeContext == null) && !warnedMissingPolice)
            {
                Debug.LogWarning("DemoReset needs assigned PoliceDecisionController and PoliceAIContext references.", this);
                warnedMissingPolice = true;
            }
        }

    }
}
