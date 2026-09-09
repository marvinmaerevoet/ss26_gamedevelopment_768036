namespace CustomApproachDemo.BehaviorTree
{
    public sealed class BTRepeater : BTDecorator
    {
        private readonly int repeatCount;
        private int completedRuns;

        public BTRepeater(string name, BTNode child, int repeatCount = -1) : base(name, child)
        {
            this.repeatCount = repeatCount;
        }

        protected override BTStatus OnTick()
        {
            if (Child == null)
            {
                return BTStatus.Failure;
            }

            if (repeatCount >= 0 && completedRuns >= repeatCount)
            {
                return BTStatus.Success;
            }

            BTStatus childStatus = Child.Tick();

            if (childStatus == BTStatus.Running)
            {
                return BTStatus.Running;
            }

            completedRuns++;
            Child.Reset();

            return repeatCount >= 0 && completedRuns >= repeatCount
                ? BTStatus.Success
                : BTStatus.Running;
        }

        public override void Reset()
        {
            base.Reset();
            completedRuns = 0;
        }
    }
}
