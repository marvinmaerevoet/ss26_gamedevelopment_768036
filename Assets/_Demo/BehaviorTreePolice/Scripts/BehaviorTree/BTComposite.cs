using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTComposite : BTNode
    {
        protected readonly List<BTNode> Children;

        protected BTComposite(string name, IEnumerable<BTNode> children) : base(name)
        {
            Children = children != null ? new List<BTNode>(children) : new List<BTNode>();
        }

        public override void Reset()
        {
            base.Reset();

            foreach (BTNode child in Children)
            {
                child.Reset();
            }
        }
    }
}
