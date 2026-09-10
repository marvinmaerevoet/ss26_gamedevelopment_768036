using System;

namespace BehaviorTreeDemo.AI.GitAmend.Runtime
{
    public interface IStrategy
    {
        Node.Status Process();

        void Reset()
        {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy
    {
        private readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }

        public Node.Status Process()
        {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy
    {
        private readonly Func<bool> predicate;

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }
}
