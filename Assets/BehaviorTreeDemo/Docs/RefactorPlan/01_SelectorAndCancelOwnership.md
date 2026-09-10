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

# Task 01 — Selector and Cancel Ownership

## Goal

Remove destructive reset/cancel side effects between the reactive Custom Behavior Tree and active police movement. A node may cancel only an operation it actually started and still owns.

## Current Problem

The reactive root `BTSelector` can tick a newly selected higher-priority branch and then reset lower-priority branches. `A_ArrestPlayer.Reset()` currently delegates to arrest cleanup that can stop the `NavMeshAgent` and clear its path even if that Arrest node never started or no longer owns movement. A stale Arrest reset can therefore erase Emergency, Chase, Investigate, or Patrol movement that was just established.

The analysis must distinguish:

- a never-started Arrest node,
- an active Arrest approach,
- a committed Arrest hold,
- an already-finished or previously reset node,
- a branch switch where another action has already taken movement ownership,
- a complete tree/demo reset where active Arrest state must still be cleared.

## Files / Areas To Inspect

Read the relevant implementations completely before editing:

- `AI/CustomApproach/Runtime/BTNode.cs`
- `AI/CustomApproach/Runtime/BTSelector.cs`
- `AI/CustomApproach/Runtime/BTSequence.cs`
- `AI/CustomApproach/Runtime/BTComposite.cs`
- relevant decorators that propagate `Reset()`
- `AI/CustomApproach/Nodes/A_ArrestPlayer.cs`
- `AI/CustomApproach/Nodes/PoliceMovementAction.cs`
- Emergency/Flee, Chase, Investigate, Patrol, LookAt, and LookAround actions
- `Police/PoliceAIContext.cs`, especially arrest reset/cancel and movement methods
- `AI/CustomApproach/PoliceBehaviorTreeRunner.cs`
- `Gameplay/Setup/DemoReset.cs`

List every `Reset()` override that can affect movement, `CurrentBehaviorMode`, arrest flags, or last-known-position state. Confirm the exact tick/reset order in the reactive selector.

## Required Changes

- Make Arrest reset conditional on clear local ownership/state.
- Prefer existing local fields such as `approachStarted` and `arrestCommitted`; add at most a small explicit ownership flag if the existing fields are insufficient.
- Ensure an active Arrest can still cancel its own approach and clear its own latch.
- Ensure a never-started or already-reset Arrest node performs no global movement cleanup.
- Make repeated `Reset()` calls idempotent.
- Preserve the complete demo-reset behavior.
- Inspect the selector order carefully. Change `BTSelector` only if ownership checks cannot robustly meet the acceptance criteria. If changed, preserve reactive priority and avoid changes to Sequence/Decorator semantics.
- Apply the same minimal guard to another action only if it has the identical foreign-resource reset problem.

## Explicit Non-Goals

- Do not move the full Arrest lifecycle into shared code; that is Task 02.
- Do not introduce movement commands, task handles, a scheduler, or a general resource-arbitration framework.
- Do not change Arrest distance, animation, Jail/Fade/Release flow, Patrol, Chase, Investigate, or Emergency decisions.
- Do not redesign all BT reset semantics.

## Acceptance Criteria

- Emergency movement is not erased by a reset of a lower Arrest branch.
- Chase movement is not stopped by resetting a never-started Arrest node.
- A genuinely active Arrest approach can be cancelled and its local state is cleared.
- Repeated reset produces no additional global side effects.
- A full Custom-BT/demo reset still clears active Arrest state.
- No new movement-ownership framework was introduced.
- Existing Arrest behavior remains 1.4 m ± 0.15 m with the current animation and release sequence.

## Validation

- Compile in Edit Mode.
- Inspect Console errors and warnings without clearing unrelated messages.
- Perform a static call-order review for each acceptance case.
- Review the diff for changes outside the smallest affected Custom-BT/Police files.
- Do not run Play Mode or runtime tests.

## Report Format

Report:

1. Exact root cause and selector call order.
2. Whether `BTSelector` changed and why.
3. Which ownership fields govern cleanup.
4. Other nodes inspected and whether they had the same issue.
5. Why old branches can no longer stop new movement.
6. Why full reset still works.
7. Changed files.
8. Compilation and Console result.
9. Manual Play Mode cases the user should test.

