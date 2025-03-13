using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAISetDetectionLayer : vStateAction
	{
		public LayerMask newLayer;

		public override string categoryName
		{
			get
			{
				return "Detection/";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Set Detections Layer";
			}
		}

		public vAISetDetectionLayer()
		{
			executionType = vFSMComponentExecutionType.OnStateEnter;
		}

		public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
		{
			if (executionType == vFSMComponentExecutionType.OnStateEnter)
			{
				fsmBehaviour.aiController.SetDetectionLayer(newLayer);
			}
		}
	}
}
