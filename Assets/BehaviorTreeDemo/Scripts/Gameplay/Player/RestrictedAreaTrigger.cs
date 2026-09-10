using UnityEngine;

namespace BehaviorTreeDemo.Gameplay.Player
{
    public sealed class RestrictedAreaTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            DemoPlayerState playerState = other.GetComponentInParent<DemoPlayerState>();
            if (playerState == null)
            {
                return;
            }

            playerState.IsInRestrictedArea = true;
        }

        private void OnTriggerExit(Collider other)
        {
            DemoPlayerState playerState = other.GetComponentInParent<DemoPlayerState>();
            if (playerState == null)
            {
                return;
            }

            playerState.IsInRestrictedArea = false;
        }
    }
}
