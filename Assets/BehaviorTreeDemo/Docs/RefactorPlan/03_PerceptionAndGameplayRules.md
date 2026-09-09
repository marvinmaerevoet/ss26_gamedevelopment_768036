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

# Task 03 — Perception and Gameplay Rules

## Goal

Give all five decision layers exactly the same perception snapshot and mission rules. Perception and suspicion must remain shared gameplay behavior rather than graph-specific logic.

## Current Problem

All four Sheriffs currently have an empty `obstacleMask`, so the line-of-sight raycast cannot identify blocking world geometry. The Custom Runner refreshes perception every frame while nodes may refresh it again. Chase can remain active after the MissionCrate is dropped because a memory Sequence skips its earlier suspicion condition. LastKnownPosition is currently updated for any visible Player, including a non-suspicious Player.

## Files / Areas To Inspect

- `Police/PoliceAIContext.cs`
- `Police/PoliceBlackboard.cs`
- all perception Conditions under `AI/CustomApproach/Nodes/`
- `PoliceNodeSupport.cs`
- `A_ChasePlayer.cs`
- `PoliceBehaviorTreeRunner.cs`
- `Gameplay/Carry/DemoPlayerCarryController.cs`
- `Gameplay/Carry/DemoCarryable.cs`
- `Gameplay/Player/DemoPlayerState.cs`
- `Gameplay/Player/RestrictedAreaTrigger.cs`
- Player, MissionCrate, EyePoint, four PoliceAIContexts, layers, and world colliders in the Scene

## Required Changes

### Obstacle mask

Configure the same meaningful obstacle mask for all four Sheriffs. Buildings and relevant world geometry must block vision. Ensure the Player and the observing Sheriff's own colliders do not accidentally invalidate every raycast. Preserve view distance 12 m and view angle 90°.

### Perception update contract

Provide one explicit shared refresh/snapshot per decision cycle. Conditions read the prepared snapshot and do not silently calculate different input at different points in one tree tick. A future decision adapter must be able to invoke the same shared update without reproducing rules.

### Suspicion and Chase after drop

Suspicion is true only while the Player carries the exact configured MissionCrate. Running, Restricted Area, ordinary movement, and unrelated carryables must not cause suspicion. If the Player drops the MissionCrate during Chase and Arrest is not committed, the smuggling Chase must end. A committed Arrest continues.

### LastKnownPosition

Record LastKnownPosition only from a relevant suspicious/pursuit perception. Do not create an Investigation trail from an innocent visible Player. Keep the position and validity flag in shared Police state.

## Explicit Non-Goals

- Do not redesign FOV geometry or add hearing, memory decay, suspicion meters, or sensor frameworks.
- Do not change Arrest approach behavior or presentation.
- Do not remove `IsInRestrictedArea`; it remains a general gameplay state but is not a smuggling suspicion source.
- Do not add point reservations or squad coordination.

## Acceptance Criteria

- All four Sheriffs use identical, non-empty, intentional obstacle configuration.
- A wall in the configured obstacle layers blocks the shared visibility result.
- The decision layer consumes one stable perception result per decision cycle.
- Running without the MissionCrate is not suspicious.
- Restricted Area alone is not suspicious.
- Carrying the exact MissionCrate is suspicious while standing, walking, or running.
- Dropping it ends uncommitted Chase.
- A committed Arrest is not reversed by dropping it.
- LastKnownPosition represents a relevant suspicious/pursuit observation.
- Custom Conditions remain thin readers/adapters.

## Validation

- Compile in Edit Mode and inspect Console.
- Use Unity MCP to verify all four masks and MissionCrate references.
- Search for remaining Running/Restricted-Area suspicion checks.
- Trace a full decision cycle statically to confirm a single refresh contract.
- Do not enter Play Mode.

## Report Format

Report the final obstacle configuration, perception update owner, exact suspicion expression, Chase-drop behavior, LastKnownPosition rule, files/Scene objects changed, compile/Console result, and manual visibility/Chase tests.

