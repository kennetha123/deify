using UnityEngine;

namespace Invector.vShooter
{
	[vClassHeader("Draw/Hide Shooter Melee Weapons", "This component works with vItemManager, vWeaponHolderManager and vShooterMeleeInput", useHelpBox = true)]
	public class vDrawHideShooterWeapons : vDrawHideMeleeWeapons
	{
		private vShooterMeleeInput shooter;

		protected override void Start()
		{
			base.Start();
			shooter = GetComponent<vShooterMeleeInput>();
		}

		protected override bool CanHideWeapons()
		{
			if (!shooter || !shooter.shooterManager || !shooter.shooterManager.CurrentWeapon || (!forceHide && (shooter.isAiming || !(shooter.aimTimming <= 0f) || shooter.isReloading)))
			{
				if (base.CanHideWeapons())
				{
					if (!forceHide)
					{
						if (!shooter.isAiming && shooter.aimTimming <= 0f)
						{
							return !shooter.isReloading;
						}
						return false;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		protected override bool CanDrawWeapons()
		{
			if (!shooter || !shooter.shooterManager || !shooter.shooterManager.CurrentWeapon || shooter.shooterManager.CurrentWeapon.gameObject.activeInHierarchy)
			{
				return base.CanDrawWeapons();
			}
			return true;
		}

		protected override GameObject RightWeaponObject(bool checkIsActve = false)
		{
			if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.rWeapon && (!checkIsActve || shooter.shooterManager.rWeapon.gameObject.activeInHierarchy))
			{
				return shooter.shooterManager.rWeapon.gameObject;
			}
			return base.RightWeaponObject(checkIsActve);
		}

		protected override GameObject LeftWeaponObject(bool checkIsActve = false)
		{
			if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.lWeapon && (!checkIsActve || shooter.shooterManager.lWeapon.gameObject.activeInHierarchy))
			{
				return shooter.shooterManager.lWeapon.gameObject;
			}
			return base.LeftWeaponObject(checkIsActve);
		}

		protected override void DrawRightWeapon(bool immediate = false)
		{
			base.DrawRightWeapon(immediate);
		}

		protected override bool DrawWeaponsImmediateConditions()
		{
			if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.CurrentWeapon)
			{
				return DrawShooterWeaponImmediateConditions();
			}
			return base.DrawWeaponsImmediateConditions();
		}

		protected virtual bool DrawShooterWeaponImmediateConditions()
		{
			if (!shooter || !shooter.shooterManager || shooter.cc.customAction || !shooter.shooterManager.CurrentWeapon)
			{
				return false;
			}
			if (shooter.CurrentActiveWeapon == null && (shooter.aimInput.GetButtonDown() || (shooter.shooterManager.hipfireShot && shooter.shotInput.GetButtonDown())))
			{
				return true;
			}
			return false;
		}

		protected override void HandleInput()
		{
			base.HandleInput();
			HandleShooterInput();
		}

		protected virtual void HandleShooterInput()
		{
			if ((bool)shooter && (bool)shooter.shooterManager && !shooter.cc.customAction && (bool)shooter.shooterManager.CurrentWeapon && shooter.CurrentActiveWeapon == null && !shooter.isAiming && !shooter.shooterManager.hipfireShot && shooter.shotInput.GetButtonDown() && !IsEquipping)
			{
				if (CanHideRightWeapon() || CanHideLeftWeapon())
				{
					HideWeapons();
				}
				else if (CanDrawRightWeapon() || CanDrawLeftWeapon())
				{
					DrawWeapons();
				}
			}
		}
	}
}
