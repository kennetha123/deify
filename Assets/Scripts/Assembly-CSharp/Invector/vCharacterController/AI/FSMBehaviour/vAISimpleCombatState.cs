using System;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAISimpleCombatState : vAICombatState, vIStateAttackListener
	{
		public bool engageInStrafe;

		public vAIMovementSpeed engageSpeed = vAIMovementSpeed.Running;

		public vAIMovementSpeed combatSpeed = vAIMovementSpeed.Walking;

		public override Type requiredType
		{
			get
			{
				return typeof(vIControlAIMelee);
			}
		}

		public override void OnStateEnter(vIFSMBehaviourController fsmBehaviour)
		{
			base.OnStateEnter(fsmBehaviour);
			if (fsmBehaviour.aiController is vIControlAICombat)
			{
				vIControlAICombat obj = fsmBehaviour.aiController as vIControlAICombat;
				obj.InitAttackTime();
				obj.isInCombat = true;
			}
		}

		public override void OnStateExit(vIFSMBehaviourController fsmBehaviour)
		{
			base.OnStateExit(fsmBehaviour);
			if (fsmBehaviour.aiController is vIControlAICombat)
			{
				vIControlAICombat vIControlAICombat = fsmBehaviour.aiController as vIControlAICombat;
				if (vIControlAICombat.currentTarget.transform == null || vIControlAICombat.currentTarget.isDead || !vIControlAICombat.targetInLineOfSight)
				{
					vIControlAICombat.ResetAttackTime();
				}
				vIControlAICombat.isInCombat = false;
			}
		}

		protected override void UpdateCombatState(vIControlAICombat controller)
		{
			if (!(controller.currentTarget.transform == null) && !controller.currentTarget.isLost && controller != null)
			{
				if (controller.canAttack)
				{
					EngageTarget(controller);
				}
				else
				{
					CombatMovement(controller);
				}
				ControlLookPoint(controller);
			}
		}

		protected virtual void EngageTarget(vIControlAICombat controller)
		{
			if (controller.currentTarget.transform == null)
			{
				return;
			}
			controller.SetSpeed(engageSpeed);
			if (controller.targetDistance <= controller.attackDistance)
			{
				controller.Stop();
				controller.Attack();
			}
			else if (!controller.animatorStateInfos.HasAnyTag("Attack", "LockMovement", "CustomAction"))
			{
				if (engageInStrafe)
				{
					controller.StrafeMoveTo(controller.currentTarget.transform.position, (controller.currentTarget.transform.position - controller.transform.position).normalized);
				}
				else
				{
					controller.MoveTo(controller.currentTarget.transform.position);
				}
			}
			else
			{
				controller.Stop();
			}
		}

		protected virtual void CombatMovement(vIControlAICombat controller)
		{
			controller.SetSpeed(combatSpeed);
			if (controller.strafeCombatMovement)
			{
				StrafeCombatMovement(controller);
			}
			else
			{
				SimpleCombatMovement(controller);
			}
			if (controller.canBlockInCombat)
			{
				controller.Blocking();
			}
		}

		protected virtual void ControlLookPoint(vIControlAICombat controller)
		{
			if (!(controller.currentTarget.transform == null) && controller.currentTarget.hasCollider && controller.targetInLineOfSight)
			{
				controller.LookTo(controller.currentTarget.transform.position);
			}
		}

		protected virtual void SimpleCombatMovement(vIControlAICombat controller)
		{
			bool flag = controller.targetDistance > controller.combatRange * 0.8f;
			bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
			Vector3 vector = (controller.currentTarget.transform.position - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
			controller.StrafeMoveTo(controller.transform.position + vector, (controller.currentTarget.transform.position - controller.transform.position).normalized);
		}

		protected virtual void StrafeCombatMovement(vIControlAICombat controller)
		{
			bool flag = controller.targetDistance > controller.combatRange * 0.8f;
			bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
			Vector3 vector = (controller.currentTarget.transform.position - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
			controller.StrafeMoveTo(controller.transform.position + controller.transform.right * (controller.stopingDistance + 1f) * controller.strafeCombatSide + vector, (controller.currentTarget.transform.position - controller.transform.position).normalized);
		}

		public virtual void OnReceiveAttack(vIControlAICombat controller, ref vDamage damage, vIMeleeFighter attacker, ref bool canBlock)
		{
			if (damage.damageValue > 0 && attacker != null && attacker.character != null && !attacker.character.isDead)
			{
				controller.SetCurrentTarget(attacker.transform);
			}
		}
	}
}
