using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vEquipmentDisplay : vItemSlot
	{
		public Text slotIdentifier;

		public override void AddItem(vItem item)
		{
			if (base.item != item)
			{
				base.AddItem(item);
				if (item != null && item.amount > 1)
				{
					amountText.text = item.amount.ToString("00");
				}
				else
				{
					amountText.text = "";
				}
			}
		}

		public void ItemIdentifier(int identifier = 0, bool showIdentifier = false)
		{
			if ((bool)slotIdentifier)
			{
				if (showIdentifier)
				{
					slotIdentifier.text = identifier.ToString();
				}
				else
				{
					slotIdentifier.text = string.Empty;
				}
			}
		}
	}
}
