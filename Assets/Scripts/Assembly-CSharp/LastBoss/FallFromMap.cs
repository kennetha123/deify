using Invector;
using Invector.vCharacterController;
using LastBoss.Character;
using UnityEngine;

namespace LastBoss
{
	public class FallFromMap : MonoBehaviour
	{
		private vThirdPersonController hub;

		private RevengeMode revenge;

		private void OnTriggerEnter(Collider other)
		{
			if (other.tag == "Player")
			{
				hub = GameObject.FindGameObjectWithTag("Player").GetComponent<vThirdPersonController>();
				revenge = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>();
				revenge.revengeModePoints = 0;
				hub.TakeDamage(new vDamage(100000));
			}
		}
	}
}
