using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTParallel : BTComposite
    {
        private readonly int? requiredChildIndex;

        public BTParallel(string name, IEnumerable<BTNode> children, int? requiredChildIndex = null)
            : base(name, children)
        {
            this.requiredChildIndex = requiredChildIndex;
        }

        protected override BTStatus OnTick()
        {
            if (ChildNodes.Count == 0)
            {
                return BTStatus.Success;
            }

            if (requiredChildIndex.HasValue &&
                (requiredChildIndex.Value < 0 || requiredChildIndex.Value >= ChildNodes.Count))
            {
                return BTStatus.Failure;
            }

            bool allSucceeded = true;
            BTStatus requiredStatus = BTStatus.Running;

            for (int i = 0; i < ChildNodes.Count; i++)
            {
                BTStatus childStatus = ChildNodes[i].Tick();

                if (requiredChildIndex.HasValue && i == requiredChildIndex.Value)
                {
                    requiredStatus = childStatus;
                    continue;
                }

                if (!requiredChildIndex.HasValue && childStatus == BTStatus.Failure)
                {
                    return BTStatus.Failure;
                }

                if (!requiredChildIndex.HasValue && childStatus == BTStatus.Running)
                {
                    allSucceeded = false;
                }
            }

            if (requiredChildIndex.HasValue)
            {
                return requiredStatus;
            }

            return allSucceeded ? BTStatus.Success : BTStatus.Running;
        }
    }
}
