using Invector.vCharacterController;
using Invector.vItemManager;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
	[vClassHeader("Draw/Hide Melee Weapons", "This component works with vItemManager, vWeaponHolderManager and vMeleeCombatInput", useHelpBox = true)]
	public class vDrawHideMeleeWeapons : vMonoBehaviour
	{
		public bool hideWeaponsAutomatically = true;

		[vHideInInspector("hideWeaponsAutomatically", false)]
		public float hideWeaponsTimer = 5f;

		[vHelpBox("Set Lock input to Inventory when Lock method is called", vHelpBoxAttribute.MessageType.None)]
		public bool lockInventoryInputOnLock;

		[vReadOnly(true)]
		public bool isLocked;

		public GenericInput hideAndDrawWeaponsInput = new GenericInput("H", "LB", "LB");

		private vWeaponHolderManager holderManager;

		private vMeleeCombatInput melee;

		protected float currentTimer;

		protected bool forceHide;

		protected virtual bool IsEquipping
		{
			get
			{
				if (melee != null && (bool)melee.cc)
				{
					return melee.cc.IsAnimatorTag("IsEquipping");
				}
				return false;
			}
		}

		protected virtual void Start()
		{
			holderManager = GetComponent<vWeaponHolderManager>();
			vThirdPersonInput component = GetComponent<vThirdPersonInput>();
			if ((bool)holderManager && (bool)component)
			{
				component.onUpdateInput.AddListener(ControlWeapons);
				melee = component as vMeleeCombatInput;
				if (melee == null)
				{
					Debug.LogWarning("You're missing a vMeleeCombatInput, please add one", base.gameObject);
				}
			}
		}

		protected virtual void ControlWeapons(vThirdPersonInput tpInput)
		{
			if (!isLocked)
			{
				HandleInput();
				DrawWeaponsImmediateHandle();
				HideWeaponsAutomatically();
			}
		}

		protected virtual GameObject RightWeaponObject(bool checkIsActve = false)
		{
			if (!melee || !melee.meleeManager || !melee.meleeManager.rightWeapon || (checkIsActve && !melee.meleeManager.rightWeapon.gameObject.activeInHierarchy))
			{
				return null;
			}
			return melee.meleeManager.rightWeapon.gameObject;
		}

		protected virtual GameObject LeftWeaponObject(bool checkIsActve = false)
		{
			if (!melee || !melee.meleeManager || !melee.meleeManager.leftWeapon || (checkIsActve && !melee.meleeManager.leftWeapon.gameObject.activeInHierarchy))
			{
				return null;
			}
			return melee.meleeManager.leftWeapon.gameObject;
		}

		public virtual void LockDrawHideInput(bool value)
		{
			isLocked = value;
			if (lockInventoryInputOnLock && (bool)holderManager.itemManager)
			{
				holderManager.itemManager.LockInventoryInput(value);
			}
		}

		public virtual void HideWeapons(bool immediate = false)
		{
			if (CanHideRightWeapon())
			{
				HideRightWeapon(immediate);
			}
			else if (CanHideLeftWeapon())
			{
				HideLeftWeapon(immediate);
			}
		}

		public virtual void ForceHideWeapons(bool immediate = false)
		{
			forceHide = true;
			HideWeapons(immediate);
			Invoke("ResetForceHide", 1f);
		}

		protected virtual void ResetForceHide()
		{
			forceHide = false;
		}

		public virtual void DrawWeapons(bool immediate = false)
		{
			if (CanDrawRightWeapon())
			{
				DrawRightWeapon(immediate);
			}
			else if (CanDrawLeftWeapon())
			{
				DrawLeftWeapon(immediate);
			}
		}

		protected virtual void HideWeaponsAutomatically()
		{
			if (hideWeaponsAutomatically)
			{
				if (HideTimerConditions())
				{
					currentTimer += Time.deltaTime;
				}
				else
				{
					currentTimer = 0f;
				}
				if (currentTimer >= hideWeaponsTimer && !IsEquipping)
				{
					currentTimer = 0f;
					HideWeapons();
				}
			}
			else if (currentTimer > 0f)
			{
				currentTimer = 0f;
			}
		}

		protected virtual bool HideTimerConditions()
		{
			if (CanHideWeapons())
			{
				if (!CanHideRightWeapon())
				{
					return CanHideLeftWeapon();
				}
				return true;
			}
			return false;
		}

		protected virtual bool CanHideWeapons()
		{
			if ((bool)melee && (bool)melee.meleeManager)
			{
				if (!forceHide)
				{
					if (!melee.isAttacking && !melee.isBlocking)
					{
						if (!melee.meleeManager.rightWeapon)
						{
							return melee.meleeManager.leftWeapon;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected virtual bool CanDrawWeapons()
		{
			if ((bool)melee)
			{
				return melee.meleeManager;
			}
			return false;
		}

		protected virtual bool CanHideRightWeapon()
		{
			if (CanHideWeapons() && (bool)RightWeaponObject())
			{
				return RightWeaponObject().activeInHierarchy;
			}
			return false;
		}

		protected virtual bool CanHideLeftWeapon()
		{
			if (CanHideWeapons() && (bool)LeftWeaponObject())
			{
				return LeftWeaponObject().activeInHierarchy;
			}
			return false;
		}

		protected virtual bool CanDrawRightWeapon()
		{
			if (CanDrawWeapons() && (bool)RightWeaponObject())
			{
				return !RightWeaponObject().activeInHierarchy;
			}
			return false;
		}

		protected virtual bool CanDrawLeftWeapon()
		{
			if (CanDrawWeapons() && (bool)LeftWeaponObject())
			{
				return !LeftWeaponObject().activeInHierarchy;
			}
			return false;
		}

		protected virtual void HandleInput()
		{
			if (hideAndDrawWeaponsInput.GetButtonDown() && !IsEquipping)
			{
				if (CanHideRightWeapon() || CanHideLeftWeapon())
				{
					HideWeapons();
				}
				else if (CanDrawRightWeapon() || CanDrawLeftWeapon())
				{
					DrawWeapons();
				}
			}
		}

		protected virtual void DrawWeaponsImmediateHandle()
		{
			if (DrawWeaponsImmediateConditions())
			{
				DrawWeapons(true);
			}
		}

		protected virtual bool DrawWeaponsImmediateConditions()
		{
			if (!melee || melee.cc.customAction || !melee.meleeManager || melee.meleeManager.CurrentAttackWeapon == null)
			{
				return false;
			}
			if (!melee.weakAttackInput.GetButtonDown() && !melee.strongAttackInput.GetButtonDown())
			{
				return melee.blockInput.GetButton();
			}
			return true;
		}

		protected virtual void HideRightWeapon(bool immediate = false)
		{
			GameObject weapon = RightWeaponObject(true);
			if (!weapon)
			{
				return;
			}
			vIEquipment component = weapon.GetComponent<vIEquipment>();
			if (component == null)
			{
				Debug.LogWarning(weapon.name + " need to have an  vIEquipment Component", weapon.gameObject);
				return;
			}
			vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
			HideWeaponsHandle(melee, component, null, delegate
			{
				if ((bool)holder)
				{
					holder.SetActiveWeapon(true);
				}
				weapon.gameObject.SetActive(false);
				if (CanHideLeftWeapon())
				{
					HideLeftWeapon(immediate);
				}
			}, immediate);
		}

		protected virtual void HideLeftWeapon(bool immediate = false)
		{
			GameObject weapon = LeftWeaponObject(true);
			if (!weapon)
			{
				return;
			}
			vIEquipment component = weapon.GetComponent<vIEquipment>();
			if (component == null)
			{
				Debug.LogWarning(weapon.name + " need to have an  vIEquipment Component", weapon.gameObject);
				return;
			}
			vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
			HideWeaponsHandle(melee, component, null, delegate
			{
				if ((bool)holder)
				{
					holder.SetActiveWeapon(true);
				}
				weapon.gameObject.SetActive(false);
			}, immediate);
		}

		protected virtual void DrawRightWeapon(bool immediate = false)
		{
			GameObject weapon = RightWeaponObject();
			if (!weapon)
			{
				return;
			}
			vIEquipment component = weapon.GetComponent<vIEquipment>();
			if (component == null)
			{
				Debug.LogWarning(weapon.name + " need to have an  vIEquipment Component", weapon.gameObject);
				return;
			}
			vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
			DrawWeaponsHandle(melee, component, null, delegate
			{
				if ((bool)holder)
				{
					holder.SetActiveWeapon(false);
				}
				weapon.gameObject.SetActive(true);
				if (CanDrawLeftWeapon())
				{
					DrawLeftWeapon(immediate);
				}
			}, immediate);
		}

		protected virtual void DrawLeftWeapon(bool immediate = false)
		{
			GameObject weapon = LeftWeaponObject();
			if (!weapon)
			{
				return;
			}
			vIEquipment component = weapon.GetComponent<vIEquipment>();
			if (component == null)
			{
				Debug.LogWarning(weapon.name + " need to have an  vIEquipment Component", weapon.gameObject);
				return;
			}
			vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
			DrawWeaponsHandle(melee, component, null, delegate
			{
				if ((bool)holder)
				{
					holder.SetActiveWeapon(false);
				}
				weapon.gameObject.SetActive(true);
			}, immediate);
		}

		protected virtual void DrawWeaponsHandle(vThirdPersonInput tpInput, vIEquipment equipment, UnityAction onStart, UnityAction onFinish, bool immediate = false)
		{
			if (holderManager.inEquip)
			{
				return;
			}
			if (!immediate)
			{
				if (!string.IsNullOrEmpty(equipment.referenceItem.EnableAnim))
				{
					tpInput.animator.SetBool("FlipEquip", equipment.equipPoint.equipPointName.Contains("Left"));
					tpInput.animator.CrossFade(equipment.referenceItem.EnableAnim, 0.25f);
				}
				else
				{
					immediate = true;
				}
			}
			StartCoroutine(holderManager.EquipRoutine(equipment.referenceItem.enableDelayTime, immediate, onStart, onFinish));
		}

		protected virtual void HideWeaponsHandle(vThirdPersonInput tpInput, vIEquipment equipment, UnityAction onStart, UnityAction onFinish, bool immediate = false)
		{
			if (holderManager.inUnequip)
			{
				return;
			}
			if (!immediate)
			{
				if (!string.IsNullOrEmpty(equipment.referenceItem.DisableAnim))
				{
					tpInput.animator.SetBool("FlipEquip", equipment.equipPoint.equipPointName.Contains("Left"));
					tpInput.animator.CrossFade(equipment.referenceItem.DisableAnim, 0.25f);
				}
				else
				{
					immediate = true;
				}
			}
			StartCoroutine(holderManager.UnequipRoutine(equipment.referenceItem.disableDelayTime, immediate, onStart, onFinish));
		}
	}
}
