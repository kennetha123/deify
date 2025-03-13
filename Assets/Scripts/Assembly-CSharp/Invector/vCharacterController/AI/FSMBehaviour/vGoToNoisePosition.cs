using System.Collections.Generic;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vGoToNoisePosition : vStateAction
	{
		public bool findNewNoise;

		public bool specificType;

		[vHideInInspector("findNewNoise;specificType", false)]
		public List<string> noiseTypes;

		public bool lookToNoisePosition = true;

		public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

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
				return "Go To Noise Position";
			}
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (fsmBehaviour.aiController == null || !fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
			{
				return;
			}
			fsmBehaviour.aiController.SetSpeed(speed);
			vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
			vNoise vNoise = null;
			vNoise = ((!findNewNoise) ? aIComponent.lastListenedNoise : ((!specificType) ? aIComponent.GetNearNoise() : aIComponent.GetNearNoiseByTypes(noiseTypes)));
			if (vNoise != null)
			{
				fsmBehaviour.aiController.MoveTo(vNoise.position);
				if (lookToNoisePosition)
				{
					fsmBehaviour.aiController.LookTo(vNoise.position, 1f, 0f);
				}
			}
		}
	}
}
