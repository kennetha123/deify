using System;
using System.Collections.Generic;

namespace Invector.vItemManager
{
	[Serializable]
	public class ItemReference
	{
		public int id;

		public int amount;

		public List<vItemAttribute> attributes;

		public bool changeAttributes;

		public bool autoEquip;

		public int indexArea;

		public ItemReference(int id)
		{
			this.id = id;
			autoEquip = false;
		}
	}
}
