using System.Collections.Generic;
using Invector.vCharacterController;

namespace Invector.vItemManager
{
	public class vCanUseItemControlTest : vMonoBehaviour
	{
		private vThirdPersonController tp;

		private void Start()
		{
			tp = GetComponent<vThirdPersonController>();
			vItemManager component = GetComponent<vItemManager>();
			if ((bool)component)
			{
				component.canUseItemDelegate -= CanUseItem;
				component.canUseItemDelegate += CanUseItem;
			}
		}

		private void OnDestroy()
		{
			vItemManager component = GetComponent<vItemManager>();
			if ((bool)component)
			{
				component.canUseItemDelegate -= CanUseItem;
			}
		}

		private void CanUseItem(vItem item, ref List<bool> validateResult)
		{
			if (item.GetItemAttribute(vItemAttributes.Health) != null)
			{
				bool flag = tp.currentHealth < (float)tp.maxHealth;
				vHUDController.instance.ShowText(flag ? "Increase health" : ("Can't use " + item.name + " because your health is full"), 4f);
				if (!flag)
				{
					validateResult.Add(flag);
				}
			}
		}
	}
}
