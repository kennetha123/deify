using UnityEngine;

namespace EnviroSamples
{
	public class FPSController : MonoBehaviour
	{
		public float speed = 2f;

		public float sensitivity = 2f;

		private CharacterController player;

		public GameObject eyes;

		private float moveFB;

		private float moveLR;

		private float rotX;

		private float rotY;

		private void Start()
		{
			player = GetComponent<CharacterController>();
		}

		private void Update()
		{
			moveFB = Input.GetAxis("Vertical") * speed;
			moveLR = Input.GetAxis("Horizontal") * speed;
			rotX = Input.GetAxis("Mouse X") * sensitivity;
			rotY -= Input.GetAxis("Mouse Y") * sensitivity;
			rotY = Mathf.Clamp(rotY, -60f, 60f);
			Vector3 vector = new Vector3(moveLR, 0f, moveFB);
			base.transform.Rotate(0f, rotX, 0f);
			eyes.transform.localRotation = Quaternion.Euler(rotY, 0f, 0f);
			vector = base.transform.rotation * vector;
			vector.y -= 4000f * Time.deltaTime;
			player.Move(vector * Time.deltaTime);
		}
	}
}
