# Scene Integration Notes

These findings apply to `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`.

## Project-owned Scene cleanup

- Removed the null duplicate `MeshCollider` override from `Demo/SM_Bld_Jail_01`. Its valid `SM_Bld_Jail_01_Collision` collider remains.
- Removed the null inherited `MeshCollider` components from both `SM_Bld_Double_Roof_01` Scene instances. Each containing `SM_Bld_Large_01` retains its valid building collision mesh and child colliders.
- Enabled the existing third-person camera collision feature for normal raycast layers. The camera ignores colliders below its Player target.
- Removed the ineffective `Global Volume` Scene object. The project uses the Built-in Render Pipeline, the camera has no volume/post-processing consumer, and the referenced Synty profile was therefore not applied.
- Removed the obsolete `Restricted Area` trigger, its project-owned component, and its unused Player state. Suspicion remains exclusively tied to carrying the configured MissionCrate.
- Removed the obsolete `Custom Approach Setup` Scene object and its editor-only bootstrap component. The finished Scene is fully wired without it.

## Deliberately unchanged

- The three null Avatar references on the two Synty rope objects and the train-station platform remain vendor-owned and have no demonstrated project runtime effect.
- Vision Spot Light shadows remain disabled. There is no demonstrated need to add four shadow-casting lights.
- Project-owned roots are grouped under `Actors`, `Mission`, `Markers`, `Systems`, and `UI`; world transforms and serialized references are preserved.
- No neutral Sheriff prefab was created. The original Sheriff is a Synty prefab instance and the three copies are Scene objects. Each Scene Sheriff now owns a neutral host plus both concrete adapters without changing Vendor prefabs.

## Sheriff consistency checklist

For `Sheriff 01`, `Sheriff 02`, `Sheriff 03`, and `Sheriff 04`, verify:

- one local `NavMeshAgent`, `PoliceBlackboard`, `PoliceAIContext`, `PoliceDecisionHost`, `PoliceBehaviorTreeRunner`, `GitAmendPoliceDecisionController`, `PoliceAIGizmos`, `BasicAnimationDriver`, and Animator;
- two local host slots; both concrete adapters are serialized disabled and the earlier-running host enables only the selected one at Scene start;
- local `EyePoint`, lantern glow, `Vision Light Origin`, and synchronized Spot Light;
- own component references remain on the same Sheriff;
- shared Player, MissionCrate, patrol points, and safe point references are intentional;
- view distance/angle remain 12/90 and the obstacle mask uses normal raycast layers;
- `Sheriff.controller` remains assigned.

## Dual-approach integration

- `Systems/Behavior Tree Approach Switcher` maps `F1` to CustomApproach and `F2` to GitAmend, then reloads the active Build Scene.
- `Demo Reset` references the four `PoliceDecisionHost` components and therefore resets only the active adapter without knowing its concrete type.
- `UI/Custom Approach Behavior Tree Debug UI` and `UI/GitAmend Behaviour Tree Debug UI` are mutually exclusive. Both use `1` to `4` for Sheriff selection.
- `UI/Notifications UI/Approach Indicator` shows the controls and current selection independently of either adapter UI.
- `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` is the sole enabled Build Settings entry.
