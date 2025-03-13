using UnityEngine;

namespace Invector.vCharacterController
{
	public class vJumpMultiplierTrigger : MonoBehaviour
	{
		public float multiplier = 5f;

		public float timeToReset = 0.5f;

		private void OnTriggerStay(Collider other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				vThirdPersonController component = other.GetComponent<vThirdPersonController>();
				if ((bool)component && (component.isJumping || !component.isGrounded))
				{
					component.SetJumpMultiplier(multiplier, timeToReset);
					component.isJumping = false;
					component.isGrounded = true;
					component.Jump();
				}
			}
		}
	}
}
