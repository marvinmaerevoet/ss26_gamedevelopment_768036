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

# Task 08 — Scene and Environment Cleanup

## Goal

Clean up confirmed Scene/environment integration issues using only project-owned Scene overrides or assets, while preserving vendor packages and all world transforms.

## Current Problem

The Scene contains three active MeshColliders with missing mesh references: one on `Demo/SM_Bld_Jail_01` and one on each `SM_Bld_Double_Roof_01` occurrence. The Jail also has a valid collision mesh, so the missing duplicate must be assessed before removal. Vendor Rope and Trainstation objects contain missing Avatar references but have no proven project-owned runtime impact. Additional questions concern Sheriff prefab consistency, root hierarchy, camera collision mask, Spot Light shadows, and the usefulness of the existing Global Volume.

## Files / Areas To Inspect

- `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`
- `Demo/SM_Bld_Jail_01`
- both `Demo/.../SM_Bld_Double_Roof_01` objects
- their source Synty prefabs and model assets, read-only
- `Demo/SM_Prop_Rope_03` instances and Trainstation platform, read-only
- Player camera component and collision mask
- all four Sheriff roots, components, lanterns, Vision lights, and prefab links
- `Global Volume`, profile, camera, and active render pipeline
- root hierarchy, spawn markers, patrol markers, systems, and UI roots

## Required Changes

### Missing colliders

Determine whether each missing collider is redundant or needs a project-owned replacement. Use only Scene overrides or project-owned collider assets. Never modify Synty prefabs/models. Preserve valid sibling colliders.

### Sheriff consistency

Assess whether a neutral project-owned Sheriff base prefab would materially reduce configuration drift. Create one only if it is a small, safe operation and does not entangle a concrete decision adapter. Otherwise document `Leave as-is` and provide a validation checklist.

### Hierarchy

Consider moderate groups such as `Actors`, `MissionMarkers`, `PoliceMarkers`, `Systems`, and `UI`. Reparent only when World position/rotation/scale and serialized reset assumptions are guaranteed to remain unchanged. Cosmetic grouping may be skipped.

### Camera, lights, and volume

If camera collision is an intended active feature, configure its mask meaningfully. Change Vision Spot Light shadows only with a demonstrated visual/technical reason. Determine whether Global Volume has an effective consumer in the Built-in pipeline before keeping or removing it.

Document vendor missing Avatars unless a real project-owned runtime impact is demonstrated; do not repair vendor assets.

## Explicit Non-Goals

- Do not alter Third-Party assets or prefabs.
- Do not rebake NavMesh unless an actual correction requires it and scope is separately approved.
- Do not change world transforms, gameplay positions, lighting style, or AI logic.
- Do not force hierarchy or prefab work that offers only cosmetic value.

## Acceptance Criteria

- Every changed collider is either a valid project-owned replacement or a knowingly removed redundant Scene override.
- No vendor asset is changed.
- Valid Jail collision is preserved.
- All four Sheriffs retain their local components, references, lights, animations, and start transforms.
- Any hierarchy change preserves exact world transforms and reset behavior.
- Camera mask, light shadows, and Global Volume are changed only after a concrete finding.
- Missing vendor Avatar references are documented and left alone unless proven relevant.

## Validation

- Record relevant GlobalObjectIds, transforms, prefab links, and references before changes.
- Save the Scene only if Scene changes were made.
- Compile in Edit Mode and inspect Console.
- Re-scan for Missing Scripts, missing owned references, null material slots, and affected collider meshes.
- Do not enter Play Mode.

## Report Format

Report each investigated object and decision, exact Scene overrides/assets changed, Sheriff prefab decision, hierarchy decision, camera/light/volume findings, vendor references left untouched, Scene dirty/save state, and compilation/Console result with manual collision/visual tests.

