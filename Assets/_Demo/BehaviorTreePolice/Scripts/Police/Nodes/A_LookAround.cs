using Demo.BehaviorTreePolice.BehaviorTree;
using UnityEngine;

namespace Demo.BehaviorTreePolice.Police.Nodes
{
    public sealed class A_LookAround : BTAction
    {
        private readonly PoliceAIContext context;
        private float startTime;
        private bool started;
        private bool warnedMissingContext;

        public A_LookAround(PoliceAIContext context) : base("Look Around")
        {
            this.context = context;
        }

        protected override BTStatus Execute()
        {
            if (context == null)
            {
                PoliceNodeSupport.WarnOnce($"{Name} needs a PoliceAIContext.", ref warnedMissingContext);
                return BTStatus.Failure;
            }

            if (!started)
            {
                started = true;
                startTime = Time.time;
                context.StopMovement();
            }

            if (context.Self != null)
            {
                context.Self.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);
            }

            float duration = Mathf.Max(0f, context.lookAroundDuration);
            if (Time.time - startTime < duration)
            {
                return BTStatus.Running;
            }

            started = false;
            return BTStatus.Success;
        }

        public override void Reset()
        {
            base.Reset();
            started = false;
            startTime = 0f;
        }
    }
}
