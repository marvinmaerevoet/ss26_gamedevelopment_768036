using Demo.BehaviorTreePolice.Player;
using Demo.BehaviorTreePolice.Police;
using Demo.BehaviorTreePolice.UI;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Demo.BehaviorTreePolice.SceneSetup
{
    public sealed class PoliceBTDemoSceneSetup : MonoBehaviour
    {
        private const string EyePointName = "EyePoint";

        [ContextMenu("Police BT Demo/Add Player Demo State To Selected")]
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

        [ContextMenu("Police BT Demo/Add Police Components To Selected")]
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
            PoliceBTGizmos gizmos = GetOrAddComponent<PoliceBTGizmos>(selected);

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

            Undo.RecordObject(gizmos, "Configure Police BT Gizmos");
            gizmos.Context = context;

            EditorUtility.SetDirty(context);
            EditorUtility.SetDirty(gizmos);
            EditorUtility.SetDirty(blackboard);
            #else
            Debug.LogWarning("Add Police Components To Selected is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Police BT Demo/Create Patrol Points Around Selected Police")]
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

        [ContextMenu("Police BT Demo/Create Restricted Area Trigger")]
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

        [ContextMenu("Police BT Demo/Create Safe Point")]
        public void CreateSafePoint()
        {
            #if UNITY_EDITOR
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a police NPC GameObject first.");
                return;
            }

            GameObject safePoint = CreateUndoGameObject("Police Safe Point");
            safePoint.transform.position = selected.transform.position - selected.transform.forward * 5f;

            PoliceAIContext context = selected.GetComponent<PoliceAIContext>();
            if (context != null)
            {
                Undo.RecordObject(context, "Assign Police Safe Point");
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

        [ContextMenu("Police BT Demo/Create Debug UI")]
        public void CreateDebugUI()
        {
            #if UNITY_EDITOR
            GameObject debugUIObject = CreateUndoGameObject("Police BT Debug UI");
            PoliceBTDebugUI debugUI = GetOrAddComponent<PoliceBTDebugUI>(debugUIObject);

            PoliceBehaviorTreeRunner runner = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            if (runner != null)
            {
                Undo.RecordObject(debugUI, "Assign Police BT Debug Target");
                debugUI.Target = runner;
                EditorUtility.SetDirty(debugUI);
            }
            #else
            Debug.LogWarning("Create Debug UI is only available in the Unity Editor.");
            #endif
        }

        [ContextMenu("Police BT Demo/Auto Wire Selected Police To First Player")]
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
        #endif
    }
}
