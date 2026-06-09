using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTSelector : BTComposite
    {
        private int currentChildIndex;

        public BTSelector(string name, IEnumerable<BTNode> children) : base(name, children)
        {
        }

        protected override BTStatus OnTick()
        {
            while (currentChildIndex < Children.Count)
            {
                BTStatus childStatus = Children[currentChildIndex].Tick();

                if (childStatus == BTStatus.Running)
                {
                    return BTStatus.Running;
                }

                if (childStatus == BTStatus.Success)
                {
                    ResetProgress();
                    return BTStatus.Success;
                }

                currentChildIndex++;
            }

            ResetProgress();
            return BTStatus.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            ResetProgress();
        }

        private void ResetProgress()
        {
            currentChildIndex = 0;
        }
    }
}
