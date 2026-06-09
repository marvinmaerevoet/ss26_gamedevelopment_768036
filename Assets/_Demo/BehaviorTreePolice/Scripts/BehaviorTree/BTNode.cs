namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTNode
    {
        public string Name { get; }
        public BTStatus LastStatus { get; protected set; }

        protected BTNode(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
            LastStatus = BTStatus.Running;
        }

        public BTStatus Tick()
        {
            LastStatus = OnTick();
            return LastStatus;
        }

        public virtual void Reset()
        {
            LastStatus = BTStatus.Running;
        }

        protected abstract BTStatus OnTick();
    }
}
