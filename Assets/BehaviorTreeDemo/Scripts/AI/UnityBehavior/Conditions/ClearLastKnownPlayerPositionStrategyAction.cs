using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearLastKnownPlayerPositionStrategy", story: "Execute ClearLastKnownPlayerPositionStrategy", category: "Action", id: "87ae27bb6b5f58eb78bbe8e350906cfe")]
public partial class ClearLastKnownPlayerPositionStrategyAction : Action
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

