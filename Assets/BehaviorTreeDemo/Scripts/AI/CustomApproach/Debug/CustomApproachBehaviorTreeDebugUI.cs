using System.Collections.Generic;
using System.Text;
using CustomApproachDemo.BehaviorTree;
using CustomApproachDemo.Player;
using CustomApproachDemo.Police;
using UnityEngine;
using UnityEngine.UI;

namespace CustomApproachDemo.UI
{
    public sealed class CustomApproachBehaviorTreeDebugUI : MonoBehaviour
    {
        [Header("Target")]
        public PoliceBehaviorTreeRunner target;

        [Header("UI")]
        public Canvas canvas;
        public Text text;
        public int fontSize = 20;
        public int titleFontSize = 26;
        public bool showBlackboardSummary = true;
        public bool showTree = true;
        public bool showLegend = true;
        public float refreshInterval = 0.1f;
        public int maxDepth = 8;
        public bool onlyShowImportantBranches;
        public bool useRichText = true;
        public bool useAsciiSymbols;

        private readonly StringBuilder builder = new StringBuilder(4096);
        private readonly HashSet<BTNode> activePath = new HashSet<BTNode>();

        private PoliceAIContext context;
        private PoliceBlackboard blackboard;
        private RectTransform panelRect;
        private float nextRefreshTime;
        private bool warnedMissingFont;

        private void Awake()
        {
            ResolveReferences();
            EnsureUI();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + Mathf.Max(0.02f, refreshInterval);
            ResolveReferences();
            EnsureUI();
            UpdateText();
        }

        private void ResolveReferences()
        {
            if (target == null)
            {
                target = FindAnyObjectByType<PoliceBehaviorTreeRunner>();
            }

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
                GameObject canvasObject = new GameObject("CustomApproach BehaviorTree Debug Canvas");
                canvasObject.transform.SetParent(transform, false);

                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);

                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (panelRect == null)
            {
                Transform panel = canvas.transform.Find("CustomApproach BehaviorTree Debug Panel");
                panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
            }

            if (panelRect == null)
            {
                GameObject panelObject = new GameObject("CustomApproach BehaviorTree Debug Panel");
                panelObject.transform.SetParent(canvas.transform, false);

                Image image = panelObject.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0.72f);

                panelRect = panelObject.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(0f, 1f);
                panelRect.pivot = new Vector2(0f, 1f);
                panelRect.anchoredPosition = new Vector2(12f, -12f);
                panelRect.sizeDelta = new Vector2(760f, -24f);
            }

            if (text == null)
            {
                text = panelRect.GetComponentInChildren<Text>();
            }

            if (text == null)
            {
                GameObject textObject = new GameObject("CustomApproach BehaviorTree Debug Text");
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
                textRect.offsetMin = new Vector2(16f, 16f);
                textRect.offsetMax = new Vector2(-16f, -16f);
            }

