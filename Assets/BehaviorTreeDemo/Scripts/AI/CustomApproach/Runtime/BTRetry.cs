namespace CustomApproachDemo.BehaviorTree
{
    public sealed class BTRetry : BTDecorator
    {
        private readonly int maxAttempts;
        private int attempts;

        public BTRetry(string name, BTNode child, int maxAttempts) : base(name, child)
        {
            this.maxAttempts = maxAttempts < 1 ? 1 : maxAttempts;
        }

        protected override BTStatus OnTick()
        {
            if (Child == null)
            {
                return BTStatus.Failure;
            }

            BTStatus childStatus = Child.Tick();

            if (childStatus == BTStatus.Running)
            {
                return BTStatus.Running;
            }

            if (childStatus == BTStatus.Success)
            {
                attempts = 0;
                return BTStatus.Success;
            }

            attempts++;

            if (attempts >= maxAttempts)
            {
                attempts = 0;
                return BTStatus.Failure;
            }

            Child.Reset();
            return BTStatus.Running;
        }

        public override void Reset()
        {
            base.Reset();
            attempts = 0;
        }
    }
}
