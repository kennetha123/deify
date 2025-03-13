using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
	public class vItemEnumsList : ScriptableObject
	{
		[SerializeField]
		public List<string> itemTypeEnumValues = new List<string>();

		[SerializeField]
		public List<string> itemAttributesEnumValues = new List<string>();
	}
}
