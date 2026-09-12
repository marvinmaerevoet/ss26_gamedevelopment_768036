using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerVisible", story: "IsPlayerVisible", category: "Conditions", id: "18fb49fb88fd7cca8d86d0a4e6c9fcd5")]
public partial class IsPlayerVisibleCondition : Condition
{

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
