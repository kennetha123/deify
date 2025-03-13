using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vItemManager;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
	[vClassHeader("SHOOTER MANAGER", true, "icon_v2", false, "", iconName = "shooterIcon")]
	public class vShooterManager : vMonoBehaviour
	{
		[Serializable]
		public class OnReloadWeapon : UnityEvent<vShooterWeapon>
		{
		}

		public delegate void TotalAmmoHandler(int ammoID, ref int ammo);

		[vEditorToolbar("Damage Layers", false, "", false, false)]
		[Tooltip("Layer to aim and apply damage")]
		public LayerMask damageLayer = 1;

		[Tooltip("Tags to ignore (auto add this gameObject tag to avoid damage your self)")]
		public vTagMask ignoreTags = new vTagMask("Player");

		[Tooltip("Layer to block aim")]
		public LayerMask blockAimLayer = 1;

		public float blockAimOffsetY = 0.35f;

		public float blockAimOffsetX = 0.35f;

		[vEditorToolbar("Cancel Reload", false, "", false, false)]
		[vHelpBox("You can call the <b>CancelReload</b> method using events to interupt the reload routine and animation, for example, when doing an Custom Action or receiving a specific hitReaction ID", vHelpBoxAttribute.MessageType.None)]
		[Tooltip("It will always automatically use the CancelReload")]
		public bool useCancelReload = true;

		[Tooltip("This is a list of HitReaction ID that will be ignored by the CancelReload routine")]
		public List<int> ignoreReacionIDList = new List<int> { -1 };

		[vEditorToolbar("Aim", false, "", false, false)]
		[Header("- Float Values")]
		[Tooltip("min distance to aim")]
		public float minDistanceToAim = 1f;

		public float checkAimRadius = 0.1f;

		[Tooltip("Check true to make the character always aim and walk on strafe mode")]
		[Header("- Shooter Settings")]
		public bool alwaysAiming;

		public bool onlyWalkWhenAiming = true;

		public bool useDefaultMovesetWhenNotAiming = true;

		[vEditorToolbar("IK", false, "", false, false)]
		[Tooltip("smooth of the right hand when correcting the aim")]
		public float smoothArmIKRotation = 30f;

		[Tooltip("smooth of the right arm when correcting the aim")]
		public float smoothArmAlignWeight = 4f;

		[Tooltip("Limit the maxAngle for the right hand to correct the aim")]
		public float maxAimAngle = 60f;

		[Tooltip("Check this to syinc the weapon aim to the camera aim")]
		public bool raycastAimTarget = true;

		[Tooltip("Move camera angle when shot using recoil properties of weapon")]
		public bool applyRecoilToCamera = true;

		[Tooltip("Check this to use IK on the left hand")]
		public bool useLeftIK = true;

		[Tooltip("Check this to use IK on the left hand")]
		public bool useRightIK = true;

		[vHelpBox("Use this properties to add more adjustment to IK when using weapon", vHelpBoxAttribute.MessageType.None)]
		[Tooltip("Instead of adjust each weapon individually, make a single offset here for each character")]
		public Vector3 ikRotationOffsetR;

		[Tooltip("Instead of adjust each weapon individually, make a single offset here for each character")]
		public Vector3 ikPositionOffsetR;

		[Tooltip("Instead of adjust each weapon individually, make a single offset here for each character")]
		public Vector3 ikRotationOffsetL;

		[Tooltip("Instead of adjust each weapon individually, make a single offset here for each character")]
		public Vector3 ikPositionOffsetL;

		[vEditorToolbar("Ammo UI", false, "", false, false)]
		[Tooltip("Use the vAmmoDisplay to shot ammo count")]
		public bool useAmmoDisplay = true;

		[Tooltip("ID to find ammoDisplay for leftWeapon")]
		public int leftWeaponAmmoDisplayID = -1;

		[Tooltip("ID to find ammoDisplay for rightWeapon")]
		public int rightWeaponAmmoDisplayID = 1;

		[vEditorToolbar("LockOn", false, "", false, false)]
		[Header("- LockOn (need the shooter lockon component)")]
		[Tooltip("Allow the use of the LockOn or not")]
		public bool useLockOn;

		[Tooltip("Allow the use of the LockOn only with a Melee Weapon")]
		public bool useLockOnMeleeOnly = true;

		[vEditorToolbar("HipFire", false, "", false, false)]
		[Header("- HipFire Options")]
		[Tooltip("If enable, remember to change your weak attack input to other input - this allows shot without aim")]
		public bool hipfireShot;

		[Tooltip("Precision of the weapon when shooting using hipfire (without aiming)")]
		public float hipfireDispersion = 0.5f;

		[Tooltip("Time to keep aiming after shot")]
		[SerializeField]
		protected float hipfireAimTime = 2f;

		[vEditorToolbar("Camera Sway", false, "", false, false)]
		[Header("- Camera Sway Settings")]
		[Tooltip("Camera Sway movement while aiming")]
		public float cameraMaxSwayAmount = 2f;

		[Tooltip("Camera Sway Speed while aiming")]
		public float cameraSwaySpeed = 0.5f;

		[vEditorToolbar("Weapons", false, "", false, false)]
		public vShooterWeapon rWeapon;

		[vEditorToolbar("Weapons", false, "", false, false)]
		public vShooterWeapon lWeapon;

		public int reloadAnimatorLayer = 4;

		[HideInInspector]
		public vAmmoManager ammoManager;

		public TotalAmmoHandler totalAmmoHandler;

		public OnReloadWeapon onStartReloadWeapon;

		public OnReloadWeapon onFinishReloadWeapon;

		[HideInInspector]
		public vAmmoDisplay ammoDisplayR;

		[HideInInspector]
		public vAmmoDisplay ammoDisplayL;

		[HideInInspector]
		public vThirdPersonCamera tpCamera;

		[HideInInspector]
		public bool showCheckAimGizmos;

		private Animator animator;

		private int totalAmmo;

		private int secundaryTotalAmmo;

		private bool usingThirdPersonController;

		private float currentShotTime;

		private float hipfirePrecisionAngle;

		private float hipfirePrecision;

		internal bool isReloadingWeapon;

		private bool cancelReload;

		public float HipfireAimTime
		{
			get
			{
				return hipfireAimTime + (CurrentWeapon ? CurrentWeapon.shootFrequency : 0f);
			}
		}

		public virtual bool isShooting
		{
			get
			{
				return currentShotTime > 0f;
			}
		}

		public virtual vShooterWeapon CurrentWeapon
		{
			get
			{
				if (!rWeapon)
				{
					if (!lWeapon)
					{
						return null;
					}
					return lWeapon;
				}
				return rWeapon;
			}
		}

		public virtual bool IsLeftWeapon
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
			if (applyRecoilToCamera)
			{
				tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
			}
			ammoManager = GetComponent<vAmmoManager>();
			ammoManager.updateTotalAmmo = AmmoManagerWasUpdated;
			vThirdPersonController component = GetComponent<vThirdPersonController>();
			usingThirdPersonController = component;
			if (usingThirdPersonController && useCancelReload)
			{
				component.onReceiveDamage.AddListener(CancelReload);
			}
			if (useAmmoDisplay)
			{
				GetAmmoDisplays();
			}
			if ((bool)animator)
			{
				Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
				Transform boneTransform2 = animator.GetBoneTransform(HumanBodyBones.LeftHand);
				vShooterWeapon componentInChildren = boneTransform.GetComponentInChildren<vShooterWeapon>();
				vShooterWeapon componentInChildren2 = boneTransform2.GetComponentInChildren<vShooterWeapon>();
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
			if (useAmmoDisplay)
			{
				if ((bool)ammoDisplayR)
				{
					ammoDisplayR.UpdateDisplay("");
				}
				if ((bool)ammoDisplayL)
				{
					ammoDisplayL.UpdateDisplay("");
				}
			}
			UpdateTotalAmmo();
		}

		public virtual void SetLeftWeapon(GameObject weapon)
		{
			if (!(weapon != null))
			{
				return;
			}
			vShooterWeapon component = weapon.GetComponent<vShooterWeapon>();
			lWeapon = component;
			if (!lWeapon)
			{
				return;
			}
			lWeapon.ignoreTags = ignoreTags;
			lWeapon.hitLayer = damageLayer;
			lWeapon.root = base.transform;
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
			if (usingThirdPersonController)
			{
				if (useAmmoDisplay && !ammoDisplayL)
				{
					GetAmmoDisplays();
				}
				if (useAmmoDisplay && (bool)ammoDisplayL)
				{
					ammoDisplayL.Show();
				}
				UpdateLeftAmmo();
			}
			currentShotTime = 0f;
		}

		public virtual void SetRightWeapon(GameObject weapon)
		{
			if (!(weapon != null))
			{
				return;
			}
			vShooterWeapon component = weapon.GetComponent<vShooterWeapon>();
			rWeapon = component;
			if (!rWeapon)
			{
				return;
			}
			rWeapon.ignoreTags = ignoreTags;
			rWeapon.hitLayer = damageLayer;
			rWeapon.root = base.transform;
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
			if (usingThirdPersonController)
			{
				if (useAmmoDisplay && !ammoDisplayR)
				{
					GetAmmoDisplays();
				}
				if (useAmmoDisplay && (bool)ammoDisplayR)
				{
					ammoDisplayR.Show();
				}
				UpdateRightAmmo();
			}
			currentShotTime = 0f;
		}

		public virtual void OnDestroyWeapon(GameObject otherGameObject)
		{
			if (usingThirdPersonController)
			{
				vAmmoDisplay vAmmoDisplay = ((rWeapon != null && otherGameObject == rWeapon.gameObject) ? ammoDisplayR : ((lWeapon != null && otherGameObject == lWeapon.gameObject) ? ammoDisplayL : null));
				if (useAmmoDisplay && (bool)vAmmoDisplay)
				{
					vAmmoDisplay.UpdateDisplay("");
					vAmmoDisplay.Hide();
				}
			}
			currentShotTime = 0f;
		}

		protected virtual void GetAmmoDisplays()
		{
			vAmmoDisplay[] array = UnityEngine.Object.FindObjectsOfType<vAmmoDisplay>();
			if (array.Length == 0)
			{
				return;
			}
			if (!ammoDisplayL)
			{
				ammoDisplayL = array.vToList().Find((vAmmoDisplay d) => d.displayID == leftWeaponAmmoDisplayID);
			}
			if (!ammoDisplayR)
			{
				ammoDisplayR = array.vToList().Find((vAmmoDisplay d) => d.displayID == rightWeaponAmmoDisplayID);
			}
		}

		public virtual int GetMoveSetID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
			{
				result = (int)rWeapon.moveSetID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
			{
				result = (int)lWeapon.moveSetID;
			}
			return result;
		}

		public virtual int GetUpperBodyID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
			{
				result = (int)rWeapon.upperBodyID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
			{
				result = (int)lWeapon.upperBodyID;
			}
			return result;
		}

		public virtual int GetShotID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
			{
				result = (int)rWeapon.shotID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
			{
				result = (int)lWeapon.shotID;
			}
			return result;
		}

		public virtual int GetEquipID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
			{
				result = rWeapon.equipID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
			{
				result = lWeapon.equipID;
			}
			return result;
		}

		public virtual int GetReloadID()
		{
			int result = 0;
			if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
			{
				result = rWeapon.reloadID;
			}
			else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
			{
				result = lWeapon.reloadID;
			}
			return result;
		}

		public virtual bool WeaponHasAmmo(bool secundaryWeapon = false)
		{
			if (!secundaryWeapon)
			{
				return totalAmmo + (CurrentWeapon ? CurrentWeapon.ammoCount : 0) > 0;
			}
			return secundaryTotalAmmo + (((bool)CurrentWeapon && (bool)CurrentWeapon.secundaryWeapon) ? CurrentWeapon.secundaryWeapon.ammoCount : 0) > 0;
		}

		public virtual void ReloadWeapon()
		{
			vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
			if (!vShooterWeapon2 || !vShooterWeapon2.gameObject.activeInHierarchy)
			{
				return;
			}
			UpdateTotalAmmo();
			bool flag = false;
			if (vShooterWeapon2.ammoCount < vShooterWeapon2.clipSize && (vShooterWeapon2.isInfinityAmmo || WeaponHasAmmo()) && !vShooterWeapon2.autoReload)
			{
				onStartReloadWeapon.Invoke(vShooterWeapon2);
				if ((bool)animator)
				{
					animator.SetInteger("ReloadID", GetReloadID());
					animator.SetTrigger("Reload");
				}
				if ((bool)CurrentWeapon && CurrentWeapon.gameObject.activeInHierarchy)
				{
					StartCoroutine(AddAmmoToWeapon(CurrentWeapon, CurrentWeapon.reloadTime));
				}
				flag = true;
			}
			if ((bool)vShooterWeapon2.secundaryWeapon && vShooterWeapon2.secundaryWeapon.ammoCount >= vShooterWeapon2.secundaryWeapon.clipSize && (vShooterWeapon2.secundaryWeapon.isInfinityAmmo || WeaponHasAmmo(true)) && !vShooterWeapon2.secundaryWeapon.autoReload)
			{
				if (!flag && (bool)animator)
				{
					flag = true;
					animator.SetInteger("ReloadID", vShooterWeapon2.secundaryWeapon.reloadID);
					animator.SetTrigger("Reload");
				}
				StartCoroutine(AddAmmoToWeapon(CurrentWeapon.secundaryWeapon, flag ? CurrentWeapon.reloadTime : CurrentWeapon.secundaryWeapon.reloadTime, !flag));
			}
		}

		protected virtual IEnumerator AddAmmoToWeapon(vShooterWeapon weapon, float delayTime, bool ignoreEffects = false)
		{
			if (weapon.ammoCount >= weapon.clipSize || (!weapon.isInfinityAmmo && !WeaponHasAmmo()) || weapon.autoReload || cancelReload)
			{
				yield break;
			}
			if (!ignoreEffects)
			{
				weapon.ReloadEffect();
			}
			yield return new WaitForSeconds(delayTime);
			if (!cancelReload)
			{
				int num = (weapon.reloadOneByOne ? 1 : (weapon.clipSize - weapon.ammoCount));
				if (weapon.isInfinityAmmo)
				{
					weapon.AddAmmo(num);
				}
				else
				{
					if (WeaponAmmo(weapon).count < num)
					{
						num = WeaponAmmo(weapon).count;
					}
					weapon.AddAmmo(num);
					WeaponAmmo(weapon).Use(num);
				}
				if (weapon.reloadOneByOne && weapon.ammoCount < weapon.clipSize && WeaponHasAmmo())
				{
					if (WeaponAmmo(weapon).count == 0)
					{
						if (!ignoreEffects)
						{
							weapon.FinishReloadEffect();
						}
						if (!ignoreEffects)
						{
							isReloadingWeapon = false;
						}
						if (!ignoreEffects)
						{
							onFinishReloadWeapon.Invoke(weapon);
						}
					}
					else
					{
						if (!ignoreEffects)
						{
							isReloadingWeapon = true;
						}
						if (!cancelReload)
						{
							if (!ignoreEffects)
							{
								animator.SetInteger("ReloadID", weapon.reloadID);
								animator.SetTrigger("Reload");
							}
							StartCoroutine(AddAmmoToWeapon(weapon, delayTime, ignoreEffects));
						}
					}
				}
				else
				{
					if (!ignoreEffects)
					{
						weapon.FinishReloadEffect();
					}
					if (!ignoreEffects)
					{
						isReloadingWeapon = false;
					}
					if (!ignoreEffects)
					{
						onFinishReloadWeapon.Invoke(weapon);
					}
				}
			}
			UpdateTotalAmmo();
		}

		public virtual void CancelReload()
		{
			StartCoroutine(CancelReloadRoutine());
		}

		public virtual void CancelReload(vDamage damage)
		{
			if (!ignoreReacionIDList.Contains(damage.reaction_id))
			{
				StartCoroutine(CancelReloadRoutine());
			}
		}

		protected virtual IEnumerator CancelReloadRoutine()
		{
			if (!(CurrentWeapon != null))
			{
				yield break;
			}
			animator.ResetTrigger("Reload");
			cancelReload = true;
			StopCoroutine("AddAmmoToWeapon");
			yield return new WaitForSeconds(CurrentWeapon.reloadTime + 0.1f);
			cancelReload = false;
			if (isReloadingWeapon)
			{
				isReloadingWeapon = false;
				if ((bool)CurrentWeapon)
				{
					onFinishReloadWeapon.Invoke(CurrentWeapon);
				}
			}
			UpdateTotalAmmo();
		}

		public virtual void ReloadWeaponAuto(vShooterWeapon weapon, bool secundaryWeapon = false)
		{
			if (!weapon)
			{
				return;
			}
			UpdateTotalAmmo();
			if (weapon.ammoCount >= weapon.clipSize || (!weapon.isInfinityAmmo && !WeaponHasAmmo(secundaryWeapon)))
			{
				return;
			}
			int num = weapon.clipSize - weapon.ammoCount;
			if (weapon.isInfinityAmmo)
			{
				weapon.AddAmmo(num);
				return;
			}
			if (WeaponAmmo(weapon).count < num)
			{
				num = WeaponAmmo(weapon).count;
			}
			weapon.AddAmmo(num);
			WeaponAmmo(weapon).Use(num);
		}

		public virtual vAmmo WeaponAmmo(vShooterWeapon weapon)
		{
			if (!weapon)
			{
				return null;
			}
			vAmmo result = new vAmmo();
			if ((bool)ammoManager && ammoManager.ammos != null && ammoManager.ammos.Count > 0)
			{
				result = ammoManager.GetAmmo(weapon.ammoID);
			}
			return result;
		}

		public virtual void AmmoManagerWasUpdated()
		{
			bool flag = true;
			if ((bool)CurrentWeapon)
			{
				if (CurrentWeapon.autoReload)
				{
					ReloadWeaponAuto(CurrentWeapon);
					flag = false;
				}
				if ((bool)CurrentWeapon.secundaryWeapon && CurrentWeapon.secundaryWeapon.autoReload)
				{
					ReloadWeaponAuto(CurrentWeapon.secundaryWeapon, true);
					flag = false;
				}
			}
			if (flag)
			{
				UpdateTotalAmmo();
			}
		}

		public virtual void UpdateTotalAmmo()
		{
			UpdateLeftAmmo();
			UpdateRightAmmo();
		}

		public virtual void UpdateLeftAmmo()
		{
			if ((bool)lWeapon)
			{
				UpdateTotalAmmo(lWeapon, ref totalAmmo, -1);
				UpdateTotalAmmo(lWeapon.secundaryWeapon, ref secundaryTotalAmmo, -1);
			}
		}

		public virtual bool IsCurrentWeaponActive()
		{
			if ((bool)CurrentWeapon)
			{
				return CurrentWeapon.gameObject.activeInHierarchy;
			}
			return false;
		}

		public virtual void UpdateRightAmmo()
		{
			if ((bool)rWeapon)
			{
				UpdateTotalAmmo(rWeapon, ref totalAmmo, 1);
				UpdateTotalAmmo(rWeapon.secundaryWeapon, ref secundaryTotalAmmo, 1);
			}
		}

		protected virtual void UpdateTotalAmmo(vShooterWeapon weapon, ref int targetTotalAmmo, int displayId)
		{
			if (!weapon)
			{
				return;
			}
			int num = 0;
			if (weapon.isInfinityAmmo)
			{
				num = 9999;
			}
			else
			{
				vAmmo vAmmo = WeaponAmmo(weapon);
				if (vAmmo != null)
				{
					num += vAmmo.count;
				}
			}
			targetTotalAmmo = num;
			UpdateAmmoDisplay(displayId);
		}

		protected virtual void UpdateAmmoDisplay(int displayId)
		{
			if (!useAmmoDisplay)
			{
				return;
			}
			vShooterWeapon vShooterWeapon2 = ((displayId == 1) ? rWeapon : lWeapon);
			if (!vShooterWeapon2)
			{
				return;
			}
			if (!ammoDisplayR || !ammoDisplayL)
			{
				GetAmmoDisplays();
			}
			vAmmoDisplay vAmmoDisplay = ((displayId == 1) ? ammoDisplayR : ammoDisplayL);
			if (useAmmoDisplay && (bool)vAmmoDisplay)
			{
				if ((bool)vShooterWeapon2.secundaryWeapon)
				{
					string text = ((!vShooterWeapon2.autoReload) ? string.Format("{0} / {1}", vShooterWeapon2.ammoCount, vShooterWeapon2.isInfinityAmmo ? "Infinity" : totalAmmo.ToString()) : (vShooterWeapon2.isInfinityAmmo ? "Infinity" : (vShooterWeapon2.ammoCount + totalAmmo).ToString()));
					string text2 = ((!vShooterWeapon2.secundaryWeapon.autoReload) ? string.Format("{0} / {1}", vShooterWeapon2.secundaryWeapon.ammoCount, vShooterWeapon2.secundaryWeapon.isInfinityAmmo ? "Infinity" : secundaryTotalAmmo.ToString()) : (vShooterWeapon2.secundaryWeapon.isInfinityAmmo ? "Infinity" : (vShooterWeapon2.secundaryWeapon.ammoCount + secundaryTotalAmmo).ToString()));
					vAmmoDisplay.UpdateDisplay(text + "\n" + text2, vShooterWeapon2.ammoID);
				}
				else
				{
					string text3 = ((!vShooterWeapon2.autoReload) ? string.Format("{0} / {1}", vShooterWeapon2.ammoCount, vShooterWeapon2.isInfinityAmmo ? "Infinity" : totalAmmo.ToString()) : (vShooterWeapon2.isInfinityAmmo ? "Infinity" : (vShooterWeapon2.ammoCount + totalAmmo).ToString()));
					vAmmoDisplay.UpdateDisplay(text3, vShooterWeapon2.ammoID);
				}
			}
		}

		public virtual void Shoot(Vector3 aimPosition, bool applyHipfirePrecision = false, bool useSecundaryWeapon = false)
		{
			if (isShooting)
			{
				return;
			}
			vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
			if (!vShooterWeapon2 || !vShooterWeapon2.gameObject.activeInHierarchy)
			{
				return;
			}
			vShooterWeapon secundaryWeapon = vShooterWeapon2.secundaryWeapon;
			if (!useSecundaryWeapon || (bool)secundaryWeapon)
			{
				vShooterWeapon vShooterWeapon3 = (useSecundaryWeapon ? secundaryWeapon : vShooterWeapon2);
				if (vShooterWeapon3.autoReload)
				{
					ReloadWeaponAuto(vShooterWeapon3, useSecundaryWeapon);
				}
				Vector3 aimPosition2 = (applyHipfirePrecision ? (aimPosition + HipFirePrecision(aimPosition)) : aimPosition);
				bool applyRecoil = false;
				vShooterWeapon3.Shoot(aimPosition2, base.transform, delegate(bool sucessful)
				{
					applyRecoil = sucessful;
				});
				if (applyRecoilToCamera && applyRecoil)
				{
					float horizontal = UnityEngine.Random.Range(vShooterWeapon3.recoilLeft, vShooterWeapon3.recoilRight);
					float up = UnityEngine.Random.Range(0f, vShooterWeapon3.recoilUp);
					StartCoroutine(Recoil(horizontal, up));
				}
				UpdateAmmoDisplay(rWeapon ? 1 : (-1));
				if (vShooterWeapon3.autoReload)
				{
					ReloadWeaponAuto(vShooterWeapon3, useSecundaryWeapon);
				}
				currentShotTime = vShooterWeapon2.shootFrequency;
			}
		}

		protected virtual IEnumerator Recoil(float horizontal, float up)
		{
			yield return new WaitForSeconds(0.02f);
			if ((bool)animator)
			{
				animator.SetTrigger("Shoot");
			}
			if (tpCamera != null)
			{
				tpCamera.RotateCamera(horizontal, up);
			}
		}

		protected virtual Vector3 HipFirePrecision(Vector3 _aimPosition)
		{
			vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
			if (!vShooterWeapon2)
			{
				return Vector3.zero;
			}
			hipfirePrecisionAngle = UnityEngine.Random.Range(-1000, 1000);
			hipfirePrecision = UnityEngine.Random.Range(0f - hipfireDispersion, hipfireDispersion);
			return (Quaternion.AngleAxis(hipfirePrecisionAngle, _aimPosition - vShooterWeapon2.muzzle.position) * Vector3.up).normalized * hipfirePrecision;
		}

		public virtual void CameraSway()
		{
			vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
			if (!vShooterWeapon2)
			{
				return;
			}
			float num = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed) - 0.5f;
			float num2 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed + 100f) - 0.5f;
			float num3 = cameraMaxSwayAmount * (1f - vShooterWeapon2.precision);
			if (num3 != 0f)
			{
				num *= num3;
				num2 *= num3;
				float num4 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed) - 0.5f;
				float num5 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed + 100f) - 0.5f;
				num4 *= 0f - num3 * 0.25f;
				num5 *= num3 * 0.25f;
				if (tpCamera != null)
				{
					vThirdPersonCamera.instance.offsetMouse.x = num + num4;
					vThirdPersonCamera.instance.offsetMouse.y = num2 + num5;
				}
			}
		}

		public virtual void UpdateShotTime()
		{
			if (currentShotTime > 0f)
			{
				currentShotTime -= Time.deltaTime;
			}
		}
	}
}
