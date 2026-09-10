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

# Task 07 — Movement Lifecycle and Reset Cleanup

## Goal

Resolve remaining small lifecycle, movement-result, and reset inconsistencies after the core ownership and Arrest refactors, without introducing a generalized command system.

## Current Problem

`TrySetDestination()` ignores the return value from `NavMeshAgent.SetDestination()`. Arrest approach can remain Running indefinitely when no valid/reachable stand position exists. Decision components and ArrestSequence do not have fully defined disable cleanup. Emergency can inherit a previous Walk/Run mode. Original and additional Sheriffs use separate reset paths. Player speed reporting has an Update-order sensitivity.

## Files / Areas To Inspect

- `Police/PoliceAIContext.cs`
- `Police/PoliceDecisionController.cs`
- shared Arrest implementation from Task 02
- `AI/CustomApproach/PoliceBehaviorTreeRunner.cs`
- all Police movement actions
- `Gameplay/Arrest/DemoArrestSequence.cs`
- `Gameplay/Setup/DemoReset.cs`
- `Gameplay/Player/DemoPlayerState.cs`
- `Gameplay/Player/DemoSimplePlayerController.cs`
- all four Sheriffs' NavMeshAgent/Context/DecisionController wiring

## Required Changes

1. Return the actual success/failure of `NavMeshAgent.SetDestination()` from the shared API.
2. Clearly document and enforce which shared API controls Destination, Stop/ResetPath, Velocity, and Rotation. Keep the API small.
3. Give Arrest approach a defined failure/cancel or bounded timeout path. It must not remain Running forever after repeated inability to produce a usable stand position.
4. Ensure a failed/cancelled Arrest clears its local latch and approach state.
5. Make disable cleanup for DecisionController/Runner and ArrestSequence idempotent and prevent stopped components from leaving active operations or coroutines.
6. Set Emergency movement mode explicitly according to the existing intended behavior; do not inherit a prior mode accidentally.
7. Treat original Sheriff and `Sheriff_01`–`Sheriff_03` through the same neutral reset helper where practical.
8. Stabilize Player movement reports against Script Execution Order with the smallest clear mechanism.

## Explicit Non-Goals

- Do not add task handles, movement commands, a scheduler, or path-request abstractions.
- Do not change Patrol/Chase/Investigate/Arrest decisions.
- Do not tune NavMesh, rebake it, or redesign agent locomotion.
- Do not add a global reset registry unless a concrete need proves the current coordinator unmanageable.

## Acceptance Criteria

- A rejected destination reports failure.
- No Arrest approach can remain Running forever solely because no valid stand point/path exists.
- Failure/cancel/reset clears all relevant local/shared operation state.
- Disabling a Runner or ArrestSequence leaves no live movement/coroutine state.
- Emergency always selects its intended movement speed explicitly.
- All four Sheriffs receive equivalent reset treatment.
- Player CurrentSpeed/IsRunning reporting is deterministic with respect to frame order.
- Existing normal movement speeds and reset destinations remain unchanged.

## Validation

- Compile in Edit Mode and inspect Console.
- Trace success, failure, disable, repeated reset, and missing-reference paths statically.
- Verify all four Sheriff references through Unity MCP after serialized changes.
- Confirm no NavMesh bake or Scene transform changes occurred.
- Do not enter Play Mode.

## Report Format

Report the destination-result fix, movement ownership rule, Arrest failure policy, disable behavior, Emergency mode, unified Sheriff reset behavior, Player speed-report change, files/Scene references changed, and compilation/Console result with manual test cases.

