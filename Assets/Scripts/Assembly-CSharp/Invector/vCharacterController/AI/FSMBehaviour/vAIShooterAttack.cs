using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAIShooterAttack : vStateAction
	{
		public bool useRandomAttackType;

		[vHideInInspector("useRandomAttackType", false)]
		[Tooltip("Use values between 0 and 100")]
		public int chanceToStrongAttack;

		[vHideInInspector("useRandomAttackType", false)]
		public Vector2 minMaxTimeToTryStrongAttack = new Vector2(5f, 10f);

		[vHideInInspector("useRandomAttackType", false, invertValue = true)]
		public bool useStrongAttack;

		public bool overrideAttackID;

		[vHideInInspector("overrideAttackID", false)]
		public int attackID;

		[vHelpBox("Use this to ignore attack time", vHelpBoxAttribute.MessageType.None)]
		public bool forceCanAttack;

		public override string categoryName
		{
			get
			{
				return "Combat/";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Trigger ShooterAttack";
			}
		}

		public vAIShooterAttack()
		{
			executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter | vFSMComponentExecutionType.OnStateExit;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour.aiController is vIControlAIShooter)
			{
				ControlAttack(fsmBehaviour, fsmBehaviour.aiController as vIControlAIShooter);
			}
		}

		protected virtual void ControlAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			switch (executionType)
			{
			case vFSMComponentExecutionType.OnStateEnter:
				InitAttack(combat);
				break;
			case vFSMComponentExecutionType.OnStateUpdate:
				HandleAttack(fsmBehaviour, combat);
				break;
			case vFSMComponentExecutionType.OnStateExit:
				FinishAttack(combat);
				break;
			case vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter:
				break;
			}
		}

		protected virtual void InitAttack(vIControlAIShooter combat)
		{
			combat.isInCombat = true;
			combat.InitAttackTime();
		}

		protected virtual void HandleAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat)
		{
			combat.AimToTarget();
			if (!combat.isAiming)
			{
				return;
			}
			if (useRandomAttackType)
			{
				if (InRandomTimer(fsmBehaviour, minMaxTimeToTryStrongAttack.x, minMaxTimeToTryStrongAttack.y))
				{
					DoAttack(combat, Random.Range(0f, 100f) <= (float)chanceToStrongAttack, overrideAttackID ? attackID : (-1), forceCanAttack);
				}
				else
				{
					DoAttack(combat, false, overrideAttackID ? attackID : (-1), forceCanAttack);
				}
			}
			else
			{
				DoAttack(combat, useStrongAttack, overrideAttackID ? attackID : (-1), forceCanAttack);
			}
		}

		protected virtual void DoAttack(vIControlAIShooter combat, bool isStrongAttack = false, int attackId = -1, bool forceCanAttack = false)
		{
			combat.Attack(isStrongAttack, attackId, forceCanAttack);
		}

		protected virtual void FinishAttack(vIControlAIShooter combat)
		{
			combat.isInCombat = false;
			combat.ResetAttackTime();
		}
	}
}
