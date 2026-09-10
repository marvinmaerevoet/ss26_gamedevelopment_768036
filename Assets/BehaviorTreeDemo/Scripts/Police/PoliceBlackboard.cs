using UnityEngine;

namespace BehaviorTreeDemo.Police
{
    public enum PoliceBehaviorMode
    {
        None = 0,
        Patrol = 1,
        Chase = 3,
        Investigate = 4,
        Arrest = 5,
        Emergency = 6
    }

    public sealed class PoliceBlackboard : MonoBehaviour
    {
        public Transform Player;
        public Vector3 LastKnownPlayerPosition;
        public bool HasLastKnownPlayerPosition;
        public bool PlayerVisible;
        public bool PlayerSuspicious;
        public bool PlayerInArrestRange;
        public bool OfficerHealthLow;
        public bool BackupCalled;
        public Transform CurrentPatrolPoint;
        public int CurrentPatrolIndex;
        public PoliceBehaviorMode CurrentBehaviorMode;
    }
}
