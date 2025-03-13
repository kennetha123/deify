using UnityEngine;

namespace Invector.vCharacterController
{
	public class vRagdollCollision
	{
		private GameObject sender;

		private Collision collision;

		private float impactForce;

		public GameObject Sender
		{
			get
			{
				return sender;
			}
		}

		public Collision Collision
		{
			get
			{
				return collision;
			}
		}

		public float ImpactForce
		{
			get
			{
				return impactForce;
			}
		}

		public vRagdollCollision(GameObject sender, Collision collision)
		{
			this.sender = sender;
			this.collision = collision;
			impactForce = collision.relativeVelocity.magnitude;
		}
	}
}
