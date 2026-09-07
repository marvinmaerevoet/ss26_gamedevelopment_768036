using UnityEngine;

namespace CustomApproachDemo.Player
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
