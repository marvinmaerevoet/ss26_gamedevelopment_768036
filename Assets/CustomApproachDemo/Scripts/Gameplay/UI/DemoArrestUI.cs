using CustomApproachDemo.Player;
using UnityEngine;

namespace CustomApproachDemo.Gameplay.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class DemoArrestUI : MonoBehaviour
    {
        [SerializeField] private DemoPlayerState playerState;
        [SerializeField, Min(0f)] private float visibleDuration = 4.5f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.8f;
        private CanvasGroup canvasGroup;
        private bool showing;
        private float elapsed;
        private int shownFrame;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (playerState != null) playerState.Arrested += ShowArrested;
        }

        private void Start()
        {
            if (playerState != null && playerState.IsArrested && !showing) ShowArrested();
        }

        private void OnDisable()
        {
            if (playerState != null) playerState.Arrested -= ShowArrested;
        }

        public void ShowArrested()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            elapsed = 0f;
            shownFrame = Time.frameCount;
            showing = true;
        }

        private void Update()
        {
            if (!showing || Time.frameCount == shownFrame) return;
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            if (elapsed < Mathf.Max(0f, visibleDuration)) return;
            float progress = fadeDuration > 0f
                ? Mathf.Clamp01((elapsed - Mathf.Max(0f, visibleDuration)) / fadeDuration)
                : 1f;
            canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, progress);
            showing = progress < 1f;
        }
    }
}
