using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTSelector : BTComposite
    {
        private readonly bool rememberRunningChild;
        private int currentChildIndex;
        private int runningChildIndex = -1;

        public BTSelector(string name, IEnumerable<BTNode> children, bool rememberRunningChild = true) : base(name, children)
        {
            this.rememberRunningChild = rememberRunningChild;
        }

        protected override BTStatus OnTick()
        {
            int startIndex = rememberRunningChild ? currentChildIndex : 0;

            for (int i = startIndex; i < Children.Count; i++)
            {
                BTStatus childStatus = Children[i].Tick();

                if (childStatus == BTStatus.Running)
                {
                    if (!rememberRunningChild)
                    {
                        ResetPreviouslyRunningChildIfChanged(i);
                        ResetLowerPriorityChildren(i);
                    }

                    currentChildIndex = rememberRunningChild ? i : 0;
                    runningChildIndex = i;
                    return BTStatus.Running;
                }

                if (childStatus == BTStatus.Success)
                {
                    if (!rememberRunningChild)
                    {
                        ResetPreviouslyRunningChildIfChanged(i);
                        ResetLowerPriorityChildren(i);
                    }

                    ResetProgress();
                    return BTStatus.Success;
                }
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
            runningChildIndex = -1;
        }

        private void ResetPreviouslyRunningChildIfChanged(int activeChildIndex)
        {
            if (runningChildIndex < 0 || runningChildIndex == activeChildIndex || runningChildIndex >= Children.Count)
            {
                return;
            }

            Children[runningChildIndex].Reset();
        }

        private void ResetLowerPriorityChildren(int activeChildIndex)
        {
            for (int i = activeChildIndex + 1; i < Children.Count; i++)
            {
                Children[i].Reset();
            }
        }
    }
}
