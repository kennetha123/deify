using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager
{
	[vClassHeader("Remove Current Item", false, "icon_v2", false, "")]
	public class vRemoveCurrentItem : vMonoBehaviour
	{
		public enum Type
		{
			UnequipItem = 0,
			DestroyItem = 1,
			DropItem = 2
		}

		public Type type;

		[Tooltip("Immediately equip the item ignoring the Equip animation")]
		public bool immediate = true;

		[Tooltip("Index Area of your Inventory Prefab")]
		public int indexOfArea;

		public UnityEvent OnTriggerEnterEvent;

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player"))
			{
				return;
			}
			vItemManager component = other.gameObject.GetComponent<vItemManager>();
			if ((bool)component)
			{
				if (type == Type.UnequipItem)
				{
					component.UnequipCurrentEquipedItem(indexOfArea, immediate);
				}
				else if (type == Type.DestroyItem)
				{
					component.LeaveCurrentEquipedItem(indexOfArea, immediate);
				}
				else
				{
					component.DropCurrentEquipedItem(indexOfArea, immediate);
				}
			}
			OnTriggerEnterEvent.Invoke();
		}
	}
}
