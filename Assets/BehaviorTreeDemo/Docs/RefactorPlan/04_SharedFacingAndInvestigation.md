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

# Task 04 — Shared Facing and Investigation

## Goal

Make police facing and investigation presentation independent of a concrete decision system's tick frequency. Expose a small shared API and a neutral distinction between moving to investigate and actively looking around.

## Current Problem

`A_LookAtPlayer` and `A_LookAround` directly rotate the Sheriff and multiply by frame `Time.deltaTime` even though they execute on the Custom BT tick interval. A 10 Hz tree and a 60 Hz graph can therefore produce different visible rotation speeds. Animation currently infers active looking-around from `PoliceBehaviorMode.Investigate` plus low agent velocity, which can also mean path pending, blocked, or failed.

## Files / Areas To Inspect

- `AI/CustomApproach/Nodes/A_LookAtPlayer.cs`
- `AI/CustomApproach/Nodes/A_LookAround.cs`
- `AI/CustomApproach/Nodes/A_MoveToLastKnownPosition.cs`
- relevant sequence/timeout construction in `PoliceBehaviorTreeRunner.cs`
- `Police/PoliceAIContext.cs`
- `Police/PoliceBlackboard.cs`
- `Police/PoliceBehaviorMode.cs` or enum declaration location
- `Animation/BasicAnimationDriver.cs`
- `Animations/Sheriff.controller`
- NavMeshAgent `updateRotation` configuration on all Sheriffs

## Required Changes

- Provide shared functions for FacePlayer and direction/rotation progression.
- Make elapsed-time handling explicit and equivalent across adapter tick rates. Either pass decision-cycle elapsed time explicitly or let a shared per-frame presentation/mechanics component own smooth rotation.
- Add only the smallest neutral investigation phase needed to distinguish `MovingToInvestigation` from `Investigating/LookingAround`.
- Ensure the standing Inspect animation is active only during the true looking-around phase.
- Keep Custom nodes as thin status adapters around shared behavior.
- Resolve or deliberately coordinate direct rotation with `NavMeshAgent.updateRotation`.
- Preserve the current approximate two-second LookAround behavior and existing animation clips.

## Explicit Non-Goals

- Do not build a general animation state machine or rotation service framework.
- Do not change perception, Chase, Arrest, Patrol selection, or mission rules.
- Do not replace Animator Controllers or clips.
- Do not add sophisticated scan paths, IK, head tracking, or procedural animation.

## Acceptance Criteria

- Facing speed no longer depends on whether an adapter ticks at 10 Hz or 60 Hz.
- The Custom BT does not contain unique rotation mechanics future adapters must copy.
- Investigation explicitly distinguishes travel from active looking around.
- Path pending, blocked, or failure does not falsely trigger the standing Inspect pose.
- Arrest facing and current animations remain functional by static inspection.
- No unnecessary generalized framework was introduced.

## Validation

- Compile in Edit Mode and inspect Console.
- Statically compare behavior for different decision tick intervals.
- Inspect Sheriff Animator parameters/transitions and all four NavMeshAgent configurations.
- Do not run Play Mode.

## Report Format

Report the shared facing API, time source, neutral investigation phase, node simplification, NavMesh rotation decision, changed files/assets, compile/Console result, and manual tests for Chase facing, Investigate travel, Inspect pose, and Arrest facing.

