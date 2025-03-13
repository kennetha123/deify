using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine;

namespace Invector.vItemManager
{
	[vClassHeader("ItemManager", true, "icon_v2", false, "")]
	public class vItemManager : vMonoBehaviour, IActionReceiver, IActionController
	{
		public delegate void CanUseItemDelegate(vItem item, ref List<bool> validationList);

		public class ItemClass
		{
			public string name;

			public List<vItem> items;

			public List<vItemType> suportType;

			public bool limited;

			public int maxSlots;
		}

		public bool dropItemsWhenDead;

		public vInventory inventoryPrefab;

		[HideInInspector]
		public vInventory inventory;

		public vItemListData itemListData;

		[Header("---Items Filter---")]
		public List<vItemType> itemsFilter = new List<vItemType> { vItemType.Consumable };

		[SerializeField]
		public List<ItemReference> startItems = new List<ItemReference>();

		public List<vItem> items;

		public OnHandleItemEvent onStartItemUsage;

		public OnHandleItemEvent onUseItem;

		public OnHandleItemEvent onUseItemFail;

		public OnHandleItemEvent onAddItem;

		public OnHandleItemEvent onChangeItemAmount;

		public OnChangeItemAmount onLeaveItem;

		public OnChangeItemAmount onDropItem;

		public OnOpenCloseInventory onOpenCloseInventory;

		public OnChangeEquipmentEvent onEquipItem;

		public OnChangeEquipmentEvent onUnequipItem;

		[SerializeField]
		public List<EquipPoint> equipPoints;

		[SerializeField]
		public List<ApplyAttributeEvent> applyAttributeEvents;

		internal bool inEquip;

		internal bool usingItem;

		private float equipTimer;

		private Animator animator;

		private static vItemManager instance;

		bool IActionController.enabled
		{
			get
			{
				return base.enabled;
			}
			set
			{
				base.enabled = value;
			}
		}

		GameObject IActionController.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		Transform IActionController.transform
		{
			get
			{
				return base.transform;
			}
		}

		string IActionController.name
		{
			get
			{
				return base.name;
			}
		}

		public event CanUseItemDelegate canUseItemDelegate;

		private IEnumerator Start()
		{
			if (!(instance == null))
			{
				yield break;
			}
			inventory = UnityEngine.Object.FindObjectOfType<vInventory>();
			instance = this;
			vMeleeCombatInput melee = GetComponent<vMeleeCombatInput>();
			if (!inventory && (bool)inventoryPrefab)
			{
				inventory = UnityEngine.Object.Instantiate(inventoryPrefab);
			}
			if (!inventory)
			{
				Debug.LogError("No vInventory assigned!");
			}
			if ((bool)inventory)
			{
				inventory.GetItemsHandler = GetItems;
				inventory.GetItemsAllHandler = GetAllItems;
				if ((bool)melee)
				{
					inventory.IsLockedEvent = () => melee.lockInventory;
				}
				inventory.onEquipItem.AddListener(EquipItem);
				inventory.onUnequipItem.AddListener(UnequipItem);
				inventory.onDropItem.AddListener(DropItem);
				inventory.onLeaveItem.AddListener(DestroyItem);
				inventory.onUseItem.AddListener(UseItem);
				inventory.onOpenCloseInventory.AddListener(OnOpenCloseInventory);
			}
			animator = GetComponent<Animator>();
			if (dropItemsWhenDead)
			{
				vICharacter component = GetComponent<vICharacter>();
				if (component != null)
				{
					component.onDead.AddListener(DropAllItens);
				}
			}
			yield return new WaitForEndOfFrame();
			items = new List<vItem>();
			if ((bool)itemListData)
			{
				for (int i = 0; i < startItems.Count; i++)
				{
					AddItem(startItems[i], true);
				}
			}
		}

		public virtual void LockInventoryInput(bool value)
		{
			if ((bool)inventory)
			{
				inventory.lockInventoryInput = value;
			}
		}

		public virtual List<vItem> GetItems()
		{
			return items;
		}

		public virtual List<vItem> GetAllItems()
		{
			if (!itemListData)
			{
				return null;
			}
			return itemListData.items;
		}

