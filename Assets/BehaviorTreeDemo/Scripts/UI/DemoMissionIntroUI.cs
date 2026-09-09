using UnityEngine;

namespace CustomApproachDemo.Gameplay.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class DemoMissionIntroUI : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float visibleDuration = 4.5f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.8f;

        private CanvasGroup canvasGroup;
        private float elapsed;
        private bool showing;
        private int shownFrame;

        private void Start()
        {
            ShowIntro();
        }

        public void ShowIntro()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
            elapsed = 0f;
            shownFrame = Time.frameCount;
            showing = true;
        }

        public void ResetUI()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            showing = false;
            elapsed = 0f;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

        }
        private void Update()
        {
            if (!showing || Time.frameCount == shownFrame)
            {
                return;
            }

            // Do not let editor/startup stalls consume the entire visible period.
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            if (elapsed < Mathf.Max(0f, visibleDuration))
            {
                return;
            }

            float progress = fadeDuration > 0f
                ? Mathf.Clamp01((elapsed - Mathf.Max(0f, visibleDuration)) / fadeDuration)
                : 1f;
            canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, progress);
            showing = progress < 1f;
        }
    }
}

