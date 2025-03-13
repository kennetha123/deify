namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vFSMChangeBehaviour : vStateAction
	{
		public vFSMBehaviour newBehaviour;

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
				return "Change FSM Behaviour";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			fsmBehaviour.ChangeBehaviour(newBehaviour);
		}
	}
}
