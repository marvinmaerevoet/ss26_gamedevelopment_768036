using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTDecorator : BTNode
    {
        protected readonly BTNode Child;
        private readonly BTNode[] childView;
        public override IReadOnlyList<BTNode> Children => childView;

        protected BTDecorator(string name, BTNode child) : base(name)
        {
            Child = child;
            childView = child != null ? new[] { child } : new BTNode[0];
            Child?.SetParent(this);
        }

        public override void Reset()
        {
            base.Reset();
            Child?.Reset();
        }
    }
}
