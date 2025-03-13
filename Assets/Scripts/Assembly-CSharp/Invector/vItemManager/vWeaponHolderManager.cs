using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager
{
	[vClassHeader("Weapon Holder Manager", "Create a new empty object inside a bone and add the vWeaponHolder script")]
	public class vWeaponHolderManager : vMonoBehaviour
	{
		public vWeaponHolder[] holders = new vWeaponHolder[0];

		public bool debugMode;

		internal bool inEquip;

		internal bool inUnequip;

		internal vItemManager itemManager;

		internal vThirdPersonController cc;

		public Dictionary<string, List<vWeaponHolder>> holderAreas = new Dictionary<string, List<vWeaponHolder>>();

		protected float equipTime;

		private float currentUnsheatheTimer;

		private float timeOut;

		protected virtual bool IsEquipping
		{
			get
			{
				if ((bool)cc)
				{
					return cc.IsAnimatorTag("IsEquipping");
				}
				return false;
			}
		}

		private void OnDrawGizmosSelected()
		{
			holders = GetComponentsInChildren<vWeaponHolder>(true);
		}

		private void Start()
		{
			itemManager = GetComponent<vItemManager>();
			cc = GetComponent<vThirdPersonController>();
			if (!itemManager)
			{
				return;
			}
			itemManager.onEquipItem.AddListener(EquipWeapon);
			itemManager.onUnequipItem.AddListener(UnequipWeapon);
			holders = GetComponentsInChildren<vWeaponHolder>(true);
			if (holders == null)
			{
				return;
			}
			vWeaponHolder[] array = holders;
			foreach (vWeaponHolder vWeaponHolder in array)
			{
				if (!holderAreas.ContainsKey(vWeaponHolder.equipPointName))
				{
					holderAreas.Add(vWeaponHolder.equipPointName, new List<vWeaponHolder>());
					holderAreas[vWeaponHolder.equipPointName].Add(vWeaponHolder);
				}
				else
				{
					holderAreas[vWeaponHolder.equipPointName].Add(vWeaponHolder);
				}
				vWeaponHolder.SetActiveHolder(false);
				vWeaponHolder.SetActiveWeapon(false);
			}
		}

		public void EquipWeapon(vEquipArea equipArea, vItem item)
		{
			if (item == null)
			{
				return;
			}
			List<vEquipSlot> slotsInArea = equipArea.ValidSlots;
			if (slotsInArea == null || slotsInArea.Count <= 0 || !holderAreas.ContainsKey(equipArea.equipPointName))
			{
				return;
			}
			int i;
			for (i = 0; i < slotsInArea.Count; i++)
			{
				if (!(slotsInArea[i].item != null))
				{
					continue;
				}
				vWeaponHolder vWeaponHolder = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => (bool)slotsInArea[i].item && slotsInArea[i].item.id == h.itemID && ((equipArea.currentEquipedItem != null && equipArea.currentEquipedItem != item && equipArea.currentEquipedItem != slotsInArea[i].item && equipArea.currentEquipedItem.id != item.id) || equipArea.currentEquipedItem == null));
				if ((bool)vWeaponHolder)
				{
					vWeaponHolder.SetActiveHolder(true);
					vWeaponHolder.SetActiveWeapon(true);
					if (debugMode)
					{
						Debug.Log("Hold: " + slotsInArea[i].item);
					}
				}
			}
			if (!(equipArea.currentEquipedItem != null) || !(equipArea.currentEquipedItem == item))
			{
				return;
			}
			vWeaponHolder holder = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => h.itemID == equipArea.currentEquipedItem.id);
			if ((bool)holder)
			{
				bool immediate = equipArea.currentEquipedItem != item || (itemManager.inventory != null && itemManager.inventory.isOpen) || string.IsNullOrEmpty(equipArea.currentEquipedItem.EnableAnim);
				if (debugMode)
				{
					Debug.Log("UnHold: " + item.name);
				}
				StartCoroutine(EquipRoutine(equipArea.currentEquipedItem.enableDelayTime, immediate, delegate
				{
					holder.SetActiveHolder(true);
				}, delegate
				{
					holder.SetActiveWeapon(false);
				}, item.name));
			}
		}

		public void UnequipWeapon(vEquipArea equipArea, vItem item)
		{
			if (holders.Length == 0 || item == null || !(itemManager.inventory != null) || !holderAreas.ContainsKey(equipArea.equipPointName))
			{
				return;
			}
			vWeaponHolder holder = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => item.id == h.itemID);
			if (!holder)
			{
				return;
			}
			bool containsItem = equipArea.ValidSlots.Find((vEquipSlot slot) => slot.item == item) != null;
			if (debugMode)
			{
				Debug.Log(containsItem ? ("Hold: " + item.name) : ("Hide :" + item.name + " Holder"));
			}
			if (containsItem)
			{
				bool immediate = (itemManager.inventory != null && itemManager.inventory.isOpen) || string.IsNullOrEmpty(item.DisableAnim);
				StartCoroutine(UnequipRoutine(item.disableDelayTime, immediate, delegate
				{
					holder.SetActiveHolder(containsItem);
				}, delegate
				{
					holder.SetActiveWeapon(containsItem);
				}, item.name));
			}
			else
			{
				holder.SetActiveHolder(false);
				holder.SetActiveWeapon(false);
			}
		}

		internal vWeaponHolder GetHolder(GameObject equipment, int id)
		{
			EquipPoint equipPoint = itemManager.equipPoints.Find((EquipPoint e) => e.equipmentReference != null && (bool)e.equipmentReference.item && e.equipmentReference.item.id == id && e.equipmentReference.equipedObject == equipment);
			if (holderAreas.ContainsKey(equipPoint.equipPointName))
			{
				return holderAreas[equipPoint.equipPointName].Find((vWeaponHolder h) => id == h.itemID);
			}
			if (debugMode)
			{
				Debug.LogWarning(ToString() + " fail to find a holder with equipPointName " + equipPoint.equipPointName);
			}
			return null;
		}

		internal IEnumerator UnequipRoutine(float equipDelay, bool immediate = false, UnityAction onStart = null, UnityAction onFinish = null, string itemName = "")
		{
			if (debugMode)
			{
				Debug.Log("Start Unequip: " + itemName);
			}
			if (!immediate && !inEquip)
			{
				inUnequip = true;
			}
			while (!IsEquipping && !immediate)
			{
				yield return null;
			}
			if (onStart != null)
			{
				onStart();
			}
			if (!inEquip && !immediate)
			{
				float equipTime = equipDelay;
				while (!immediate && !inEquip && equipTime > 0f)
				{
					equipTime -= Time.deltaTime;
					yield return null;
				}
			}
			inUnequip = false;
			if (onFinish != null)
			{
				onFinish();
			}
			if (debugMode)
			{
				Debug.Log("Finish Unequip: " + itemName);
			}
		}

		internal IEnumerator EquipRoutine(float equipDelay, bool immediate = false, UnityAction onStart = null, UnityAction onFinish = null, string itemName = "")
		{
			if (debugMode)
			{
				Debug.Log("Start Equip: " + itemName);
			}
			if (!immediate)
			{
				inEquip = true;
			}
			while (!IsEquipping && !immediate)
			{
				yield return null;
			}
			if (onStart != null)
			{
				onStart();
			}
			if (!inUnequip && !immediate)
			{
				float equipTime = equipDelay;
				while (!immediate && !inUnequip && equipTime > 0f)
				{
					equipTime -= Time.deltaTime;
					yield return null;
				}
			}
			inEquip = false;
			if (onFinish != null)
			{
				onFinish();
			}
			if (debugMode)
			{
				Debug.Log("Finish Equip: " + itemName);
			}
		}
	}
}
