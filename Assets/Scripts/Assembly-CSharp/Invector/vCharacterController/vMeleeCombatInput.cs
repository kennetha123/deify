using Invector.vEventSystems;
using Invector.vMelee;
using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("MELEE INPUT MANAGER", true, "icon_v2", false, "", iconName = "inputIcon")]
	public class vMeleeCombatInput : vThirdPersonInput, vIMeleeFighter, vIAttackReceiver, vIAttackListener
	{
		[vEditorToolbar("Inputs", false, "", false, false)]
		[Header("Melee Inputs")]
		public GenericInput weakAttackInput = new GenericInput("Mouse0", "RB", "RB");

		public GenericInput strongAttackInput = new GenericInput("Alpha1", false, "RT", true, "RT", false);

		public GenericInput blockInput = new GenericInput("Mouse1", "LB", "LB");

		internal vMeleeManager meleeManager;

		[HideInInspector]
		public bool lockMeleeInput;

		[HideInInspector]
		public vThirdPersonController tps;

		public bool isAttacking { get; protected set; }

		public bool isBlocking { get; protected set; }

		public bool isArmed
		{
			get
			{
				if (meleeManager != null)
				{
					if (!(meleeManager.rightWeapon != null))
					{
						if (meleeManager.leftWeapon != null)
						{
							return meleeManager.leftWeapon.meleeType != vMeleeType.OnlyDefense;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public virtual bool lockInventory
		{
			get
			{
				if (!isAttacking)
				{
					return cc.isDead;
				}
				return true;
			}
		}

		protected virtual bool MeleeAttackConditions
		{
			get
			{
				if (meleeManager == null)
				{
					meleeManager = GetComponent<vMeleeManager>();
				}
				if (meleeManager != null && !cc.customAction && !cc.lockMovement)
				{
					return !cc.isCrouching;
				}
				return false;
			}
		}

		public virtual vICharacter character
		{
			get
			{
				return cc;
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

		public void SetLockMeleeInput(bool value)
		{
			lockMeleeInput = value;
			if (value)
			{
				isAttacking = false;
				isBlocking = false;
				cc.isStrafing = false;
			}
		}

		protected override void FixedUpdate()
		{
			UpdateMeleeAnimations();
			UpdateAttackBehaviour();
			base.FixedUpdate();
		}

		protected override void InputHandle()
		{
			if (!(cc == null))
			{
				if (MeleeAttackConditions && !lockMeleeInput)
				{
					MeleeWeakAttackInput();
					MeleeStrongAttackInput();
					BlockingInput();
				}
				else
				{
					isBlocking = false;
				}
				if (!isAttacking)
				{
					base.InputHandle();
				}
				onUpdateInput.Invoke(this);
			}
		}

		public virtual void MeleeWeakAttackInput()
		{
			tps = GetComponent<vThirdPersonController>();
			if (!(cc.animator == null) && weakAttackInput.GetButtonDown() && cc.currentStamina > 0f && tps.isGrounded && !sprint && !base.animator.GetBool("attackAfterRoll"))
			{
				TriggerWeakAttack();
				Object.FindObjectOfType<vHUDController>().SetStaminaDelay();
				cc.OnRoll.Invoke();
			}
		}

		public virtual void TriggerWeakAttack()
		{
			cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
			cc.animator.SetTrigger("WeakAttack");
		}

		public virtual void MeleeStrongAttackInput()
		{
			if (!(cc.animator == null) && strongAttackInput.GetButtonDown() && (!meleeManager.CurrentActiveAttackWeapon || meleeManager.CurrentActiveAttackWeapon.useStrongAttack) && MeleeAttackStaminaConditions())
			{
				TriggerStrongAttack();
				Object.FindObjectOfType<vHUDController>().SetStaminaDelay();
			}
		}

		public virtual void TriggerStrongAttack()
		{
			cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
			cc.animator.SetTrigger("StrongAttack");
		}

		public virtual void BlockingInput()
		{
			if (!(cc.animator == null))
			{
				isBlocking = blockInput.GetButton() && cc.currentStamina > 0f;
			}
		}

		protected virtual bool MeleeAttackStaminaConditions()
		{
			return cc.currentStamina - meleeManager.GetAttackStaminaCost() >= 0f;
		}

		protected virtual void UpdateMeleeAnimations()
		{
			if (!(cc.animator == null) && !(meleeManager == null) && !cc.isRolling && !cc.continueRoll)
			{
				cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
				cc.animator.SetInteger("DefenseID", meleeManager.GetDefenseID());
				cc.animator.SetBool("IsBlocking", isBlocking);
				cc.animator.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID(), 0.2f, Time.deltaTime);
			}
		}

		protected virtual void UpdateAttackBehaviour()
		{
			cc.lockSpeed = cc.IsAnimatorTag("Attack") || isAttacking;
			cc.forceRootMotion = cc.IsAnimatorTag("Attack") || isAttacking;
		}

		public virtual void OnEnableAttack()
		{
			if (meleeManager == null)
			{
				meleeManager = GetComponent<vMeleeManager>();
			}
			if (!(meleeManager == null))
			{
				cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
				cc.lockRotation = true;
				isAttacking = true;
			}
		}

		public virtual void OnDisableAttack()
		{
			cc.lockRotation = false;
			isAttacking = false;
		}

		public virtual void ResetAttackTriggers()
		{
			cc.animator.ResetTrigger("WeakAttack");
			cc.animator.ResetTrigger("StrongAttack");
		}

		public virtual void BreakAttack(int breakAtkID)
		{
			ResetAttackTriggers();
			OnRecoil(breakAtkID);
		}

		public virtual void OnRecoil(int recoilID)
		{
			cc.animator.SetInteger("RecoilID", recoilID);
			cc.animator.SetTrigger("TriggerRecoil");
			cc.animator.SetTrigger("ResetState");
			cc.animator.ResetTrigger("WeakAttack");
			cc.animator.ResetTrigger("StrongAttack");
		}

		public virtual void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
		{
			if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(damage.sender.position))
			{
				int defenseRate = meleeManager.GetDefenseRate();
				if (defenseRate > 0)
				{
					damage.ReduceDamage(defenseRate);
				}
				if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
				{
					attacker.BreakAttack(meleeManager.GetDefenseRecoilID());
				}
				meleeManager.OnDefense();
				cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
				cc.currentStamina -= damage.staminaBlockCost;
			}
			damage.hitReaction = !isBlocking;
			cc.TakeDamage(damage);
		}
	}
}
