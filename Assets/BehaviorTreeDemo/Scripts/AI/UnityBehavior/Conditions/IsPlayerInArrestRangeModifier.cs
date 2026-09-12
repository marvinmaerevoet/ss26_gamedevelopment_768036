using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IsPlayerInArrestRange", category: "Flow/Conditional", id: "19bff6ded0c9d8e3dc7de2bdbd4d7d22")]
public partial class IsPlayerInArrestRangeModifier : Modifier
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

