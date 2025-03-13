using UnityEngine;

namespace Invector
{
	public class vRotateObject : MonoBehaviour
	{
		public Vector3 rotationSpeed;

		private void Update()
		{
			base.transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
		}
	}
}
