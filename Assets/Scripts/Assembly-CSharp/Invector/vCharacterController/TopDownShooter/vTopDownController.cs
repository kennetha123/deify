using UnityEngine;

namespace Invector.vCharacterController.TopDownShooter
{
	[vClassHeader("TOPDOWN CONTROLLER", true, "icon_v2", false, "")]
	public class vTopDownController : vThirdPersonController
	{
		[vEditorToolbar("Layers", false, "", false, false)]
		public LayerMask mouseLayerMask = 1;

		[vEditorToolbar("Locomotion", false, "", false, false)]
		public bool rotateToMousePoint;

		[HideInInspector]
		public Vector3 lookPos;

		[HideInInspector]
		public Vector3 topDownMove;

		public Vector3 oldTopDownMove;

		private Vector3 camForward;

		private Vector3 lookDirection;

		private float topDownHorizontal;

		private float topDownVertical;

		private Vector2 joystickMousePos;

		private Camera cam;

		public override void Init()
		{
			base.Init();
			cam = Camera.main;
		}

		public override void UpdateMotor()
		{
			base.UpdateMotor();
			UpdateCameraToTopDown();
		}

		public override void ControlLocomotion()
		{
			if (!lockMovement && !(base.currentHealth <= 0f))
			{
				TopDownMovement();
				TopDownRotation();
			}
		}

		protected override void StrafeVelocity(float velocity)
		{
			Vector3 b = new Vector3(topDownMove.x, 0f, topDownMove.z) * ((velocity > 0f) ? velocity : 1f);
			b.y = _rigidbody.linearVelocity.y;
			_rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, b, 20f * Time.deltaTime);
		}

		protected override void StrafeMovement()
		{
			TopDownMovement();
		}

		protected virtual void TopDownMovement()
		{
			if (topDownMove.magnitude > 1f)
			{
				topDownMove.Normalize();
			}
			ConvertToTopDownDirection();
			if (strafeSpeed.walkByDefault)
			{
				TopDownSpeed(0.5f);
			}
			else
			{
				TopDownSpeed(1f);
			}
			if (base.isSprinting)
			{
				speed += 0.5f;
				strafeMagnitude += 0.5f;
			}
			if (base.stopMove)
			{
				speed = 0f;
				strafeMagnitude = 0f;
			}
			base.animator.SetFloat("InputMagnitude", strafeMagnitude, 0.2f, Time.deltaTime);
		}

		protected virtual void UpdateCameraToTopDown()
		{
			lookPos = vMousePositionHandler.Instance.WorldMousePosition(mouseLayerMask);
			if (cam != null)
			{
				camForward = Quaternion.Euler(0f, -90f, 0f) * cam.transform.right;
				oldTopDownMove = topDownMove;
				topDownMove = ((!keepDirection) ? (input.y * camForward + input.x * cam.transform.right) : oldTopDownMove);
			}
			else
			{
				topDownMove = input.y * Vector3.forward + input.x * Vector3.right;
			}
		}

		protected virtual void TopDownRotation()
		{
			if (!customAction && !base.actions)
			{
				if (locomotionType.Equals(LocomotionType.OnlyStrafe) && !base.isStrafing)
				{
					base.isStrafing = true;
				}
				lookDirection = ((!locomotionType.Equals(LocomotionType.OnlyFree) && base.isStrafing) ? (lookPos - base.transform.position) : topDownMove);
				lookDirection.y = 0f;
				if (lookDirection != Vector3.zero)
				{
					Quaternion b = Quaternion.LookRotation(lookDirection);
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, (base.isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed) * Time.deltaTime);
				}
			}
		}

		protected virtual void ConvertToTopDownDirection()
		{
			Vector3 vector = base.transform.InverseTransformDirection(topDownMove);
			topDownHorizontal = vector.x;
			topDownVertical = vector.z;
		}

		protected virtual void TopDownSpeed(float value)
		{
			speed = Mathf.Clamp(topDownVertical, -1f, 1f);
			direction = Mathf.Clamp(topDownHorizontal, -1f, 1f);
			strafeMagnitude = Mathf.Clamp(new Vector2(speed, direction).magnitude, 0f, value);
		}
	}
}
