using System;

namespace Invector.vItemManager
{
	[Serializable]
	public class vItemAttribute
	{
		public vItemAttributes name;

		public int value;

		public bool isBool;

		public vItemAttribute(vItemAttributes name, int value)
		{
			this.name = name;
			this.value = value;
		}
	}
}
