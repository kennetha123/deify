using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("Trigger Change Camera State", true, "icon_v2", false, "", helpBoxText = "Leave CameraState empty to return the original state system of the Player", useHelpBox = true)]
	public class vTriggerChangeCameraState : vMonoBehaviour
	{
		[Tooltip("Check if you want to lerp the state transitions, you can change the lerp value on the TPCamera - Smooth Follow variable")]
		public bool smoothTransition = true;

		public bool keepDirection = true;

		[Tooltip("Check your CameraState List and set the State here, use the same String value.\n*Leave this field empty to return the original state")]
		public string cameraState;

		[Tooltip("Set a new target for the camera.\n*Leave this field empty to return the original target (Player)")]
		public string customCameraPoint;

		public Color gizmoColor = Color.green;

		private Component comp;

		private vThirdPersonInput tpInput;

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				if (tpInput == null || tpInput.gameObject != other.gameObject)
				{
					tpInput = other.GetComponent<vThirdPersonInput>();
				}
				if ((bool)tpInput)
				{
					tpInput.smoothCameraState = smoothTransition;
					tpInput.customCameraState = cameraState;
					tpInput.customlookAtPoint = customCameraPoint;
					tpInput.changeCameraState = !string.IsNullOrEmpty(cameraState);
					tpInput.keepDirection = keepDirection;
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = gizmoColor;
			comp = base.gameObject.GetComponent<BoxCollider>();
			if (comp != null)
			{
				base.gameObject.GetComponent<BoxCollider>().isTrigger = true;
				base.gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
				base.gameObject.GetComponent<BoxCollider>().size = Vector3.one;
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			if (comp == null)
			{
				base.gameObject.AddComponent<BoxCollider>();
			}
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}

		private Vector3 getLargerScale(Vector3 value)
		{
			if (value.x > value.y || value.x > value.z)
			{
				return new Vector3(value.x, value.x, value.x);
			}
			if (value.y > value.x || value.y > value.z)
			{
				return new Vector3(value.y, value.y, value.y);
			}
			if (value.z > value.y || value.z > value.x)
			{
				return new Vector3(value.z, value.z, value.z);
			}
			return base.transform.localScale;
		}
	}
}
