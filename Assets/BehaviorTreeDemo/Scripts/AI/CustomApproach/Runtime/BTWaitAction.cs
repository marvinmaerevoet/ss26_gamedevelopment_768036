using UnityEngine;

namespace CustomApproachDemo.BehaviorTree
{
    public sealed class BTWaitAction : BTAction
    {
        private readonly float duration;
        private float startTime;
        private bool started;

        public BTWaitAction(string name, float duration) : base(name)
        {
            this.duration = Mathf.Max(0f, duration);
        }

        protected override BTStatus Execute()
        {
            if (!started)
            {
                started = true;
                startTime = Time.time;
            }

            return Time.time - startTime >= duration
                ? BTStatus.Success
                : BTStatus.Running;
        }

        public override void Reset()
        {
            base.Reset();
            started = false;
            startTime = 0f;
        }
    }
}
