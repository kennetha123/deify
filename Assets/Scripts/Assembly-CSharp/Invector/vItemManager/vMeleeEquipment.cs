using Invector.vMelee;

namespace Invector.vItemManager
{
	[vClassHeader("Melee Equipment", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "This is a link for ItemManager")]
	public class vMeleeEquipment : vMonoBehaviour, vIEquipment
	{
		private vMeleeWeapon _weapon;

		private bool withoutWeapon;

		public vMeleeWeapon weapon
		{
			get
			{
				if (!_weapon && !withoutWeapon)
				{
					_weapon = GetComponent<vMeleeWeapon>();
					if (!_weapon)
					{
						withoutWeapon = true;
					}
				}
				return _weapon;
			}
		}

		public vItem referenceItem { get; protected set; }

		public bool isEquiped { get; protected set; }

		public EquipPoint equipPoint { get; set; }

		public void OnEquip(vItem item)
		{
			referenceItem = item;
			isEquiped = true;
			if ((bool)weapon)
			{
				vItemAttribute itemAttribute = item.GetItemAttribute(vItemAttributes.Damage);
				vItemAttribute itemAttribute2 = item.GetItemAttribute(vItemAttributes.StaminaCost);
				vItemAttribute itemAttribute3 = item.GetItemAttribute(vItemAttributes.DefenseRate);
				vItemAttribute itemAttribute4 = item.GetItemAttribute(vItemAttributes.DefenseRange);
				if (itemAttribute != null)
				{
					weapon.damage.damageValue = itemAttribute.value;
				}
				if (itemAttribute2 != null)
				{
					weapon.staminaCost = itemAttribute2.value;
				}
				if (itemAttribute3 != null)
				{
					weapon.defenseRate = itemAttribute3.value;
				}
				if (itemAttribute4 != null)
				{
					weapon.defenseRange = itemAttribute3.value;
				}
			}
		}

		public void OnUnequip(vItem item)
		{
			isEquiped = false;
		}
	}
}
