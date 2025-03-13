using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class AICheckState : vStateDecision
	{
		[SerializeField]
		[HideInInspector]
		protected int stateIndex;

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
				return "Check FSM State";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			return fsmBehaviour.indexOffCurrentState == stateIndex + 2;
		}
	}
}
