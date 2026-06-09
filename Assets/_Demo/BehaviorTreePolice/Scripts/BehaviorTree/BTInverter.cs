namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public sealed class BTInverter : BTDecorator
    {
        public BTInverter(string name, BTNode child) : base(name, child)
        {
        }

        protected override BTStatus OnTick()
        {
            if (Child == null)
            {
                return BTStatus.Failure;
            }

            BTStatus childStatus = Child.Tick();

            return childStatus switch
            {
                BTStatus.Success => BTStatus.Failure,
                BTStatus.Failure => BTStatus.Success,
                _ => BTStatus.Running
            };
        }
    }
}
