using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.v2_5D
{
	[vClassHeader("2.5D INPUT", true, "icon_v2", false, "")]
	public class v2_5DInput : vThirdPersonInput
	{
		public v2_5DPath path;

		private Vector2 joystickMousePos;

		private Vector3 lookDirection;

		protected override void Start()
		{
			base.Start();
			path = Object.FindObjectOfType<v2_5DPath>();
			if ((bool)path)
			{
				StartCoroutine(InitPath());
			}
		}

		private IEnumerator InitPath()
		{
			path.Init();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			base.transform.position = path.ConstraintPosition(base.transform.position);
			cc.RotateToDirection(path.reference.right);
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!path)
			{
				path = Object.FindObjectOfType<v2_5DPath>();
				if ((bool)path)
				{
					StartCoroutine(InitPath());
				}
			}
			if (!cc.isDead && !cc.ragdolled)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, path.ConstraintPosition(base.transform.position), 80f * Time.deltaTime);
			}
			base.transform.position = path.ConstraintPosition(base.transform.position);
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (!cc.isStrafing && (bool)path && cc.input.magnitude > 0.1f)
			{
				cc.RotateToDirection(path.reference.right * cc.input.x);
			}
		}
	}
}
