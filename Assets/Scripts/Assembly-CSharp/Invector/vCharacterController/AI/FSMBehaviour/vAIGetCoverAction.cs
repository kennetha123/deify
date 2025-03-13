namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAIGetCoverAction : vStateAction
	{
		public vAIMovementSpeed speed = vAIMovementSpeed.Running;

		public override string categoryName
		{
			get
			{
				return "Movement/";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Get Cover";
			}
		}

		public vAIGetCoverAction()
		{
			executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateExit;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			vIControlAICombat vIControlAICombat = fsmBehaviour.aiController as vIControlAICombat;
			if (vIControlAICombat == null)
			{
				return;
			}
			vIControlAICombat.SetSpeed(speed);
			if (executionType == vFSMComponentExecutionType.OnStateUpdate && fsmBehaviour.aiController.HasComponent<vAICover>())
			{
				vAICover aIComponent = fsmBehaviour.aiController.GetAIComponent<vAICover>();
				if ((bool)fsmBehaviour.aiController.currentTarget.transform)
				{
					aIComponent.GetCoverFromTargetThreat();
				}
				else
				{
					aIComponent.GetCoverFromRandomThreat();
				}
			}
			if (executionType == vFSMComponentExecutionType.OnStateExit)
			{
				if (fsmBehaviour.aiController.HasComponent<vAICover>())
				{
					fsmBehaviour.aiController.GetAIComponent<vAICover>().OnExitCover();
				}
				vIControlAICombat.isInCombat = false;
				vIControlAICombat.isCrouching = false;
			}
			if (executionType == vFSMComponentExecutionType.OnStateEnter)
			{
				vIControlAICombat.isInCombat = true;
			}
		}
	}
}
