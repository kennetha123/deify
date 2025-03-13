namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAIFindTargetAction : vStateAction
	{
		public bool checkForObstacles = true;

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
				return "Find Target";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			FindTarget(fsmBehaviour);
		}

		protected virtual void FindTarget(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour != null)
			{
				fsmBehaviour.aiController.FindTarget(checkForObstacles);
			}
		}
	}
}
