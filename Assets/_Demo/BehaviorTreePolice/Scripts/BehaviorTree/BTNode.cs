using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demo.BehaviorTreePolice.BehaviorTree
{
    public abstract class BTNode
    {
        private static readonly IReadOnlyList<BTNode> NoChildren = Array.Empty<BTNode>();

        public static event Action<BTNode, BTStatus> NodeTicked;

        public string Name { get; }
        public BTStatus LastStatus { get; protected set; }
        public int LastTickFrame { get; private set; } = -1;
        public float LastTickTime { get; private set; } = -1f;
        public bool WasTickedRecently => LastTickTime >= 0f && Time.time - LastTickTime <= 0.25f;
        public bool IsRunning => LastStatus == BTStatus.Running;
        public BTNode Parent { get; private set; }
        public virtual IReadOnlyList<BTNode> Children => NoChildren;

        protected BTNode(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
            LastStatus = BTStatus.Running;
        }

        public BTStatus Tick()
        {
            LastStatus = OnTick();
            LastTickFrame = Time.frameCount;
            LastTickTime = Time.time;
            NodeTicked?.Invoke(this, LastStatus);
            return LastStatus;
        }

        public virtual void Reset()
        {
            LastStatus = BTStatus.Running;
            LastTickFrame = -1;
            LastTickTime = -1f;
        }

        internal void SetParent(BTNode parent)
        {
            Parent = parent;
        }

        protected abstract BTStatus OnTick();
    }
}
