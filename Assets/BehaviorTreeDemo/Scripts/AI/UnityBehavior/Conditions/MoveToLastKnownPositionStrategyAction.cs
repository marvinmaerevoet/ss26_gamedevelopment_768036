using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToLastKnownPositionStrategy", story: "Execute MoveToLastKnownPositionStrategy", category: "Action", id: "81833081845cb8491f2a990578fc16e4")]
public partial class MoveToLastKnownPositionStrategyAction : Action
{

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