		public virtual void AddItem(ItemReference itemReference, bool immediate = false)
		{
			if (itemReference == null || !(itemListData != null) || itemListData.items.Count <= 0)
			{
				return;
			}
			vItem item = itemListData.items.Find((vItem t) => t.id.Equals(itemReference.id));
			if (!item)
			{
				return;
			}
			List<vItem> list = items.FindAll((vItem i) => i.stackable && i.id == item.id && i.amount < i.maxStack);
			if (list.Count == 0)
			{
				vItem vItem2 = UnityEngine.Object.Instantiate(item);
				vItem2.name = vItem2.name.Replace("(Clone)", string.Empty);
				if (itemReference.attributes != null && vItem2.attributes != null && item.attributes.Count == itemReference.attributes.Count)
				{
					vItem2.attributes = new List<vItemAttribute>(itemReference.attributes);
				}
				vItem2.amount = 0;
				for (int j = 0; j < item.maxStack; j++)
				{
					if (vItem2.amount >= vItem2.maxStack)
					{
						break;
					}
					if (itemReference.amount <= 0)
					{
						break;
					}
					vItem2.amount++;
					itemReference.amount--;
				}
				items.Add(vItem2);
				onAddItem.Invoke(vItem2);
				if (itemReference.autoEquip)
				{
					itemReference.autoEquip = false;
					AutoEquipItem(vItem2, itemReference.indexArea, immediate);
				}
				if (itemReference.amount > 0)
				{
					AddItem(itemReference);
				}
				return;
			}
			int index = items.IndexOf(list[0]);
			for (int k = 0; k < items[index].maxStack; k++)
			{
				if (items[index].amount >= items[index].maxStack)
				{
					break;
				}
				if (itemReference.amount <= 0)
				{
					break;
				}
				items[index].amount++;
				itemReference.amount--;
				onChangeItemAmount.Invoke(items[index]);
			}
			if (itemReference.amount > 0)
			{
				AddItem(itemReference);
			}
		}

		public bool CanUseItem(vItem item)
		{
			if (this.canUseItemDelegate != null)
			{
				List<bool> validationList = new List<bool>();
				this.canUseItemDelegate(item, ref validationList);
				return !validationList.Contains(false);
			}
			return item.canBeUsed;
		}

		public virtual void UseItem(vItem item)
		{
			if ((bool)item)
			{
				if (CanUseItem(item))
				{
					StartCoroutine(UseItemRoutine(item));
				}
				else
				{
					onUseItemFail.Invoke(item);
				}
			}
		}

		public IEnumerator UseItemRoutine(vItem item)
		{
			usingItem = true;
			LockInventoryInput(true);
			onStartItemUsage.Invoke(item);
			bool canUse = CanUseItem(item);
			yield return null;
			if (canUse)
			{
				float time = item.enableDelayTime;
				if (!inventory.isOpen)
				{
					animator.SetBool("FlipAnimation", false);
					animator.CrossFade(item.EnableAnim, 0.25f);
					while (usingItem && time > 0f && canUse)
					{
						CanUseItem(item);
						time -= Time.deltaTime;
						yield return null;
					}
				}
				if (usingItem && canUse)
				{
					if (item.destroyAfterUse)
					{
						item.amount--;
					}
					onUseItem.Invoke(item);
					if (item.attributes != null && item.attributes.Count > 0 && applyAttributeEvents.Count > 0)
					{
						foreach (ApplyAttributeEvent attributeEvent in applyAttributeEvents)
						{
							foreach (vItemAttribute item2 in item.attributes.FindAll((vItemAttribute a) => a.name.Equals(attributeEvent.attribute)))
							{
								attributeEvent.onApplyAttribute.Invoke(item2.value);
							}
						}
					}
					if (item.destroyAfterUse && item.amount <= 0 && items.Contains(item))
					{
						items.Remove(item);
						UnityEngine.Object.Destroy(item);
					}
					usingItem = false;
					inventory.CheckEquipmentChanges();
				}
				else
				{
					onUseItemFail.Invoke(item);
				}
			}
			else
			{
				onUseItemFail.Invoke(item);
			}
			LockInventoryInput(false);
		}

		public virtual void DestroyItem(vItem item, int amount)
		{
			onLeaveItem.Invoke(item, amount);
			item.amount -= amount;
			if (item.amount > 0 || !items.Contains(item))
			{
				return;
			}
			vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
			if (vEquipArea2 != null)
			{
				vEquipArea2.UnequipItem(item);
			}
			items.Remove(item);
			UnityEngine.Object.Destroy(item);
		}

