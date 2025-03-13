using System.Collections;
using Invector.vShooter;
using UnityEngine;

namespace Invector.vCharacterController.v2_5D
{
	[vClassHeader("Shooter 2.5D Input", true, "icon_v2", false, "")]
	public class v2_5DShooterInput : vShooterMeleeInput
	{
		[vEditorToolbar("Default", false, "", false, false)]
		public v2_5DPath path;

		private int forward = 1;

		private Vector2 joystickMousePos;

		private Vector3 lookDirection;

		private v2_5DController _controller;

		public v2_5DController controller
		{
			get
			{
				if ((bool)cc && cc is v2_5DController && _controller == null)
				{
					_controller = cc as v2_5DController;
				}
				return _controller;
			}
		}

		protected override Vector3 targetArmAligmentDirection
		{
			get
			{
				return base.transform.forward;
			}
		}

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
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			base.transform.position = path.ConstraintPosition(base.transform.position);
			cc.RotateToDirection(path.reference.right);
		}

		protected override void FixedUpdate()
		{
			if (!path)
			{
				path = Object.FindObjectOfType<v2_5DPath>();
				if ((bool)path)
				{
					StartCoroutine(InitPath());
				}
			}
			base.FixedUpdate();
			if (!cc.isDead && !cc.ragdolled)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, path.ConstraintPosition(base.transform.position), 80f * Time.deltaTime);
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (!isAiming && !cc.isStrafing && !cc.customAction && (bool)path && cc.input.magnitude > 0.1f)
			{
				cc.RotateToDirection(path.reference.right * cc.input.x);
			}
		}

		protected override bool IsAimAlignWithForward()
		{
			return true;
		}

		protected override void UpdateAimPosition()
		{
			if (!isAiming || !controller)
			{
				return;
			}
			Vector3 lookPos = controller.lookPos;
			Vector3 position = base.transform.InverseTransformPoint(lookPos);
			position.x = 0f;
			if (position.z < -0.2f)
			{
				if (position.z > -0.4f)
				{
					position.z = -0.4f;
				}
				forward *= -1;
			}
			else if (position.z > 0.2f && position.z < 0.4f)
			{
				lookPos.z = 0.3f;
			}
			base.transform.forward = path.reference.right * forward;
			Vector3 vector = base.transform.TransformPoint(position);
			lookDirection = vector - rightUpperArm.position;
			aimPosition = rightUpperArm.position + lookDirection;
			headTrack.SetTemporaryLookPoint(aimPosition);
		}
	}
}
