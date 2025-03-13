using Invector.vCharacterController.vActions;
using Invector.vShooter;
using UnityEngine;

namespace Invector.vMelee
{
	[vClassHeader("Collect Shooter Melee Control", "This component is used when you're character doesn't have a ItemManager to manage items, this will allow you to pickup 1 weapon at the time.")]
	public class vCollectShooterMeleeControl : vCollectMeleeControl
	{
		protected vShooterManager shooterManager;

		protected override void Start()
		{
			base.Start();
			shooterManager = GetComponent<vShooterManager>();
		}

		public override void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
		{
			if ((bool)shooterManager && collectableStandAlone != null && collectableStandAlone.weapon != null)
			{
				vShooterWeapon component = collectableStandAlone.weapon.GetComponent<vShooterWeapon>();
				if ((bool)component)
				{
					Transform transform = null;
					if (component.isLeftWeapon)
					{
						transform = GetEquipPoint(leftHandler, collectableStandAlone.targetEquipPoint);
						if ((bool)transform)
						{
							collectableStandAlone.weapon.transform.SetParent(transform);
							collectableStandAlone.weapon.transform.localPosition = Vector3.zero;
							collectableStandAlone.weapon.transform.localEulerAngles = Vector3.zero;
							if ((bool)leftWeapon && leftWeapon != component.gameObject)
							{
								RemoveLeftWeapon();
							}
							shooterManager.SetLeftWeapon(component.gameObject);
							collectableStandAlone.OnEquip.Invoke();
							leftWeapon = component.gameObject;
							UpdateLeftDisplay(collectableStandAlone);
							if ((bool)rightWeapon)
							{
								RemoveRightWeapon();
							}
						}
					}
					else
					{
						transform = GetEquipPoint(rightHandler, collectableStandAlone.targetEquipPoint);
						if ((bool)transform)
						{
							collectableStandAlone.weapon.transform.SetParent(transform);
							collectableStandAlone.weapon.transform.localPosition = Vector3.zero;
							collectableStandAlone.weapon.transform.localEulerAngles = Vector3.zero;
							if ((bool)rightWeapon && rightWeapon != component.gameObject)
							{
								RemoveRightWeapon();
							}
							shooterManager.SetRightWeapon(component.gameObject);
							collectableStandAlone.OnEquip.Invoke();
							rightWeapon = component.gameObject;
							UpdateRightDisplay(collectableStandAlone);
							if ((bool)leftWeapon)
							{
								RemoveLeftWeapon();
							}
						}
					}
				}
			}
			base.HandleCollectableInput(collectableStandAlone);
		}

		public override void RemoveRightWeapon()
		{
			base.RemoveRightWeapon();
			if ((bool)shooterManager)
			{
				shooterManager.rWeapon = null;
			}
		}

		public override void RemoveLeftWeapon()
		{
			base.RemoveLeftWeapon();
			if ((bool)shooterManager)
			{
				shooterManager.lWeapon = null;
			}
		}
	}
}
