using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToPatrolPointStrategy", story: "Execute MoveToPatrolPointStrategy", category: "Action", id: "b049b168027767be30e809160de0abfd")]
public partial class MoveToPatrolPointStrategyAction : Action
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

