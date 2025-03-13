using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
	public class vItemWindowDisplay : MonoBehaviour
	{
		public vInventory inventory;

		public vItemWindow itemWindow;

		public vItemOptionWindow optionWindow;

		[HideInInspector]
		public vItemSlot currentSelectedSlot;

		[HideInInspector]
		public int amount;

		public void OnEnable()
		{
			if (inventory == null)
			{
				inventory = GetComponentInParent<vInventory>();
			}
			if ((bool)inventory && (bool)itemWindow)
			{
				inventory.onLeaveItem.RemoveListener(OnDestroyItem);
				inventory.onLeaveItem.AddListener(OnDestroyItem);
				itemWindow.CreateEquipmentWindow(inventory.items, OnSubmit, OnSelectSlot);
			}
		}

		public void OnDestroyItem(vItem item, int amount)
		{
			vItemSlot vItemSlot2 = itemWindow.slots.Find((vItemSlot slot) => slot.item.Equals(item));
			if (vItemSlot2 != null)
			{
				itemWindow.slots.Remove(vItemSlot2);
				Object.Destroy(vItemSlot2.gameObject);
			}
		}

		public void OnSubmit(vItemSlot slot)
		{
			currentSelectedSlot = slot;
			if ((bool)slot.item)
			{
				RectTransform component = slot.GetComponent<RectTransform>();
				if (optionWindow.CanOpenOptions(slot.item))
				{
					optionWindow.transform.position = component.position;
					optionWindow.gameObject.SetActive(true);
					optionWindow.EnableOptions(slot);
				}
			}
		}

		public void OnSelectSlot(vItemSlot slot)
		{
			currentSelectedSlot = slot;
		}

		public void DropItem()
		{
			if (amount <= 0)
			{
				return;
			}
			inventory.OnDropItem(currentSelectedSlot.item, amount);
			if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
			{
				if (itemWindow.slots.Contains(currentSelectedSlot))
				{
					itemWindow.slots.Remove(currentSelectedSlot);
				}
				Object.Destroy(currentSelectedSlot.gameObject);
				if (itemWindow.slots.Count > 0)
				{
					SetSelectable(itemWindow.slots[0].gameObject);
				}
			}
		}

		public void LeaveItem()
		{
			if (amount <= 0)
			{
				return;
			}
			inventory.OnLeaveItem(currentSelectedSlot.item, amount);
			if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
			{
				if (itemWindow.slots.Contains(currentSelectedSlot))
				{
					itemWindow.slots.Remove(currentSelectedSlot);
				}
				Object.Destroy(currentSelectedSlot.gameObject);
				if (itemWindow.slots.Count > 0)
				{
					SetSelectable(itemWindow.slots[0].gameObject);
				}
			}
		}

		public void UseItem()
		{
			inventory.OnUseItemImmediate(currentSelectedSlot.item);
			if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
			{
				if (itemWindow.slots.Contains(currentSelectedSlot))
				{
					itemWindow.slots.Remove(currentSelectedSlot);
				}
				Object.Destroy(currentSelectedSlot.gameObject);
				if (itemWindow.slots.Count > 0)
				{
					SetSelectable(itemWindow.slots[0].gameObject);
				}
			}
		}

		public void SetOldSelectable()
		{
			try
			{
				if (currentSelectedSlot != null)
				{
					SetSelectable(currentSelectedSlot.gameObject);
				}
				else if (itemWindow.slots.Count > 0 && itemWindow.slots[0] != null)
				{
					SetSelectable(itemWindow.slots[0].gameObject);
				}
			}
			catch
			{
			}
		}

		private void SetSelectable(GameObject target)
		{
			try
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
				EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
				ExecuteEvents.Execute(target, eventData, ExecuteEvents.selectHandler);
			}
			catch
			{
			}
		}
	}
}
