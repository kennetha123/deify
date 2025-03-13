using UnityEngine;

namespace Invector.vMelee
{
	[vClassHeader("HitBox", true, "icon_v2", false, "", openClose = false)]
	public class vHitBox : vMonoBehaviour
	{
		[HideInInspector]
		public vMeleeAttackObject attackObject;

		public Collider trigger;

		public int damagePercentage = 100;

		[vEnumFlag]
		public vHitBoxType triggerType = vHitBoxType.Damage | vHitBoxType.Recoil;

		private bool canHit;

		private void OnDrawGizmos()
		{
			trigger = base.gameObject.GetComponent<Collider>();
			if (!trigger)
			{
				trigger = base.gameObject.AddComponent<BoxCollider>();
			}
			Color color = (((triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) == 0) ? Color.green : (((triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) != 0) ? Color.yellow : (((triggerType & vHitBoxType.Recoil) != 0 && (triggerType & vHitBoxType.Damage) == 0) ? Color.red : Color.black)));
			color.a = 0.6f;
			Gizmos.color = color;
			if (!Application.isPlaying && (bool)trigger && !trigger.enabled)
			{
				trigger.enabled = true;
			}
			if ((bool)trigger && trigger.enabled && (bool)(trigger as BoxCollider))
			{
				BoxCollider boxCollider = trigger as BoxCollider;
				float x = base.transform.lossyScale.x * boxCollider.size.x;
				float y = base.transform.lossyScale.y * boxCollider.size.y;
				float z = base.transform.lossyScale.z * boxCollider.size.z;
				Gizmos.matrix = Matrix4x4.TRS(boxCollider.bounds.center, base.transform.rotation, new Vector3(x, y, z));
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
			}
		}

		private void Start()
		{
			trigger = GetComponent<Collider>();
			if (!trigger)
			{
				trigger = base.gameObject.AddComponent<BoxCollider>();
			}
			if ((bool)trigger)
			{
				trigger.isTrigger = true;
				trigger.enabled = false;
			}
			int layer = LayerMask.NameToLayer("Ignore Raycast");
			base.transform.gameObject.layer = layer;
			canHit = (triggerType & vHitBoxType.Damage) != 0 || (triggerType & vHitBoxType.Recoil) != 0;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (TriggerCondictions(other) && attackObject != null)
			{
				attackObject.OnHit(this, other);
			}
		}

		private bool TriggerCondictions(Collider other)
		{
			if (canHit)
			{
				if (attackObject != null)
				{
					if (!(attackObject.meleeManager == null))
					{
						return other.gameObject != attackObject.meleeManager.gameObject;
					}
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
