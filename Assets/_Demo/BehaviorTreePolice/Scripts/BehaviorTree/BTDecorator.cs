namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTDecorator : BTNode
    {
        protected readonly BTNode Child;

        protected BTDecorator(string name, BTNode child) : base(name)
        {
            Child = child;
        }

        public override void Reset()
        {
            base.Reset();
            Child?.Reset();
        }
    }
}
