using BehaviorTreeDemo.Police;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BehaviorTreeDemo.Gameplay.Setup
{
    [DefaultExecutionOrder(-10000)]
    public sealed class BehaviorTreeApproachSwitcher : MonoBehaviour
    {
        [Header("Approach UI")]
        [SerializeField] private GameObject customApproachDebugUI;
        [SerializeField] private GameObject gitAmendDebugUI;
        [SerializeField] private Text approachStatusText;

        private bool warnedMissingScene;

        private void Awake()
        {
            ApplyApproachPresentation();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                ReloadWith(BehaviorTreeApproach.CustomApproach);
            }
            else if (keyboard.f2Key.wasPressedThisFrame)
            {
                ReloadWith(BehaviorTreeApproach.GitAmend);
            }
            else if (keyboard.f3Key.wasPressedThisFrame)
            {
                ReloadWith(BehaviorTreeApproach.UnityBehavior);
            }
        }

        private void ReloadWith(BehaviorTreeApproach approach)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.buildIndex < 0)
            {
                if (!warnedMissingScene)
                {
                    Debug.LogError("BehaviorTreeDemo must be enabled in Build Settings before switching approaches.", this);
                    warnedMissingScene = true;
                }

                return;
            }

            BehaviorTreeApproachSelection.Select(approach);
            SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
        }

        private void ApplyApproachPresentation()
        {
            BehaviorTreeApproach approach = BehaviorTreeApproachSelection.CurrentApproach;
            bool customSelected = approach == BehaviorTreeApproach.CustomApproach;
            bool gitAmendSelected = approach == BehaviorTreeApproach.GitAmend;

            if (customApproachDebugUI != null)
            {
                customApproachDebugUI.SetActive(customSelected);
            }

            if (gitAmendDebugUI != null)
            {
                gitAmendDebugUI.SetActive(gitAmendSelected);
            }

            if (approachStatusText != null)
            {
                approachStatusText.text =
                    "ACTIVE APPROACH\n" +
                    "F1  Custom   F2  GitAmend   F3  Unity Behavior\n" +
                    $"Current: {GetApproachDisplayName(approach)}";
            }
        }

        private static string GetApproachDisplayName(BehaviorTreeApproach approach)
        {
            switch (approach)
            {
                case BehaviorTreeApproach.GitAmend:
                    return "GitAmend";
                case BehaviorTreeApproach.UnityBehavior:
                    return "Unity Behavior";
                default:
                    return "Custom Approach";
            }
        }
    }
}
