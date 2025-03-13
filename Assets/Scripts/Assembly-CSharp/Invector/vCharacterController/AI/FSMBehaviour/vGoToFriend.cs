namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vGoToFriend : vStateAction
	{
		public vAIMovementSpeed speed = vAIMovementSpeed.Running;

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
				return "Go To Friend(Companion AI)";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour.aiController.HasComponent<vAICompanion>())
			{
				fsmBehaviour.aiController.SetSpeed(speed);
				MoveToFriendPosition(fsmBehaviour.aiController.GetAIComponent<vAICompanion>());
			}
		}

		public virtual void MoveToFriendPosition(vAICompanion aICompanion)
		{
			if ((bool)aICompanion)
			{
				aICompanion.GoToFriend();
			}
		}
	}
}
