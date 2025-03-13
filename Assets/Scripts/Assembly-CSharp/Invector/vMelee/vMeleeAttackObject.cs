using System.Collections.Generic;
using Invector.vEventSystems;
using LastBoss.Character;
using LastBoss.Enemy;
using LastBoss.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee
{
	[vClassHeader("Melee Object", true, "icon_v2", false, "", openClose = false)]
	public class vMeleeAttackObject : vMonoBehaviour
	{
		public vDamage damage;

		public Transform overrideDamageSender;

		public List<vHitBox> hitBoxes;

		public int damageModifier;

		[HideInInspector]
		public bool canApplyDamage;

		public OnHitEnter onDamageHit;

		public OnHitEnter onRecoilHit;

		public UnityEvent onEnableDamage;

		public UnityEvent onDisableDamage;

		private Dictionary<vHitBox, List<GameObject>> targetColliders;

		[HideInInspector]
		public vMeleeManager meleeManager;

		private RevengeMode revenge;

		private EnemyAttributes enemy;

		[Tooltip("How many points player can get revenge mode points per hit.")]
		public int revengeModePoints;

		private bool isRevenge;

		private bool onceRevenge;

		protected virtual void Start()
		{
			if (base.transform.root.tag == "Player")
			{
				revenge = base.transform.root.GetComponent<RevengeMode>();
			}
			else
			{
				enemy = base.transform.root.GetComponent<EnemyAttributes>();
			}
			targetColliders = new Dictionary<vHitBox, List<GameObject>>();
			if (hitBoxes.Count > 0)
			{
				foreach (vHitBox hitBox in hitBoxes)
				{
					hitBox.attackObject = this;
					targetColliders.Add(hitBox, new List<GameObject>());
				}
				return;
			}
			base.enabled = false;
		}

		public virtual void SetActiveDamage(bool value)
		{
			canApplyDamage = value;
			for (int i = 0; i < hitBoxes.Count; i++)
			{
				vHitBox vHitBox2 = hitBoxes[i];
				vHitBox2.trigger.enabled = value;
				if (!value && targetColliders != null)
				{
					targetColliders[vHitBox2].Clear();
				}
			}
			if (value)
			{
				onEnableDamage.Invoke();
			}
			else
			{
				onDisableDamage.Invoke();
			}
		}

		public virtual void OnHit(vHitBox hitBox, Collider other)
		{
			if (canApplyDamage && !targetColliders[hitBox].Contains(other.gameObject) && meleeManager != null && other.gameObject != meleeManager.gameObject)
			{
				bool flag = false;
				bool flag2 = false;
				if (meleeManager == null)
				{
					meleeManager = GetComponentInParent<vMeleeManager>();
				}
				HitProperties hitProperties = meleeManager.hitProperties;
				if (((hitBox.triggerType & vHitBoxType.Damage) != 0 && hitProperties.hitDamageTags == null) || hitProperties.hitDamageTags.Count == 0)
				{
					flag = true;
				}
				else if ((hitBox.triggerType & vHitBoxType.Damage) != 0 && hitProperties.hitDamageTags.Contains(other.tag))
				{
					flag = true;
				}
				else if ((hitBox.triggerType & vHitBoxType.Recoil) != 0 && (int)hitProperties.hitRecoilLayer == ((int)hitProperties.hitRecoilLayer | (1 << other.gameObject.layer)))
				{
					hitBox.enabled = false;
				}
				if (flag || flag2)
				{
					targetColliders[hitBox].Add(other.gameObject);
					vHitInfo vHitInfo2 = new vHitInfo(this, hitBox, other, hitBox.transform.position);
					if (flag)
					{
						if ((bool)meleeManager)
						{
							meleeManager.OnDamageHit(vHitInfo2);
						}
						else
						{
							damage.sender = (overrideDamageSender ? overrideDamageSender : base.transform);
							ApplyDamage(hitBox, other, damage);
						}
						onDamageHit.Invoke(vHitInfo2);
					}
					if (flag2)
					{
						if ((bool)meleeManager)
						{
							meleeManager.OnRecoilHit(vHitInfo2);
						}
						onRecoilHit.Invoke(vHitInfo2);
					}
				}
			}
			GetRevengePointsOrExp(other);
		}

		private void GetRevengePointsOrExp(Collider other)
		{
			if (base.transform.root.tag != "Player")
			{
				if (other.transform.root.tag == "Player")
				{
					if (other.transform.root.GetComponent<RevengeMode>().isDead && other.transform.root.GetComponent<RevengeMode>().isRevengeMode)
					{
						base.transform.root.GetComponent<EnemyAttributes>().isGetExp = true;
						base.transform.root.GetComponent<EnemyAttributes>().expGet = other.transform.root.GetComponent<RevengeMode>().experience;
					}
					else if (other.transform.root.GetComponent<RevengeMode>().isDead)
					{
						base.transform.root.GetComponent<EnemyAttributes>().isRevengeTarget = true;
						other.transform.root.GetComponent<RevengeMode>().isDead = false;
					}
				}
			}
			else if ((other.transform.tag == "Enemy" || other.transform.tag == "Boss") && !revenge.isRevengeMode)
			{
				revenge.revengeModePoints += revengeModePoints;
			}
		}

		public void ApplyDamage(vHitBox hitBox, Collider other, vDamage damage)
		{
			vDamage vDamage = new vDamage(damage);
			vDamage.receiver = other.transform;
			if (base.transform.root.tag == "Player")
			{
				vDamage.damageValue = (revenge.isRevengeMode ? (Mathf.RoundToInt((float)(damage.damageValue + damageModifier) * ((float)hitBox.damagePercentage * 0.01f) * revenge.damageUpgradeSender) + meleeManager.defaultDamage.damageValue) : Mathf.RoundToInt((float)(damage.damageValue + damageModifier) * ((float)hitBox.damagePercentage * 0.01f) + (float)meleeManager.defaultDamage.damageValue));
			}
			else
			{
				if (enemy.expContainer != 0)
				{
					vDamage.damageValue = Mathf.RoundToInt((float)(damage.damageValue + damageModifier) * ((float)hitBox.damagePercentage * 0.01f) * enemy.damageUpgradeSender);
				}
				else
				{
					vDamage.damageValue = Mathf.RoundToInt((float)(damage.damageValue + damageModifier) * ((float)hitBox.damagePercentage * 0.01f));
				}
				Object.FindObjectOfType<UIReader>().UIGotDamage();
			}
			vDamage.hitPosition = hitBox.transform.position;
			other.gameObject.ApplyDamage(vDamage, meleeManager.fighter);
		}
	}
}
