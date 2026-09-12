using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerSuspicious", story: "IsPlayerSuspicious", category: "Conditions", id: "153bde598978bac6006db87e87553b7e")]
public partial class IsPlayerSuspiciousCondition : Condition
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
