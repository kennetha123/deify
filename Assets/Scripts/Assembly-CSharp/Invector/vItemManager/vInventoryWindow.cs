using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
	public class vInventoryWindow : MonoBehaviour
	{
		[SerializeField]
		private Action<string, object> myCallback;

		public vInventory inventory;

		public List<vWindowPop_up> pop_ups = new List<vWindowPop_up>();

		private GameObject lastSelected;

		public bool isOpen;

		public bool IsOpen
		{
			get
			{
				if (pop_ups != null && pop_ups.Count > 0)
				{
					return false;
				}
				return isOpen;
			}
		}

		private void Start()
		{
			inventory = GetComponentInParent<vInventory>();
		}

		private void OnEnable()
		{
			try
			{
				pop_ups = new List<vWindowPop_up>();
				if (inventory == null)
				{
					inventory = GetComponentInParent<vInventory>();
				}
				if ((bool)lastSelected)
				{
					StartCoroutine(SetSelectableHandle(lastSelected));
				}
				if ((bool)inventory)
				{
					inventory.SetCurrentWindow(this);
				}
				isOpen = true;
			}
			catch
			{
			}
		}

		private void OnDisable()
		{
			try
			{
				lastSelected = EventSystem.current.currentSelectedGameObject;
				RemoveAllPop_up();
				isOpen = false;
			}
			catch
			{
			}
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

		public void AddPop_up(vWindowPop_up pop_up)
		{
			if (!pop_ups.Contains(pop_up))
			{
				pop_ups.Add(pop_up);
				if (!pop_up.gameObject.activeSelf)
				{
					pop_up.gameObject.SetActive(true);
				}
			}
		}

		public void RemovePop_up(vWindowPop_up pop_up)
		{
			try
			{
				if (pop_ups.Contains(pop_up))
				{
					pop_ups.Remove(pop_up);
					if (pop_up.gameObject.activeSelf)
					{
						pop_up.gameObject.SetActive(false);
					}
				}
			}
			catch
			{
			}
		}

		public void RemoveLastPop_up()
		{
			if (pop_ups.Count > 0)
			{
				vWindowPop_up vWindowPop_up2 = pop_ups[pop_ups.Count - 1];
				pop_ups.Remove(vWindowPop_up2);
				if (vWindowPop_up2.gameObject.activeSelf)
				{
					vWindowPop_up2.gameObject.SetActive(false);
				}
				if (pop_ups.Count > 0 && pop_ups.Count > 0 && !pop_ups[pop_ups.Count - 1].gameObject.activeSelf)
				{
					pop_ups[pop_ups.Count - 1].gameObject.SetActive(true);
				}
			}
		}

		public void RemoveAllPop_up()
		{
			foreach (vWindowPop_up pop_up in pop_ups)
			{
				pop_up.gameObject.SetActive(false);
			}
			pop_ups.Clear();
		}

		public bool ContainsPop_up()
		{
			return pop_ups.Count > 0;
		}
	}
}
