using CustomApproachDemo.BehaviorTree;
using UnityEngine;

namespace CustomApproachDemo.Police
{
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
        public string CurrentBehaviorName;
        public string CurrentNodeName;
        public BTStatus LastTreeStatus;
    }
}
