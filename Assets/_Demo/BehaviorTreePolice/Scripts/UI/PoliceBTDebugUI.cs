using System.Text;
using Demo.BehaviorTreePolice.Player;
using Demo.BehaviorTreePolice.Police;
using UnityEngine;
using UnityEngine.UI;

namespace Demo.BehaviorTreePolice.UI
{
    public sealed class PoliceBTDebugUI : MonoBehaviour
    {
        [Header("Target")]
        public PoliceBehaviorTreeRunner Target;

        [Header("UI")]
        public Canvas Canvas;
        public Text Text;
        public int fontSize = 16;
        public Vector2 anchoredPosition = new Vector2(12f, -12f);
        public Vector2 panelSize = new Vector2(520f, 360f);

        private readonly StringBuilder builder = new StringBuilder(512);

        private PoliceAIContext context;
        private PoliceBlackboard blackboard;

        private void Awake()
        {
            ResolveReferences();
            EnsureUI();
        }

        private void Update()
        {
            ResolveReferences();
            EnsureUI();
            UpdateText();
        }

        private void ResolveReferences()
        {
            if (Target == null)
            {
                Target = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            }

            if (Target == null)
            {
                context = null;
                blackboard = null;
                return;
            }

            context = Target.GetComponent<PoliceAIContext>();
            blackboard = context != null
                ? context.PoliceBlackboard
                : Target.GetComponent<PoliceBlackboard>();
        }

        private void EnsureUI()
        {
            if (Canvas == null)
            {
                Canvas = GetComponentInChildren<Canvas>();
            }

            if (Canvas == null)
            {
                GameObject canvasObject = new GameObject("Police BT Debug Canvas");
                canvasObject.transform.SetParent(transform, false);

                Canvas = canvasObject.AddComponent<Canvas>();
                Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);

                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (Text == null)
            {
                Text = Canvas.GetComponentInChildren<Text>();
            }

            if (Text == null)
            {
                GameObject textObject = new GameObject("Police BT Debug Text");
                textObject.transform.SetParent(Canvas.transform, false);

                Text = textObject.AddComponent<Text>();
                Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                Text.alignment = TextAnchor.UpperLeft;
                Text.horizontalOverflow = HorizontalWrapMode.Wrap;
                Text.verticalOverflow = VerticalWrapMode.Overflow;
                Text.color = Color.white;

                RectTransform rect = Text.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }

            Text.fontSize = fontSize;

            RectTransform textRect = Text.rectTransform;
            textRect.anchoredPosition = anchoredPosition;
            textRect.sizeDelta = panelSize;
        }

        private void UpdateText()
        {
            if (Text == null)
            {
                return;
            }

            if (Target == null || blackboard == null)
            {
                Text.text = "Police BT Debug\nNo PoliceBehaviorTreeRunner found.";
                return;
            }

            DemoPlayerState playerState = context != null ? context.PlayerState : null;

            builder.Clear();
            builder.AppendLine("Police BT Debug");
            builder.Append("Current Behavior: ").AppendLine(blackboard.CurrentBehaviorName);
            builder.Append("Current Node: ").AppendLine(blackboard.CurrentNodeName);
            builder.Append("Last Tree Status: ").AppendLine(blackboard.LastTreeStatus.ToString());
            builder.Append("PlayerVisible: ").AppendLine(blackboard.PlayerVisible.ToString());
            builder.Append("PlayerSuspicious: ").AppendLine(blackboard.PlayerSuspicious.ToString());
            builder.Append("PlayerInArrestRange: ").AppendLine(blackboard.PlayerInArrestRange.ToString());
            builder.Append("HasLastKnownPlayerPosition: ").AppendLine(blackboard.HasLastKnownPlayerPosition.ToString());
            builder.Append("LastKnownPlayerPosition: ").AppendLine(FormatVector3(blackboard.LastKnownPlayerPosition));
            builder.Append("BackupCalled: ").AppendLine(blackboard.BackupCalled.ToString());
            builder.Append("OfficerHealthLow: ").AppendLine(blackboard.OfficerHealthLow.ToString());
            builder.Append("Player IsRunning: ").AppendLine(playerState != null ? playerState.IsRunning.ToString() : "N/A");
            builder.Append("Player IsInRestrictedArea: ").AppendLine(playerState != null ? playerState.IsInRestrictedArea.ToString() : "N/A");
            builder.Append("Player IsArrested: ").AppendLine(playerState != null ? playerState.IsArrested.ToString() : "N/A");

            Text.text = builder.ToString();
        }

        private static string FormatVector3(Vector3 value)
        {
            return $"({value.x:0.00}, {value.y:0.00}, {value.z:0.00})";
        }
    }
}
