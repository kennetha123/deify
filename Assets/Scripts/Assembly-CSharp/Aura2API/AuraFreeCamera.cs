using UnityEngine;

namespace Aura2API
{
	public class AuraFreeCamera : MonoBehaviour
	{
		public bool freeLookEnabled;

		public bool showCursor;

		public float lookSpeed = 5f;

		public float moveSpeed = 5f;

		public float sprintSpeed = 50f;

		private float m_yaw;

		private float m_pitch;

		private void Start()
		{
			m_yaw = base.transform.rotation.eulerAngles.y;
			m_pitch = base.transform.rotation.eulerAngles.x;
			Cursor.visible = showCursor;
		}

		private void Update()
		{
			if (freeLookEnabled)
			{
				m_yaw = (m_yaw + lookSpeed * Input.GetAxis("Mouse X")) % 360f;
				m_pitch = (m_pitch - lookSpeed * Input.GetAxis("Mouse Y")) % 360f;
				base.transform.rotation = Quaternion.AngleAxis(m_yaw, Vector3.up) * Quaternion.AngleAxis(m_pitch, Vector3.right);
				float num = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
				float num2 = num * Input.GetAxis("Vertical");
				float num3 = num * Input.GetAxis("Horizontal");
				float num4 = num * ((Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f));
				base.transform.position += base.transform.forward * num2 + base.transform.right * num3 + Vector3.up * num4;
			}
		}
	}
}
