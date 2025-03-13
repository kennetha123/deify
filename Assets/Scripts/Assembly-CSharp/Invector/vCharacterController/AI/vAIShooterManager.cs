using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vShooter;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI
{
	[vClassHeader("AI SHOOTER MANAGER", "Make sure to set the Damage Layers to 'Default' and 'BodyPart', or any other layer you need to inflict damage.")]
	public class vAIShooterManager : vMonoBehaviour
	{
		[Serializable]
		public class OnReloadWeapon : UnityEvent<vShooterWeapon>
		{
		}

		[Header("- Float Values")]
		[Tooltip("min distance to aim")]
		public float minDistanceToAim = 1f;

		public float checkAimRadius = 0.1f;

		[Tooltip("smooth of the right hand when correcting the aim")]
		public float smoothHandRotation = 30f;

		[Tooltip("Limit the maxAngle for the right hand to correct the aim")]
		public float maxHandAngle = 60f;

		[Tooltip("Check true to make the character always aim and walk on strafe mode")]
		[Header("- Shooter Settings")]
		public vWeaponIKAdjustList weaponIKAdjustList;

		[Tooltip("Check this to syinc the weapon aim to the camera aim")]
		public bool raycastAimTarget = true;

		[Tooltip("Check this to use IK on the left hand")]
		public bool useLeftIK = true;

		[Tooltip("Check this to use IK on the left hand")]
		public bool useRightIK = true;

		[Tooltip("Layer to aim")]
		public LayerMask damageLayer = 1;

		[Tooltip("Tags to the Aim ignore - tag this gameObject to avoid shot on yourself")]
		public List<string> ignoreTags;

		public vShooterWeapon rWeapon;

		public vShooterWeapon lWeapon;

		[HideInInspector]
		public OnReloadWeapon onReloadWeapon;

		[HideInInspector]
		public bool showCheckAimGizmos;

		private Animator animator;

		private int totalAmmo;

		private int secundaryTotalAmmo;

		private float currentShotTime;

		protected vWeaponIKAdjust currentWeaponIKAdjust;

		public virtual vWeaponIKAdjust CurrentWeaponIK
		{
			get
			{
				return currentWeaponIKAdjust;
			}
		}

		public bool isShooting
		{
			get
			{
				return currentShotTime > 0f;
			}
		}

		public virtual bool weaponHasAmmo
		{
			get
			{
				if (!currentWeapon)
				{
					return false;
				}
				return currentWeapon.ammoCount > 0;
			}
		}

		public virtual vShooterWeapon currentWeapon
		{
			get
			{
				if (!rWeapon || !rWeapon.gameObject.activeSelf)
				{
					if (!lWeapon || !lWeapon.gameObject.activeSelf)
					{
						return null;
					}
					return lWeapon;
				}
				return rWeapon;
			}
		}

		public bool isLeftWeapon
		{
			get
			{
				if (!(rWeapon == null))
				{
					return false;
				}
				return lWeapon;
			}
		}

		private void Start()
		{
			animator = GetComponent<Animator>();
			if ((bool)animator)
			{
				Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
				Transform boneTransform2 = animator.GetBoneTransform(HumanBodyBones.LeftHand);
				vShooterWeapon componentInChildren = boneTransform.GetComponentInChildren<vShooterWeapon>(true);
				vShooterWeapon componentInChildren2 = boneTransform2.GetComponentInChildren<vShooterWeapon>(true);
				if (componentInChildren != null)
				{
					SetRightWeapon(componentInChildren.gameObject);
				}
				if (componentInChildren2 != null)
				{
					SetLeftWeapon(componentInChildren2.gameObject);
				}
			}
			if (!ignoreTags.Contains(base.gameObject.tag))
			{
				ignoreTags.Add(base.gameObject.tag);
			}
		}

		public void SetDamageLayer(LayerMask mask)
		{
			damageLayer = mask;
			if ((bool)currentWeapon)
			{
				currentWeapon.hitLayer = mask;
			}
		}

		public void SetLeftWeapon(GameObject weapon)
		{
			if (!(weapon != null))
			{
				return;
			}
			vShooterWeapon componentInChildren = weapon.GetComponentInChildren<vShooterWeapon>(true);
			lWeapon = componentInChildren;
			if (!lWeapon)
			{
				return;
			}
			lWeapon.ignoreTags = ignoreTags;
			lWeapon.hitLayer = damageLayer;
			lWeapon.root = base.transform;
			lWeapon.isSecundaryWeapon = false;
			lWeapon.onDestroy.AddListener(OnDestroyWeapon);
			if (lWeapon.autoReload)
			{
				ReloadWeaponAuto(lWeapon);
			}
			if ((bool)lWeapon.secundaryWeapon)
			{
				lWeapon.secundaryWeapon.ignoreTags = ignoreTags;
				lWeapon.secundaryWeapon.hitLayer = damageLayer;
				lWeapon.secundaryWeapon.root = base.transform;
				lWeapon.secundaryWeapon.isSecundaryWeapon = true;
				if (lWeapon.secundaryWeapon.autoReload)
				{
					ReloadWeaponAuto(lWeapon.secundaryWeapon, true);
				}
			}
			currentShotTime = 0f;
			LoadIKAdjust(lWeapon.weaponCategory);
		}

		public void SetRightWeapon(GameObject weapon)
		{
			if (!(weapon != null))
			{
				return;
			}
			vShooterWeapon componentInChildren = weapon.GetComponentInChildren<vShooterWeapon>(true);
			rWeapon = componentInChildren;
			if (!rWeapon)
			{
				return;
			}
			rWeapon.ignoreTags = ignoreTags;
			rWeapon.hitLayer = damageLayer;
			rWeapon.root = base.transform;
			rWeapon.isSecundaryWeapon = false;
			rWeapon.onDestroy.AddListener(OnDestroyWeapon);
			if (rWeapon.autoReload)
			{
				ReloadWeaponAuto(rWeapon);
			}
			if ((bool)rWeapon.secundaryWeapon)
			{
				rWeapon.secundaryWeapon.ignoreTags = ignoreTags;
				rWeapon.secundaryWeapon.hitLayer = damageLayer;
				rWeapon.secundaryWeapon.root = base.transform;
				rWeapon.secundaryWeapon.isSecundaryWeapon = true;
				if (rWeapon.secundaryWeapon.autoReload)
				{
					ReloadWeaponAuto(rWeapon.secundaryWeapon, true);
				}
			}
			currentShotTime = 0f;
			LoadIKAdjust(rWeapon.weaponCategory);
		}

		public virtual void LoadIKAdjust(string category)
		{
			if ((bool)weaponIKAdjustList)
			{
				currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(category);
			}
		}

		public virtual void SetIKAdjust(vWeaponIKAdjust iKAdjust)
		{
			currentWeaponIKAdjust = iKAdjust;
		}

		public void OnDestroyWeapon(GameObject otherGameObject)
		{
			currentShotTime = 0f;
		}

		public int GetMoveSetID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = (int)rWeapon.moveSetID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = (int)lWeapon.moveSetID;
			}
			return result;
		}

		public int GetUpperBodyID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = (int)rWeapon.upperBodyID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = (int)lWeapon.upperBodyID;
			}
			return result;
		}

		public int GetShotID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = (int)rWeapon.shotID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = (int)lWeapon.shotID;
			}
			return result;
		}

		public int GetAttackID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = (int)rWeapon.shotID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = (int)lWeapon.shotID;
			}
			return result;
		}

		public int GetEquipID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = rWeapon.equipID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = lWeapon.equipID;
			}
			return result;
		}

		public int GetReloadID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
			{
				result = rWeapon.reloadID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
			{
				result = lWeapon.reloadID;
			}
			return result;
		}

		public void ReloadWeapon()
		{
			vShooterWeapon vShooterWeapon = currentWeapon;
			if (!vShooterWeapon || !vShooterWeapon.gameObject.activeSelf)
			{
				return;
			}
			bool flag = false;
			if (vShooterWeapon.ammoCount < vShooterWeapon.clipSize && !vShooterWeapon.autoReload)
			{
				onReloadWeapon.Invoke(vShooterWeapon);
				int value = vShooterWeapon.clipSize - vShooterWeapon.ammoCount;
				vShooterWeapon.AddAmmo(value);
				if ((bool)animator)
				{
					animator.SetInteger("ReloadID", GetReloadID());
					animator.SetTrigger("Reload");
				}
				vShooterWeapon.ReloadEffect();
				flag = true;
			}
			if (!vShooterWeapon.secundaryWeapon || vShooterWeapon.secundaryWeapon.ammoCount >= vShooterWeapon.secundaryWeapon.clipSize || vShooterWeapon.secundaryWeapon.autoReload)
			{
				return;
			}
			int value2 = vShooterWeapon.secundaryWeapon.clipSize - vShooterWeapon.secundaryWeapon.ammoCount;
			vShooterWeapon.secundaryWeapon.AddAmmo(value2);
			if (!flag)
			{
				if ((bool)animator)
				{
					flag = true;
					animator.SetInteger("ReloadID", vShooterWeapon.secundaryWeapon.reloadID);
					animator.SetTrigger("Reload");
				}
				vShooterWeapon.secundaryWeapon.ReloadEffect();
			}
		}

		protected void ReloadWeaponAuto(vShooterWeapon weapon, bool secundaryWeapon = false)
		{
			if ((bool)weapon && weapon.gameObject.activeSelf && weapon.ammoCount < weapon.clipSize)
			{
				int value = weapon.clipSize - weapon.ammoCount;
				weapon.AddAmmo(value);
			}
		}

		public virtual void Shoot(Vector3 aimPosition, bool useSecundaryWeapon = false)
		{
			vShooterWeapon vShooterWeapon = currentWeapon;
			if (!vShooterWeapon || !vShooterWeapon.gameObject.activeSelf)
			{
				return;
			}
			vShooterWeapon secundaryWeapon = vShooterWeapon.secundaryWeapon;
			if (!useSecundaryWeapon || (bool)secundaryWeapon)
			{
				vShooterWeapon vShooterWeapon2 = (useSecundaryWeapon ? secundaryWeapon : vShooterWeapon);
				if (vShooterWeapon2.autoReload)
				{
					ReloadWeaponAuto(vShooterWeapon2, useSecundaryWeapon);
				}
				bool applyRecoil = false;
				vShooterWeapon2.Shoot(aimPosition, base.transform, delegate(bool sucessful)
				{
					applyRecoil = sucessful;
				});
				if (applyRecoil)
				{
					StartCoroutine(Recoil());
				}
				if (vShooterWeapon2.autoReload)
				{
					ReloadWeaponAuto(vShooterWeapon2, useSecundaryWeapon);
				}
				currentShotTime = vShooterWeapon.shootFrequency;
			}
		}

		private IEnumerator Recoil()
		{
			yield return new WaitForSeconds(0.02f);
			if ((bool)animator)
			{
				animator.SetTrigger("Shoot");
			}
		}

		public void UpdateShotTime()
		{
			if (currentShotTime > 0f)
			{
				currentShotTime -= Time.deltaTime;
			}
		}
	}
}
