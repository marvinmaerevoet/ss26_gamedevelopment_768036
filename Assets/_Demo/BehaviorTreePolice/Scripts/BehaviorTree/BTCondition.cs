using System;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTCondition : BTNode
    {
        private readonly Func<bool> condition;

        public BTCondition(string name, Func<bool> condition) : base(name)
        {
            this.condition = condition;
        }

        protected override BTStatus OnTick()
        {
            return condition != null && condition.Invoke()
                ? BTStatus.Success
                : BTStatus.Failure;
        }
    }
}
