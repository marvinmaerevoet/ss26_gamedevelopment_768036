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

# Task 05 — Mission Outcome and Interaction Rules

## Goal

Prevent contradictory Delivery and Arrest outcomes, and prevent carrying interactions while the Player is arrested, using small explicit gameplay rules.

## Current Problem

Delivery can be triggered by `OnTriggerEnter`/`OnTriggerStay` while an Arrest has already committed. The Arrest sequence can later reset the same crate while Delivery remains marked complete. Conversely, a fully delivered crate must not later become a smuggling reason. The Carry controller also continues processing `E` while Player movement is arrested/locked.

## Files / Areas To Inspect

- `Gameplay/Mission/DemoDeliveryZone.cs`
- `Gameplay/Carry/DemoPlayerCarryController.cs`
- `Gameplay/Carry/DemoCarryable.cs`
- `Gameplay/Player/DemoPlayerState.cs`
- `Gameplay/Arrest/DemoArrestSequence.cs`
- shared Arrest lifecycle from Task 02
- `Police/PoliceAIContext.cs` suspicion logic
- Success, Arrest, and Release notification scripts
- MissionCrate, DeliveryZone, DeliveredCrateAnchor, Arrest Crate Spawn, and Player Scene wiring

## Required Changes

Implement the following explicit priority rules:

1. Once Arrest is committed, Delivery must no longer complete.
2. Once Delivery is fully complete, the delivered crate must not create a new smuggling Arrest.
3. During `DemoPlayerState.IsArrested == true`, `E` must perform neither Pickup nor Drop.
4. Existing normal Pickup, Drop, Delivery placement, Delivery event, Success UI, Arrest sequence, crate reset, and demo reset remain intact.

Use direct shared state/reference checks. If one small read-only property is missing, add it to the natural owner. Do not create a new mission framework merely to express this priority.

## Explicit Non-Goals

- Do not redesign UI, Fade, Jail, Carry physics, or Delivery placement.
- Do not add a global input-lock service.
- Do not introduce a large Mission State Machine, event bus, or generalized outcome manager.
- Do not change Sheriff decision priorities beyond enforcing the shared outcome rule.

## Acceptance Criteria

- Arrest committed first prevents Delivery success.
- Delivery completed first prevents later crate-based suspicion/arrest for that delivered crate.
- Success and Arrest cannot both be committed for the same mission attempt.
- Pickup and Drop are blocked while arrested.
- Normal interaction resumes after release and after demo reset.
- Demo reset clears Delivery and Arrest state and restores a pickup-capable MissionCrate.

## Validation

- Compile in Edit Mode and inspect Console.
- Statically trace Arrest-first, Delivery-first, same-frame callback ordering, release, and reset.
- Verify serialized references through Unity MCP if changed.
- Do not enter Play Mode.

## Report Format

Report the priority rule, state owner used for each guard, Carry input guard, changed files/Scene references, reset behavior, compile/Console result, and a concise manual matrix covering Arrest-first, Delivery-first, interaction lock, and replay after reset.

