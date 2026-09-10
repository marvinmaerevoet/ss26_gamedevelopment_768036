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

# Task 02 — Shared Arrest Lifecycle

## Goal

Move the complete gameplay-level Sheriff Arrest lifecycle behind a BT-neutral shared Police API. A future decision adapter should start Arrest, query neutral progress, and translate that progress to its own task status without reimplementing Arrest mechanics.

## Current Problem

Arrest ownership is split between `PoliceAIContext`, `A_ArrestPlayer`, `DemoPlayerState`, `DemoArrestSequence`, and demo reset. The Context owns distance correction and latch fields, while the Custom action owns approach/commit flags, writes `IsArrested`, waits during the external sequence, and decides when shared cleanup occurs. Replacing the Custom tree would therefore require copying important gameplay semantics.

## Files / Areas To Inspect

- `Police/PoliceAIContext.cs`
- `Police/PoliceBlackboard.cs`
- `Police/PoliceMovementStatus.cs`
- `Police/PoliceDecisionController.cs`
- `AI/CustomApproach/Nodes/A_ArrestPlayer.cs`
- Arrest/Chase conditions and relevant Runner tree construction
- `Gameplay/Player/DemoPlayerState.cs`
- `Gameplay/Arrest/DemoArrestSequence.cs`
- `Gameplay/Setup/DemoReset.cs`
- `Animation/BasicAnimationDriver.cs`
- Sheriff and Player Animator Controllers, read-only unless a necessary parameter mismatch is proven
- all four Sheriff component/reference sets in `BehaviorTreeDemo.unity`

## Required Changes

Create the smallest neutral Arrest lifecycle that owns:

- begin,
- approach progress,
- valid NavMesh stand position,
- commit,
- hold while the external Arrest/Jail sequence is active,
- release detection,
- cancel,
- full reset.

Expose a neutral result/status sufficient for adapters to map to Running/Success/Failure. Do not expose `BTStatus` or Custom-BT node types from shared code.

Keep these current behaviors:

- horizontal target band of 1.4 m ± 0.15 m,
- too-near and too-far correction,
- no second Sheriff taking ownership after another has committed,
- Sheriff stop and face Player before commit,
- Player `IsArrested` set only at commit,
- PointHand and HandsOnHips presentation,
- external Fade/Jail/Release presentation,
- committed Arrest not cancelled merely because visibility or carried-crate suspicion changes.

Reduce `A_ArrestPlayer` to a thin adapter that calls the shared lifecycle and maps its neutral status. Make shared reset clear every Arrest latch and approach field even if no Custom tree exists.

## Explicit Non-Goals

- Do not place Fade, UI, Jail teleport, crate reset, or release notification inside a BT node.
- Do not create a general-purpose state-machine framework.
- Do not alter Sheriff detection, pursuit eligibility, animations, distances, or timings.
- Do not implement another decision system.

## Acceptance Criteria

- Shared Police code contains no `BTStatus`, `BTNode`, or Custom-runner dependency.
- `A_ArrestPlayer` contains no duplicate approach/commit/hold implementation.
- Only the owning Sheriff has an active local Arrest lifecycle.
- Commit sets Player Arrest exactly once and only after correct positioning.
- The shared lifecycle holds until the existing external sequence releases the Player.
- Cancel and reset are idempotent and clear all shared Arrest state.
- Full demo reset does not rely on a Custom-node side effect to clear the latch.
- Existing presentation flow remains unchanged.

## Validation

- Compile in Edit Mode and inspect Console.
- Inspect all four Sheriff references after any serialized API changes.
- Search shared folders for Custom-BT type references.
- Statically trace begin → approach → commit → hold → release and all cancel/reset exits.
- Do not run Play Mode.

## Report Format

Report the new neutral lifecycle API, state owner, status mapping, exact `A_ArrestPlayer` responsibilities, reset behavior, changed Scene references if any, compile/Console result, and manual tests for approach, multi-Sheriff ownership, release, and reset.

