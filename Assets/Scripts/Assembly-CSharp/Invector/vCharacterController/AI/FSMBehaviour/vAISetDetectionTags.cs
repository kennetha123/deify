namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAISetDetectionTags : vStateAction
	{
		public vTagMask tags;

		public override string categoryName
		{
			get
			{
				return "Detection/";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Set Detections Tags";
			}
		}

		public vAISetDetectionTags()
		{
			executionType = vFSMComponentExecutionType.OnStateEnter;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (executionType == vFSMComponentExecutionType.OnStateEnter)
			{
				fsmBehaviour.aiController.SetDetectionTags(tags);
			}
		}
	}
}
