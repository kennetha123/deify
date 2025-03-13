using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class AICheckTargetDead : vStateDecision
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
				return "Check if Target is Dead";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			return TargetIsDead(fsmBehaviour);
		}

		protected virtual bool TargetIsDead(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour == null)
			{
				return true;
			}
			if ((Transform)fsmBehaviour.aiController.currentTarget == null)
			{
				return true;
			}
			return fsmBehaviour.aiController.currentTarget.isDead;
		}
	}
}
