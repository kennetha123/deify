using UnityEngine;

namespace Invector.vCharacterController.v2_5D
{
	[vClassHeader("2.5D CONTROLLER", true, "icon_v2", false, "")]
	public class v2_5DController : vThirdPersonController
	{
		[vEditorToolbar("Layers", false, "", false, false)]
		public LayerMask mouseLayerMask = 1;

		private float inputHorizontal;

		private float inputVertical;

		public Vector3 lookPos
		{
			get
			{
				return vMousePositionHandler.Instance.WorldMousePosition(mouseLayerMask);
			}
		}

		protected override void StrafeLimitSpeed(float value)
		{
			float num = (base.isSprinting ? 1.5f : 1f);
			speed = Mathf.Clamp(base.transform.InverseTransformDirection(Camera.main.transform.right * input.x * num).z, 0f - num, num);
			direction = 0f;
			strafeMagnitude = Mathf.Clamp(new Vector2(speed, direction).magnitude, 0f, value * num);
		}

		protected override void StrafeVelocity(float velocity)
		{
			Vector3 b = base.transform.forward * speed * velocity;
			b.y = _rigidbody.linearVelocity.y;
			_rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, b, 20f * Time.deltaTime);
		}
	}
}
