namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAICheckTargetTag : vStateDecision
	{
		public vTagMask targetTags;

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
				return "Check Target Tag";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour.aiController.currentTarget.transform != null)
			{
				return targetTags.Contains(fsmBehaviour.aiController.currentTarget.transform.gameObject.tag);
			}
			return true;
		}
	}
}
