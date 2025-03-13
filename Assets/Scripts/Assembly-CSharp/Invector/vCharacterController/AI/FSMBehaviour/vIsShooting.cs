namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vIsShooting : vStateDecision
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
				return "Is Shooting?";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour.aiController is vIControlAIShooter)
			{
				vIControlAIShooter vIControlAIShooter = fsmBehaviour.aiController as vIControlAIShooter;
				if (!vIControlAIShooter.shooterManager)
				{
					return false;
				}
				if (!vIControlAIShooter.shooterManager.isShooting)
				{
					return vIControlAIShooter.isAttacking;
				}
				return true;
			}
			return true;
		}
	}
}
