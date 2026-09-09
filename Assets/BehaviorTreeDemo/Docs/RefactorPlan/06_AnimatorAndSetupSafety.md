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

# Task 06 — Animator and Setup Safety

## Goal

Prevent Editor/setup tools from overwriting the working project state and document their true scope as authoring helpers.

## Current Problem

`PoliceAnimatorControllerSetup.CreateController()` can delete existing Animator Controller assets and recreate them. The generator does not represent the current Player arrest Enter/Hold/Release flow, current Sheriff animation transitions, or current movement thresholds. Running it can therefore destroy working controller behavior and may change GUID-backed references. `CustomApproachSceneSetup` is a valid runtime-assembly MonoBehaviour with editor-only ContextMenu operations, but its helpers no longer reproduce the complete modern mission and multi-Sheriff Scene.

## Files / Areas To Inspect

- `Animation/Editor/PoliceAnimatorControllerSetup.cs`
- `AI/CustomApproach/Setup/CustomApproachSceneSetup.cs`
- `Animations/Player.controller`
- `Animations/Sheriff.controller`
- `Animations/Lydia.controller`
- `Animation/BasicAnimationDriver.cs`
- current Player, Sheriff, and Lydia Animator references
- menu names, ContextMenu labels, comments, and README setup instructions

## Required Changes

### Animator generator safety

Add the smallest reliable protection so the ordinary command never deletes or overwrites existing working controllers. Prefer refusing with a clear message when a target exists. If regeneration is retained, it must require an explicit, separately named destructive action with clear intent; do not silently regenerate.

Do not modernize the entire generator unless the task cannot otherwise make it safe. Preserve existing controller GUIDs and Scene references.

### Scene setup scope

Clearly identify `CustomApproachSceneSetup` as a bootstrap/authoring helper for selected components, not a complete reconstruction tool for the current Demo. Keep it a valid MonoBehaviour in the runtime assembly because it is attached to a Scene object and its ContextMenus are guarded by `UNITY_EDITOR`.

Avoid adding support for every modern gameplay system in this task.

## Explicit Non-Goals

- Do not replace or rebuild working Animator Controllers.
- Do not change animation clips, transitions, thresholds, or runtime presentation.
- Do not recreate the Scene.
- Do not move the attached setup MonoBehaviour into an `Editor` assembly.
- Do not build a new setup framework.

## Acceptance Criteria

- Running the normal setup command cannot delete or overwrite existing controllers.
- Existing Player/Sheriff/Lydia controller GUIDs and assignments remain intact.
- The setup helper's scope is accurately communicated in code/menu/documentation.
- `CustomApproachSceneSetup` remains a valid attached MonoBehaviour and editor-only APIs remain guarded.
- No runtime gameplay or animation behavior is changed.

## Validation

- Record current controller GUIDs before editing and compare afterward.
- Compile in Edit Mode and inspect Console.
- Inspect the menu/guard path statically; do not invoke a destructive regenerate action.
- Verify the Scene remains not dirty unless an explicit Scene change was truly required.
- Do not enter Play Mode.

## Report Format

Report the previous destructive path, chosen guard, behavior when assets already exist, setup-helper wording/scope, files changed, controller GUID/reference verification, compilation/Console result, and any remaining manual setup caveats.

