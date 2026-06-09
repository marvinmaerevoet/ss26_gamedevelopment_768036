using System.Collections.Generic;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTComposite : BTNode
    {
        protected readonly List<BTNode> ChildNodes;
        public override IReadOnlyList<BTNode> Children => ChildNodes;

        protected BTComposite(string name, IEnumerable<BTNode> children) : base(name)
        {
            ChildNodes = children != null ? new List<BTNode>(children) : new List<BTNode>();

            foreach (BTNode child in ChildNodes)
            {
                child?.SetParent(this);
            }
        }

        public override void Reset()
        {
            base.Reset();

            foreach (BTNode child in ChildNodes)
            {
                child?.Reset();
            }
        }
    }
}
