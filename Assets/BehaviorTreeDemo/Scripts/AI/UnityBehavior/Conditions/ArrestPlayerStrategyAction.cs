using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ArrestPlayerStrategy", story: "Execute ArrestPlayerStrategy", category: "Action", id: "2f30d5114178f966a73e4f8a5f4e784a")]
public partial class ArrestPlayerStrategyAction : Action
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

