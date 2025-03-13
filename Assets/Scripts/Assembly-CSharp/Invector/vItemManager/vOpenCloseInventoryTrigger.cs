using UnityEngine.Events;

namespace Invector.vItemManager
{
	[vClassHeader("vOpenClose Inventory Trigger", false, "icon_v2", false, "")]
	public class vOpenCloseInventoryTrigger : vMonoBehaviour
	{
		public UnityEvent onOpen;

		public UnityEvent onClose;

		protected virtual void Start()
		{
			vInventory componentInParent = GetComponentInParent<vInventory>();
			if ((bool)componentInParent)
			{
				componentInParent.onOpenCloseInventory.AddListener(OpenCloseInventory);
			}
		}

		public void OpenCloseInventory(bool value)
		{
			if (value)
			{
				onOpen.Invoke();
			}
			else
			{
				onClose.Invoke();
			}
		}
	}
}
