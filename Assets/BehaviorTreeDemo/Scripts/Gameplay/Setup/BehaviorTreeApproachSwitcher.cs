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

            if (customApproachDebugUI != null)
            {
                customApproachDebugUI.SetActive(customSelected);
            }

            if (gitAmendDebugUI != null)
            {
                gitAmendDebugUI.SetActive(!customSelected);
            }

            if (approachStatusText != null)
            {
                approachStatusText.text =
                    "ACTIVE APPROACH\n" +
                    "F1  Custom Approach   F2  GitAmend\n" +
                    $"Current: {(customSelected ? "Custom Approach" : "GitAmend")}";
            }
        }
    }
}
