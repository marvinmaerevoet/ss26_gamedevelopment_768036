using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BehaviorTreeDemo.AI.GitAmend.Runtime;
using BehaviorTreeDemo.Police;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BehaviorTreeDemo.AI.GitAmend.Diagnostics
{
    public sealed class GitAmendBehaviorTreeDebugUI : MonoBehaviour
    {
        private const string TitleColor = "#FFF1D6";
        private const string AccentColor = "#D9A85F";
        private const string LabelColor = "#A99780";
        private const string ValueColor = "#E8DDCF";
        private const string IdleColor = "#7F756A";
        private const string RunningColor = "#F0C878";
        private const string SuccessColor = "#87C79A";
        private const string FailureColor = "#D98B7F";

        [Header("Target")]
        public GitAmendPoliceDecisionController target;
        [SerializeField] private bool enableNumberKeySelection = true;

        [Header("UI")]
        public Canvas canvas;
        public Text text;
        [SerializeField] private int fontSize = 16;
        [SerializeField] private int titleFontSize = 24;
        [SerializeField, Min(480f)] private float panelWidth = 580f;
        [SerializeField, Min(640f)] private float panelHeight = 900f;
        [SerializeField] private Vector2 panelOffset = new Vector2(24f, -24f);
        [SerializeField] private Color panelColor = new Color(0.09f, 0.065f, 0.035f, 0.94f);
        [SerializeField] private int sortingOrder = 75;
        [SerializeField, Min(0.02f)] private float refreshInterval = 0.1f;
        [SerializeField] private bool useRichText = true;

        private readonly StringBuilder builder = new StringBuilder(4096);
        private readonly List<GitAmendPoliceDecisionController> sheriffTargets =
            new List<GitAmendPoliceDecisionController>(4);

        private GitAmendPoliceDecisionController resolvedTarget;
        private PoliceAIContext context;
        private PoliceBlackboard blackboard;
        private RectTransform panelRect;
        private Image panelImage;
        private int selectedTargetIndex = -1;
        private float nextRefreshTime;
        private bool warnedMissingFont;

        private void Awake()
        {
            RefreshSheriffTargets();
            ResolveReferences();
            EnsureUI();
        }

        private void Update()
        {
            HandleSheriffSelection();

            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + Mathf.Max(0.02f, refreshInterval);
            ResolveReferences();
            EnsureUI();
            UpdateText();
        }

        private void RefreshSheriffTargets()
        {
            sheriffTargets.Clear();
            GitAmendPoliceDecisionController[] foundTargets =
                FindObjectsByType<GitAmendPoliceDecisionController>(FindObjectsInactive.Include);

            foreach (GitAmendPoliceDecisionController controller in foundTargets)
            {
                if (controller != null && controller.gameObject.scene == gameObject.scene)
                {
                    sheriffTargets.Add(controller);
                }
            }

            sheriffTargets.Sort((left, right) => string.Compare(
                left.gameObject.name,
                right.gameObject.name,
                StringComparison.OrdinalIgnoreCase));

            if (target == null && sheriffTargets.Count > 0)
            {
                target = sheriffTargets[0];
            }

            selectedTargetIndex = sheriffTargets.IndexOf(target);
        }

        private void HandleSheriffSelection()
        {
            if (!enableNumberKeySelection || sheriffTargets.Count == 0)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) SelectSheriff(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) SelectSheriff(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) SelectSheriff(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) SelectSheriff(3);
        }

        private void SelectSheriff(int index)
        {
            if (index < 0 || index >= sheriffTargets.Count || sheriffTargets[index] == null)
            {
                return;
            }

            selectedTargetIndex = index;
            target = sheriffTargets[index];
            resolvedTarget = null;
            nextRefreshTime = 0f;
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (target == null && sheriffTargets.Count > 0)
            {
                target = sheriffTargets[0];
            }

            if (target == resolvedTarget)
            {
                return;
            }

            resolvedTarget = target;
            selectedTargetIndex = sheriffTargets.IndexOf(target);
            context = target != null ? target.GetComponent<PoliceAIContext>() : null;
            blackboard = context != null ? context.PoliceBlackboard : null;
        }

        private void EnsureUI()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("GitAmend Behaviour Tree Debug Canvas");
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;

                CanvasGroup group = canvasObject.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            canvas.sortingOrder = sortingOrder;

            if (panelRect == null)
            {
                Transform panel = canvas.transform.Find("GitAmend Behaviour Tree Debug Panel");
                panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
            }

            if (panelRect == null)
            {
                GameObject panelObject = new GameObject("GitAmend Behaviour Tree Debug Panel");
                panelObject.transform.SetParent(canvas.transform, false);
                panelImage = panelObject.AddComponent<Image>();
                panelImage.raycastTarget = false;
                panelRect = panelObject.GetComponent<RectTransform>();
            }

            if (panelImage == null)
            {
                panelImage = panelRect.GetComponent<Image>();
            }

            panelImage.color = panelColor;
            panelImage.raycastTarget = false;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = panelOffset;
            panelRect.sizeDelta = new Vector2(Mathf.Max(480f, panelWidth), Mathf.Max(640f, panelHeight));

            if (text == null)
            {
                text = panelRect.GetComponentInChildren<Text>();
            }

            if (text == null)
            {
                GameObject textObject = new GameObject("GitAmend Behaviour Tree Debug Text");
                textObject.transform.SetParent(panelRect, false);
                text = textObject.AddComponent<Text>();
                text.font = LoadBuiltinFont();
                text.alignment = TextAnchor.UpperLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.color = Color.white;

                RectTransform textRect = text.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(22f, 20f);
                textRect.offsetMax = new Vector2(-22f, -20f);
            }

            text.supportRichText = useRichText;
            text.fontSize = Mathf.Max(12, fontSize);
            text.lineSpacing = 0.95f;
            text.raycastTarget = false;
        }

        private Font LoadBuiltinFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (Exception exception)
            {
                WarnMissingFont($"Could not load Unity built-in font LegacyRuntime.ttf: {exception.Message}");
            }

            if (font == null)
            {
                WarnMissingFont("Unity built-in font LegacyRuntime.ttf was not found. GitAmend debug UI will continue without assigning a font.");
            }

            return font;
        }

        private void WarnMissingFont(string message)
        {
            if (warnedMissingFont) return;
            Debug.LogWarning(message, this);
            warnedMissingFont = true;
        }

        private void UpdateText()
        {
            if (text == null) return;

            builder.Clear();
            AppendTitle();
            AppendSheriffSelector();
            AppendSectionHeader("TREE");

            Node root = target != null ? target.Tree : null;
            if (root != null)
            {
                AppendNode(root, string.Empty, true, 0);
            }
            else
            {
                builder.AppendLine(Colorize("Tree is waiting for its controller.", IdleColor));
            }

            builder.AppendLine(Colorize("No active path/per-node status in the original runtime.", IdleColor));

            AppendSectionHeader("GITAMEND");
            AppendField("Root Status", GetTreeStatus(), GetTreeStatusColor());
            AppendField("Last Process", GetLastProcessAge(), ValueColor);
            AppendField("Tick Model", "Process() every Update", ValueColor);

            AppendSectionHeader("SHARED PERCEPTION");
            AppendPerception();
            AppendSectionHeader("SHARED STATE");
            AppendState();

            text.text = builder.ToString();
        }

        private void AppendTitle()
        {
            if (useRichText)
            {
                builder.Append("<size=").Append(titleFontSize).Append("><b><color=")
                    .Append(TitleColor).Append(">GITAMEND APPROACH · BEHAVIOUR TREE</color></b></size>")
                    .AppendLine();
                builder.AppendLine(Colorize("NATIVE PROCESS() VIEW", AccentColor));
            }
            else
            {
                builder.AppendLine("GITAMEND APPROACH - BEHAVIOUR TREE");
                builder.AppendLine("NATIVE PROCESS() VIEW");
            }

            AppendDivider();
        }

        private void AppendSheriffSelector()
        {
            string sheriffName = selectedTargetIndex >= 0 ? $"Sheriff {selectedTargetIndex + 1:00}" : "Sheriff --";
            builder.Append(useRichText ? "<b>" : string.Empty)
                .Append(Colorize(sheriffName, TitleColor))
                .Append(useRichText ? "</b>" : string.Empty)
                .Append("    ")
                .Append(Colorize(GetTreeStatus(), GetTreeStatusColor()))
                .AppendLine();

            builder.Append(Colorize("Select", LabelColor)).Append("  ");
            for (int index = 0; index < sheriffTargets.Count && index < 4; index++)
            {
                string option = $"[{index + 1}] {index + 1:00}";
                builder.Append(Colorize(
                    index == selectedTargetIndex && useRichText ? $"<b>{option}</b>" : option,
                    index == selectedTargetIndex ? AccentColor : IdleColor));
                if (index < sheriffTargets.Count - 1 && index < 3) builder.Append("   ");
            }

            builder.AppendLine();
        }

        private void AppendNode(Node node, string prefix, bool isLast, int depth)
        {
            if (node == null || depth > 8) return;

            bool isRoot = depth == 0;
            string connector = isRoot ? string.Empty : isLast ? "└─ " : "├─ ";
            builder.Append(prefix).Append(connector).Append(Colorize(node.name, depth <= 1 ? AccentColor : ValueColor));
            if (node.priority != 0) builder.Append(Colorize($"  [priority {node.priority}]", LabelColor));
            builder.AppendLine();

            string childPrefix = prefix + (isRoot ? string.Empty : isLast ? "   " : "│  ");
            for (int index = 0; index < node.children.Count; index++)
            {
                AppendNode(node.children[index], childPrefix, index == node.children.Count - 1, depth + 1);
            }
        }

        private void AppendPerception()
        {
            if (blackboard == null)
            {
                AppendField("Visible", "N/A", IdleColor);
                AppendField("Suspicious", "N/A", IdleColor);
                AppendField("Arrest Range", "N/A", IdleColor);
                return;
            }

            AppendField("Visible", FormatBoolean(blackboard.PlayerVisible), GetBooleanColor(blackboard.PlayerVisible));
            AppendField("Suspicious", FormatBoolean(blackboard.PlayerSuspicious), GetBooleanColor(blackboard.PlayerSuspicious));
            AppendField("Arrest Range", FormatBoolean(blackboard.PlayerInArrestRange), GetBooleanColor(blackboard.PlayerInArrestRange));
            AppendField("Distance", GetPlayerDistance(), ValueColor);
            AppendField("Snapshot", context != null ? $"#{context.PerceptionRevision}" : "N/A", ValueColor);
            AppendField("Last Known", blackboard.HasLastKnownPlayerPosition
                ? FormatVector(blackboard.LastKnownPlayerPosition)
                : "None", blackboard.HasLastKnownPlayerPosition ? ValueColor : IdleColor);
        }

        private void AppendState()
        {
            if (blackboard == null)
            {
                AppendField("Mode", "N/A", IdleColor);
                AppendField("Movement", "N/A", IdleColor);
                AppendField("Arrest", "N/A", IdleColor);
                return;
            }

            AppendField("Mode", blackboard.CurrentBehaviorMode.ToString(), ValueColor);
            AppendField("Movement", context != null ? context.GetMovementStatus().ToString() : "N/A", ValueColor);
            AppendField("Arrest", GetArrestState(), ValueColor);
        }

        private string GetTreeStatus()
        {
            return target == null || target.LastTickTime < 0f ? "WAITING" : target.LastTreeStatus.ToString().ToUpperInvariant();
        }

        private string GetTreeStatusColor()
        {
            if (target == null || target.LastTickTime < 0f) return IdleColor;
            switch (target.LastTreeStatus)
            {
                case Node.Status.Success: return SuccessColor;
                case Node.Status.Failure: return FailureColor;
                default: return RunningColor;
            }
        }

        private string GetLastProcessAge()
        {
            return target == null || target.LastTickTime < 0f
                ? "N/A"
                : string.Format(CultureInfo.InvariantCulture, "{0:0.00}s ago", Mathf.Max(0f, Time.time - target.LastTickTime));
        }

        private string GetPlayerDistance()
        {
            if (context == null || context.Self == null || blackboard == null || blackboard.Player == null) return "N/A";
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.0} m",
                Vector3.Distance(context.Self.position, blackboard.Player.position));
        }

        private string GetArrestState()
        {
            if (context == null) return "N/A";
            if (context.IsArrestCommitted) return "Committed";
            if (context.IsArrestApproachActive) return "Approaching";
            if (context.IsArrestLatched) return "Latched";
            return "None";
        }

        private void AppendSectionHeader(string title)
        {
            builder.AppendLine();
            builder.Append(useRichText ? "<b>" : string.Empty)
                .Append(Colorize(title, AccentColor))
                .Append(useRichText ? "</b>" : string.Empty)
                .AppendLine();
        }

        private void AppendField(string label, string value, string color)
        {
            builder.Append(Colorize(label.PadRight(18), LabelColor)).Append(Colorize(value, color)).AppendLine();
        }

        private void AppendDivider()
        {
            builder.AppendLine(Colorize("────────────────────────────────────────────", "#5A4227"));
        }

        private string Colorize(string value, string color)
        {
            return useRichText ? $"<color={color}>{value}</color>" : value;
        }

        private static string FormatBoolean(bool value) => value ? "YES" : "NO";
        private static string GetBooleanColor(bool value) => value ? SuccessColor : IdleColor;

        private static string FormatVector(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0:0.0}, {1:0.0}, {2:0.0})",
                value.x,
                value.y,
                value.z);
        }
    }
}
