namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vSetDamageSenderAsTarget : vStateAction
	{
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
				return "Set DamageSender as Target";
			}
		}

		public vSetDamageSenderAsTarget()
		{
			executionType = vFSMComponentExecutionType.OnStateEnter;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if ((bool)fsmBehaviour.aiController.receivedDamage.lastSender)
			{
				fsmBehaviour.aiController.SetCurrentTarget(fsmBehaviour.aiController.receivedDamage.lastSender);
			}
		}
	}
}
