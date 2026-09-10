# Scene Integration Notes

These findings apply to `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`.

## Project-owned Scene cleanup

- Removed the null duplicate `MeshCollider` override from `Demo/SM_Bld_Jail_01`. Its valid `SM_Bld_Jail_01_Collision` collider remains.
- Removed the null inherited `MeshCollider` components from both `SM_Bld_Double_Roof_01` Scene instances. Each containing `SM_Bld_Large_01` retains its valid building collision mesh and child colliders.
- Enabled the existing third-person camera collision feature for normal raycast layers. The camera ignores colliders below its Player target.
- Removed the ineffective `Global Volume` Scene object. The project uses the Built-in Render Pipeline, the camera has no volume/post-processing consumer, and the referenced Synty profile was therefore not applied.
- Removed the obsolete `Restricted Area` trigger, its project-owned component, and its unused Player state. Suspicion remains exclusively tied to carrying the configured MissionCrate.

## Deliberately unchanged

- The three null Avatar references on the two Synty rope objects and the train-station platform remain vendor-owned and have no demonstrated project runtime effect.
- Vision Spot Light shadows remain disabled. There is no demonstrated need to add four shadow-casting lights.
- Project-owned roots are grouped under `Actors`, `Mission`, `Markers`, `Systems`, and `UI`; world transforms and serialized references are preserved.
- No neutral Sheriff prefab was created. The original Sheriff is a Synty prefab instance, while the three copies are Scene objects containing the concrete Custom Approach runner. Turning that current composition into a shared base prefab would entangle a decision adapter.

## Sheriff consistency checklist

For `Sheriff 01`, `Sheriff 02`, `Sheriff 03`, and `Sheriff 04`, verify:

- one local `NavMeshAgent`, `PoliceBlackboard`, `PoliceAIContext`, `PoliceDecisionController`, `PoliceAIGizmos`, `BasicAnimationDriver`, and Animator;
- local `EyePoint`, lantern glow, `Vision Light Origin`, and synchronized Spot Light;
- own component references remain on the same Sheriff;
- shared Player, MissionCrate, patrol points, and safe point references are intentional;
- view distance/angle remain 12/90 and the obstacle mask uses normal raycast layers;
- `Sheriff.controller` remains assigned.
