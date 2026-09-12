using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToSafePointStrategy", story: "Execute MoveToSafePointStrategy", category: "Action", id: "d676f2842545bd2d45dffa2fec1470a8")]
public partial class MoveToSafePointStrategyAction : Action
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

