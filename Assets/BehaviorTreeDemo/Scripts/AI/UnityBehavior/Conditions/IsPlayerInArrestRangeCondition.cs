using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerInArrestRange", story: "IsPlayerInArrestRange", category: "Conditions", id: "5144361bf4ac188a729c9b6b22c508b1")]
public partial class IsPlayerInArrestRangeCondition : Condition
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
