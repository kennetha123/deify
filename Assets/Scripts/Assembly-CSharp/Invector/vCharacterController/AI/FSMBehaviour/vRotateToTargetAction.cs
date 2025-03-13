namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vRotateToTargetAction : vStateAction
	{
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
				return "Rotate To Target";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour != null && (bool)fsmBehaviour.aiController.currentTarget.transform && fsmBehaviour.aiController.targetInLineOfSight)
			{
				fsmBehaviour.aiController.RotateTo(fsmBehaviour.aiController.lastTargetPosition);
			}
		}
	}
}
