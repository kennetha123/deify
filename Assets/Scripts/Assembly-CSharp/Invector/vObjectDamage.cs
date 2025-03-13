using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
	[vClassHeader("OBJECT DAMAGE", true, "icon_v2", false, "", iconName = "DamageIcon")]
	public class vObjectDamage : vMonoBehaviour
	{
		[Serializable]
		public class OnHitEvent : UnityEvent<Collider>
		{
		}

		public enum CollisionMethod
		{
			OnTriggerEnter = 0,
			OnColliderEnter = 1,
			OnParticleCollision = 2
		}

		public vDamage damage;

		[Tooltip("Assign this to set other damage sender")]
		public Transform overrideDamageSender;

		[Tooltip("List of tags that can be hit")]
		public List<string> tags;

		[Tooltip("Check to use the damage Frequence")]
		public bool continuousDamage;

		[Tooltip("Apply damage to each end of the frequency in seconds ")]
		public float damageFrequency = 0.5f;

		private List<Collider> targets;

		private List<Collider> disabledTarget;

		private float currentTime;

		public OnHitEvent onHit;

		public CollisionMethod collisionMethod;

		public ParticleSystem part;

		public List<ParticleCollisionEvent> collisionEvents;

		protected virtual void Start()
		{
			targets = new List<Collider>();
			disabledTarget = new List<Collider>();
			if (collisionMethod == CollisionMethod.OnParticleCollision)
			{
				part = GetComponent<ParticleSystem>();
				collisionEvents = new List<ParticleCollisionEvent>();
			}
		}

		protected virtual void Update()
		{
			if (!continuousDamage || targets == null || targets.Count <= 0)
			{
				return;
			}
			if (currentTime > 0f)
			{
				currentTime -= Time.deltaTime;
				return;
			}
			currentTime = damageFrequency;
			foreach (Collider target in targets)
			{
				if (target != null)
				{
					if (target.enabled)
					{
						onHit.Invoke(target);
						ApplyDamage(target.transform, base.transform.position);
					}
					else
					{
						disabledTarget.Add(target);
					}
				}
			}
			if (disabledTarget.Count > 0)
			{
				int num = disabledTarget.Count;
				while (num >= 0 && disabledTarget.Count != 0)
				{
					try
					{
						if (targets.Contains(disabledTarget[num]))
						{
							targets.Remove(disabledTarget[num]);
						}
					}
					catch
					{
						break;
					}
					num--;
				}
			}
			if (disabledTarget.Count > 0)
			{
				disabledTarget.Clear();
			}
		}

		protected virtual void OnCollisionEnter(Collision hit)
		{
			if (collisionMethod == CollisionMethod.OnColliderEnter && !continuousDamage && tags.Contains(hit.gameObject.tag))
			{
				ApplyDamage(hit.transform, hit.contacts[0].point);
			}
		}

		protected virtual void OnTriggerEnter(Collider hit)
		{
			if (collisionMethod == CollisionMethod.OnTriggerEnter)
			{
				if (continuousDamage && tags.Contains(hit.transform.tag) && !targets.Contains(hit))
				{
					targets.Add(hit);
				}
				else if (tags.Contains(hit.gameObject.tag))
				{
					onHit.Invoke(hit);
					ApplyDamage(hit.transform, base.transform.position);
				}
			}
		}

		protected virtual void OnTriggerExit(Collider hit)
		{
			if ((collisionMethod != CollisionMethod.OnColliderEnter || continuousDamage) && tags.Contains(hit.gameObject.tag) && targets.Contains(hit))
			{
				targets.Remove(hit);
			}
		}

		protected virtual void OnParticleCollision(GameObject hit)
		{
			if (collisionMethod != CollisionMethod.OnParticleCollision)
			{
				return;
			}
			int num = part.GetCollisionEvents(hit, collisionEvents);
			Collider component = hit.GetComponent<Collider>();
			for (int i = 0; i < num; i++)
			{
				if ((bool)component)
				{
					if (continuousDamage && tags.Contains(hit.transform.tag) && !targets.Contains(component))
					{
						targets.Add(component);
					}
					else if (tags.Contains(hit.gameObject.tag))
					{
						onHit.Invoke(component);
						ApplyDamage(hit.transform, base.transform.position);
					}
				}
			}
		}

		public virtual void ClearTargets()
		{
			targets.Clear();
		}

		protected virtual void ApplyDamage(Transform target, Vector3 hitPoint)
		{
			damage.hitReaction = true;
			damage.sender = (overrideDamageSender ? overrideDamageSender : base.transform);
			damage.hitPosition = hitPoint;
			damage.receiver = target;
			target.gameObject.ApplyDamage(new vDamage(damage));
		}
	}
}
