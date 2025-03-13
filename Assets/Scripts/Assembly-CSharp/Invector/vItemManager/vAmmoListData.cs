using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
	public class vAmmoListData : ScriptableObject
	{
		public List<vItemListData> itemListDatas;

		[HideInInspector]
		public List<vAmmo> ammos = new List<vAmmo>();
	}
}
