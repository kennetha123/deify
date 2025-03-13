namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vLookToDamageSender : vStateAction
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
				return "Look To Damage Sender (Headtrack)";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if ((bool)fsmBehaviour.aiController.receivedDamage.lastSender)
			{
				fsmBehaviour.aiController.LookToTarget(fsmBehaviour.aiController.receivedDamage.lastSender);
			}
		}
	}
}
