using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vItemWindow : MonoBehaviour
	{
		public vItemSlot slotPrefab;

		public RectTransform contentWindow;

		public Text itemtext;

		private OnSubmitSlot onSubmitSlot;

		private OnSelectSlot onSelectSlot;

		public OnCompleteSlotList onCompleteSlotListCallBack;

		public List<vItemSlot> slots;

		private vItem currentItem;

		private StringBuilder text;

		public List<vItemType> supportedItems;

		public bool updateSlotCount = true;

		public void CreateEquipmentWindow(List<vItem> items, OnSubmitSlot onPickUpItemCallBack = null, OnSelectSlot onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
		{
			List<vItem> list = ((supportedItems.Count == 0) ? items : items.FindAll((vItem i) => supportedItems.Contains(i.type)));
			if (list.Count == 0)
			{
				if ((bool)itemtext)
				{
					itemtext.text = "";
				}
				if (slots.Count > 0 && destroyAdictionSlots)
				{
					for (int j = 0; j < slots.Count; j++)
					{
						Object.Destroy(slots[j].gameObject);
					}
					slots.Clear();
				}
				return;
			}
			bool flag = false;
			onSubmitSlot = onPickUpItemCallBack;
			onSelectSlot = onSelectSlotCallBack;
			if (slots == null)
			{
				slots = new List<vItemSlot>();
			}
			int count = slots.Count;
			if (updateSlotCount)
			{
				if (count < list.Count)
				{
					for (int k = count; k < list.Count; k++)
					{
						vItemSlot vItemSlot2 = Object.Instantiate(slotPrefab);
						slots.Add(vItemSlot2);
						RectTransform component = vItemSlot2.GetComponent<RectTransform>();
						component.SetParent(contentWindow);
						component.localPosition = Vector3.zero;
						component.localScale = Vector3.one;
					}
				}
				else if (count > list.Count)
				{
					for (int num = count - 1; num > list.Count - 1; num--)
					{
						Object.Destroy(slots[slots.Count - 1].gameObject);
						slots.RemoveAt(slots.Count - 1);
					}
				}
			}
			count = slots.Count;
			for (int l = 0; l < list.Count; l++)
			{
				vItemSlot vItemSlot3 = null;
				if (l < count)
				{
					vItemSlot3 = slots[l];
					vItemSlot3.AddItem(list[l]);
					vItemSlot3.CheckItem(list[l].isInEquipArea);
					vItemSlot3.onSubmitSlotCallBack = OnSubmit;
					vItemSlot3.onSelectSlotCallBack = OnSelect;
					if (currentItem != null && currentItem == list[l])
					{
						flag = true;
						SetSelectable(vItemSlot3.gameObject);
					}
				}
			}
			if (slots.Count > 0 && !flag)
			{
				StartCoroutine(SetSelectableHandle(slots[0].gameObject));
			}
			if (onCompleteSlotListCallBack != null)
			{
				onCompleteSlotListCallBack(slots);
			}
		}

		public void CreateEquipmentWindow(List<vItem> items, List<vItemType> type, vItem currentItem = null, OnSubmitSlot onPickUpItemCallback = null, OnSelectSlot onSelectSlotCallBack = null)
		{
			this.currentItem = currentItem;
			List<vItem> items2 = items.FindAll((vItem item) => type.Contains(item.type));
			CreateEquipmentWindow(items2, onPickUpItemCallback);
		}

		private IEnumerator SetSelectableHandle(GameObject target)
		{
			if (base.enabled)
			{
				yield return new WaitForEndOfFrame();
				SetSelectable(target);
			}
		}

		private void SetSelectable(GameObject target)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
			EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
			ExecuteEvents.Execute(target, eventData, ExecuteEvents.selectHandler);
		}

		public void OnSubmit(vItemSlot slot)
		{
			if (onSubmitSlot != null)
			{
				onSubmitSlot(slot);
			}
		}

		public void OnSelect(vItemSlot slot)
		{
			if (itemtext != null)
			{
				if (slot.item == null)
				{
					itemtext.text = "";
				}
				else if (slot.item.displayAttributes)
				{
					this.text = new StringBuilder();
					this.text.Append(slot.item.name + "\n");
					this.text.AppendLine(slot.item.description);
					if (slot.item.attributes != null)
					{
						for (int i = 0; i < slot.item.attributes.Count; i++)
						{
							string text = InsertSpaceBeforeUpperCAse(slot.item.attributes[i].name.ToString());
							this.text.AppendLine(text + " : " + slot.item.attributes[i].value);
						}
					}
					itemtext.text = this.text.ToString();
				}
			}
			if (onSelectSlot != null)
			{
				onSelectSlot(slot);
			}
		}

		public string InsertSpaceBeforeUpperCAse(string input)
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

		public void OnCancel()
		{
		}
	}
}
