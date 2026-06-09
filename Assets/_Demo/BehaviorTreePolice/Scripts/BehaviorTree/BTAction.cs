using System;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTAction : BTNode
    {
        private readonly Func<BTStatus> action;

        public BTAction(string name, Func<BTStatus> action) : base(name)
        {
            this.action = action;
        }

        protected override BTStatus OnTick()
        {
            return action?.Invoke() ?? BTStatus.Failure;
        }
    }
}