            text.supportRichText = useRichText;
            text.fontSize = fontSize;
        }

        private Font LoadBuiltinFont()
        {
            Font font = null;

            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (System.Exception exception)
            {
                WarnMissingFont($"Could not load Unity built-in font LegacyRuntime.ttf: {exception.Message}");
            }

            if (font == null)
            {
                WarnMissingFont("Unity built-in font LegacyRuntime.ttf was not found. CustomApproach BehaviorTree Debug UI will continue without assigning a font.");
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

            activePath.Clear();
            if (target != null)
            {
                foreach (BTNode node in target.LastTickPath)
                {
                    if (node != null)
                    {
                        activePath.Add(node);
                    }
                }
            }

            builder.Clear();
            AppendTitle("POLICE BEHAVIOR TREE");

            if (showBlackboardSummary)
            {
                AppendBlackboardSummary();
            }

            if (showLegend)
            {
                AppendLegend();
            }

            if (showTree)
            {
                BTNode root = target != null ? target.TreeRoot : null;
                if (root != null)
                {
                    AppendNode(root, string.Empty, true, 0);
                }
                else
                {
                    builder.AppendLine(Colorize("No PoliceBehaviorTreeRunner tree found.", "#B0B0B0"));
                }
            }

            text.text = builder.ToString();
        }

        private void AppendTitle(string title)
        {
            if (useRichText)
            {
                builder.Append("<size=").Append(titleFontSize).Append("><b><color=#9CDCFE>");
                builder.Append(title);
                builder.AppendLine("</color></b></size>");
                return;
            }

            builder.AppendLine(title);
        }

        private void AppendBlackboardSummary()
        {
            if (target == null || blackboard == null)
            {
                builder.AppendLine("Behavior: N/A | Node: N/A | Status: N/A");
                builder.AppendLine("Visible: N/A | Suspicious: N/A | ArrestRange: N/A | Arrested: N/A");
                builder.AppendLine();
                return;
            }

            DemoPlayerState playerState = context != null ? context.PlayerState : null;

            builder.Append("Behavior: ").Append(blackboard.CurrentBehaviorMode);
            builder.Append(" | Node: ").Append(target.CurrentNodeName);
            builder.Append(" | Status: ").AppendLine(target.LastTreeStatus.ToString());

            builder.Append("Visible: ").Append(blackboard.PlayerVisible);
            builder.Append(" | Suspicious: ").Append(blackboard.PlayerSuspicious);
            builder.Append(" | ArrestRange: ").Append(blackboard.PlayerInArrestRange);
            builder.Append(" | Arrested: ").AppendLine(playerState != null ? playerState.IsArrested.ToString() : "N/A");
            builder.AppendLine();
        }

        private void AppendLegend()
        {
            builder.Append("Legend: ");
            builder.Append(FormatStatusSample(BTStatus.Success));
            builder.Append(" Success  ");
            builder.Append(FormatStatusSample(BTStatus.Failure));
            builder.Append(" Failure  ");
            builder.Append(FormatStatusSample(BTStatus.Running));
            builder.Append(" Running  ");
            builder.Append(Colorize(useAsciiSymbols ? "[-]" : "\u25CB", "#808080"));
            builder.AppendLine(" Idle");
            builder.AppendLine();
        }

        private string FormatStatusSample(BTStatus status)
        {
            return Colorize(GetStatusSymbol(status, true), GetStatusColor(status, true));
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

            string connector = isRoot ? string.Empty : isLast ? "\u2514\u2500 " : "\u251C\u2500 ";
            string line = prefix + connector + GetStatusSymbol(node.LastStatus, recentlyTicked) + " " + node.Name;
            string color = GetStatusColor(node.LastStatus, recentlyTicked);

            if (active || (recentlyTicked && node.IsRunning))
            {
                line = useRichText ? "<b>" + line + "</b>" : "> " + line;
            }

            builder.AppendLine(Colorize(line, color));

            IReadOnlyList<BTNode> children = node.Children;
            if (children == null || children.Count == 0 || depth >= maxDepth)
            {
                return;
            }

            string childPrefix = prefix + (isRoot ? string.Empty : isLast ? "   " : "\u2502  ");

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

            return depth <= 1 ||
                   active ||
                   (node.Parent != null && activePath.Contains(node.Parent));
        }

        private string GetStatusSymbol(BTStatus status, bool recentlyTicked)
        {
            if (!recentlyTicked)
            {
                return useAsciiSymbols ? "[-]" : "\u25CB";
            }

            switch (status)
            {
                case BTStatus.Success:
                    return useAsciiSymbols ? "[S]" : "\u2714";
                case BTStatus.Failure:
                    return useAsciiSymbols ? "[F]" : "\u2716";
                case BTStatus.Running:
                    return useAsciiSymbols ? "[R]" : "\u25B6";
                default:
                    return useAsciiSymbols ? "[-]" : "\u25CB";
            }
        }

        private static string GetStatusColor(BTStatus status, bool recentlyTicked)
        {
            if (!recentlyTicked)
            {
                return "#808080";
            }

            switch (status)
            {
                case BTStatus.Success:
                    return "#6DFF8F";
                case BTStatus.Failure:
                    return "#FF6666";
                case BTStatus.Running:
                    return "#FFD866";
                default:
                    return "#808080";
            }
        }

        private string Colorize(string value, string color)
        {
            return useRichText ? $"<color={color}>{value}</color>" : value;
        }
    }
}
