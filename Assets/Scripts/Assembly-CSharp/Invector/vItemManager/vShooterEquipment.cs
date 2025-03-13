using Invector.vShooter;

namespace Invector.vItemManager
{
	[vClassHeader("Shooter Equipment", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "This is a link for ItemManager")]
	public class vShooterEquipment : vMonoBehaviour, vIEquipment
	{
		private vShooterWeapon _weapon;

		private bool withoutWeapon;

		public vItem referenceItem { get; protected set; }

		public vShooterWeapon weapon
		{
			get
			{
				if (!_weapon && !withoutWeapon)
				{
					_weapon = GetComponent<vShooterWeapon>();
					if (!_weapon)
					{
						withoutWeapon = true;
					}
				}
				return _weapon;
			}
		}

		public bool isEquiped { get; protected set; }

		public EquipPoint equipPoint { get; set; }

		public void OnEquip(vItem item)
		{
			if (!weapon)
			{
				return;
			}
			referenceItem = item;
			isEquiped = true;
			weapon.changeAmmoHandle = ChangeAmmo;
			weapon.checkAmmoHandle = CheckAmmo;
			vItemAttribute itemAttribute = item.GetItemAttribute(weapon.isSecundaryWeapon ? vItemAttributes.SecundaryDamage : vItemAttributes.Damage);
			if (itemAttribute != null)
			{
				weapon.maxDamage = itemAttribute.value;
			}
			if (!weapon.secundaryWeapon)
			{
				return;
			}
			vIEquipment[] components = weapon.secundaryWeapon.GetComponents<vIEquipment>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] != null)
				{
					components[i].OnEquip(item);
				}
			}
		}

		private bool CheckAmmo(ref bool isValid, ref int totalAmmo)
		{
			if (!referenceItem)
			{
				return false;
			}
			vItemAttribute itemAttribute = referenceItem.GetItemAttribute(weapon.isSecundaryWeapon ? vItemAttributes.SecundaryAmmoCount : vItemAttributes.AmmoCount);
			isValid = itemAttribute != null && !itemAttribute.isBool;
			if (isValid)
			{
				totalAmmo = itemAttribute.value;
			}
			if (isValid)
			{
				return itemAttribute.value > 0;
			}
			return false;
		}

		private void ChangeAmmo(int value)
		{
			if ((bool)referenceItem)
			{
				vItemAttribute itemAttribute = referenceItem.GetItemAttribute(weapon.isSecundaryWeapon ? vItemAttributes.SecundaryAmmoCount : vItemAttributes.AmmoCount);
				if (itemAttribute != null)
				{
					itemAttribute.value += value;
				}
			}
		}

		public void OnUnequip(vItem item)
		{
			isEquiped = false;
			if (!weapon || !item)
			{
				return;
			}
			weapon.changeAmmoHandle = null;
			weapon.checkAmmoHandle = null;
			if (!weapon.secundaryWeapon)
			{
				return;
			}
			vIEquipment[] components = weapon.secundaryWeapon.GetComponents<vIEquipment>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] != null)
				{
					components[i].OnUnequip(item);
				}
			}
		}
	}
}
