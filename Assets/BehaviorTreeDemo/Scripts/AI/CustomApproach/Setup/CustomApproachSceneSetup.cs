using CustomApproachDemo.Player;
using CustomApproachDemo.Police;
using CustomApproachDemo.Animation;
using CustomApproachDemo.UI;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomApproachDemo.Setup
{
    public sealed class CustomApproachSceneSetup : MonoBehaviour
    {
        private const string EyePointName = "EyePoint";
        private const string PlayerAnimatorControllerPath = "Assets/BehaviorTreeDemo/Animations/Player.controller";
        private const string SheriffAnimatorControllerPath = "Assets/BehaviorTreeDemo/Animations/Sheriff.controller";

        [ContextMenu("Custom Approach Demo/Add Player Demo State To Selected")]
        public void AddPlayerDemoStateToSelected()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a player GameObject first.");
                return;
            }

            GetOrAddComponent<DemoPlayerState>(selected);
            #else
            Debug.LogWarning("Add Player Demo State To Selected is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Add Police Components To Selected")]
        public void AddPoliceComponentsToSelected()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            NavMeshAgent agent = GetOrAddComponent<NavMeshAgent>(selected);
            PoliceBlackboard blackboard = GetOrAddComponent<PoliceBlackboard>(selected);
            PoliceAIContext context = GetOrAddComponent<PoliceAIContext>(selected);
            GetOrAddComponent<PoliceBehaviorTreeRunner>(selected);
            PoliceAIGizmos gizmos = GetOrAddComponent<PoliceAIGizmos>(selected);

            Transform eyePoint = FindOrCreateEyePoint(selected.transform);
            DemoPlayerState playerState = FindAnyObjectByType<DemoPlayerState>();

            Undo.RecordObject(context, "Configure Police AI Context");
            context.NavMeshAgent = agent;
            context.PoliceBlackboard = blackboard;
            context.Self = selected.transform;
            context.EyePoint = eyePoint;

            if (playerState != null)
            {
                context.PlayerState = playerState;
                blackboard.Player = playerState.transform;
            }

            Undo.RecordObject(gizmos, "Configure Police AI Gizmos");
            gizmos.Context = context;

            EditorUtility.SetDirty(context);
            EditorUtility.SetDirty(gizmos);
            EditorUtility.SetDirty(blackboard);
            #else
            Debug.LogWarning("Add Police Components To Selected is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create Patrol Points Around Selected Police")]
        public void CreatePatrolPointsAroundSelectedPolice()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            PoliceAIContext context = selected.GetComponent<PoliceAIContext>();
            if (context == null)
            {
                Debug.LogWarning("Selected police NPC needs PoliceAIContext. Run Add Police Components To Selected first.");
                return;
            }

            GameObject parent = CreateUndoGameObject("Police Patrol Points");
            parent.transform.position = selected.transform.position;

            Vector3 origin = selected.transform.position;
            Vector3[] offsets =
            {
                new Vector3(4f, 0f, 4f),
                new Vector3(-4f, 0f, 4f),
                new Vector3(-4f, 0f, -4f),
                new Vector3(4f, 0f, -4f)
            };

            Transform[] patrolPoints = new Transform[offsets.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                GameObject point = CreateUndoGameObject($"PatrolPoint_{i + 1:00}");
                point.transform.SetParent(parent.transform);
                point.transform.position = origin + offsets[i];
                patrolPoints[i] = point.transform;
            }

            Undo.RecordObject(context, "Assign Police Patrol Points");
            context.PatrolPoints = patrolPoints;
            EditorUtility.SetDirty(context);
            #else
            Debug.LogWarning("Create Patrol Points Around Selected Police is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create Restricted Area Trigger")]
        public void CreateRestrictedAreaTrigger()
        {
            #if UNITY_EDITOR
            GameObject anchor = Selection.activeGameObject != null
                ? Selection.activeGameObject
                : gameObject;

            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = "Restricted Area Trigger";
            Undo.RegisterCreatedObjectUndo(trigger, "Create Restricted Area Trigger");

            trigger.transform.position = anchor.transform.position + anchor.transform.forward * 4f;
            trigger.transform.localScale = new Vector3(4f, 2f, 4f);

            BoxCollider boxCollider = trigger.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Undo.RecordObject(boxCollider, "Configure Restricted Area Trigger");
                boxCollider.isTrigger = true;
                EditorUtility.SetDirty(boxCollider);
            }

            GetOrAddComponent<RestrictedAreaTrigger>(trigger);
            #else
            Debug.LogWarning("Create Restricted Area Trigger is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create Safe Point")]
        public void CreateSafePoint()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            GameObject safePoint = CreateUndoGameObject("Sheriff Safe Point");
            safePoint.transform.position = selected.transform.position - selected.transform.forward * 5f;

            PoliceAIContext context = selected.GetComponent<PoliceAIContext>();
            if (context != null)
            {
                Undo.RecordObject(context, "Assign Sheriff Safe Point");
                context.safePoint = safePoint.transform;
                EditorUtility.SetDirty(context);
            }
            else
            {
                Debug.LogWarning("Created safe point, but selected object has no PoliceAIContext to assign it to.");
            }
            #else
            Debug.LogWarning("Create Safe Point is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create BehaviorTree Debug UI")]
        public void CreateBehaviorTreeDebugUI()
        {
            #if UNITY_EDITOR
            GameObject debugUIObject = CreateUndoGameObject("CustomApproach BehaviorTree Debug UI");
            CustomApproachBehaviorTreeDebugUI debugUI = GetOrAddComponent<CustomApproachBehaviorTreeDebugUI>(debugUIObject);

            PoliceBehaviorTreeRunner runner = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            if (runner != null)
            {
                Undo.RecordObject(debugUI, "Assign CustomApproach BehaviorTree Debug Target");
                debugUI.target = runner;
                EditorUtility.SetDirty(debugUI);
            }
            #else
            Debug.LogWarning("Create Tree Debug UI is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create Demo Reset")]
        public void CreateDemoReset()
        {
            #if UNITY_EDITOR
            GameObject resetObject = CreateUndoGameObject("Custom Approach Demo Reset");
            CustomApproachDemoReset demoReset = GetOrAddComponent<CustomApproachDemoReset>(resetObject);

            DemoPlayerState playerState = FindAnyObjectByType<DemoPlayerState>();
            PoliceBehaviorTreeRunner runner = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            PoliceAIContext context = runner != null
                ? runner.GetComponent<PoliceAIContext>()
                : FindAnyObjectByType<PoliceAIContext>();

            Undo.RecordObject(demoReset, "Configure Custom Approach Demo Reset");
            demoReset.playerState = playerState;
            demoReset.playerTransform = playerState != null ? playerState.transform : null;
            demoReset.policeDecisionController = runner;
            demoReset.policeContext = context;
            demoReset.policeTransform = context != null ? context.transform : null;
            EditorUtility.SetDirty(demoReset);
            #else
            Debug.LogWarning("Create Demo Reset is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Auto Wire Selected Police To First Player")]
        public void AutoWireSelectedPoliceToFirstPlayer()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            PoliceAIContext context = selected.GetComponent<PoliceAIContext>();
            PoliceBlackboard blackboard = selected.GetComponent<PoliceBlackboard>();
            DemoPlayerState playerState = FindAnyObjectByType<DemoPlayerState>();

            if (context == null)
            {
                Debug.LogWarning("Selected police NPC needs PoliceAIContext. Run Add Police Components To Selected first.");
                return;
            }

            if (blackboard == null)
            {
                Debug.LogWarning("Selected police NPC needs PoliceBlackboard. Run Add Police Components To Selected first.");
                return;
            }

            if (playerState == null)
            {
                Debug.LogWarning("No DemoPlayerState found in the scene.");
                return;
            }

            Undo.RecordObject(context, "Wire Police To Player");
            Undo.RecordObject(blackboard, "Wire Police Blackboard To Player");

            context.PlayerState = playerState;
            context.PoliceBlackboard = blackboard;
            blackboard.Player = playerState.transform;

            EditorUtility.SetDirty(context);
            EditorUtility.SetDirty(blackboard);
            #else
            Debug.LogWarning("Auto Wire Selected Police To First Player is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Add Simple Player Controller To Selected")]
        public void AddSimplePlayerControllerToSelected()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a player GameObject first.");
                return;
            }

            CharacterController characterController = GetOrAddComponent<CharacterController>(selected);
            DemoPlayerState playerState = GetOrAddComponent<DemoPlayerState>(selected);
            DemoSimplePlayerController playerController = GetOrAddComponent<DemoSimplePlayerController>(selected);

            Undo.RecordObject(playerController, "Configure Simple Player Controller");
            playerController.playerState = playerState;
            EditorUtility.SetDirty(playerController);
            EditorUtility.SetDirty(characterController);
            EditorUtility.SetDirty(playerState);
            #else
            Debug.LogWarning("Add Simple Player Controller To Selected is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Add Third Person Camera To Main Camera")]
        public void AddThirdPersonCameraToMainCamera()
        {
            #if UNITY_EDITOR
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = FindAnyObjectByType<Camera>();
            }

            if (camera == null)
            {
                GameObject cameraObject = CreateUndoGameObject("Main Camera");
                camera = Undo.AddComponent<Camera>(cameraObject);
            }

            if (!camera.CompareTag("MainCamera"))
            {
                Undo.RecordObject(camera.gameObject, "Set Main Camera Tag");
                camera.gameObject.tag = "MainCamera";
                EditorUtility.SetDirty(camera.gameObject);
            }

            DemoThirdPersonCamera thirdPersonCamera = GetOrAddComponent<DemoThirdPersonCamera>(camera.gameObject);
            DemoPlayerState playerState = FindAnyObjectByType<DemoPlayerState>();

            if (playerState != null)
            {
                Undo.RecordObject(thirdPersonCamera, "Assign Third Person Camera Target");
                thirdPersonCamera.target = playerState.transform;
                EditorUtility.SetDirty(thirdPersonCamera);
            }
            #else
            Debug.LogWarning("Add Third Person Camera To Main Camera is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Add Basic Animation Driver To Selected")]
        public void AddBasicAnimationDriverToSelected()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a player or sheriff GameObject first.");
                return;
            }

            BasicAnimationDriver driver = GetOrAddComponent<BasicAnimationDriver>(selected);
            Animator animator = selected.GetComponentInChildren<Animator>();
            NavMeshAgent agent = selected.GetComponent<NavMeshAgent>();
            DemoPlayerState playerState = selected.GetComponent<DemoPlayerState>();
            PoliceBlackboard blackboard = selected.GetComponent<PoliceBlackboard>();

            if (agent == null)
            {
                agent = selected.GetComponentInParent<NavMeshAgent>();
            }

            if (playerState == null)
            {
                playerState = selected.GetComponentInParent<DemoPlayerState>();
            }

            if (blackboard == null)
            {
                blackboard = selected.GetComponentInParent<PoliceBlackboard>();
            }

            Undo.RecordObject(driver, "Configure Basic Animation Driver");
            driver.animator = animator;
            driver.agent = agent;
            driver.playerState = playerState;
            driver.blackboard = blackboard;
            EditorUtility.SetDirty(driver);

            if (animator != null)
            {
                Undo.RecordObject(animator, "Disable Animator Root Motion");
                animator.applyRootMotion = false;
                EditorUtility.SetDirty(animator);
            }
            #else
            Debug.LogWarning("Add Basic Animation Driver To Selected is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Setup Synty Animations")]
        public void SetupSyntyAnimations()
        {
            #if UNITY_EDITOR
            DemoPlayerState playerState = FindAnyObjectByType<DemoPlayerState>();
            PoliceAIContext sheriffContext = FindAnyObjectByType<PoliceAIContext>();

            RuntimeAnimatorController playerController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            RuntimeAnimatorController sheriffController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SheriffAnimatorControllerPath);

            if (playerController == null || sheriffController == null)
            {
                Debug.LogWarning("Bitte zuerst Tools/Custom Approach Demo/Create Animator Controllers ausfuehren.");
            }

            if (playerState == null)
            {
                Debug.LogWarning("No DemoPlayerState found in the scene. Player animation setup skipped.");
            }
            else
            {
                Animator playerAnimator = FindOrAddAnimator(playerState.gameObject);
                ConfigureAnimator(playerAnimator, playerController);

                BasicAnimationDriver playerDriver = GetOrAddComponent<BasicAnimationDriver>(playerState.gameObject);
                Undo.RecordObject(playerDriver, "Configure Player Basic Animation Driver");
                playerDriver.animator = playerAnimator;
                playerDriver.agent = null;
                playerDriver.playerState = playerState;
                playerDriver.blackboard = null;
                playerDriver.forceRootMotionOff = true;
                EditorUtility.SetDirty(playerDriver);
            }

            if (sheriffContext == null)
            {
                Debug.LogWarning("No PoliceAIContext found in the scene. Sheriff animation setup skipped.");
            }
            else
            {
                Animator sheriffAnimator = FindOrAddAnimator(sheriffContext.gameObject);
                ConfigureAnimator(sheriffAnimator, sheriffController);

                NavMeshAgent sheriffAgent = sheriffContext.NavMeshAgent != null
                    ? sheriffContext.NavMeshAgent
                    : sheriffContext.GetComponent<NavMeshAgent>();
                PoliceBlackboard sheriffBlackboard = sheriffContext.PoliceBlackboard != null
                    ? sheriffContext.PoliceBlackboard
                    : sheriffContext.GetComponent<PoliceBlackboard>();

                BasicAnimationDriver sheriffDriver = GetOrAddComponent<BasicAnimationDriver>(sheriffContext.gameObject);
                Undo.RecordObject(sheriffDriver, "Configure Sheriff Basic Animation Driver");
                sheriffDriver.animator = sheriffAnimator;
                sheriffDriver.agent = sheriffAgent;
                sheriffDriver.playerState = null;
                sheriffDriver.blackboard = sheriffBlackboard;
                sheriffDriver.forceRootMotionOff = true;
                EditorUtility.SetDirty(sheriffDriver);
            }
            #else
            Debug.LogWarning("Setup Synty Animations is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Custom Approach Demo/Create Full Demo Helpers For Selected Police")]
        public void CreateFullDemoHelpersForSelectedPolice()
        {
            #if UNITY_EDITOR
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            AddPoliceComponentsToSelected();
            CreatePatrolPointsAroundSelectedPolice();
            CreateSafePoint();
            AutoWireSelectedPoliceToFirstPlayer();
            #else
            Debug.LogWarning("Create Full Demo Helpers For Selected Police is only available in the Unity Editor.");
            #endif
        }

        #if UNITY_EDITOR
        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(target);
        }

        private static Transform FindOrCreateEyePoint(Transform parent)
        {
            Transform eyePoint = parent.Find(EyePointName);
            if (eyePoint != null)
            {
                return eyePoint;
            }

            GameObject eyePointObject = CreateUndoGameObject(EyePointName);
            eyePointObject.transform.SetParent(parent, false);
            eyePointObject.transform.localPosition = new Vector3(0f, 1.6f, 0.2f);
            eyePointObject.transform.localRotation = Quaternion.identity;

            return eyePointObject.transform;
        }

        private static GameObject CreateUndoGameObject(string objectName)
        {
            GameObject instance = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {objectName}");
            return instance;
        }

        private static Animator FindOrAddAnimator(GameObject target)
        {
            Animator animator = target.GetComponentInChildren<Animator>();
            return animator != null ? animator : Undo.AddComponent<Animator>(target);
        }

        private static void ConfigureAnimator(Animator animator, RuntimeAnimatorController controller)
        {
            if (animator == null)
            {
                return;
            }

            Undo.RecordObject(animator, "Configure Demo Animator");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            EditorUtility.SetDirty(animator);
        }
        #endif
    }
}
