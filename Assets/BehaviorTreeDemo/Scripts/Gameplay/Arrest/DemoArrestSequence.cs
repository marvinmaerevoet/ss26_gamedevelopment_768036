using System.Collections;
using CustomApproachDemo.Gameplay.Carry;
using CustomApproachDemo.Player;
using CustomApproachDemo.Gameplay.UI;
using CustomApproachDemo.Animation;
using UnityEngine;

namespace CustomApproachDemo.Gameplay.Arrest
{
    public sealed class DemoArrestSequence : MonoBehaviour
    {
        [SerializeField] private DemoPlayerState playerState;
        [SerializeField] private DemoSimplePlayerController movement;
        [SerializeField] private Transform jailSpawn;
        [SerializeField] private DemoPlayerCarryController carryController;
        [SerializeField] private DemoCarryable missionCrate;
        [SerializeField] private Transform crateReset;
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private DemoJailReleaseUI releaseUI;
        [SerializeField] private BasicAnimationDriver playerAnimation;
        [SerializeField, Min(0f)] private float arrestWait = 5f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.8f;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.8f;
        [SerializeField, Min(0f)] private float releaseAnimationDuration = 3.233f;
        public bool IsSequenceRunning { get; private set; }

        private void OnEnable()
        {
            if (playerState != null) playerState.Arrested += BeginSequence;
        }

        private void Start()
        {
            if (playerState != null && playerState.IsArrested) BeginSequence();
        }

        private void OnDisable()
        {
            if (playerState != null) playerState.Arrested -= BeginSequence;
        }

        private void BeginSequence()
        {
            if (IsSequenceRunning) return;
            if (playerState == null || movement == null || jailSpawn == null || fadeGroup == null || releaseUI == null ||
                carryController == null || missionCrate == null || crateReset == null || playerAnimation == null)
            {
                Debug.LogError("Arrest sequence references are incomplete.", this);
                return;
            }
            IsSequenceRunning = true;
            StartCoroutine(RunSequence());
        }

        public void ResetSequence()
        {
            StopAllCoroutines();
            IsSequenceRunning = false;
            if (playerAnimation != null) playerAnimation.ResetReleaseAnimationTrigger();
            if (fadeGroup == null) return;
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        private IEnumerator RunSequence()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, arrestWait));
            yield return Fade(0f, 1f, fadeOutDuration);
            // Render one fully black frame before changing the player pose.
            yield return null;
            movement.TeleportTo(jailSpawn);
            carryController.ResetCarryable(missionCrate, crateReset);
            yield return null;
            yield return Fade(1f, 0f, fadeInDuration);

            // Keep IsArrested true while the one-shot release animation plays so
            // the existing movement lock remains responsible for player control.
            if (playerAnimation.TriggerReleaseAnimation())
            {
                yield return new WaitForSecondsRealtime(Mathf.Max(0f, releaseAnimationDuration));
            }

            playerState.IsArrested = false;
            IsSequenceRunning = false;
            releaseUI.ShowReleased();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            fadeGroup.alpha = from;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            }
            fadeGroup.alpha = to;
        }
    }
}
