using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vCheckTargetDistance : vStateDecision
	{
		protected enum CompareValueMethod
		{
			Greater = 0,
			Less = 1,
			Equal = 2
		}

		[SerializeField]
		protected CompareValueMethod compareMethod;

		public float distance;

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
				return "Check Target Distance";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (!fsmBehaviour.aiController.currentTarget.transform)
			{
				return false;
			}
			float targetDistance = fsmBehaviour.aiController.targetDistance;
			return CompareDistance(targetDistance, distance);
		}

		private bool CompareDistance(float distA, float distB)
		{
			switch (compareMethod)
			{
			case CompareValueMethod.Equal:
				return distA.Equals(distB);
			case CompareValueMethod.Greater:
				return distA > distB;
			case CompareValueMethod.Less:
				return distA < distB;
			default:
				return false;
			}
		}
	}
}
