using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "isOfficerHealthLow", story: "If Health lower than [x]", category: "Flow/Conditional", id: "bdc44140f1f5c339efc04c270cf6d4fa")]
public partial class IsOfficerHealthLowModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<int> X;

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

