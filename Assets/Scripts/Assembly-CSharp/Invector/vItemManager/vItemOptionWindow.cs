using UnityEngine.UI;

namespace Invector.vItemManager
{
	[vClassHeader("Item Options Window", true, "icon_v2", false, "")]
	public class vItemOptionWindow : vMonoBehaviour
	{
		public Button useItemButton;

		public Button dropItemButton;

		public Button destroyItemButton;

		public virtual void EnableOptions(vItemSlot slot)
		{
		}

		protected virtual void ValidateButtons(vItem item, out bool result)
		{
			useItemButton.interactable = item.canBeUsed;
			dropItemButton.interactable = item.canBeDroped;
			destroyItemButton.interactable = item.canBeDestroyed;
			result = useItemButton.interactable || useItemButton.interactable || destroyItemButton.interactable;
		}

		public virtual bool CanOpenOptions(vItem item)
		{
			if (item == null)
			{
				return false;
			}
			bool result = false;
			ValidateButtons(item, out result);
			return result;
		}
	}
}
