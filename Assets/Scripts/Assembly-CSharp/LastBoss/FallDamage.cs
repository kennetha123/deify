using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using UnityEngine;

namespace LastBoss
{
	public class FallDamage : MonoBehaviour
	{
		public float damageThreshold = 3f;

		public float damageMultiplier = 2.85f;

		private vThirdPersonController vController;

		private vControlAIMelee melee;

		private float startYPos;

		private float endYPos;

		private bool firstCall = true;

		private bool damaged;

		private void Awake()
		{
			if (base.gameObject.tag == "Player")
			{
				vController = GetComponent<vThirdPersonController>();
			}
			else
			{
				melee = GetComponent<vControlAIMelee>();
			}
		}

		private void Update()
		{
			if ((vController != null && !vController.isGrounded) || (melee != null && !melee.isGrounded))
			{
				if (base.transform.position.y > startYPos)
				{
					firstCall = true;
				}
				if (firstCall)
				{
					firstCall = false;
					damaged = true;
					startYPos = base.transform.position.y;
				}
				return;
			}
			endYPos = base.transform.position.y;
			if (damaged && startYPos - endYPos > damageThreshold)
			{
				damaged = false;
				firstCall = true;
				float num = startYPos - endYPos - damageThreshold;
				float f = ((damageMultiplier == 0f) ? num : (num * damageMultiplier));
				if (base.gameObject.tag == "Player")
				{
					vController.TakeDamage(new vDamage(Mathf.RoundToInt(f)));
				}
				else
				{
					melee.TakeDamage(new vDamage(Mathf.RoundToInt(f)));
				}
			}
		}
	}
}
