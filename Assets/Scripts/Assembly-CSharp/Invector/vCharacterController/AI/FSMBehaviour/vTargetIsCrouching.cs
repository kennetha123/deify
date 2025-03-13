namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vTargetIsCrouching : vStateDecision
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
				return "Target Is Crouching?";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour.aiController == null || !fsmBehaviour.aiController.currentTarget.isCharacter)
			{
				return false;
			}
			return fsmBehaviour.aiController.currentTarget.character.isCrouching;
		}
	}
}
