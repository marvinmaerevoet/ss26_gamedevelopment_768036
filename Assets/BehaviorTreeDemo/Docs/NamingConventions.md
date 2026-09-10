# Naming and ownership

Shared code uses `BehaviorTreeDemo.<Area>` namespaces. Police perception, movement, arrest lifecycle, gameplay, UI, and animation remain outside every decision adapter.

The current custom implementation uses these namespaces:

- `BehaviorTreeDemo.AI.CustomApproach` for the runner;
- `BehaviorTreeDemo.AI.CustomApproach.Runtime` for generic tree primitives;
- `BehaviorTreeDemo.AI.CustomApproach.Nodes` for police conditions and actions;
- `BehaviorTreeDemo.AI.CustomApproach.Diagnostics` for the custom runner UI;
- `BehaviorTreeDemo.AI.CustomApproach.Setup` for its selective authoring helper.

The `A_` and `C_` prefixes are retained as the Custom Approach teaching convention: actions and conditions remain immediately distinguishable in the tree definition. `CustomApproachSceneSetup` and `CustomApproachBehaviorTreeDebugUI` also keep their names because they are intentionally specific to this adapter.

The shared Scene uses the neutral names `Vision Light Origin`, `Arrest Crate Spawn`, `Custom Approach Setup`, and `Demo Reset`. Runtime links remain serialized object references rather than name lookups.

The empty adapter folders for Git Amend, Unity Behavior, Behavior Designer, and Game Creator Behavior are retained because the repository structure reserves those comparison slots. The empty `Materials`, `Prefabs`, and Custom Approach `Editor` folders are likewise retained as existing Unity folder assets; no placeholder scripts were added.

Removed dead declarations were limited to items with no source or serialized consumer: `BasicAnimationDriver.movingThreshold`, the unused `PoliceBehaviorMode.Suspicion` member, and the obsolete `CustomApproachDemo` path exclusion inside a search already restricted to `Assets/ThirdParty/Synty`.
