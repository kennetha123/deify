namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vLookToTargetAction : vStateAction
	{
		public override string categoryName
		{
			get
			{
				return "Controller/";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Look To Target (Headtrack)";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour != null && (bool)fsmBehaviour.aiController.currentTarget.transform && fsmBehaviour.aiController.targetInLineOfSight)
			{
				fsmBehaviour.aiController.LookTo(fsmBehaviour.aiController.lastTargetPosition, 3f, 0f);
			}
		}
	}
}
