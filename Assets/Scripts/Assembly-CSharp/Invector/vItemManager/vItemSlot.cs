using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vItemSlot : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image icon;

		public Image blockIcon;

		public Image checkIcon;

		public Text amountText;

		public vItem item;

		public bool isValid = true;

		[HideInInspector]
		public bool isChecked;

		public ItemSlotEvent onSubmitSlotCallBack;

		public ItemSlotEvent onSelectSlotCallBack;

		public ItemSlotEvent onDeselectSlotCallBack;

		private Color color = Color.white;

		public OnHandleItemEvent onAddItem;

		public OnHandleItemEvent onRemoveItem;

		public void Start()
		{
			SetValid(isValid);
			CheckItem(false);
		}

		private void LateUpdate()
		{
			if (item != null && base.gameObject.activeSelf)
			{
				if (item.stackable)
				{
					amountText.text = item.amount.ToString();
				}
				else
				{
					amountText.text = "";
				}
			}
		}

		public virtual void CheckItem(bool value)
		{
			isChecked = value;
			if ((bool)checkIcon)
			{
				checkIcon.gameObject.SetActive(isChecked);
			}
		}

		public virtual void SetValid(bool value)
		{
			isValid = value;
			Selectable component = GetComponent<Selectable>();
			if ((bool)component)
			{
				component.interactable = value;
			}
			if (!(blockIcon == null))
			{
				blockIcon.color = (value ? Color.clear : Color.white);
				blockIcon.SetAllDirty();
				isValid = value;
			}
		}

		public virtual void AddItem(vItem item)
		{
			if (item != null)
			{
				onAddItem.Invoke(item);
				this.item = item;
				icon.sprite = item.icon;
				color.a = 1f;
				icon.color = color;
				if (item.stackable)
				{
					amountText.text = item.amount.ToString();
				}
				else
				{
					amountText.text = "";
				}
			}
			else
			{
				RemoveItem();
			}
		}

		public virtual void RemoveItem()
		{
			onRemoveItem.Invoke(item);
			color.a = 0f;
			icon.color = color;
			item = null;
			amountText.text = "";
			icon.sprite = null;
			icon.SetAllDirty();
		}

		public virtual bool isOcupad()
		{
			return item != null;
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
			if (onSelectSlotCallBack != null)
			{
				onSelectSlotCallBack(this);
			}
		}

		public virtual void OnDeselect(BaseEventData eventData)
		{
			if (onDeselectSlotCallBack != null)
			{
				onDeselectSlotCallBack(this);
			}
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (isValid && onSubmitSlotCallBack != null)
			{
				onSubmitSlotCallBack(this);
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			if (vInput.instance.inputDevice == InputDevice.MouseKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
				if (onSelectSlotCallBack != null)
				{
					onSelectSlotCallBack(this);
				}
			}
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
			if (vInput.instance.inputDevice == InputDevice.MouseKeyboard && onDeselectSlotCallBack != null)
			{
				onDeselectSlotCallBack(this);
			}
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (vInput.instance.inputDevice == InputDevice.MouseKeyboard && eventData.button == PointerEventData.InputButton.Left && isValid && onSubmitSlotCallBack != null)
			{
				onSubmitSlotCallBack(this);
			}
		}
	}
}
