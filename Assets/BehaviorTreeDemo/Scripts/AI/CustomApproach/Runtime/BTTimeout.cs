using UnityEngine;

namespace CustomApproachDemo.BehaviorTree
{
    public sealed class BTTimeout : BTDecorator
    {
        private readonly float seconds;
        private float startTime;
        private bool hasStarted;

        public BTTimeout(string name, BTNode child, float seconds) : base(name, child)
        {
            this.seconds = Mathf.Max(0f, seconds);
        }

        protected override BTStatus OnTick()
        {
            if (Child == null)
            {
                return BTStatus.Failure;
            }

            if (!hasStarted)
            {
                hasStarted = true;
                startTime = Time.time;
            }

            if (Time.time - startTime >= seconds)
            {
                Child.Reset();
                return BTStatus.Failure;
            }

            BTStatus childStatus = Child.Tick();

            if (childStatus != BTStatus.Running)
            {
                hasStarted = false;
            }

            return childStatus;
        }

        public override void Reset()
        {
            base.Reset();
            hasStarted = false;
            startTime = 0f;
        }
    }
}
