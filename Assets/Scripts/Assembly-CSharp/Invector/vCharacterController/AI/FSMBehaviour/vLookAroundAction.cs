namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vLookAroundAction : vStateAction
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
				return "Look Around (Headtrack)";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			fsmBehaviour.aiController.LookAround();
		}
	}
}
