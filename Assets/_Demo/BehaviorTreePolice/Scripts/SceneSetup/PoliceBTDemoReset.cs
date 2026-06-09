using Demo.BehaviorTreePolice.Player;
using Demo.BehaviorTreePolice.Police;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Demo.BehaviorTreePolice.SceneSetup
{
    public sealed class PoliceBTDemoReset : MonoBehaviour
    {
        [Header("References")]
        public DemoPlayerState playerState;
        public Transform playerTransform;
        public PoliceBehaviorTreeRunner policeRunner;
        public PoliceAIContext policeContext;
        public Transform policeTransform;

        [Header("Reset Options")]
        public bool resetPlayerPosition = true;
        public bool resetPolicePosition = true;
        public bool resetArrestedState = true;
        public bool resetBlackboard = true;
        public bool resetAgent = true;
        public bool resetTree = true;

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

            ResetPlayer();
            ResetPolice();
            ResetBlackboard();

            if (policeContext != null)
            {
                policeContext.lowHealthDemoToggle = false;
            }

            if (resetTree && policeRunner != null)
            {
                policeRunner.ResetTree();
            }

            Debug.Log("Police BT Demo reset.", this);
        }

        private void ResolveReferences(bool warnIfMissing)
        {
            if (playerState == null)
            {
                playerState = FindAnyObjectByType<DemoPlayerState>();
            }

            if (policeRunner == null)
            {
                policeRunner = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            }

            if (policeContext == null && policeRunner != null)
            {
                policeContext = policeRunner.GetComponent<PoliceAIContext>();
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

            if (!resetPlayerPosition || playerTransform == null)
            {
                return;
            }

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

            PoliceBlackboard blackboard = policeContext.PoliceBlackboard;
            blackboard.PlayerVisible = false;
            blackboard.PlayerSuspicious = false;
            blackboard.PlayerInArrestRange = false;
            blackboard.HasLastKnownPlayerPosition = false;
            blackboard.BackupCalled = false;
            blackboard.OfficerHealthLow = false;
            blackboard.CurrentBehaviorName = "Reset";
            blackboard.CurrentNodeName = "Reset";
        }

        private void WarnMissingReferences()
        {
            if (playerState == null && !warnedMissingPlayer)
            {
                Debug.LogWarning("PoliceBTDemoReset could not find a DemoPlayerState.", this);
                warnedMissingPlayer = true;
            }

            if ((policeRunner == null || policeContext == null) && !warnedMissingPolice)
            {
                Debug.LogWarning("PoliceBTDemoReset could not find a PoliceBehaviorTreeRunner or PoliceAIContext.", this);
                warnedMissingPolice = true;
            }
        }
    }
}
