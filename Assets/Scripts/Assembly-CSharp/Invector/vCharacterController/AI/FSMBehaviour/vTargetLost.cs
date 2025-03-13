namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vTargetLost : vStateDecision
	{
		public override string categoryName
		{
			get
			{
				return "";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Lost the Target?";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour != null && fsmBehaviour.aiController != null && (bool)fsmBehaviour.aiController.currentTarget.transform)
			{
				return fsmBehaviour.aiController.currentTarget.isLost;
			}
			return true;
		}
	}
}
