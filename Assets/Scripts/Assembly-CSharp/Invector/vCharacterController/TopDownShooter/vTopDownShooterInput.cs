using Invector.vShooter;
using UnityEngine;

namespace Invector.vCharacterController.TopDownShooter
{
	[vClassHeader("TopDown Shooter Input", true, "icon_v2", false, "")]
	public class vTopDownShooterInput : vShooterMeleeInput
	{
		[vEditorToolbar("Default", false, "", false, false)]
		public bool alwaysAimForward;

		public float aimMinDistance = 2f;

		private vTopDownController _topDown;

		public vTopDownController topDownController
		{
			get
			{
				if ((bool)cc && cc is vTopDownController && _topDown == null)
				{
					_topDown = cc as vTopDownController;
				}
				return _topDown;
			}
		}

		protected override Vector3 targetArmAligmentDirection
		{
			get
			{
				return base.transform.forward;
			}
		}

		protected override Vector3 targetArmAlignmentPosition
		{
			get
			{
				return aimPosition;
			}
		}

		protected override void UpdateAimPosition()
		{
			if (!topDownController)
			{
				base.UpdateAimPosition();
				return;
			}
			Vector3 b = topDownController.lookPos;
			if (Vector3.Distance(cc._capsuleCollider.bounds.center, b) < cc.colliderRadius + aimMinDistance)
			{
				b = base.transform.position + base.transform.forward * (cc.colliderRadius + aimMinDistance);
				if (!alwaysAimForward)
				{
					b.y = base.transform.position.y;
					b += Vector3.up * Vector3.Distance(base.transform.position, rightUpperArm.position);
				}
			}
			if (alwaysAimForward)
			{
				b.y = base.transform.position.y;
				b += Vector3.up * Vector3.Distance(base.transform.position, rightUpperArm.position);
			}
			aimPosition = b;
		}

		protected override void CheckAimConditions()
		{
			if ((bool)shooterManager && !(CurrentActiveWeapon == null))
			{
				RaycastHit hitInfo;
				if (Physics.SphereCast(new Ray(rightUpperArm.position, aimPosition - rightUpperArm.position), shooterManager.checkAimRadius, out hitInfo, shooterManager.minDistanceToAim, shooterManager.blockAimLayer))
				{
					aimConditions = false;
				}
				else
				{
					aimConditions = true;
				}
				aimWeight = Mathf.Lerp(aimWeight, aimConditions ? 1 : 0, 1f * Time.deltaTime);
			}
		}
	}
}
