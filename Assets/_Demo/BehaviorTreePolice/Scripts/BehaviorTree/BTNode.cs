using System;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTNode
    {
        public static event Action<BTNode, BTStatus> NodeTicked;

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
            NodeTicked?.Invoke(this, LastStatus);
            return LastStatus;
        }

        public virtual void Reset()
        {
            LastStatus = BTStatus.Running;
        }

        protected abstract BTStatus OnTick();
    }
}
