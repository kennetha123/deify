using System;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

namespace Invector.vItemManager
{
	public class vControlAreaByInput : MonoBehaviour
	{
		[Serializable]
		public class SlotsSelector
		{
			public GenericInput input;

			public int indexOfSlot;

			public vEquipmentDisplay equipDisplay;
		}

		public List<SlotsSelector> slotsSelectors;

		public vEquipArea equipArea;

		public vInventory inventory;

		private void Start()
		{
			inventory = GetComponentInParent<vInventory>();
		}

		private void Update()
		{
			if (!inventory || !equipArea || inventory.lockInventoryInput)
			{
				return;
			}
			for (int i = 0; i < slotsSelectors.Count; i++)
			{
				if (slotsSelectors[i].input.GetButtonDown() && (bool)inventory && !inventory.isOpen && inventory.canEquip && slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
				{
					equipArea.SetEquipSlot(slotsSelectors[i].indexOfSlot);
				}
				if (slotsSelectors[i].equipDisplay != null && slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0 && equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item != slotsSelectors[i].equipDisplay.item)
				{
					slotsSelectors[i].equipDisplay.AddItem(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item);
				}
			}
		}
	}
}
