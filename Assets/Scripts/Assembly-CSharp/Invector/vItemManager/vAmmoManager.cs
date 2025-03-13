using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
	[vClassHeader("Ammo Manager", true, "icon_v2", false, "", iconName = "ammoIcon")]
	public class vAmmoManager : vMonoBehaviour
	{
		public delegate void OnUpdateTotalAmmo();

		public vAmmoListData ammoListData;

		[HideInInspector]
		public vItemManager itemManager;

		public List<vAmmo> ammos = new List<vAmmo>();

		public OnUpdateTotalAmmo updateTotalAmmo = delegate
		{
		};

		private void Start()
		{
			itemManager = GetComponent<vItemManager>();
			if ((bool)itemManager)
			{
				itemManager.onAddItem.AddListener(AddAmmo);
				itemManager.onDropItem.AddListener(DropAmmo);
				itemManager.onLeaveItem.AddListener(LeaveAmmo);
				itemManager.onChangeItemAmount.AddListener(ChangeItemAmount);
			}
			if ((bool)ammoListData)
			{
				ammos.Clear();
				for (int i = 0; i < ammoListData.ammos.Count; i++)
				{
					vAmmo vAmmo2 = new vAmmo(ammoListData.ammos[i]);
					vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
					ammos.Add(vAmmo2);
				}
			}
		}

		public vAmmo GetAmmo(int id)
		{
			return ammos.Find((vAmmo a) => a.ammoID == id);
		}

		public void AddAmmo(string ammoName, int id, int amount)
		{
			vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == id);
			if (vAmmo2 == null)
			{
				vAmmo2 = new vAmmo(ammoName, id, amount);
				ammos.Add(vAmmo2);
				vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
			}
			else if (vAmmo2 != null)
			{
				vAmmo2.AddAmmo(amount);
			}
			UpdateTotalAmmo();
		}

		public void AddAmmo(vItem item)
		{
			if (item.type == vItemType.Ammo)
			{
				vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
				if (vAmmo2 == null)
				{
					vAmmo2 = new vAmmo(item.name, item.id, item.amount);
					ammos.Add(vAmmo2);
					vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
				}
				vAmmo2.ammoItems.Add(item);
			}
			UpdateTotalAmmo();
		}

		protected void ChangeItemAmount(vItem item)
		{
			if (item.type == vItemType.Ammo)
			{
				vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
				if (vAmmo2 == null)
				{
					vAmmo2 = new vAmmo(item.name, item.id, item.amount);
					ammos.Add(vAmmo2);
					vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
				}
			}
			UpdateTotalAmmo();
		}

		public void LeaveAmmo(vItem item, int amount)
		{
			if (item.type == vItemType.Ammo)
			{
				vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
				if (vAmmo2 != null && item.amount - amount <= 0 && vAmmo2.ammoItems.Contains(item))
				{
					vAmmo2.ammoItems.Remove(item);
				}
			}
			UpdateTotalAmmo();
		}

		public void DropAmmo(vItem item, int amount)
		{
			if (item.type == vItemType.Ammo)
			{
				vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
				if (vAmmo2 != null && item.amount - amount <= 0 && vAmmo2.ammoItems.Contains(item))
				{
					vAmmo2.ammoItems.Remove(item);
				}
			}
			UpdateTotalAmmo();
		}

		public void UpdateTotalAmmo()
		{
			updateTotalAmmo();
		}

		private void OnDestroyAmmoItem(vItem item)
		{
			if ((bool)itemManager)
			{
				itemManager.DestroyItem(item, item.amount);
			}
		}
	}
}
