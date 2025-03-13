using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
	[vClassHeader("vInventory", true, "icon_v2", false, "")]
	public class vInventory : vMonoBehaviour
	{
		public delegate List<vItem> GetItemsDelegate();

		public delegate bool LockInventoryInputEvent();

		public GetItemsDelegate GetItemsHandler;

		public GetItemsDelegate GetItemsAllHandler;

		public LockInventoryInputEvent IsLockedEvent;

		[vEditorToolbar("Settings", false, "", false, false)]
		public vInventoryWindow firstWindow;

		[Range(0f, 1f)]
		public float timeScaleWhileIsOpen;

		public float originalTimeScale = 1f;

		public bool updatedTimeScale;

		public bool dontDestroyOnLoad = true;

		public List<ChangeEquipmentControl> changeEquipmentControllers;

		[HideInInspector]
		public List<vInventoryWindow> windows = new List<vInventoryWindow>();

		[HideInInspector]
		public vInventoryWindow currentWindow;

		[vEditorToolbar("Input Mapping", false, "", false, false)]
		public GenericInput openInventory = new GenericInput("I", "Start", "Start");

		public GenericInput openCurrentSlot = new GenericInput("O", "A", "A");

		public GenericInput removeEquipment = new GenericInput("Backspace", "X", "X");

		[Header("This fields will override the EventSystem Input")]
		public GenericInput horizontal = new GenericInput("Horizontal", "D-Pad Horizontal", "Horizontal");

		public GenericInput vertical = new GenericInput("Vertical", "D-Pad Vertical", "Vertical");

		public GenericInput submit = new GenericInput("Return", "A", "A");

		public GenericInput cancel = new GenericInput("Backspace", "B", "B");

		[vEditorToolbar("Events", false, "", false, false)]
		public OnOpenCloseInventory onOpenCloseInventory;

		public OnUpdateItemUsageTime onUpdateItemUsageTime;

		public OnHandleItemEvent onUseItem;

		public OnChangeItemAmount onLeaveItem;

		public OnChangeItemAmount onDropItem;

		public OnChangeEquipmentEvent onEquipItem;

		public OnChangeEquipmentEvent onUnequipItem;

		[HideInInspector]
		public bool isOpen;

		[HideInInspector]
		public bool canEquip;

		[HideInInspector]
		public bool lockInventoryInput;

		[HideInInspector]
		public vEquipArea[] equipAreas;

		private vEquipArea currentEquipArea;

		private StandaloneInputModule inputModule;

		public List<vItem> items
		{
			get
			{
				if (GetItemsHandler != null)
				{
					return GetItemsHandler();
				}
				return new List<vItem>();
			}
		}

		public List<vItem> allItems
		{
			get
			{
				if (GetItemsAllHandler != null)
				{
					return GetItemsAllHandler();
				}
				return new List<vItem>();
			}
		}

		private void Start()
		{
			canEquip = true;
			inputModule = Object.FindObjectOfType<StandaloneInputModule>();
			equipAreas = GetComponentsInChildren<vEquipArea>(true);
			vEquipArea[] array = equipAreas;
			foreach (vEquipArea obj in array)
			{
				obj.Init();
				obj.onEquipItem.AddListener(EquipItem);
				obj.onUnequipItem.AddListener(UnequipItem);
				obj.onSelectEquipArea.AddListener(SelectArea);
			}
			if (dontDestroyOnLoad)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
			if ((bool)vGameController.instance)
			{
				vGameController.instance.OnReloadGame.AddListener(OnReloadGame);
			}
		}

		public void OnReloadGame()
		{
			StartCoroutine(ReloadEquipment());
		}

		private IEnumerator ReloadEquipment()
		{
			yield return new WaitForEndOfFrame();
			inputModule = Object.FindObjectOfType<StandaloneInputModule>();
			isOpen = true;
			foreach (ChangeEquipmentControl changeEquipmentController in changeEquipmentControllers)
			{
				if (!(changeEquipmentController.equipArea != null))
				{
					continue;
				}
				foreach (vEquipSlot equipSlot in changeEquipmentController.equipArea.equipSlots)
				{
					if (changeEquipmentController.equipArea.currentEquipedItem == null)
					{
						UnequipItem(changeEquipmentController.equipArea, equipSlot.item);
						changeEquipmentController.equipArea.UnequipItem(equipSlot);
					}
					else
					{
						changeEquipmentController.equipArea.UnequipItem(equipSlot);
					}
				}
			}
			isOpen = false;
		}

		public bool IsLocked()
		{
			if (IsLockedEvent == null || !IsLockedEvent())
			{
				return lockInventoryInput;
			}
			return true;
		}

		private void LateUpdate()
		{
			if (!IsLocked())
			{
				ControlWindowsInput();
				if (!isOpen)
				{
					ChangeEquipmentInput();
				}
				else
				{
					UpdateEventSystemInput();
					RemoveEquipmentInput();
					OpenCurrentSlot();
				}
				if (Input.GetKeyDown(KeyCode.P))
				{
					CloseInventory();
				}
			}
		}

		public virtual void ControlWindowsInput()
		{
			if (windows.Count == 0 || windows[windows.Count - 1] == firstWindow)
			{
				if (!firstWindow.gameObject.activeSelf && openInventory.GetButtonDown() && canEquip)
				{
					OpenInventory();
					return;
				}
				if (firstWindow.gameObject.activeSelf && (openInventory.GetButtonDown() || cancel.GetButtonDown()))
				{
					CloseInventory();
					return;
				}
			}
			if (!isOpen)
			{
				return;
			}
			if (openInventory.GetButtonDown())
			{
				CloseInventory();
				return;
			}
			if (windows.Count > 0 && windows[windows.Count - 1] != firstWindow && cancel.GetButtonDown())
			{
				currentEquipArea = null;
				if (windows[windows.Count - 1].ContainsPop_up())
				{
					windows[windows.Count - 1].RemoveLastPop_up();
					return;
				}
				windows[windows.Count - 1].gameObject.SetActive(false);
				windows.RemoveAt(windows.Count - 1);
				if (windows.Count > 0)
				{
					windows[windows.Count - 1].gameObject.SetActive(true);
					currentWindow = windows[windows.Count - 1];
				}
				else
				{
					currentWindow = null;
				}
			}
			if (currentWindow != null && !currentWindow.gameObject.activeSelf)
			{
				if (windows.Contains(currentWindow))
				{
					windows.Remove(currentWindow);
				}
				if (windows.Count > 0)
				{
					windows[windows.Count - 1].gameObject.SetActive(true);
					currentWindow = windows[windows.Count - 1];
				}
				else
				{
					currentWindow = null;
				}
			}
		}

		public virtual void OpenInventory()
		{
			if (!isOpen)
			{
				firstWindow.gameObject.SetActive(true);
				isOpen = true;
				onOpenCloseInventory.Invoke(true);
				if (!updatedTimeScale)
				{
					updatedTimeScale = true;
					originalTimeScale = Time.timeScale;
				}
				Time.timeScale = timeScaleWhileIsOpen;
			}
		}

		public virtual void CloseInventory()
		{
			if (!isOpen)
			{
				return;
			}
			if (!firstWindow.gameObject.activeSelf)
			{
				for (int num = windows.Count - 1; num > 0; num--)
				{
					if (windows[num].ContainsPop_up())
					{
						windows[num].RemoveLastPop_up();
					}
					windows[num].gameObject.SetActive(false);
				}
			}
			windows.Clear();
			currentEquipArea = null;
			currentWindow = null;
			firstWindow.gameObject.SetActive(false);
			isOpen = false;
			onOpenCloseInventory.Invoke(false);
			Time.timeScale = originalTimeScale;
			if (updatedTimeScale)
			{
				updatedTimeScale = false;
			}
		}

		private void RemoveEquipmentInput()
		{
			if (currentEquipArea != null && removeEquipment.GetButtonDown())
			{
				currentEquipArea.UnequipCurrentItem();
			}
		}

		private void OpenCurrentSlot()
		{
			if (vInput.instance.inputDevice == InputDevice.Mobile && currentEquipArea != null && openCurrentSlot.GetButtonDown() && currentEquipArea.currentSelectedSlot != null)
			{
				currentEquipArea.OnSubmitSlot(currentEquipArea.currentSelectedSlot);
			}
		}

		private void SelectArea(vEquipArea equipArea)
		{
			currentEquipArea = equipArea;
		}

		private void ChangeEquipmentInput()
		{
			if (changeEquipmentControllers.Count <= 0 || !canEquip)
			{
				return;
			}
			foreach (ChangeEquipmentControl changeEquipmentController in changeEquipmentControllers)
			{
				UseItemInput(changeEquipmentController);
				if (!(changeEquipmentController.equipArea != null))
				{
					continue;
				}
				if (vInput.instance.inputDevice == InputDevice.MouseKeyboard || vInput.instance.inputDevice == InputDevice.Mobile)
				{
					if (changeEquipmentController.previousItemInput.GetButtonDown())
					{
						changeEquipmentController.equipArea.PreviousEquipSlot();
					}
					if (changeEquipmentController.nextItemInput.GetButtonDown())
					{
						changeEquipmentController.equipArea.NextEquipSlot();
					}
				}
				else if (vInput.instance.inputDevice == InputDevice.Joystick)
				{
					if (changeEquipmentController.previousItemInput.GetAxisButtonDown(-1f))
					{
						changeEquipmentController.equipArea.PreviousEquipSlot();
					}
					if (changeEquipmentController.nextItemInput.GetAxisButtonDown(1f))
					{
						changeEquipmentController.equipArea.NextEquipSlot();
					}
				}
			}
		}

		public void CheckEquipmentChanges()
		{
			foreach (ChangeEquipmentControl changeEquipmentController in changeEquipmentControllers)
			{
				foreach (vEquipSlot equipSlot in changeEquipmentController.equipArea.equipSlots)
				{
					if (equipSlot != null && equipSlot.item != null && !items.Contains(equipSlot.item))
					{
						changeEquipmentController.equipArea.UnequipItem(equipSlot);
						if ((bool)changeEquipmentController.display)
						{
							changeEquipmentController.display.RemoveItem();
						}
					}
				}
			}
		}

		private void UpdateEventSystemInput()
		{
			if ((bool)inputModule)
			{
				inputModule.horizontalAxis = horizontal.buttonName;
				inputModule.verticalAxis = vertical.buttonName;
				inputModule.submitButton = submit.buttonName;
				inputModule.cancelButton = cancel.buttonName;
			}
			else
			{
				inputModule = Object.FindObjectOfType<StandaloneInputModule>();
			}
		}

		private void UseItemInput(ChangeEquipmentControl changeEquip)
		{
			if (changeEquip.display != null && changeEquip.display.item != null && changeEquip.display.item.type == vItemType.Consumable && changeEquip.useItemInput.GetButtonDown() && changeEquip.display.item.amount > 0)
			{
				OnUseItem(changeEquip.display.item);
			}
		}

		internal void OnUseItem(vItem item)
		{
			onUseItem.Invoke(item);
		}

		internal void OnUseItemImmediate(vItem item)
		{
			onUseItem.Invoke(item);
		}

		internal void OnLeaveItem(vItem item, int amount)
		{
			onLeaveItem.Invoke(item, amount);
			CheckEquipmentChanges();
		}

		internal void OnDropItem(vItem item, int amount)
		{
			onDropItem.Invoke(item, amount);
			CheckEquipmentChanges();
		}

		internal void SetCurrentWindow(vInventoryWindow inventoryWindow)
		{
			if (!windows.Contains(inventoryWindow))
			{
				windows.Add(inventoryWindow);
				if (currentWindow != null)
				{
					currentWindow.gameObject.SetActive(false);
				}
				currentWindow = inventoryWindow;
			}
		}

		public void EquipItem(vEquipArea equipArea, vItem item)
		{
			onEquipItem.Invoke(equipArea, item);
			ChangeEquipmentDisplay(equipArea, item, false);
		}

		public void UnequipItem(vEquipArea equipArea, vItem item)
		{
			onUnequipItem.Invoke(equipArea, item);
			ChangeEquipmentDisplay(equipArea, item);
		}

		private void ChangeEquipmentDisplay(vEquipArea equipArea, vItem item, bool removeItem = true)
		{
			if (changeEquipmentControllers.Count <= 0)
			{
				return;
			}
			ChangeEquipmentControl changeEquipmentControl = changeEquipmentControllers.Find((ChangeEquipmentControl changeEquip) => changeEquip.equipArea != null && changeEquip.equipArea.equipPointName == equipArea.equipPointName && changeEquip.display != null);
			if (changeEquipmentControl != null)
			{
				if (removeItem && changeEquipmentControl.display.item == item)
				{
					changeEquipmentControl.display.RemoveItem();
					changeEquipmentControl.display.ItemIdentifier(changeEquipmentControl.equipArea.indexOfEquipedItem + 1, true);
				}
				else if (equipArea.currentEquipedItem == item)
				{
					changeEquipmentControl.display.AddItem(item);
					changeEquipmentControl.display.ItemIdentifier(changeEquipmentControl.equipArea.indexOfEquipedItem + 1, true);
				}
			}
		}
	}
}
