using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Demo.BehaviorTreePolice.EditorTools
{
    public static class PoliceBTSafeAnimatorSetup
    {
        public const string AnimationsFolder = "Assets/_Demo/BehaviorTreePolice/Animations";
        public const string PlayerControllerPath = AnimationsFolder + "/Player_Demo_Safe.controller";
        public const string SheriffControllerPath = AnimationsFolder + "/Sheriff_Demo_Safe.controller";

        private const string MenuPath = "Tools/Police BT Demo/Create Safe Synty Animator Controllers";

        [MenuItem(MenuPath)]
        public static void CreateSafeSyntyAnimatorControllers()
        {
            ClipSelection clips = FindSyntyClips();
            if (!clips.HasRequiredClips)
            {
                Debug.LogError(
                    "Could not create Police BT demo animator controllers. " +
                    "Required clips were not found. Search terms: Idle, Walk, Run under Assets/Synty/AnimationBaseLocomotion.");
                return;
            }

            EnsureFolder("Assets/_Demo", "BehaviorTreePolice");
            EnsureFolder("Assets/_Demo/BehaviorTreePolice", "Animations");

            AnimatorController playerController = CreateController(PlayerControllerPath);
            ConfigurePlayerController(playerController, clips);

            AnimatorController sheriffController = CreateController(SheriffControllerPath);
            ConfigureSheriffController(sheriffController, clips);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Created safe Police BT demo animator controllers.\n" +
                $"Idle: {GetPath(clips.Idle)}\n" +
                $"Walk: {GetPath(clips.Walk)}\n" +
                $"Run: {GetPath(clips.Run)}\n" +
                $"Investigate: {GetPath(clips.Investigate)}\n" +
                $"Emergency/Limp: {GetPath(clips.Emergency)}\n" +
                $"Arrest: {GetPath(clips.Arrest)}\n" +
                $"Player Controller: {PlayerControllerPath}\n" +
                $"Sheriff Controller: {SheriffControllerPath}");
        }

        private static void ConfigurePlayerController(AnimatorController controller, ClipSelection clips)
        {
            AddParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            AddParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsRunning", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsArrested", AnimatorControllerParameterType.Bool);

            AnimatorState locomotion = CreateLocomotionBlendTree(controller, clips, "Locomotion", 4f);
            if (clips.Arrest != null)
            {
                CreateOptionalBoolState(controller, locomotion, "Arrested", clips.Arrest, "IsArrested");
            }
        }

        private static void ConfigureSheriffController(AnimatorController controller, ClipSelection clips)
        {
            AddParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            AddParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsRunning", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsChasing", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsInvestigating", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsArrested", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsEmergency", AnimatorControllerParameterType.Bool);

            AnimatorState locomotion = CreateLocomotionBlendTree(controller, clips, "Locomotion", 3.5f);
            if (clips.Investigate != null)
            {
                CreateOptionalBoolState(controller, locomotion, "Investigate", clips.Investigate, "IsInvestigating");
            }

            if (clips.Emergency != null)
            {
                CreateOptionalBoolState(controller, locomotion, "Emergency", clips.Emergency, "IsEmergency");
            }

            if (clips.Arrest != null)
            {
                CreateOptionalBoolState(controller, locomotion, "Arrest", clips.Arrest, "IsArrested");
            }
        }

        private static AnimatorController CreateController(string path)
        {
            if (!path.StartsWith(AnimationsFolder, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Refusing to create animator controller outside the Police BT demo folder.");
            }

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimatorState CreateLocomotionBlendTree(
            AnimatorController controller,
            ClipSelection clips,
            string stateName,
            float runThreshold)
        {
            AnimatorState state = controller.CreateBlendTreeInController(stateName, out BlendTree blendTree);
            state.name = stateName;

            blendTree.name = stateName + " Blend Tree";
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = "Speed";
            blendTree.useAutomaticThresholds = false;
            blendTree.AddChild(clips.Idle, 0f);
            blendTree.AddChild(clips.Walk, 1.5f);
            blendTree.AddChild(clips.Run, runThreshold);

            controller.layers[0].stateMachine.defaultState = state;
            return state;
        }

        private static void CreateOptionalBoolState(
            AnimatorController controller,
            AnimatorState locomotion,
            string stateName,
            Motion motion,
            string parameterName)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState state = stateMachine.AddState(stateName);
            state.motion = motion;

            AnimatorStateTransition enter = stateMachine.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = 0.1f;
            enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, parameterName);

            AnimatorStateTransition exit = state.AddTransition(locomotion);
            exit.hasExitTime = false;
            exit.duration = 0.1f;
            exit.AddCondition(AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType parameterType)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return;
                }
            }

            controller.AddParameter(parameterName, parameterType);
        }

        private static ClipSelection FindSyntyClips()
        {
            List<ClipCandidate> candidates = FindAllAnimationClips();

            return new ClipSelection
            {
                Idle = FindBestClip(candidates, new[] { "idle", "standing", "masc" }, new[] { "polygon", "animationbaselocomotion" }),
                Walk = FindBestClip(candidates, new[] { "walk", "_f_", "masc" }, new[] { "polygon", "animationbaselocomotion", "locomotion" }),
                Run = FindBestClip(candidates, new[] { "run", "_f_", "masc" }, new[] { "polygon", "animationbaselocomotion", "locomotion" }),
                Investigate = FindBestClip(candidates, new[] { "inspect", "masc" }, new[] { "polygon", "animationidles", "inspect_torso" }),
                Emergency = FindBestClip(candidates, new[] { "limp", "masc" }, new[] { "polygon", "animationidles" }),
                Arrest = FindBestClip(candidates, new[] { "pointhand", "index_f", "masc" }, new[] { "polygon", "animationidles", "index_f_masc" })
            };
        }

        private static List<ClipCandidate> FindAllAnimationClips()
        {
            List<ClipCandidate> candidates = new List<ClipCandidate>();
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Synty" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || path.Contains("/_Demo/"))
                {
                    continue;
                }

                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                    {
                        candidates.Add(new ClipCandidate(clip, path));
                    }
                }
            }

            candidates.Sort((left, right) => string.Compare(left.SearchableText, right.SearchableText, StringComparison.OrdinalIgnoreCase));
            return candidates;
        }

        private static AnimationClip FindBestClip(
            List<ClipCandidate> candidates,
            string[] requiredTerms,
            string[] preferredTerms)
        {
            ClipCandidate best = default;
            int bestScore = int.MinValue;

            foreach (ClipCandidate candidate in candidates)
            {
                string searchable = candidate.SearchableText;
                if (!ContainsAll(searchable, requiredTerms))
                {
                    continue;
                }

                if (searchable.Contains("rootmotion"))
                {
                    continue;
                }

                int score = 0;
                foreach (string preferredTerm in preferredTerms)
                {
                    if (searchable.Contains(preferredTerm))
                    {
                        score += 10;
                    }
                }

                if (searchable.Contains("/animations/polygon/masculine/"))
                {
                    score += 20;
                }

                if (searchable.Contains("/samples/"))
                {
                    score -= 5;
                }

                if (candidate.Path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                }

                if (score > bestScore ||
                    (score == bestScore &&
                     string.Compare(candidate.SearchableText, best.SearchableText, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best.Clip;
        }

        private static bool ContainsAll(string text, string[] terms)
        {
            foreach (string term in terms)
            {
                if (!text.Contains(term))
                {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string path = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static string GetPath(AnimationClip clip)
        {
            return clip == null ? "not found" : AssetDatabase.GetAssetPath(clip);
        }

        private readonly struct ClipCandidate
        {
            public readonly AnimationClip Clip;
            public readonly string Path;
            public readonly string SearchableText;

            public ClipCandidate(AnimationClip clip, string path)
            {
                Clip = clip;
                Path = path.Replace('\\', '/');
                SearchableText = (Path + "/" + clip.name).ToLowerInvariant();
            }
        }

        private struct ClipSelection
        {
            public AnimationClip Idle;
            public AnimationClip Walk;
            public AnimationClip Run;
            public AnimationClip Investigate;
            public AnimationClip Emergency;
            public AnimationClip Arrest;

            public bool HasRequiredClips => Idle != null && Walk != null && Run != null;
        }
    }
}
