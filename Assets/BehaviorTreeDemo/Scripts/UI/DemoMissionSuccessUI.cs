using CustomApproachDemo.Gameplay.Mission;
using UnityEngine;

namespace CustomApproachDemo.Gameplay.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class DemoMissionSuccessUI : MonoBehaviour
    {
        [SerializeField] private DemoDeliveryZone deliveryZone;
        [SerializeField, Min(0f)] private float visibleDuration = 4.5f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.8f;

        private CanvasGroup canvasGroup;
        private bool completionShown;
        private bool showing;
        private float elapsed;
        private int shownFrame;

        // Delivery remains the authoritative state, even after the overlay fades.
        public bool IsMissionCompleted => deliveryZone != null && deliveryZone.IsDelivered;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (deliveryZone == null) return;
            deliveryZone.Delivered += HandleDelivered;
            if (deliveryZone.IsDelivered) HandleDelivered();
        }

        private void OnDisable()
        {
            if (deliveryZone != null) deliveryZone.Delivered -= HandleDelivered;
        }

        private void HandleDelivered()
        {
            if (completionShown || !IsMissionCompleted) return;
            completionShown = true;
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
            completionShown = false;
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