		public virtual void DropItem(vItem item, int amount)
		{
			item.amount -= amount;
			if (item.dropObject != null)
			{
				vItemCollection component = UnityEngine.Object.Instantiate(item.dropObject, base.transform.position, base.transform.rotation).GetComponent<vItemCollection>();
				if (component != null)
				{
					component.items.Clear();
					ItemReference itemReference = new ItemReference(item.id);
					itemReference.amount = amount;
					itemReference.attributes = new List<vItemAttribute>(item.attributes);
					component.items.Add(itemReference);
				}
			}
			onDropItem.Invoke(item, amount);
			if (item.amount > 0 || !items.Contains(item))
			{
				return;
			}
			vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
			if (vEquipArea2 != null)
			{
				vEquipArea2.UnequipItem(item);
			}
			items.Remove(item);
			UnityEngine.Object.Destroy(item);
		}

		public virtual void DropAllItens(GameObject target = null)
		{
			if (target != null && target != base.gameObject)
			{
				return;
			}
			List<ItemReference> list = new List<ItemReference>();
			int i;
			for (i = 0; i < items.Count; i++)
			{
				if (list.Find((ItemReference _item) => _item.id == items[i].id) != null)
				{
					continue;
				}
				List<vItem> sameItens = items.FindAll((vItem _item) => _item.id == items[i].id);
				ItemReference itemReference = new ItemReference(items[i].id);
				int a;
				for (a = 0; a < sameItens.Count; a++)
				{
					if (sameItens[a].type != 0)
					{
						EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipmentReference.item == sameItens[a]);
						if (equipPoint != null && equipPoint.equipmentReference.equipedObject != null)
						{
							UnequipItem(equipPoint.area, equipPoint.equipmentReference.item);
						}
					}
					else
					{
						vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(sameItens[a].id)));
						if (vEquipArea2 != null)
						{
							vEquipArea2.UnequipItem(sameItens[a]);
						}
					}
					itemReference.amount += sameItens[a].amount;
					UnityEngine.Object.Destroy(sameItens[a]);
				}
				list.Add(itemReference);
				if (equipPoints != null)
				{
					EquipPoint equipPoint2 = equipPoints.Find((EquipPoint e) => e.equipmentReference != null && e.equipmentReference.item != null && e.equipmentReference.item.id == itemReference.id && e.equipmentReference.equipedObject != null);
					if (equipPoint2 != null)
					{
						UnityEngine.Object.Destroy(equipPoint2.equipmentReference.equipedObject);
						equipPoint2.equipmentReference = null;
					}
				}
				if ((bool)items[i].dropObject)
				{
					vItemCollection component = UnityEngine.Object.Instantiate(items[i].dropObject, base.transform.position, base.transform.rotation).GetComponent<vItemCollection>();
					if (component != null)
					{
						component.items.Clear();
						component.items.Add(itemReference);
					}
				}
			}
			items.Clear();
		}

		public virtual bool ContainItem(int id)
		{
			return items.Exists((vItem i) => i.id == id);
		}

		public virtual bool ContainItem(string itemName)
		{
			return items.Exists((vItem i) => i.name == itemName);
		}

		public virtual bool ContainItem(int id, int amount)
		{
			return items.Find((vItem i) => i.id == id && i.amount >= amount) != null;
		}

		public virtual bool ContainItem(string itemName, int amount)
		{
			return items.Find((vItem i) => i.name == itemName && i.amount >= amount) != null;
		}

		public virtual bool ContainItems(int id, int count)
		{
			List<vItem> list = items.FindAll((vItem i) => i.id == id);
			if (list != null)
			{
				return list.Count >= count;
			}
			return false;
		}

		public virtual bool ContainItems(string itemName, int count)
		{
			List<vItem> list = items.FindAll((vItem i) => i.name == itemName);
			if (list != null)
			{
				return list.Count >= count;
			}
			return false;
		}

		public virtual bool EquipAreaHasSomeItem(int indexOfArea)
		{
			return inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item != null);
		}

		public virtual bool ItemIsInSomeEquipArea(int id)
		{
			if (!inventory || inventory.equipAreas.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < inventory.equipAreas.Length; i++)
			{
				if (inventory.equipAreas[i].equipSlots.Exists((vEquipSlot slot) => slot.item.id.Equals(id)))
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool ItemIsInSomeEquipArea(string itemName)
		{
			if (!inventory || inventory.equipAreas.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < inventory.equipAreas.Length; i++)
			{
				if (inventory.equipAreas[i].equipSlots.Exists((vEquipSlot slot) => slot.item.name.Equals(itemName)))
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool ItemIsInSpecificEquipArea(int id, int indexOfArea)
		{
			if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
			{
				return false;
			}
			if (inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item.id.Equals(id)))
			{
				return true;
			}
			return false;
		}

		public virtual bool ItemIsInSpecificEquipArea(string itemName, int indexOfArea)
		{
			if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
			{
				return false;
			}
			if (inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item.name.Equals(itemName)))
			{
				return true;
			}
			return false;
		}

		public virtual bool EquipPointHasSomeItem(string equipPointName)
		{
			return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null);
		}

		public virtual bool ItemIsInSomeEquipPont(int id)
		{
			return equipPoints.Exists((EquipPoint ep) => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
		}

		public virtual bool ItemIsInSomeEquipPont(string itemName)
		{
			return equipPoints.Exists((EquipPoint ep) => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
		}

		public virtual bool ItemIsInSpecificEquipPoint(int id, string equipPointName)
		{
			return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
		}

		public virtual bool ItemIsInSpecificEquipPoint(string itemName, string equipPointName)
		{
			return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
		}

		public virtual vItem GetItem(int id)
		{
			return items.Find((vItem i) => i.id == id);
		}

		public virtual vItem GetItem(string itemName)
		{
			return items.Find((vItem i) => i.name == itemName);
		}

		public virtual vItem GetItemInEquipPoint(string equipPointName)
		{
			EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName.Equals(equipPointName));
			if (equipPoint != null && equipPoint.equipmentReference != null && (bool)equipPoint.equipmentReference.item)
			{
				return equipPoint.equipmentReference.item;
			}
			return null;
		}

		public virtual List<vItem> GetItems(int id)
		{
			return items.FindAll((vItem i) => i.id == id);
		}

		public virtual List<vItem> GetItems(string itemName)
		{
			return items.FindAll((vItem i) => i.name == itemName);
		}

		public virtual List<vItem> GetItemsInEquipArea(int indexOfArea)
		{
			List<vItem> list = new List<vItem>();
			if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
			{
				return list;
			}
			List<vEquipSlot> validSlots = inventory.equipAreas[indexOfArea].ValidSlots;
			for (int i = 0; i < validSlots.Count; i++)
			{
				if (validSlots[i].item != null)
				{
					list.Add(validSlots[i].item);
				}
			}
			return list;
		}

		public virtual List<vItem> GetAllItemInAllEquipAreas()
		{
			List<vItem> list = new List<vItem>();
			if (!inventory || inventory.equipAreas.Length == 0)
			{
				return list;
			}
			for (int i = 0; i < inventory.equipAreas.Length; i++)
			{
				List<vEquipSlot> validSlots = inventory.equipAreas[i].ValidSlots;
				for (int j = 0; j < validSlots.Count; j++)
				{
					if (validSlots[j].item != null)
					{
						list.Add(validSlots[j].item);
					}
				}
			}
			return list;
		}

		public virtual void EquipItem(vEquipArea equipArea, vItem item)
		{
			onEquipItem.Invoke(equipArea, item);
			if (item != equipArea.currentEquipedItem)
			{
				return;
			}
			EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName == equipArea.equipPointName);
			if (equipPoint == null || !(item != null) || !(equipPoint.equipmentReference.item != item))
			{
				return;
			}
			equipTimer = item.enableDelayTime;
			if (item.type != 0)
			{
				if (!inventory.isOpen)
				{
					animator.SetBool("FlipEquip", equipArea.equipPointName.Contains("Left"));
					animator.CrossFade(item.EnableAnim, 0.25f);
				}
				equipPoint.area = equipArea;
				StartCoroutine(EquipItemRoutine(equipPoint, item));
			}
		}

		public virtual void UnequipItem(vEquipArea equipArea, vItem item)
		{
			onUnequipItem.Invoke(equipArea, item);
			EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName == equipArea.equipPointName && ep.equipmentReference.item != null && ep.equipmentReference.item == item);
			if (equipPoint == null || !(item != null))
			{
				return;
			}
			equipTimer = item.disableDelayTime;
			if (item.type != 0)
			{
				if (!inventory.isOpen && !inEquip && equipPoint.equipmentReference.equipedObject.activeInHierarchy)
				{
					animator.SetBool("FlipEquip", equipArea.equipPointName.Contains("Left"));
					animator.CrossFade(item.DisableAnim, 0.25f);
				}
				StartCoroutine(UnequipItemRoutine(equipPoint, item));
			}
		}

		public virtual void UnequipItem(vItem item)
		{
			vEquipArea equipArea = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
			if (!(equipArea != null))
			{
				return;
			}
			onUnequipItem.Invoke(equipArea, item);
			EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName == equipArea.equipPointName && ep.equipmentReference.item != null && ep.equipmentReference.item == item);
			if (equipPoint == null || !(item != null))
			{
				return;
			}
			equipTimer = item.enableDelayTime;
			if (item.type != 0)
			{
				if (!inventory.isOpen && !inEquip)
				{
					animator.SetBool("FlipAnimation", equipArea.equipPointName.Contains("Left"));
					animator.CrossFade(item.DisableAnim, 0.25f);
				}
				StartCoroutine(UnequipItemRoutine(equipPoint, item));
			}
		}

		private IEnumerator EquipItemRoutine(EquipPoint equipPoint, vItem item)
		{
			LockInventoryInput(true);
			if (!inEquip)
			{
				inventory.canEquip = false;
				inEquip = true;
				if (equipPoint != null)
				{
					if ((bool)item.originalObject)
					{
						if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
						{
							vIEquipment[] components = equipPoint.equipmentReference.equipedObject.GetComponents<vIEquipment>();
							for (int i = 0; i < components.Length; i++)
							{
								if (components[i] != null)
								{
									components[i].OnUnequip(equipPoint.equipmentReference.item);
									components[i].equipPoint = null;
								}
							}
							UnityEngine.Object.Destroy(equipPoint.equipmentReference.equipedObject);
						}
						if (!inventory.isOpen && !string.IsNullOrEmpty(item.EnableAnim))
						{
							while (equipTimer > 0f && !(item == null))
							{
								yield return null;
								equipTimer -= Time.deltaTime;
							}
						}
						Transform transform = equipPoint.handler.customHandlers.Find((Transform p) => p.name == item.customHandler);
						Transform transform2 = ((transform != null) ? transform : equipPoint.handler.defaultHandler);
						GameObject gameObject = UnityEngine.Object.Instantiate(item.originalObject, transform2.position, transform2.rotation);
						gameObject.transform.parent = transform2;
						if (equipPoint.equipPointName.Contains("Left"))
						{
							Vector3 localScale = gameObject.transform.localScale;
							localScale.x *= -1f;
							gameObject.transform.localScale = localScale;
						}
						equipPoint.equipmentReference.item = item;
						equipPoint.equipmentReference.equipedObject = gameObject;
						vIEquipment[] components2 = equipPoint.equipmentReference.equipedObject.GetComponents<vIEquipment>();
						for (int j = 0; j < components2.Length; j++)
						{
							if (components2[j] != null)
							{
								components2[j].OnEquip(equipPoint.equipmentReference.item);
								components2[j].equipPoint = equipPoint;
							}
						}
						equipPoint.onInstantiateEquiment.Invoke(gameObject);
					}
					else if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
					{
						vIEquipment[] components3 = equipPoint.equipmentReference.equipedObject.GetComponents<vIEquipment>();
						for (int k = 0; k < components3.Length; k++)
						{
							if (components3[k] != null)
							{
								components3[k].OnUnequip(equipPoint.equipmentReference.item);
								components3[k].equipPoint = null;
							}
						}
						UnityEngine.Object.Destroy(equipPoint.equipmentReference.equipedObject);
						equipPoint.equipmentReference.item = null;
					}
				}
				inEquip = false;
				inventory.canEquip = true;
				if (equipPoint != null)
				{
					CheckTwoHandItem(equipPoint, item);
				}
			}
			LockInventoryInput(false);
		}

		protected virtual void CheckTwoHandItem(EquipPoint equipPoint, vItem item)
		{
			if (item == null)
			{
				return;
			}
			EquipPoint equipPoint2 = equipPoints.Find((EquipPoint ePoint) => ePoint.area != null && ePoint.equipPointName.Equals("LeftArm") && ePoint.area.currentEquipedItem != null);
			if (equipPoint.equipPointName.Equals("LeftArm"))
			{
				equipPoint2 = equipPoints.Find((EquipPoint ePoint) => ePoint.area != null && ePoint.equipPointName.Equals("RightArm") && ePoint.area.currentEquipedItem != null);
			}
			else if (!equipPoint.equipPointName.Equals("RightArm"))
			{
				return;
			}
			if (equipPoint2 != null && (item.twoHandWeapon || equipPoint2.area.currentEquipedItem.twoHandWeapon))
			{
				equipPoint2.area.RemoveCurrentItem();
			}
		}

		private IEnumerator UnequipItemRoutine(EquipPoint equipPoint, vItem item)
		{
			LockInventoryInput(true);
			if (!inEquip)
			{
				inventory.canEquip = false;
				inEquip = true;
				if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
				{
					vIEquipment[] components = equipPoint.equipmentReference.equipedObject.GetComponents<vIEquipment>();
					for (int i = 0; i < components.Length; i++)
					{
						if (components[i] != null)
						{
							components[i].OnUnequip(equipPoint.equipmentReference.item);
							components[i].equipPoint = null;
						}
					}
					if (!inventory.isOpen)
					{
						while (equipTimer > 0f && !string.IsNullOrEmpty(item.DisableAnim))
						{
							equipTimer -= Time.deltaTime;
							yield return new WaitForEndOfFrame();
						}
					}
					UnityEngine.Object.Destroy(equipPoint.equipmentReference.equipedObject);
					equipPoint.equipmentReference.item = null;
				}
				inEquip = false;
				inventory.canEquip = true;
			}
			LockInventoryInput(false);
		}

		protected virtual void OnOpenCloseInventory(bool value)
		{
			if (value)
			{
				animator.SetTrigger("ResetState");
			}
			onOpenCloseInventory.Invoke(value);
		}

		public virtual void EquipItemToEquipArea(int indexOfArea, int indexOfSlot, vItem item, bool immediate = false)
		{
			if (!inventory)
			{
				return;
			}
			if (immediate)
			{
				inventory.isOpen = immediate;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				vEquipArea vEquipArea2 = inventory.equipAreas[indexOfArea];
				if (vEquipArea2 != null)
				{
					vEquipArea2.AddItemToEquipSlot(indexOfSlot, item);
				}
			}
			if (immediate)
			{
				inventory.isOpen = false;
			}
		}

		public virtual void UnequipItemOfEquipArea(int indexOfArea, int indexOfSlot, bool immediate = false)
		{
			if (!inventory)
			{
				return;
			}
			if (immediate)
			{
				inventory.isOpen = immediate;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				vEquipArea vEquipArea2 = inventory.equipAreas[indexOfArea];
				if (vEquipArea2 != null)
				{
					vEquipArea2.RemoveItemOfEquipSlot(indexOfSlot);
				}
			}
			if (immediate)
			{
				inventory.isOpen = false;
			}
		}

		public virtual void EquipCurrentItemToArea(vItem item, int indexOfArea, bool immediate = false)
		{
			if ((bool)inventory || items.Count != 0)
			{
				if (immediate)
				{
					inventory.isOpen = immediate;
				}
				if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
				{
					inventory.equipAreas[indexOfArea].AddCurrentItem(item);
				}
				if (immediate)
				{
					inventory.isOpen = false;
				}
			}
		}

		public virtual void UnequipCurrentEquipedItem(int indexOfArea, bool immediate = false)
		{
			if ((bool)inventory || items.Count != 0)
			{
				if (immediate)
				{
					inventory.isOpen = immediate;
				}
				if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
				{
					inventory.equipAreas[indexOfArea].RemoveCurrentItem();
				}
				if (immediate)
				{
					inventory.isOpen = false;
				}
			}
		}

		public virtual void DropCurrentEquipedItem(int indexOfArea, bool immediate = false)
		{
			if (!inventory && items.Count == 0)
			{
				return;
			}
			if (immediate)
			{
				inventory.isOpen = immediate;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				vItem currentEquipedItem = inventory.equipAreas[indexOfArea].currentEquipedItem;
				if ((bool)currentEquipedItem)
				{
					DropItem(currentEquipedItem, currentEquipedItem.amount);
				}
			}
			if (immediate)
			{
				inventory.isOpen = false;
			}
		}

		public virtual void LeaveCurrentEquipedItem(int indexOfArea, bool immediate = false)
		{
			if (!inventory && items.Count == 0)
			{
				return;
			}
			if (immediate)
			{
				inventory.isOpen = immediate;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				vItem currentEquipedItem = inventory.equipAreas[indexOfArea].currentEquipedItem;
				if ((bool)currentEquipedItem)
				{
					DestroyItem(currentEquipedItem, currentEquipedItem.amount);
				}
			}
			if (immediate)
			{
				inventory.isOpen = false;
			}
		}

		public virtual void AutoEquipItem(vItem item, int indexArea, bool immediate = false)
		{
			if (!inventory)
			{
				return;
			}
			if (immediate)
			{
				inventory.isOpen = immediate;
			}
			if (inventory.equipAreas != null && inventory.equipAreas.Length != 0 && indexArea < inventory.equipAreas.Length)
			{
				vEquipSlot vEquipSlot2 = inventory.equipAreas[indexArea].equipSlots.Find((vEquipSlot slot) => slot.isValid && slot.item == null && slot.itemType.Contains(item.type));
				if ((bool)vEquipSlot2 && !inventory.equipAreas[indexArea].equipSlots.Exists((vEquipSlot slot) => slot.item == item))
				{
					int indexOfSlot = inventory.equipAreas[indexArea].equipSlots.IndexOf(vEquipSlot2);
					if (vEquipSlot2.item != item)
					{
						EquipItemToEquipArea(indexArea, indexOfSlot, item);
					}
				}
			}
			else
			{
				Debug.LogWarning("Fail to auto equip " + item.name + " on equipArea " + indexArea);
			}
			if (immediate)
			{
				inventory.isOpen = false;
			}
		}

		public virtual void CollectItems(List<ItemReference> collection, bool immediate = false)
		{
			foreach (ItemReference item in collection)
			{
				AddItem(item, immediate);
			}
		}

		public virtual void CollectItem(ItemReference itemRef, bool immediate = false)
		{
			AddItem(itemRef, immediate);
		}

		public virtual void OnReceiveAction(vTriggerGenericAction action)
		{
			vItemCollection componentInChildren = action.GetComponentInChildren<vItemCollection>();
			if (componentInChildren != null && componentInChildren.items.Count > 0)
			{
				List<ItemReference> collection = componentInChildren.items.vCopy();
				StartCoroutine(CollectItemsWithDelay(collection, componentInChildren.onCollectDelay, componentInChildren.textDelay, componentInChildren.immediate));
			}
		}

		public virtual IEnumerator CollectItemsWithDelay(List<ItemReference> collection, float onCollectDelay, float textDelay, bool immediate)
		{
			yield return new WaitForSeconds(onCollectDelay);
			int i;
			for (i = 0; i < collection.Count; i++)
			{
				yield return new WaitForSeconds(textDelay);
				vItem vItem2 = itemListData.items.Find((vItem _item) => _item.id == collection[i].id);
				if (vItem2 != null && vItemCollectionDisplay.Instance != null)
				{
					vItemCollectionDisplay.Instance.FadeText("Acquired: " + collection[i].amount + " " + vItem2.name, 4f, 0.25f);
				}
				CollectItem(collection[i], immediate);
			}
		}

		public virtual IEnumerator CollectItemWithDelay(ItemReference itemRef, float onCollectDelay, float textDelay, bool immediate)
		{
			yield return new WaitForSeconds(onCollectDelay + textDelay);
			vItem vItem2 = itemListData.items.Find((vItem _item) => _item.id == itemRef.id);
			if (vItem2 != null && vItemCollectionDisplay.Instance != null)
			{
				vItemCollectionDisplay.Instance.FadeText("Acquired: " + itemRef.amount + " " + vItem2.name, 4f, 0.25f);
			}
			CollectItem(itemRef, immediate);
		}

		Type IActionController.GetType()
		{
			return GetType();
		}
	}
}
