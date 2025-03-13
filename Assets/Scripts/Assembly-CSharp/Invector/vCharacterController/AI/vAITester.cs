using UnityEngine;

namespace Invector.vCharacterController.AI
{
	public class vAITester : MonoBehaviour
	{
		public vControlAI ai;

		public Transform target;

		public void MoveToTarget()
		{
			ai.MoveTo(target.position);
			ai.SetSpeed(vAIMovementSpeed.Running);
		}

		public void Stop()
		{
			ai.Stop();
		}

		public void LookToTarget()
		{
			ai.LookToTarget(target, 2f, 0f);
		}
	}
}
