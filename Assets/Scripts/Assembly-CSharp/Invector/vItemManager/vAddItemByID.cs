using UnityEngine;

namespace Invector.vItemManager
{
	[vClassHeader("Add Item By ID", "This is a simple example on how to add items using script", openClose = false)]
	public class vAddItemByID : vMonoBehaviour
	{
		public int id;

		public int amount;

		public bool autoEquip;

		public bool destroyAfter;

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				vItemManager component = other.gameObject.GetComponent<vItemManager>();
				if ((bool)component)
				{
					ItemReference itemReference = new ItemReference(id);
					itemReference.amount = amount;
					itemReference.autoEquip = autoEquip;
					component.AddItem(itemReference);
				}
				if (destroyAfter)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
