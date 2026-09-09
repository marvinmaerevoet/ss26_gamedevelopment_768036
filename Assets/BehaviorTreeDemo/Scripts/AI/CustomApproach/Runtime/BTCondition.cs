using System;

namespace CustomApproachDemo.BehaviorTree
{
    public class BTCondition : BTNode
    {
        private readonly Func<bool> condition;

        protected BTCondition(string name) : base(name)
        {
        }

        public BTCondition(string name, Func<bool> condition) : base(name)
        {
            this.condition = condition;
        }

        protected override BTStatus OnTick()
        {
            return Evaluate() ? BTStatus.Success : BTStatus.Failure;
        }

        protected virtual bool Evaluate()
        {
            return condition != null && condition.Invoke();
        }
    }
}
