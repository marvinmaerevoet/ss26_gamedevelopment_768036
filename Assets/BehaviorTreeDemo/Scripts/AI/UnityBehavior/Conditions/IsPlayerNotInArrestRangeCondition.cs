using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerNotInArrestRange", story: "IsPlayerNotInArrestRange", category: "Conditions", id: "089b93bc7966f74d761ce06ad8983035")]
public partial class IsPlayerNotInArrestRangeCondition : Condition
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
