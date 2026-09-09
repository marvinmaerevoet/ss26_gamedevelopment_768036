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

# Task 10 — Documentation and Final Replaceability Audit

## Goal

Update project documentation to the implemented shared behavior contract and perform the final read-only replaceability audit before adding further decision systems.

## Current Problem

README and Custom-tree documentation still describe obsolete suspicion rules, old Blackboard debug fields, old paths, and setup capabilities. Without a shared feature-parity contract, later approaches may reproduce different gameplay rather than different decision implementations. Build/repository details and adapter-deletion consequences also need a final explicit check.

## Files / Areas To Inspect

- `Assets/BehaviorTreeDemo/README.md`
- all files under `Assets/BehaviorTreeDemo/Docs/`
- completed Task 01–09 diffs and commits
- all shared Police, Gameplay, Animation, UI, and Reset APIs
- `AI/CustomApproach/` runtime, nodes, runner, debug, and setup
- `BehaviorTreeDemo.unity`, Build Settings, Project Version, Packages, Git status
- Scene references to all decision, setup, and debug components
- own code references from shared areas to each decision adapter

## Required Changes

### Documentation

Remove obsolete statements including Running/Restricted Area suspicion, old Blackboard debug fields, old paths, and outdated setup claims. Document actual setup, ownership, Scene path, reset, manual testing, and known vendor integration notes.

### Feature-parity contract

Document these shared requirements for all five adapters:

- Perception: 12 m, 90°, same obstacle mask and line-of-sight rule.
- Suspicion: exclusively carrying the configured MissionCrate.
- Patrol: random valid point; avoid immediate repetition when alternatives exist.
- Movement: Walk 3, Run 6.
- Chase: visible and suspicious; dropping the crate ends uncommitted smuggling Chase.
- LastKnownPosition: last relevant position of a suspicious/pursued Player.
- Investigate: Walk to LKP, then approximately two seconds of LookAround.
- Arrest: highest relevant normal priority, 1.4 m ± 0.15 m, PointHand, Player HandsOnHips, neutral commit/hold/release lifecycle.
- Reset: shared state resets independently; the adapter resets only its graph/tree state.
- Multi-Sheriff: four local Contexts and no cross-writing of local state.
- Presentation: identical animation and UI independent of adapter.

### Final replaceability audit

Answer explicitly what happens if `Scripts/AI/CustomApproach/` is removed and replaced:

- which shared files, if any, fail to compile,
- which Scene references need adapter rewiring,
- which gameplay logic still exists only in Custom code,
- whether any Shared → Custom type dependency remains,
- whether Reset and presentation work through neutral contracts.

Assign a new `Behavior Tree Replaceability: X / 10` score with concrete evidence.

### Test recommendations

Document a small future test list without implementing it unless separately requested:

- branch switch cannot stop newly selected movement,
- reset of a never-started Arrest cannot affect foreign movement,
- Arrest cancel clears flags,
- Delivery and Arrest cannot both succeed,
- Drop ends Chase according to contract,
- all adapters receive identical perception inputs,
- Patrol handles 0/1/n valid points,
- each adapter fulfills neutral reset behavior.

### Build and repository closeout

Check whether `BehaviorTreeDemo.unity` is in Build Settings, whether Recovery/temp files are unintended, whether the local vendor integration patch is documented, whether the Unity version is documented, and whether any Missing Scripts, broken project-owned references, compile errors, or relevant warnings remain.

## Explicit Non-Goals

- Do not implement another Behavior Tree approach.
- Do not fix new audit findings automatically; document and prioritize them.
- Do not add a large test suite, CI system, or build pipeline without a separate request.
- Do not modify Third-Party content or count the known GameCreator SerializeReference warning as a project-owned defect.

## Acceptance Criteria

- Documentation matches actual implemented rules and paths.
- One neutral feature-parity contract can be handed to every adapter implementation.
- Shared → Custom source and serialized dependencies are explicitly enumerated.
- Remaining Custom-only gameplay logic is identified precisely.
- Replaceability receives an evidence-based score.
- Build/Repository/Scene health is reported without speculative fixes.
- The plan table records completed commit hashes and manual playtest statuses.

## Validation

- This task's audit portion is read-only.
- For documentation edits, no compilation is needed unless code/assets also changed unexpectedly; if that occurs, stop and report scope drift.
- Inspect Edit Mode Console, Scene dirty state, Git status/diff, Build Settings, Missing Scripts, GUID-backed references, and adapter cross-references.
- Do not enter Play Mode or run runtime tests.

## Report Format

Report updated documentation, final parity contract location, Shared → Custom dependency results, Scene rewiring requirements, remaining Custom-only mechanics, final replaceability score, Build/repository findings, Console/Scene status, recommended small tests, and any blockers before implementing the second adapter.

