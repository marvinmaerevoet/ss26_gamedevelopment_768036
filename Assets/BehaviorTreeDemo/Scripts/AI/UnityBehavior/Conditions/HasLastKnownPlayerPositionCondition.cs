using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasLastKnownPlayerPosition", story: "HasLastKnownPlayerPosition", category: "Conditions", id: "12af2233d21478c5be40dc775e12234f")]
public partial class HasLastKnownPlayerPositionCondition : Condition
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
