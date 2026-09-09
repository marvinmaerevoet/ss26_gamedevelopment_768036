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

# Behavior Tree Demo Refactor Plan

## Goal

Prepare the existing Western/police demo so that the same game can be implemented and compared with five decision-layer approaches:

1. CustomApproach
2. GitAmend
3. UnityBehavior
4. BehaviorDesigner
5. GameCreatorBehavior

The intended dependency direction is:

```text
BehaviorTreeDemo / Gameplay
             ↑
Shared Police API
             ↑
PoliceDecisionController
             ↑
CustomApproach / GitAmend / UnityBehavior / BehaviorDesigner / GameCreatorBehavior
```

The game, mission, player, presentation, perception, and shared police mechanics must not depend on a concrete decision layer. Each approach should consume the same inputs and use the same shared actions.

## Current Problem

The project is already largely separated by type, but behavior ownership still crosses boundaries. Arrest lifecycle, branch cancellation, perception cadence, investigation presentation, mission outcome rules, and reset cleanup must be made deterministic before additional adapters are compared. Editor tools and documentation also contain stale assumptions that could overwrite or misrepresent the current project state.

The tasks below are deliberately separated. Execute only one task at a time. Do not combine adjacent cleanup work simply because the same file is open.

## Files / Areas To Inspect

Before each task, inspect only the files and Scene objects listed in that task. Common reference areas are:

- `Assets/BehaviorTreeDemo/Scripts/Police/`
- `Assets/BehaviorTreeDemo/Scripts/Gameplay/`
- `Assets/BehaviorTreeDemo/Scripts/Animation/`
- `Assets/BehaviorTreeDemo/Scripts/UI/`
- `Assets/BehaviorTreeDemo/Scripts/AI/CustomApproach/`
- `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`
- `Assets/BehaviorTreeDemo/Docs/`

## Required Changes

Execute the tasks in order. Each task is a standalone Codex assignment and must have its own implementation review, Edit Mode validation, diff review, commit, and later manual playtest.

| Task | Topic | Status | Commit | Manual Playtest |
|---|---|---|---|---|
| [01](01_SelectorAndCancelOwnership.md) | Selector and cancel ownership | DONE | THIS COMMIT | PENDING |
| [02](02_SharedArrestLifecycle.md) | Shared arrest lifecycle | DONE | THIS COMMIT | PENDING |
| [03](03_PerceptionAndGameplayRules.md) | Perception and gameplay rules | DONE | THIS COMMIT | PENDING |
| [04](04_SharedFacingAndInvestigation.md) | Shared facing and investigation | DONE | THIS COMMIT | PENDING |
| [05](05_MissionOutcomeAndInteractionRules.md) | Mission outcome and interaction rules | DONE | THIS COMMIT | PENDING |
| [06](06_AnimatorAndSetupSafety.md) | Animator and setup safety | DONE | THIS COMMIT | PENDING |
| [07](07_MovementLifecycleAndResetCleanup.md) | Movement lifecycle and reset cleanup | PENDING | — | NOT RUN |
| [08](08_SceneAndEnvironmentCleanup.md) | Scene and environment cleanup | PENDING | — | NOT RUN |
| [09](09_NamingStructureAndDeadCode.md) | Naming, structure, and dead code | PENDING | — | NOT RUN |
| [10](10_DocumentationAndFinalReplaceabilityAudit.md) | Documentation and final replaceability audit | PENDING | — | NOT RUN |

Workflow after each future implementation task:

1. Perform only that task.
2. Run Edit Mode compilation and inspect the Console.
3. Stop immediately if compilation errors occur. Do not continue with later tasks.
4. Review `git diff` and verify that the change stayed in scope.
5. Create one dedicated commit for the completed task.
6. Record the commit hash in this table.
7. Let the user perform the manual Play Mode test.
8. Record the manual result before approving the next risky task.

If fully automated processing is requested later, it must still use stop-on-error behavior and one commit per task. Automatic processing does not authorize Play Mode or runtime tests.

## Explicit Non-Goals

- Do not implement GitAmend, UnityBehavior, BehaviorDesigner, or GameCreatorBehavior in this plan task.
- Do not refactor code while writing or updating the plan.
- Do not introduce a generalized architecture beyond the concrete needs described by Tasks 01–10.
- Do not treat naming cleanup as more important than behavioral parity.
- Do not use this document as authorization to alter vendor packages.

## Acceptance Criteria

- Tasks 01–10 remain individually executable and independently reviewable.
- The order prioritizes behavior correctness and replaceability before cosmetic cleanup.
- Every implementation task has a place for its commit and manual playtest result.
- A compilation failure blocks automatic continuation.
- The final target remains a shared game with replaceable decision adapters.

## Validation

For this planning-only task, verify that all eleven Markdown documents exist and that no gameplay, Scene, asset, package, or C# file was changed. Compilation is not required for Markdown-only changes. Do not enter Play Mode.

For future implementation tasks, validation means Edit Mode compilation, Console inspection, and scoped diff review. Runtime verification is always a separate manual user step.

## Report Format

For each completed implementation task, report:

1. Exact files and Scene objects changed.
2. The behavior or ownership problem that was resolved.
3. Why the chosen solution is the smallest robust solution.
4. Edit Mode compilation and Console result.
5. Git diff scope and commit hash.
6. Remaining uncertainty and the precise manual Play Mode checks for the user.
