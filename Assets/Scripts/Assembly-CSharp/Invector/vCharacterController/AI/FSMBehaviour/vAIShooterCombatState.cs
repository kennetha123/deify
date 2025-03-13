using System;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAIShooterCombatState : vAICombatState
	{
		public vAIMovementSpeed engageSpeed = vAIMovementSpeed.Running;

		public override Type requiredType
		{
			get
			{
				return typeof(vIControlAIShooter);
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
				vIControlAICombat obj = fsmBehaviour.aiController as vIControlAICombat;
				obj.ResetAttackTime();
				obj.isInCombat = false;
			}
		}

		protected override void UpdateCombatState(vIControlAICombat controller)
		{
			if (!(controller.currentTarget.transform == null) && controller != null)
			{
				if (controller.targetDistance > controller.attackDistance)
				{
					EngageTarget(controller);
				}
				else
				{
					CombatMovement(controller);
				}
				ControlLookPoint(controller);
				HandleShotAttack(controller);
			}
		}

		protected virtual void HandleShotAttack(vIControlAICombat controller)
		{
			controller.AimToTarget();
			if (controller.canAttack)
			{
				controller.Attack();
			}
		}

		protected virtual void EngageTarget(vIControlAICombat controller)
		{
			if (!(controller.currentTarget.transform == null))
			{
				controller.SetSpeed(engageSpeed);
				if (!controller.animatorStateInfos.HasAnyTag("Attack", "LockMovement", "CustomAction"))
				{
					Vector3 lastTargetPosition = controller.lastTargetPosition;
					controller.StrafeMoveTo(lastTargetPosition, lastTargetPosition - controller.transform.position);
				}
			}
		}

		protected virtual void CombatMovement(vIControlAICombat controller)
		{
			controller.SetSpeed(engageSpeed);
			if (controller.strafeCombatMovement)
			{
				StrafeCombafeMovement(controller);
			}
			else
			{
				SimpleCombatMovement(controller);
			}
		}

		protected virtual void ControlLookPoint(vIControlAICombat controller)
		{
			if (!(controller.currentTarget.transform == null) && controller.currentTarget.hasCollider)
			{
				Vector3 lastTargetPosition = controller.lastTargetPosition;
				controller.LookTo(lastTargetPosition);
			}
		}

		protected virtual void SimpleCombatMovement(vIControlAICombat controller)
		{
			bool flag = controller.targetDistance > controller.combatRange * 0.8f;
			bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
			Vector3 lastTargetPosition = controller.lastTargetPosition;
			Vector3 vector = (lastTargetPosition - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
			controller.StrafeMoveTo(controller.transform.position + vector, (lastTargetPosition - controller.transform.position).normalized);
		}

		protected virtual void StrafeCombafeMovement(vIControlAICombat controller)
		{
			bool flag = controller.targetDistance > controller.combatRange * 0.8f;
			bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
			Vector3 lastTargetPosition = controller.lastTargetPosition;
			Vector3 vector = (lastTargetPosition - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
			controller.StrafeMoveTo(controller.transform.position + controller.transform.right * (controller.stopingDistance + 1f) * controller.strafeCombatSide + vector, (lastTargetPosition - controller.transform.position).normalized);
		}
	}
}
