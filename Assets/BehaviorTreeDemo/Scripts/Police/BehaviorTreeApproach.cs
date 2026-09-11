using UnityEngine;

namespace BehaviorTreeDemo.Police
{
    public enum BehaviorTreeApproach
    {
        CustomApproach = 0,
        GitAmend = 1,
        UnityBehavior = 2
    }

    public static class BehaviorTreeApproachSelection
    {
        public static BehaviorTreeApproach CurrentApproach { get; private set; } = BehaviorTreeApproach.CustomApproach;

        public static void Select(BehaviorTreeApproach approach)
        {
            CurrentApproach = approach;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForApplicationStart()
        {
            CurrentApproach = BehaviorTreeApproach.CustomApproach;
        }
    }
}
