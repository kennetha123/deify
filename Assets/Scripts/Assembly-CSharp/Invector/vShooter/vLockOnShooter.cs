using Invector.vCharacterController;

namespace Invector.vShooter
{
	[vClassHeader("Shooter Lock-On", true, "icon_v2", false, "")]
	public class vLockOnShooter : vLockOn
	{
		protected vShooterMeleeInput shooterMelee;

		protected override void Start()
		{
			base.Start();
			shooterMelee = tpInput as vShooterMeleeInput;
		}

		protected override void UpdateLockOn(vThirdPersonInput tpInput)
		{
			if (shooterMelee == null || shooterMelee.shooterManager == null || (shooterMelee.shooterManager.useLockOn && shooterMelee.shooterManager.rWeapon != null) || (shooterMelee.shooterManager.useLockOnMeleeOnly && shooterMelee.shooterManager.rWeapon == null))
			{
				base.UpdateLockOn(tpInput);
			}
			else if (shooterMelee.shooterManager.rWeapon != null)
			{
				isLockingOn = false;
				LockOn(false);
				StopLockOn();
				base.aimImage.transform.gameObject.SetActive(false);
			}
		}

		protected override void LockOnInput()
		{
			if (tpInput.tpCamera == null || tpInput.cc == null)
			{
				return;
			}
			if (lockOnInput.GetButtonDown() && !tpInput.cc.actions)
			{
				isLockingOn = !isLockingOn;
				LockOn(isLockingOn);
			}
			else if (isLockingOn && tpInput.tpCamera.lockTarget == null)
			{
				isLockingOn = false;
				LockOn(false);
			}
			if (strafeWhileLockOn && !tpInput.cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
			{
				if (shooterMelee.isAiming || (strafeWhileLockOn && isLockingOn && tpInput.tpCamera.lockTarget != null))
				{
					tpInput.cc.isStrafing = true;
				}
				else
				{
					tpInput.cc.isStrafing = false;
				}
			}
		}
	}
}
