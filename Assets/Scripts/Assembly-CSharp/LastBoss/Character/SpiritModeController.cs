using Invector.vCharacterController;
using UnityEngine;

namespace LastBoss.Character
{
	public class SpiritModeController : MonoBehaviour
	{
		public int maxDistance;

		[HideInInspector]
		public int distancePower;

		public float maxTimer;

		[HideInInspector]
		public float timerDecrease;

		private RevengeMode revenge;

		private vThirdPersonInput inp;

		public int moveSpeed;

		private GameObject mainCam;

		private Vector2 input;

		[HideInInspector]
		public Quaternion playerRotation = Quaternion.identity;

		private void LateUpdate()
		{
			playerRotation.eulerAngles = new Vector3(0f, mainCam.transform.rotation.eulerAngles.y, 0f);
			base.transform.rotation = playerRotation;
		}

		private void Start()
		{
			inp = GameObject.FindGameObjectWithTag("Player").GetComponent<vThirdPersonInput>();
			mainCam = GameObject.FindGameObjectWithTag("MainCamera");
			revenge = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>();
			timerDecrease = maxTimer;
			distancePower = maxDistance;
		}

		private void Update()
		{
			timerDecrease -= Time.deltaTime;
			if (timerDecrease < 0f)
			{
				revenge.GetoutSpiritMode();
			}
			if (inp.horizontalInput.GetAxis() != 0f || inp.verticallInput.GetAxis() != 0f)
			{
				distancePower--;
			}
			if (distancePower < 0)
			{
				revenge.GetoutSpiritMode();
			}
		}

		private void FixedUpdate()
		{
			base.transform.Translate(new Vector3(inp.horizontalInput.GetAxis(), 0f, inp.verticallInput.GetAxis()) * Time.deltaTime * moveSpeed);
		}
	}
}
