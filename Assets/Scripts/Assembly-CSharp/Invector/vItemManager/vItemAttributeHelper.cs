using System.Collections.Generic;

namespace Invector.vItemManager
{
	public static class vItemAttributeHelper
	{
		public static bool Contains(this List<vItemAttribute> attributes, vItemAttributes name)
		{
			return attributes.Find((vItemAttribute at) => at.name == name) != null;
		}

		public static vItemAttribute GetAttributeByType(this List<vItemAttribute> attributes, vItemAttributes name)
		{
			return attributes.Find((vItemAttribute at) => at.name == name);
		}

		public static bool Equals(this vItemAttribute attributeA, vItemAttribute attributeB)
		{
			return attributeA.name == attributeB.name;
		}

		public static List<vItemAttribute> CopyAsNew(this List<vItemAttribute> copy)
		{
			List<vItemAttribute> list = new List<vItemAttribute>();
			if (copy != null)
			{
				for (int i = 0; i < copy.Count; i++)
				{
					vItemAttribute item = new vItemAttribute(copy[i].name, copy[i].value);
					list.Add(item);
				}
			}
			return list;
		}
	}
}
