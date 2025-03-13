namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vGoToTarget : vStateAction
	{
		public bool useStrafeMovement;

		public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

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
				return "Chase Target";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour.aiController != null)
			{
				if (executionType == vFSMComponentExecutionType.OnStateEnter)
				{
					fsmBehaviour.aiController.ForceUpdatePath(2f);
				}
				fsmBehaviour.aiController.SetSpeed(speed);
				if (useStrafeMovement)
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.lastTargetPosition, fsmBehaviour.aiController.lastTargetPosition - fsmBehaviour.transform.position);
				}
				else
				{
					fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.lastTargetPosition);
				}
			}
		}
	}
}
