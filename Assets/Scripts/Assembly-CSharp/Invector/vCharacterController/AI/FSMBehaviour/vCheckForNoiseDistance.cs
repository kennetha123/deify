using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vCheckForNoiseDistance : vStateDecision
	{
		protected enum CompareValueMethod
		{
			Greater = 0,
			Less = 1,
			Equal = 2
		}

		public bool findNewNoise;

		public bool specificType;

		[vHideInInspector("findNewNoise;specificType", false)]
		public List<string> noiseTypes;

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
				return "Check For Noise Distance";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour.aiController != null && fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
			{
				vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
				vNoise vNoise = null;
				vNoise = ((!findNewNoise) ? aIComponent.lastListenedNoise : ((!specificType) ? aIComponent.GetNearNoise() : aIComponent.GetNearNoiseByTypes(noiseTypes)));
				if (vNoise != null)
				{
					return CompareDistance(Vector3.Distance(fsmBehaviour.aiController.transform.position, vNoise.position), distance);
				}
			}
			return true;
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
