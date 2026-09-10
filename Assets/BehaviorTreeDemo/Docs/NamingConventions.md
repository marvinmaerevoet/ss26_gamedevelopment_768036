# Naming and ownership

Shared code uses `BehaviorTreeDemo.<Area>` namespaces. Police perception, movement, arrest lifecycle, gameplay, UI, and animation remain outside every decision adapter.

The current custom implementation uses these namespaces:

- `BehaviorTreeDemo.AI.CustomApproach` for the runner;
- `BehaviorTreeDemo.AI.CustomApproach.Runtime` for generic tree primitives;
- `BehaviorTreeDemo.AI.CustomApproach.Nodes` for police conditions and actions;
- `BehaviorTreeDemo.AI.CustomApproach.Diagnostics` for the custom runner UI.

The `A_` and `C_` prefixes are retained as the Custom Approach teaching convention: actions and conditions remain immediately distinguishable in the tree definition. `CustomApproachBehaviorTreeDebugUI` keeps its name because it is intentionally specific to this adapter.

The shared Scene uses the neutral names `Vision Light Origin`, `Arrest Crate Spawn`, and `Demo Reset`. It is fully wired and contains no setup-helper component. Runtime links remain serialized object references rather than name lookups.

The empty adapter folders for Git Amend, Unity Behavior, Behavior Designer, and Game Creator Behavior are retained because the repository structure reserves those comparison slots. The empty `Materials` and `Prefabs` folders are likewise retained as existing Unity folder assets; no placeholder scripts were added. The obsolete empty Custom Approach `Editor` folder was removed with its retired bootstrap tools.

The final baseline cleanup removed only proven obsolete bootstrap/self-repair paths, duplicate raw-input paths, and unconsumed diagnostics. Shared gameplay fallbacks that still serve teleport and transform-speed recovery remain intentionally available.
