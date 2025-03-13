using System.Collections.Generic;
using Invector.vEventSystems;
using Invector.vMelee;
using LastBoss.Enemy;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	[vClassHeader(" AI MELEE CONTROLLER", true, "icon_v2", false, "", iconName = "AI-icon")]
	public class vControlAIMelee : vControlAICombat, vIControlAIMelee, vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver, vIMeleeFighter, vIAttackReceiver, vIAttackListener
	{
		private int _moveSetID;

		private int _attackID;

		private int _defenceID;

		private int _recoilID;

		public vMeleeManager MeleeManager { get; set; }

		public bool isEquipping { get; protected set; }

		protected int moveSetID
		{
			get
			{
				return _moveSetID;
			}
			set
			{
				if (value != _moveSetID || base.animator.GetFloat("MoveSet_ID") != (float)value)
				{
					_moveSetID = value;
					base.animator.SetFloat("MoveSet_ID", _moveSetID, 0.25f, Time.deltaTime);
				}
			}
		}

		protected int attackID
		{
			get
			{
				return _attackID;
			}
			set
			{
				if (value != _attackID)
				{
					_attackID = value;
					base.animator.SetInteger("AttackID", _attackID);
				}
			}
		}

		protected int defenceID
		{
			get
			{
				return _defenceID;
			}
			set
			{
				if (value != _defenceID)
				{
					_defenceID = value;
					base.animator.SetInteger("DefenseID", _defenceID);
				}
			}
		}

		public override bool isArmed
		{
			get
			{
				if (!(MeleeManager != null))
				{
					return false;
				}
				return MeleeManager.rightWeapon != null;
			}
		}

		public vICharacter character
		{
			get
			{
				return this;
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

		Transform vIMeleeFighter.transform
		{
			get
			{
				return base.transform;
			}
		}

		GameObject vIMeleeFighter.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		protected override void Start()
		{
			base.Start();
			MeleeManager = GetComponent<vMeleeManager>();
			if (GetComponent<EnemyAttributes>().expContainer != 0)
			{
				maxHealth *= 2;
			}
			base.currentHealth = maxHealth;
		}

		public override void CreateSecondaryComponents()
		{
			base.CreateSecondaryComponents();
			if (GetComponent<vMeleeManager>() == null)
			{
				base.gameObject.AddComponent<vMeleeManager>();
			}
		}

		public virtual void SetMeleeHitTags(List<string> tags)
		{
			if ((bool)MeleeManager)
			{
				MeleeManager.hitProperties.hitDamageTags = tags;
			}
		}

		public override void Attack(bool strongAttack = false, int _newAttackID = -1, bool forceCanAttack = false)
		{
			if ((bool)MeleeManager && _newAttackID != -1)
			{
				attackID = _newAttackID;
			}
			else
			{
				attackID = MeleeManager.GetAttackID();
			}
			base.Attack(strongAttack, _newAttackID, forceCanAttack);
		}

		protected override void UpdateCombatAnimator()
		{
			base.UpdateCombatAnimator();
			isEquipping = IsAnimatorTag("IsEquipping");
			if ((bool)MeleeManager)
			{
				moveSetID = MeleeManager.GetMoveSetID();
				defenceID = MeleeManager.GetDefenseID();
			}
		}

		protected override void TryBlockAttack(vDamage damage)
		{
			base.TryBlockAttack(damage);
			if (!MeleeManager || !damage.sender)
			{
				return;
			}
			if (isBlocking && MeleeManager.CanBlockAttack(damage.sender.position))
			{
				vIMeleeFighter component = damage.sender.GetComponent<vIMeleeFighter>();
				int defenseRate = MeleeManager.GetDefenseRate();
				if (defenseRate > 0)
				{
					damage.ReduceDamage(defenseRate);
				}
				if (component != null && MeleeManager.CanBreakAttack())
				{
					component.OnRecoil(MeleeManager.GetDefenseRecoilID());
				}
				MeleeManager.OnDefense();
			}
			else
			{
				damage.hitReaction = true;
			}
		}

		protected virtual void TryApplyRecoil(vIMeleeFighter fighter)
		{
			if ((bool)MeleeManager && fighter != null && isBlocking && MeleeManager.CanBlockAttack(fighter.transform.position) && MeleeManager.CanBreakAttack())
			{
				fighter.OnRecoil(MeleeManager.GetDefenseRecoilID());
			}
		}

		public void BreakAttack(int breakAtkID)
		{
			ResetAttackTime();
			ResetAttackTriggers();
			OnRecoil(breakAtkID);
		}

		public void OnRecoil(int recoilID)
		{
			if (base.animator != null && base.animator.enabled && !base.isRolling)
			{
				base.animator.SetInteger("RecoilID", recoilID);
				base.animator.SetTrigger("TriggerRecoil");
				base.animator.SetTrigger("ResetState");
				ResetAttackTriggers();
			}
		}

		public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
		{
			TakeDamage(damage);
			TryApplyRecoil(attacker);
		}
	}
}
