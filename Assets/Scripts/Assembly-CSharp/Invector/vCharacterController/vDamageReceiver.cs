using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	[vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
	[vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
	public class vDamageReceiver : vMonoBehaviour, vIDamageReceiver, vIAttackReceiver
	{
		[vEditorToolbar("Default", false, "", false, false)]
		public float damageMultiplier = 1f;

		[HideInInspector]
		public vRagdoll ragdoll;

		public bool overrideReactionID;

		[vHideInInspector("overrideReactionID", false)]
		public int reactionID;

		[vEditorToolbar("Random", false, "", false, false)]
		public bool useRandomValues;

		[vHideInInspector("useRandomValues", false)]
		public bool fixedValues;

		[vHideInInspector("useRandomValues", false)]
		public float minDamageMultiplier;

		[vHideInInspector("useRandomValues", false)]
		public float maxDamageMultiplier;

		[vHideInInspector("useRandomValues", false)]
		public int minReactionID;

		[vHideInInspector("useRandomValues", false)]
		public int maxReactionID;

		[vHideInInspector("useRandomValues;fixedValues", false)]
		[Tooltip("Change Between 0 and 100")]
		public float changeToMaxValue;

		[SerializeField]
		protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();

		public UnityEvent OnGetMaxValue;

		private bool inAddDamage;

		public GameObject targetReceiver;

		private vIHealthController healthController;

		public OnReceiveDamage onReceiveDamage
		{
			get
			{
				return _onReceiveDamage;
			}
			protected set
			{
				_onReceiveDamage = value;
			}
		}

		protected virtual bool randomChange
		{
			get
			{
				return Random.Range(0f, 100f) < changeToMaxValue;
			}
		}

		Transform vIDamageReceiver.transform
		{
			get
			{
				return base.transform;
			}
		}

		GameObject vIDamageReceiver.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		private void Start()
		{
			ragdoll = GetComponentInParent<vRagdoll>();
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision == null || !ragdoll || !ragdoll.isActive)
			{
				return;
			}
			ragdoll.OnRagdollCollisionEnter(new vRagdollCollision(base.gameObject, collision));
			if (!inAddDamage)
			{
				float num = collision.relativeVelocity.x + collision.relativeVelocity.y + collision.relativeVelocity.z;
				if (num > 10f || num < -10f)
				{
					inAddDamage = true;
					new vDamage((int)Mathf.Abs(num) - 10)
					{
						ignoreDefense = true,
						sender = collision.transform,
						hitPosition = collision.contacts[0].point
					};
					Invoke("ResetAddDamage", 0.1f);
				}
			}
		}

		private void ResetAddDamage()
		{
			inAddDamage = false;
		}

		public void TakeDamage(vDamage damage)
		{
			if (!ragdoll || ragdoll.iChar.isDead)
			{
				return;
			}
			inAddDamage = true;
			float num = ((useRandomValues && !fixedValues) ? Random.Range(minDamageMultiplier, maxDamageMultiplier) : ((useRandomValues && fixedValues) ? (randomChange ? maxDamageMultiplier : minDamageMultiplier) : damageMultiplier));
			if (overrideReactionID)
			{
				if (useRandomValues && !fixedValues)
				{
					damage.reaction_id = Random.Range(minReactionID, maxReactionID);
				}
				else if (useRandomValues && fixedValues)
				{
					damage.reaction_id = (randomChange ? maxReactionID : minReactionID);
				}
				else
				{
					damage.reaction_id = reactionID;
				}
			}
			vDamage vDamage = new vDamage(damage);
			float num2 = vDamage.damageValue;
			vDamage.damageValue = (int)(num2 * num);
			if (num == maxDamageMultiplier)
			{
				OnGetMaxValue.Invoke();
			}
			ragdoll.ApplyDamage(damage);
			onReceiveDamage.Invoke(vDamage);
			Invoke("ResetAddDamage", 0.1f);
		}

		public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
		{
			float num = ((useRandomValues && !fixedValues) ? Random.Range(minDamageMultiplier, maxDamageMultiplier) : ((useRandomValues && fixedValues) ? (randomChange ? maxDamageMultiplier : minDamageMultiplier) : damageMultiplier));
			if (overrideReactionID)
			{
				if (useRandomValues && !fixedValues)
				{
					damage.reaction_id = Random.Range(minReactionID, maxReactionID);
				}
				else if (useRandomValues && fixedValues)
				{
					damage.reaction_id = (randomChange ? maxReactionID : minReactionID);
				}
				else
				{
					damage.reaction_id = reactionID;
				}
			}
			if ((bool)ragdoll && !ragdoll.iChar.isDead)
			{
				vDamage vDamage = new vDamage(damage);
				float num2 = vDamage.damageValue;
				vDamage.damageValue = (int)(num2 * num);
				if (num == maxDamageMultiplier)
				{
					OnGetMaxValue.Invoke();
				}
				ragdoll.gameObject.ApplyDamage(vDamage, attacker);
				onReceiveDamage.Invoke(vDamage);
				return;
			}
			if (healthController == null && (bool)targetReceiver)
			{
				healthController = targetReceiver.GetComponent<vIHealthController>();
			}
			else if (healthController == null)
			{
				healthController = GetComponentInParent<vIHealthController>();
			}
			if (healthController != null)
			{
				vDamage vDamage2 = new vDamage(damage);
				float num3 = vDamage2.damageValue;
				vDamage2.damageValue = (int)(num3 * num);
				if (num == maxDamageMultiplier)
				{
					OnGetMaxValue.Invoke();
				}
				try
				{
					healthController.gameObject.ApplyDamage(vDamage2, attacker);
					onReceiveDamage.Invoke(vDamage2);
				}
				catch
				{
					base.enabled = false;
				}
			}
		}
	}
}
