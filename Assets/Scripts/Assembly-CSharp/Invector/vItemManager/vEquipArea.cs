using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vEquipArea : MonoBehaviour
	{
		public delegate void OnPickUpItem(vEquipArea area, vItemSlot slot);

		public OnPickUpItem onPickUpItemCallBack;

		public vInventory inventory;

		public vInventoryWindow rootWindow;

		public vItemWindow itemPicker;

		public Text itemtext;

		public List<vEquipSlot> equipSlots;

		public string equipPointName;

		public OnChangeEquipmentEvent onEquipItem;

		public OnChangeEquipmentEvent onUnequipItem;

		public OnSelectEquipArea onSelectEquipArea;

		[HideInInspector]
		public vEquipSlot currentSelectedSlot;

		[HideInInspector]
		public int indexOfEquipedItem;

		public vItem lastEquipedItem;

		private StringBuilder text;

		public vItem currentEquipedItem
		{
			get
			{
				List<vEquipSlot> validSlots = ValidSlots;
				if (validSlots.Count > 0)
				{
					return validSlots[indexOfEquipedItem].item;
				}
				return null;
			}
		}

		public List<vEquipSlot> ValidSlots
		{
			get
			{
				return equipSlots.FindAll((vEquipSlot slot) => slot.isValid);
			}
		}

		public void Init()
		{
			Start();
		}

		private void Start()
		{
			inventory = GetComponentInParent<vInventory>();
			if (equipSlots.Count == 0)
			{
				vEquipSlot[] componentsInChildren = GetComponentsInChildren<vEquipSlot>(true);
				equipSlots = componentsInChildren.vToList();
			}
			rootWindow = GetComponentInParent<vInventoryWindow>();
			foreach (vEquipSlot equipSlot in equipSlots)
			{
				equipSlot.onSubmitSlotCallBack = OnSubmitSlot;
				equipSlot.onSelectSlotCallBack = OnSelectSlot;
				equipSlot.onDeselectSlotCallBack = OnDeselect;
				equipSlot.amountText.text = "";
			}
		}

		public bool ContainsItem(vItem item)
		{
			return ValidSlots.Find((vEquipSlot slot) => slot.item == item) != null;
		}

		public void OnSubmitSlot(vItemSlot slot)
		{
			if (itemPicker != null)
			{
				currentSelectedSlot = slot as vEquipSlot;
				itemPicker.gameObject.SetActive(true);
				itemPicker.CreateEquipmentWindow(inventory.items, currentSelectedSlot.itemType, slot.item, OnPickItem);
			}
		}

		public void UnequipItem(vEquipSlot slot)
		{
			if ((bool)slot)
			{
				vItem item = slot.item;
				if (ValidSlots[indexOfEquipedItem].item == item)
				{
					lastEquipedItem = item;
				}
				slot.RemoveItem();
				onUnequipItem.Invoke(this, item);
			}
		}

		public void UnequipItem(vItem item)
		{
			vEquipSlot vEquipSlot2 = ValidSlots.Find((vEquipSlot _slot) => _slot.item == item);
			if ((bool)vEquipSlot2)
			{
				if (ValidSlots[indexOfEquipedItem].item == item)
				{
					lastEquipedItem = item;
				}
				vEquipSlot2.RemoveItem();
				onUnequipItem.Invoke(this, item);
			}
		}

		public void UnequipCurrentItem()
		{
			if ((bool)currentSelectedSlot)
			{
				vItem item = currentSelectedSlot.item;
				if (ValidSlots[indexOfEquipedItem].item == item)
				{
					lastEquipedItem = item;
				}
				currentSelectedSlot.RemoveItem();
				onUnequipItem.Invoke(this, item);
			}
		}

		public void OnSelectSlot(vItemSlot slot)
		{
			if (equipSlots.Contains(slot as vEquipSlot))
			{
				currentSelectedSlot = slot as vEquipSlot;
			}
			else
			{
				currentSelectedSlot = null;
			}
			onSelectEquipArea.Invoke(this);
			if (!(itemtext != null))
			{
				return;
			}
			if (slot.item == null)
			{
				itemtext.text = "";
				return;
			}
			this.text = new StringBuilder();
			this.text.Append(slot.item.name + "\n");
			this.text.AppendLine(slot.item.description);
			if (slot.item.attributes != null)
			{
				for (int i = 0; i < slot.item.attributes.Count; i++)
				{
					string text = InsertSpaceBeforeUpperCase(slot.item.attributes[i].name.ToString());
					this.text.AppendLine(text + " : " + slot.item.attributes[i].value);
				}
			}
			itemtext.text = this.text.ToString();
		}

		public string InsertSpaceBeforeUpperCase(string input)
		{
			string text = "";
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				if (char.IsUpper(c) && !string.IsNullOrEmpty(text))
				{
					text += " ";
				}
				text += c;
			}
			return text;
		}

		public void OnDeselect(vItemSlot slot)
		{
			if (equipSlots.Contains(slot as vEquipSlot))
			{
				currentSelectedSlot = null;
			}
		}

		public void OnPickItem(vItemSlot slot)
		{
			if (onPickUpItemCallBack != null)
			{
				onPickUpItemCallBack(this, slot);
			}
			if (currentSelectedSlot.item != null && slot.item != currentSelectedSlot.item)
			{
				currentSelectedSlot.item.isInEquipArea = false;
				vItem item = currentSelectedSlot.item;
				if (item == slot.item)
				{
					lastEquipedItem = item;
				}
				currentSelectedSlot.RemoveItem();
				onUnequipItem.Invoke(this, item);
			}
			if (slot.item != currentSelectedSlot.item)
			{
				currentSelectedSlot.AddItem(slot.item);
				onEquipItem.Invoke(this, currentSelectedSlot.item);
			}
			itemPicker.gameObject.SetActive(false);
		}

		public void NextEquipSlot()
		{
			if (equipSlots != null && equipSlots.Count != 0)
			{
				lastEquipedItem = currentEquipedItem;
				List<vEquipSlot> validSlots = ValidSlots;
				if (indexOfEquipedItem + 1 < validSlots.Count)
				{
					indexOfEquipedItem++;
				}
				else
				{
					indexOfEquipedItem = 0;
				}
				if (currentEquipedItem != null)
				{
					onEquipItem.Invoke(this, currentEquipedItem);
				}
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}

		public void PreviousEquipSlot()
		{
			if (equipSlots != null && equipSlots.Count != 0)
			{
				lastEquipedItem = currentEquipedItem;
				List<vEquipSlot> validSlots = ValidSlots;
				if (indexOfEquipedItem - 1 >= 0)
				{
					indexOfEquipedItem--;
				}
				else
				{
					indexOfEquipedItem = validSlots.Count - 1;
				}
				if (currentEquipedItem != null)
				{
					onEquipItem.Invoke(this, currentEquipedItem);
				}
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}

		public void SetEquipSlot(int indexOfSlot)
		{
			if (equipSlots != null && equipSlots.Count != 0 && indexOfSlot < equipSlots.Count && equipSlots[indexOfSlot].item != currentEquipedItem)
			{
				lastEquipedItem = currentEquipedItem;
				indexOfEquipedItem = indexOfSlot;
				if (currentEquipedItem != null)
				{
					onEquipItem.Invoke(this, currentEquipedItem);
				}
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}

		public void AddItemToEquipSlot(int indexOfSlot, vItem item)
		{
			if (indexOfSlot >= equipSlots.Count || !(item != null))
			{
				return;
			}
			vEquipSlot vEquipSlot2 = equipSlots[indexOfSlot];
			if (!(vEquipSlot2 != null) || !vEquipSlot2.isValid || !vEquipSlot2.itemType.Contains(item.type))
			{
				return;
			}
			if (vEquipSlot2.item != null && vEquipSlot2.item != item)
			{
				if (currentEquipedItem == vEquipSlot2.item)
				{
					lastEquipedItem = vEquipSlot2.item;
				}
				vEquipSlot2.item.isInEquipArea = false;
				onUnequipItem.Invoke(this, item);
			}
			vEquipSlot2.AddItem(item);
			onEquipItem.Invoke(this, item);
		}

		public void RemoveItemOfEquipSlot(int indexOfSlot)
		{
			if (indexOfSlot >= equipSlots.Count)
			{
				return;
			}
			vEquipSlot vEquipSlot2 = equipSlots[indexOfSlot];
			if (vEquipSlot2 != null && vEquipSlot2.item != null)
			{
				vItem item = vEquipSlot2.item;
				item.isInEquipArea = false;
				if (currentEquipedItem == item)
				{
					lastEquipedItem = currentEquipedItem;
				}
				vEquipSlot2.RemoveItem();
				onUnequipItem.Invoke(this, item);
			}
		}

		public void AddCurrentItem(vItem item)
		{
			if (indexOfEquipedItem >= equipSlots.Count)
			{
				return;
			}
			vEquipSlot vEquipSlot2 = equipSlots[indexOfEquipedItem];
			if (vEquipSlot2.item != null && item != vEquipSlot2.item)
			{
				if (currentEquipedItem == vEquipSlot2.item)
				{
					lastEquipedItem = vEquipSlot2.item;
				}
				vEquipSlot2.item.isInEquipArea = false;
				onUnequipItem.Invoke(this, currentSelectedSlot.item);
			}
			vEquipSlot2.AddItem(item);
			onEquipItem.Invoke(this, item);
		}

		public void RemoveCurrentItem()
		{
			if ((bool)currentEquipedItem)
			{
				lastEquipedItem = currentEquipedItem;
				ValidSlots[indexOfEquipedItem].RemoveItem();
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}
	}
}
