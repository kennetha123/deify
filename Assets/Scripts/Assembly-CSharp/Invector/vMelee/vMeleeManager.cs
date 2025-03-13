using System;
using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vMelee
{
	public class vMeleeManager : vMonoBehaviour
	{
		public List<vBodyMember> Members = new List<vBodyMember>();

		public vDamage defaultDamage = new vDamage(10);

		public HitProperties hitProperties;

		public vMeleeWeapon leftWeapon;

		public vMeleeWeapon rightWeapon;

		public vOnHitEvent onDamageHit;

		public vOnHitEvent onRecoilHit;

		[Tooltip("NPC ONLY- Ideal distance for the attack")]
		public float defaultAttackDistance = 1f;

		[Tooltip("Default cost for stamina when attack")]
		public float defaultStaminaCost = 20f;

		[Tooltip("Default recovery delay for stamina when attack")]
		public float defaultStaminaRecoveryDelay = 1f;

		[Range(0f, 100f)]
		public int defaultDefenseRate = 50;

		[Range(0f, 180f)]
		public float defaultDefenseRange = 90f;

		[HideInInspector]
		public vIMeleeFighter fighter;

		private int damageMultiplier;

		private int currentRecoilID;

		private int currentReactionID;

		private bool ignoreDefense;

		private bool activeRagdoll;

		private bool inRecoil;

		private string attackName;

		public virtual vMeleeWeapon CurrentActiveAttackWeapon
		{
			get
			{
				if ((bool)rightWeapon && rightWeapon.gameObject.activeInHierarchy && (rightWeapon.meleeType == vMeleeType.OnlyAttack || rightWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return rightWeapon;
				}
				if ((bool)leftWeapon && leftWeapon.gameObject.activeInHierarchy && (leftWeapon.meleeType == vMeleeType.OnlyAttack || leftWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return leftWeapon;
				}
				return null;
			}
		}

		public virtual vMeleeWeapon CurrentActiveDefenseWeapon
		{
			get
			{
				if ((bool)rightWeapon && rightWeapon.gameObject.activeInHierarchy && (rightWeapon.meleeType == vMeleeType.OnlyDefense || rightWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return rightWeapon;
				}
				if ((bool)leftWeapon && leftWeapon.gameObject.activeInHierarchy && (leftWeapon.meleeType == vMeleeType.OnlyDefense || leftWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return leftWeapon;
				}
				return null;
			}
		}

		public virtual vMeleeWeapon CurrentAttackWeapon
		{
			get
			{
				if ((bool)rightWeapon && (rightWeapon.meleeType == vMeleeType.OnlyAttack || rightWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return rightWeapon;
				}
				if ((bool)leftWeapon && (leftWeapon.meleeType == vMeleeType.OnlyAttack || leftWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return leftWeapon;
				}
				return null;
			}
		}

		public virtual vMeleeWeapon CurrentDefenseWeapon
		{
			get
			{
				if ((bool)rightWeapon && (rightWeapon.meleeType == vMeleeType.OnlyDefense || rightWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return rightWeapon;
				}
				if ((bool)leftWeapon && (leftWeapon.meleeType == vMeleeType.OnlyDefense || leftWeapon.meleeType == vMeleeType.AttackAndDefense))
				{
					return leftWeapon;
				}
				return null;
			}
		}

		protected virtual void Start()
		{
			Init();
		}

		protected virtual void Init()
		{
			fighter = base.gameObject.GetMeleeFighter();
			foreach (vBodyMember member in Members)
			{
				member.attackObject.damage.damageValue = defaultDamage.damageValue;
				if (member.bodyPart == HumanBodyBones.LeftLowerArm.ToString())
				{
					vMeleeWeapon componentInChildren = member.attackObject.GetComponentInChildren<vMeleeWeapon>(true);
					leftWeapon = componentInChildren;
				}
				if (member.bodyPart == HumanBodyBones.RightLowerArm.ToString())
				{
					vMeleeWeapon componentInChildren2 = member.attackObject.GetComponentInChildren<vMeleeWeapon>(true);
					rightWeapon = componentInChildren2;
				}
				member.attackObject.meleeManager = this;
				member.SetActiveDamage(false);
			}
			if (leftWeapon != null)
			{
				leftWeapon.SetActiveDamage(false);
				leftWeapon.meleeManager = this;
			}
			if (rightWeapon != null)
			{
				rightWeapon.meleeManager = this;
				rightWeapon.SetActiveDamage(false);
			}
		}

		public virtual void SetActiveAttack(List<string> bodyParts, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
		{
			for (int i = 0; i < bodyParts.Count; i++)
			{
				string bodyPart = bodyParts[i];
				SetActiveAttack(bodyPart, type, active, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll, attackName);
			}
		}

		public virtual void SetActiveAttack(string bodyPart, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
		{
			this.damageMultiplier = damageMultiplier;
			currentRecoilID = recoilID;
			currentReactionID = reactionID;
			this.ignoreDefense = ignoreDefense;
			this.activeRagdoll = activeRagdoll;
			this.attackName = attackName;
			if (type == vAttackType.Unarmed)
			{
				vBodyMember vBodyMember2 = Members.Find((vBodyMember member) => member.bodyPart == bodyPart);
				if (vBodyMember2 != null)
				{
					vBodyMember2.SetActiveDamage(active);
				}
			}
			else if (bodyPart == "RightLowerArm" && rightWeapon != null)
			{
				rightWeapon.meleeManager = this;
				rightWeapon.SetActiveDamage(active);
			}
			else if (bodyPart == "LeftLowerArm" && leftWeapon != null)
			{
				leftWeapon.meleeManager = this;
				leftWeapon.SetActiveDamage(active);
			}
		}

		public virtual void OnDamageHit(vHitInfo hitInfo)
		{
			vDamage vDamage = new vDamage(hitInfo.attackObject.damage);
			vDamage.sender = base.transform;
			vDamage.reaction_id = currentReactionID;
			vDamage.recoil_id = currentRecoilID;
			if (activeRagdoll)
			{
				vDamage.activeRagdoll = activeRagdoll;
			}
			if (attackName != string.Empty)
			{
				vDamage.damageType = attackName;
			}
			if (ignoreDefense)
			{
				vDamage.ignoreDefense = ignoreDefense;
			}
			vDamage.damageValue *= ((damageMultiplier <= 1) ? 1 : damageMultiplier);
			hitInfo.attackObject.ApplyDamage(hitInfo.hitBox, hitInfo.targetCollider, vDamage);
			onDamageHit.Invoke(hitInfo);
		}

		public virtual void OnRecoilHit(vHitInfo hitInfo)
		{
			if (hitProperties.useRecoil && InRecoilRange(hitInfo) && !inRecoil)
			{
				inRecoil = true;
				int recoilID = currentRecoilID;
				if (fighter != null)
				{
					fighter.OnRecoil(recoilID);
				}
				onRecoilHit.Invoke(hitInfo);
				Invoke("ResetRecoil", 1f);
			}
		}

		public virtual void OnDefense()
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				leftWeapon.OnDefense();
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				rightWeapon.OnDefense();
			}
		}

		public virtual int GetAttackID()
		{
			if (rightWeapon != null && rightWeapon.meleeType != 0 && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.attackID;
			}
			return 0;
		}

		public virtual float GetAttackStaminaCost()
		{
			if (rightWeapon != null && rightWeapon.meleeType != 0 && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.staminaCost;
			}
			return defaultStaminaCost;
		}

		public virtual float GetAttackStaminaRecoveryDelay()
		{
			if (rightWeapon != null && rightWeapon.meleeType != 0 && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.staminaRecoveryDelay;
			}
			return defaultStaminaRecoveryDelay;
		}

		public virtual float GetAttackDistance()
		{
			if (rightWeapon != null && rightWeapon.meleeType != 0 && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.distanceToAttack;
			}
			return defaultAttackDistance;
		}

		public virtual int GetDefenseID()
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				GetComponent<Animator>().SetBool("FlipAnimation", false);
				return leftWeapon.defenseID;
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				GetComponent<Animator>().SetBool("FlipAnimation", true);
				return rightWeapon.defenseID;
			}
			return 0;
		}

		public int GetDefenseRate()
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				return leftWeapon.defenseRate;
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.defenseRate;
			}
			return defaultDefenseRate;
		}

		public virtual int GetMoveSetID()
		{
			if (rightWeapon != null && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.movesetID;
			}
			return 0;
		}

		public virtual bool CanBreakAttack()
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				return leftWeapon.breakAttack;
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.breakAttack;
			}
			return false;
		}

		public virtual bool CanBlockAttack(Vector3 attackPoint)
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				return Math.Abs(base.transform.HitAngle(attackPoint)) <= leftWeapon.defenseRange;
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				return Math.Abs(base.transform.HitAngle(attackPoint)) <= rightWeapon.defenseRange;
			}
			return Math.Abs(base.transform.HitAngle(attackPoint)) <= defaultDefenseRange;
		}

		public virtual int GetDefenseRecoilID()
		{
			if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
			{
				return leftWeapon.recoilID;
			}
			if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
			{
				return rightWeapon.recoilID;
			}
			return 0;
		}

		protected virtual bool InRecoilRange(vHitInfo hitInfo)
		{
			Vector3 vector = new Vector3(base.transform.position.x, hitInfo.hitPoint.y, base.transform.position.z);
			return (Quaternion.LookRotation(hitInfo.hitPoint - vector).eulerAngles - base.transform.eulerAngles).NormalizeAngle().y <= hitProperties.recoilRange;
		}

		public virtual void SetLeftWeapon(GameObject weaponObject)
		{
			if ((bool)weaponObject)
			{
				leftWeapon = weaponObject.GetComponent<vMeleeWeapon>();
				if ((bool)leftWeapon)
				{
					leftWeapon.SetActiveDamage(false);
					leftWeapon.meleeManager = this;
				}
			}
		}

		public virtual void SetRightWeapon(GameObject weaponObject)
		{
			if ((bool)weaponObject)
			{
				rightWeapon = weaponObject.GetComponent<vMeleeWeapon>();
				if ((bool)rightWeapon)
				{
					rightWeapon.meleeManager = this;
					rightWeapon.SetActiveDamage(false);
				}
			}
		}

		public virtual void SetLeftWeapon(vMeleeWeapon weapon)
		{
			if ((bool)weapon)
			{
				leftWeapon = weapon;
				leftWeapon.SetActiveDamage(false);
				leftWeapon.meleeManager = this;
			}
		}

		public virtual void SetRightWeapon(vMeleeWeapon weapon)
		{
			if ((bool)weapon)
			{
				rightWeapon = weapon;
				rightWeapon.meleeManager = this;
				rightWeapon.SetActiveDamage(false);
			}
		}

		protected virtual void ResetRecoil()
		{
			inRecoil = false;
		}
	}
}
