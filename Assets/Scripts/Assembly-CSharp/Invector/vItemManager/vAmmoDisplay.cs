using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vAmmoDisplay : MonoBehaviour
	{
		[Serializable]
		public class OnChangeAmmoEvent : UnityEvent<int>
		{
		}

		public int displayID = 1;

		[SerializeField]
		protected Text display;

		private vItem currentItem;

		public UnityEvent onShow;

		public UnityEvent onHide;

		public OnChangeAmmoEvent onChangeAmmo;

		private int currentAmmoId;

		private void Start()
		{
			if (display == null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			display.text = "";
			currentAmmoId = -1;
		}

		public void Show()
		{
			display.gameObject.SetActive(true);
			onShow.Invoke();
		}

		public void Hide()
		{
			display.gameObject.SetActive(false);
			onHide.Invoke();
		}

		public void UpdateDisplay(string text, int id = 0)
		{
			if (!text.Equals("") && !display.gameObject.activeSelf)
			{
				display.gameObject.SetActive(true);
			}
			if (currentAmmoId != id)
			{
				onChangeAmmo.Invoke(id);
				currentAmmoId = id;
			}
			display.text = text;
		}
	}
}
