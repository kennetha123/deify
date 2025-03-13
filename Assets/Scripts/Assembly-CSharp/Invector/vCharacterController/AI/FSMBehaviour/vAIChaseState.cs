namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class vAIChaseState : vFSMState
	{
		public bool chaseInStrafe;

		public vAIMovementSpeed chaseSpeed = vAIMovementSpeed.Running;

		public override void UpdateState(vIFSMBehaviourController fsmBehaviour)
		{
			base.UpdateState(fsmBehaviour);
			if (fsmBehaviour != null && fsmBehaviour.aiController.currentTarget.transform != null)
			{
				fsmBehaviour.aiController.SetSpeed(chaseSpeed);
				if (chaseInStrafe)
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.lastTargetPosition, (fsmBehaviour.aiController.lastTargetPosition - fsmBehaviour.aiController.transform.position).normalized);
				}
				else
				{
					fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.lastTargetPosition);
				}
			}
		}
	}
}
