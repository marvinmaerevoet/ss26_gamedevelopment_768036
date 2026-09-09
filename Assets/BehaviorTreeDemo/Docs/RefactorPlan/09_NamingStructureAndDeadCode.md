# Safety Rules

- NEVER enter Unity Play Mode.
- NEVER run runtime/play tests.
- Runtime playtesting is performed manually by the user.
- Validation means Edit Mode compilation + Console inspection only.
- Do not modify Third-Party/vendor code unless the task explicitly allows it.
- Preserve Unity `.meta` files and GUIDs.
- Use Unity Editor / AssetDatabase / MCP for asset or Scene operations.
- No unrelated refactorings.
- Prefer minimal, understandable solutions.
- No DI framework.
- No Service Locator.
- No global Event Bus.
- No unnecessary interface proliferation.
- Do not over-engineer this university demo.
- Stop if the required change becomes materially larger than the task scope.
- Report uncertainties instead of inventing architecture.

# Task 09 — Naming, Structure, and Dead Code

## Goal

After functional stabilization, make shared and Custom-specific ownership clearer through careful naming and remove only proven dead code. All Unity moves and type changes must preserve GUIDs and serialized data.

## Current Problem

Shared classes still use `CustomApproachDemo.*` namespaces although the game is now `BehaviorTreeDemo`. Some common objects/assets retain Custom-specific or obsolete names. Several fields and helpers appear unused or stale, but must be verified before removal. Folder structure is mostly correct but contains empty/legacy areas and mixed runtime-state/Inspector exposure.

## Files / Areas To Inspect

- every own C# namespace, class, file, and serialized field
- `Assets/BehaviorTreeDemo/` folders and `.meta` files
- Scene GameObject names and serialized MonoBehaviour references
- own Animator Controllers, materials, menus, logs, and Docs
- `BasicAnimationDriver.movingThreshold`
- `PoliceAIContext.CancelArrestApproach`
- `PoliceBehaviorMode.Suspicion`
- `loggedArrest` and `loggedBackup`
- stale `CustomApproachDemo` path filters and labels
- empty legacy folders
- all references to any proposed rename before changing it

## Required Changes

Evaluate and, only where beneficial, migrate toward:

- shared namespaces: `BehaviorTreeDemo.<Area>`
- Custom runtime: `BehaviorTreeDemo.AI.CustomApproach.Runtime`
- Custom nodes: `BehaviorTreeDemo.AI.CustomApproach.Nodes`
- Custom debug/setup: clearly under `BehaviorTreeDemo.AI.CustomApproach.*`

Evaluate these candidate names:

- `CustomApproachDemoReset` → `DemoReset`
- `VisionLightCone` → `VisionLightOrigin`
- `CrateReset` → `ArrestCrateSpawn`
- `CA_ReadableNight_*` → a neutral demo prefix
- Custom-specific reset log text → neutral text

Do not automatically rename `A_*` and `C_*`; the existing Custom-node scheme is consistent and understandable for a teaching demo. If retained, document it as the CustomApproach convention.

For dead code, prove no source, serialized, reflection, ContextMenu, animation, or Scene dependency exists before deletion. Preserve `FormerlySerializedAs` or add migration support where field renames require it. Use AssetDatabase/MCP for all Unity assets and Scene objects.

## Explicit Non-Goals

- Do not change behavior, tuning, Scene transforms, or architecture.
- Do not perform a blind global text replacement.
- Do not add Assembly Definitions in this task unless separately approved.
- Do not remove apparently unused public APIs without checking serialized/editor/reflection use.
- Do not create placeholder files in empty future-adapter folders.

## Acceptance Criteria

- Shared code has clearly shared naming; Custom code has clearly Custom naming.
- Every moved Unity asset retains its GUID.
- Scene and serialized references remain valid.
- Only proven dead code is removed.
- Retained legacy names have a documented reason.
- No Third-Party path or namespace is altered.
- No behavior change appears in the code diff beyond necessary namespace/type migration.

## Validation

- Capture before/after GUID mappings.
- Compile in Edit Mode and inspect Console after each coherent rename group; stop on error.
- Search for old names, Missing Scripts, broken serialized references, and stale menu/log text.
- Save the Scene only when an authorized Scene rename/reference update requires it.
- Do not enter Play Mode.

## Report Format

Report every old → new name, intentionally retained names, dead-code evidence, deleted items, moved assets with GUID confirmation, Scene/reference validation, files/assets changed, and compilation/Console result with manual smoke-test checklist.

