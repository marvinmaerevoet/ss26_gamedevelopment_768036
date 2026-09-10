using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BehaviorTreeDemo.AI.CustomApproach.Runtime;
using BehaviorTreeDemo.Gameplay.Player;
using BehaviorTreeDemo.Police;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BehaviorTreeDemo.AI.CustomApproach.Diagnostics
{
    public sealed class CustomApproachBehaviorTreeDebugUI : MonoBehaviour
    {
        private const string TitleColor = "#E8EDF2";
        private const string AccentColor = "#8EC5DE";
        private const string ActiveColor = "#D7EDF8";
        private const string LabelColor = "#84919E";
        private const string ValueColor = "#D6DCE3";
        private const string IdleColor = "#68717D";
        private const string RunningColor = "#E7BD69";
        private const string SuccessColor = "#78BF93";
        private const string FailureColor = "#CF7D7D";

        [Header("Target")]
        public PoliceBehaviorTreeRunner target;
        [SerializeField] private bool enableNumberKeySelection = true;

        [Header("UI")]
        public Canvas canvas;
        public Text text;
        public int fontSize = 16;
        public int titleFontSize = 24;
        [SerializeField, Min(480f)] private float panelWidth = 620f;
        [SerializeField, Min(640f)] private float panelHeight = 1032f;
        [SerializeField] private Vector2 panelOffset = new Vector2(24f, -24f);
        [SerializeField] private Color panelColor = new Color(0.035f, 0.045f, 0.06f, 0.92f);
        [SerializeField] private int sortingOrder = 75;
        public bool showBlackboardSummary = true;
        public bool showTree = true;
        public bool showLegend = true;
        public float refreshInterval = 0.1f;
        public int maxDepth = 8;
        public bool onlyShowImportantBranches;
        public bool useRichText = true;
        public bool useAsciiSymbols;

        private readonly StringBuilder builder = new StringBuilder(8192);
        private readonly HashSet<BTNode> activePath = new HashSet<BTNode>();
        private readonly List<PoliceBehaviorTreeRunner> sheriffTargets = new List<PoliceBehaviorTreeRunner>(4);

        private PoliceBehaviorTreeRunner resolvedTarget;
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
            PoliceBehaviorTreeRunner[] foundTargets = FindObjectsByType<PoliceBehaviorTreeRunner>(
                FindObjectsInactive.Include);

            foreach (PoliceBehaviorTreeRunner runner in foundTargets)
            {
                if (runner != null && runner.gameObject.scene == gameObject.scene)
                {
                    sheriffTargets.Add(runner);
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

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectSheriff(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectSheriff(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SelectSheriff(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SelectSheriff(3);
            }
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
            if (target == null)
            {
                RefreshSheriffTargets();
            }

            if (target == resolvedTarget)
            {
                return;
            }

            resolvedTarget = target;
            selectedTargetIndex = sheriffTargets.IndexOf(target);
            context = target != null ? target.GetComponent<PoliceAIContext>() : null;
            blackboard = context != null
                ? context.PoliceBlackboard
                : target != null
                    ? target.GetComponent<PoliceBlackboard>()
                    : null;
        }

        private void EnsureUI()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Custom Approach Behavior Tree Debug Canvas");
                canvasObject.transform.SetParent(transform, false);

                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;

                CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            canvas.sortingOrder = sortingOrder;

            if (panelRect == null)
            {
                Transform panel = canvas.transform.Find("Custom Approach Behavior Tree Debug Panel");
                panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
            }

            if (panelRect == null)
            {
                GameObject panelObject = new GameObject("Custom Approach Behavior Tree Debug Panel");
                panelObject.transform.SetParent(canvas.transform, false);
                panelImage = panelObject.AddComponent<Image>();
                panelImage.raycastTarget = false;
                panelRect = panelObject.GetComponent<RectTransform>();
            }

            if (panelImage == null)
            {
                panelImage = panelRect.GetComponent<Image>();
            }

            if (panelImage != null)
            {
                panelImage.color = panelColor;
                panelImage.raycastTarget = false;
            }

            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = panelOffset;
            panelRect.sizeDelta = new Vector2(
                Mathf.Max(480f, panelWidth),
                Mathf.Max(640f, panelHeight));

            if (text == null)
            {
                text = panelRect.GetComponentInChildren<Text>();
            }

            if (text == null)
            {
                GameObject textObject = new GameObject("Custom Approach Behavior Tree Debug Text");
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
            text.lineSpacing = 0.92f;
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
                WarnMissingFont("Unity built-in font LegacyRuntime.ttf was not found. Custom Approach Behavior Tree Debug UI will continue without assigning a font.");
            }

            return font;
        }

        private void WarnMissingFont(string message)
        {
            if (warnedMissingFont)
            {
                return;
            }

            Debug.LogWarning(message, this);
            warnedMissingFont = true;
        }

        private void UpdateText()
        {
            if (text == null)
            {
                return;
            }

            BuildActivePathCache();
            builder.Clear();

            AppendTitle();
            AppendSheriffSelector();

            if (showTree)
            {
                AppendSectionHeader("TREE");
                AppendActivePathSummary();

                BTNode root = target != null ? target.TreeRoot : null;
                if (root != null)
                {
                    AppendNode(root, string.Empty, true, 0);
                }
                else
                {
                    builder.AppendLine(Colorize("Tree is waiting for its runner.", IdleColor));
                }
            }

            if (showBlackboardSummary)
            {
                AppendSectionHeader("PERCEPTION");
                AppendPerceptionSummary();
                AppendSectionHeader("STATE");
                AppendStateSummary();
            }

            if (showLegend)
            {
                AppendLegend();
            }

            text.text = builder.ToString();
        }

        private void BuildActivePathCache()
        {
            activePath.Clear();
            if (target == null)
            {
                return;
            }

            foreach (BTNode node in target.LastTickPath)
            {
                if (node != null)
                {
                    activePath.Add(node);
                }
            }
        }

        private void AppendTitle()
        {
            if (useRichText)
            {
                builder.Append("<size=").Append(titleFontSize).Append("><b><color=")
                    .Append(TitleColor).Append(">CUSTOM APPROACH · BEHAVIOR TREE</color></b></size>")
                    .AppendLine();
                builder.AppendLine(Colorize("DIAGNOSTICS", AccentColor));
            }
            else
            {
                builder.AppendLine("CUSTOM APPROACH - BEHAVIOR TREE");
                builder.AppendLine("DIAGNOSTICS");
            }

            AppendDivider();
        }

        private void AppendSheriffSelector()
        {
            string sheriffName = selectedTargetIndex >= 0
                ? $"Sheriff {selectedTargetIndex + 1:00}"
                : "Sheriff --";
            string treeStatus = GetTreeStatusLabel();
            string treeStatusColor = GetTreeStatusColor();

            builder.Append(useRichText ? "<b>" : string.Empty)
                .Append(Colorize(sheriffName, TitleColor))
                .Append(useRichText ? "</b>" : string.Empty)
                .Append("    ")
                .Append(Colorize(treeStatus, treeStatusColor))
                .AppendLine();

            builder.Append(Colorize("Select", LabelColor)).Append("  ");
            for (int i = 0; i < sheriffTargets.Count && i < 4; i++)
            {
                string option = $"[{i + 1}] {i + 1:00}";
                if (i == selectedTargetIndex)
                {
                    option = useRichText ? $"<b>{option}</b>" : $"> {option}";
                    builder.Append(Colorize(option, AccentColor));
                }
                else
                {
                    builder.Append(Colorize(option, IdleColor));
                }

                if (i < sheriffTargets.Count - 1 && i < 3)
                {
                    builder.Append("   ");
                }
            }

            builder.AppendLine();
        }

        private void AppendSectionHeader(string title)
        {
            builder.AppendLine();
            builder.Append(useRichText ? "<b>" : string.Empty)
                .Append(Colorize(title, AccentColor))
                .Append(useRichText ? "</b>" : string.Empty)
                .AppendLine();
        }

        private void AppendActivePathSummary()
        {
            builder.Append(Colorize("ACTIVE PATH", LabelColor)).Append("  ");
            if (target == null || target.LastTickPath.Count == 0)
            {
                builder.AppendLine(Colorize(
                    target != null && !string.IsNullOrEmpty(target.CurrentNodeName)
                        ? target.CurrentNodeName
                        : "Waiting for first tick",
                    IdleColor));
                return;
            }

            for (int i = 0; i < target.LastTickPath.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(Colorize(useAsciiSymbols ? " > " : " › ", IdleColor));
                }

                builder.Append(Colorize(GetDisplayNodeName(target.LastTickPath[i]), ActiveColor));
            }

            builder.AppendLine();
        }

        private void AppendNode(BTNode node, string prefix, bool isLast, int depth)
        {
            if (node == null || depth > maxDepth)
            {
                return;
            }

            bool isRoot = depth == 0;
            bool active = activePath.Contains(node);
            bool recentlyTicked = node.WasTickedRecently;

            if (!ShouldShowNode(node, depth, active))
            {
                return;
            }

            string connector = isRoot
                ? string.Empty
                : useAsciiSymbols
                    ? isLast ? "`- " : "+- "
                    : isLast ? "└─ " : "├─ ";
            string activeMarker = active ? (useAsciiSymbols ? "> " : "▸ ") : "  ";
            string statusSymbol = GetStatusSymbol(node.LastStatus, recentlyTicked || active);
            string displayName = GetDisplayNodeName(node);
            string line = prefix + connector + activeMarker + statusSymbol + " " + displayName;
            string lineColor = active ? ActiveColor : GetStatusColor(node.LastStatus, recentlyTicked);

            if (active || isRoot)
            {
                line = useRichText ? "<b>" + line + "</b>" : line;
            }

            builder.Append(Colorize(line, lineColor));
            if (active || recentlyTicked)
            {
                builder.Append("  ").Append(Colorize(
                    GetStatusLabel(node.LastStatus),
                    GetStatusColor(node.LastStatus, true)));
            }

            builder.AppendLine();

            IReadOnlyList<BTNode> children = node.Children;
            if (children == null || children.Count == 0 || depth >= maxDepth)
            {
                return;
            }

            string childPrefix = prefix + (isRoot
                ? string.Empty
                : useAsciiSymbols
                    ? isLast ? "   " : "|  "
                    : isLast ? "   " : "│  ");

            for (int i = 0; i < children.Count; i++)
            {
                AppendNode(children[i], childPrefix, i == children.Count - 1, depth + 1);
            }
        }

        private bool ShouldShowNode(BTNode node, int depth, bool active)
        {
            if (!onlyShowImportantBranches)
            {
                return true;
            }

            return depth <= 1 || active || (node.Parent != null && activePath.Contains(node.Parent));
        }

        private void AppendPerceptionSummary()
        {
            if (blackboard == null)
            {
                AppendField("Visible", "N/A", IdleColor);
                AppendField("Suspicious", "N/A", IdleColor);
                AppendField("Distance", "N/A", IdleColor);
                AppendField("Snapshot", "N/A", IdleColor);
                return;
            }

            AppendField("Visible", FormatBoolean(blackboard.PlayerVisible), GetBooleanColor(blackboard.PlayerVisible));
            AppendField("Suspicious", FormatBoolean(blackboard.PlayerSuspicious), GetBooleanColor(blackboard.PlayerSuspicious));
            AppendField("Arrest Range", FormatBoolean(blackboard.PlayerInArrestRange), GetBooleanColor(blackboard.PlayerInArrestRange));
            AppendField("Distance", GetPlayerDistance(), ValueColor);
            AppendField("Snapshot", context != null ? $"#{context.PerceptionRevision}" : "N/A", ValueColor);
            AppendField("Refresh Age", GetPerceptionAge(), ValueColor);
        }

        private void AppendStateSummary()
        {
            if (blackboard == null)
            {
                AppendField("Mode", "N/A", IdleColor);
                AppendField("Movement", "N/A", IdleColor);
                AppendField("Active Node", "N/A", IdleColor);
                AppendField("Last Known", "N/A", IdleColor);
                return;
            }

            AppendField("Mode", blackboard.CurrentBehaviorMode.ToString(), ValueColor);
            AppendField("Movement", GetMovementSummary(), ValueColor);
            AppendField("Arrest", GetArrestSummary(), ValueColor);
            AppendField("Active Node", target != null && !string.IsNullOrEmpty(target.CurrentNodeName)
                ? target.CurrentNodeName
                : "N/A", ValueColor);
            AppendField("Last Known", blackboard.HasLastKnownPlayerPosition
                ? FormatVector(blackboard.LastKnownPlayerPosition)
                : "None", blackboard.HasLastKnownPlayerPosition ? ValueColor : IdleColor);
        }

        private void AppendLegend()
        {
            builder.AppendLine();
            builder.Append(Colorize("STATUS", LabelColor)).Append("  ")
                .Append(Colorize(GetStatusSymbol(BTStatus.Running, true) + " RUNNING", RunningColor)).Append("   ")
                .Append(Colorize(GetStatusSymbol(BTStatus.Success, true) + " SUCCESS", SuccessColor)).Append("   ")
                .Append(Colorize(GetStatusSymbol(BTStatus.Failure, true) + " FAILURE", FailureColor))
                .AppendLine();
        }

        private void AppendField(string label, string value, string valueColor)
        {
            builder.Append(Colorize(label.PadRight(18), LabelColor))
                .Append(Colorize(value, valueColor))
                .AppendLine();
        }

        private void AppendDivider()
        {
            builder.AppendLine(Colorize(
                useAsciiSymbols ? "------------------------------------------------" : "────────────────────────────────────────────────",
                "#34404B"));
        }

        private string GetTreeStatusLabel()
        {
            if (target == null || target.LastTickTime < 0f)
            {
                return "IDLE";
            }

            return GetStatusLabel(target.LastTreeStatus);
        }

        private string GetTreeStatusColor()
        {
            if (target == null || target.LastTickTime < 0f)
            {
                return IdleColor;
            }

            return GetStatusColor(target.LastTreeStatus, true);
        }

        private string GetPlayerDistance()
        {
            if (context == null || context.PlayerState == null)
            {
                return "N/A";
            }

            Transform origin = context.Self != null ? context.Self : context.transform;
            float distance = Vector3.Distance(origin.position, context.PlayerState.transform.position);
            return distance.ToString("0.0", CultureInfo.InvariantCulture) + " m";
        }

        private string GetPerceptionAge()
        {
            if (context == null || context.LastPerceptionRefreshTime < 0f)
            {
                return "N/A";
            }

            float age = Mathf.Max(0f, Time.time - context.LastPerceptionRefreshTime);
            return age.ToString("0.00", CultureInfo.InvariantCulture) + " s";
        }

        private string GetMovementSummary()
        {
            if (context == null)
            {
                return "N/A";
            }

            PoliceMovementStatus movementStatus = context.GetMovementStatus();
            return context.CurrentMovementMode + " · " + movementStatus;
        }

        private string GetArrestSummary()
        {
            if (context == null)
            {
                return "N/A";
            }

            if (context.IsArrestCommitted)
            {
                return "Committed";
            }

            if (context.IsArrestApproachActive)
            {
                return "Approaching";
            }

            if (context.IsArrestLatched)
            {
                return "Latched";
            }

            DemoPlayerState playerState = context.PlayerState;
            return playerState != null && playerState.IsArrested ? "Player Arrested" : "Idle";
        }

        private string GetStatusSymbol(BTStatus status, bool hasStatus)
        {
            if (!hasStatus)
            {
                return useAsciiSymbols ? "[-]" : "·";
            }

            switch (status)
            {
                case BTStatus.Success:
                    return useAsciiSymbols ? "[S]" : "✓";
                case BTStatus.Failure:
                    return useAsciiSymbols ? "[F]" : "×";
                case BTStatus.Running:
                    return useAsciiSymbols ? "[R]" : "▶";
                default:
                    return useAsciiSymbols ? "[-]" : "·";
            }
        }

        private static string GetStatusLabel(BTStatus status)
        {
            return status.ToString().ToUpperInvariant();
        }

        private static string GetStatusColor(BTStatus status, bool hasStatus)
        {
            if (!hasStatus)
            {
                return IdleColor;
            }

            switch (status)
            {
                case BTStatus.Success:
                    return SuccessColor;
                case BTStatus.Failure:
                    return FailureColor;
                case BTStatus.Running:
                    return RunningColor;
                default:
                    return IdleColor;
            }
        }

        private string FormatBoolean(bool value)
        {
            if (useAsciiSymbols)
            {
                return value ? "YES" : "NO";
            }

            return value ? "✓ YES" : "· NO";
        }

        private static string GetBooleanColor(bool value)
        {
            return value ? SuccessColor : IdleColor;
        }

        private static string FormatVector(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0:0.0}, {1:0.0}, {2:0.0})",
                value.x,
                value.y,
                value.z);
        }

        private static string GetDisplayNodeName(BTNode node)
        {
            if (node == null)
            {
                return "Unknown";
            }

            switch (node.Name)
            {
                case "Root Selector":
                    return "Root";
                case "EmergencyBehavior":
                    return "Emergency";
                case "ArrestBehavior":
                    return "Arrest";
                case "ChaseBehavior":
                    return "Chase";
                case "InvestigateBehavior":
                    return "Investigate";
                case "PatrolBehavior":
                    return "Patrol";
                case "Move To Last Known Position Timeout":
                    return "Move To Last Known (5s)";
                case "Move To Patrol Point Retry":
                    return "Move To Patrol (Retry 2)";
                default:
                    return node.Name;
            }
        }

        private string Colorize(string value, string color)
        {
            return useRichText ? $"<color={color}>{value}</color>" : value;
        }
    }
}
