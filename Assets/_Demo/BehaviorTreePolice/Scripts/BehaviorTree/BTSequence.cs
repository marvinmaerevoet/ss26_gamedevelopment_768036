using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTSequence : BTComposite
    {
        private int currentChildIndex;

        public BTSequence(string name, IEnumerable<BTNode> children) : base(name, children)
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

                if (childStatus == BTStatus.Failure)
                {
                    ResetProgress();
                    return BTStatus.Failure;
                }

                currentChildIndex++;
            }

            ResetProgress();
            return BTStatus.Success;
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
