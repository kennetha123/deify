namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vPlayAnimationAction : vStateAction
	{
		public string _animationState;

		public int _layer;

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
				return "Play Animation";
			}
		}

		public vPlayAnimationAction()
		{
			executionType = vFSMComponentExecutionType.OnStateEnter;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			fsmBehaviour.aiController.animator.Play(_animationState, _layer);
		}
	}
}
