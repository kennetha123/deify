namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAISendMessage : vStateAction
	{
		public string listenerName;

		public string message;

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
				return "SendMessage";
			}
		}

		public vAISendMessage()
		{
			executionType = vFSMComponentExecutionType.OnStateEnter;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if ((bool)fsmBehaviour.messageReceiver)
			{
				fsmBehaviour.messageReceiver.Send(listenerName, message);
			}
		}
	}
}
