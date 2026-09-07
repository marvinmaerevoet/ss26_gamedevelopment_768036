using System;

namespace CustomApproachDemo.BehaviorTree
{
    public class BTAction : BTNode
    {
        private readonly Func<BTStatus> action;

        protected BTAction(string name) : base(name)
        {
        }

        public BTAction(string name, Func<BTStatus> action) : base(name)
        {
            this.action = action;
        }

        protected override BTStatus OnTick()
        {
            return Execute();
        }

        protected virtual BTStatus Execute()
        {
            return action?.Invoke() ?? BTStatus.Failure;
        }
    }
}
