using Invector.vEventSystems;
using Invector.vMelee;
using UnityEngine;

namespace Invector.vCharacterController.ClickToMove
{
	public class vMeleeClickToMove : vClickToMoveInput, vIMeleeFighter, vIAttackReceiver, vIAttackListener
	{
		private vMeleeManager meleeManager;

		public bool isBlocking { get; set; }

		public bool isAttacking { get; set; }

		public bool isArmed { get; set; }

		public vICharacter character
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

		protected override void Start()
		{
			base.Start();
			meleeManager = base.gameObject.GetComponent<vMeleeManager>();
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			UpdateMeleeAnimations();
			UpdateAttackBehaviour();
		}

		public override void MoveCharacter(Vector3 position, bool rotateToDirection = true)
		{
			if ((bool)base.target && meleeManager.hitProperties.hitDamageTags.Contains(base.target.gameObject.tag))
			{
				if (Physics.Raycast(cc._capsuleCollider.bounds.center, (base.target.bounds.center - cc._capsuleCollider.bounds.center).normalized, meleeManager.GetAttackDistance()))
				{
					RotateTo((base.target.bounds.center - cc._capsuleCollider.bounds.center).normalized);
					ClearTarget();
					TriggerAttack();
				}
				else
				{
					base.MoveCharacter(position, rotateToDirection);
				}
			}
			else
			{
				base.MoveCharacter(position, rotateToDirection);
			}
		}

		protected virtual void TriggerAttack()
		{
			if (MeleeAttackStaminaConditions())
			{
				base.animator.SetInteger("AttackID", meleeManager.GetAttackID());
				base.animator.SetTrigger("WeakAttack");
			}
		}

		protected virtual void RotateTo(Vector3 direction)
		{
			direction.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(direction, base.transform.up);
		}

		protected virtual bool MeleeAttackStaminaConditions()
		{
			return cc.currentStamina - meleeManager.GetAttackStaminaCost() >= 0f;
		}

		protected virtual void UpdateMeleeAnimations()
		{
			if (!(cc.animator == null) && !(meleeManager == null))
			{
				cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
				cc.animator.SetInteger("DefenseID", meleeManager.GetDefenseID());
				cc.animator.SetBool("IsBlocking", isBlocking);
				cc.animator.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID(), 0.2f, Time.deltaTime);
			}
		}

		protected virtual void UpdateAttackBehaviour()
		{
			if (!cc.IsAnimatorTag("Attack"))
			{
				cc.lockSpeed = cc.IsAnimatorTag("Attack") || isAttacking;
				cc.forceRootMotion = cc.IsAnimatorTag("Attack") || isAttacking;
			}
		}

		public void OnEnableAttack()
		{
			cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
			cc.currentStamina -= meleeManager.GetAttackStaminaCost();
			cc.lockRotation = true;
			isAttacking = true;
		}

		public void OnDisableAttack()
		{
			cc.lockRotation = false;
			isAttacking = false;
		}

		public void ResetAttackTriggers()
		{
			cc.animator.ResetTrigger("WeakAttack");
			cc.animator.ResetTrigger("StrongAttack");
		}

		public void BreakAttack(int breakAtkID)
		{
			ResetAttackTriggers();
			OnRecoil(breakAtkID);
		}

		public void OnRecoil(int recoilID)
		{
			cc.animator.SetInteger("RecoilID", recoilID);
			cc.animator.SetTrigger("TriggerRecoil");
			cc.animator.SetTrigger("ResetState");
			cc.animator.ResetTrigger("WeakAttack");
			cc.animator.ResetTrigger("StrongAttack");
		}

		public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
		{
			if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attacker.character.transform.position))
			{
				int defenseRate = meleeManager.GetDefenseRate();
				if (defenseRate > 0)
				{
					damage.ReduceDamage(defenseRate);
				}
				if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
				{
					attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
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
