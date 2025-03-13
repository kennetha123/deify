using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
	[vClassHeader("Shooter Weapon", true, "icon_v2", false, "", openClose = false)]
	public class vShooterWeapon : vShooterWeaponBase
	{
		[Serializable]
		public class OnChangePowerCharger : UnityEvent<float>
		{
		}

		public delegate bool CheckAmmoHandle(ref bool isValid, ref int totalAmmo);

		public delegate void ChangeAmmoHandle(int value);

		[vEditorToolbar("Weapon Settings", false, "", false, false)]
		public bool isLeftWeapon;

		[Tooltip("Hold Charge Input to charge")]
		public bool chargeWeapon;

		[vHideInInspector("chargeWeapon", false)]
		public bool autoShotOnFinishCharge;

		[vHideInInspector("chargeWeapon", false)]
		public float chargeSpeed = 0.1f;

		[vHideInInspector("chargeWeapon", false)]
		public float chargeDamageMultiplier = 2f;

		[vHideInInspector("chargeWeapon", false)]
		public bool changeVelocityByCharge = true;

		[vHideInInspector("chargeWeapon", false)]
		public float chargeVelocityMultiplier = 2f;

		[Tooltip("Change between automatic weapon or shot once")]
		[vHideInInspector("chargeWeapon", true)]
		public bool automaticWeapon;

		[vEditorToolbar("Ammo", false, "", false, false)]
		public float reloadTime = 1f;

		public bool reloadOneByOne;

		[Tooltip("Max clip size of your weapon")]
		public int clipSize;

		[Tooltip("Automatically reload the weapon when it's empty")]
		public bool autoReload;

		[Tooltip("Ammo ID - make sure your AmmoManager and ItemListData use the same ID")]
		[vHideInInspector("isInfinityAmmo", true)]
		public int ammoID;

		[vEditorToolbar("Weapon ID", false, "", false, false)]
		[Tooltip("What moveset the underbody will play")]
		public float moveSetID;

		[Tooltip("What moveset the uperbody will play")]
		public float upperBodyID;

		[Tooltip("What shot animation will trigger")]
		public float shotID;

		[vHideInInspector("autoReload", true)]
		[Tooltip("What reload animation will play")]
		public int reloadID;

		[Tooltip("What equip animation will play")]
		public int equipID;

		[vEditorToolbar("IK Options", false, "", false, false)]
		[Tooltip("IK will help the right hand to align where you actually is aiming")]
		public bool alignRightHandToAim = true;

		[Tooltip("IK will help the right hand to align where you actually is aiming")]
		public bool alignRightUpperArmToAim = true;

		public bool raycastAimTarget = true;

		[Tooltip("Left IK on Idle")]
		public bool useIkOnIdle = true;

		[Tooltip("Left IK on free locomotion")]
		public bool useIkOnFree = true;

		[Tooltip("Left IK on strafe locomotion")]
		public bool useIkOnStrafe = true;

		[Tooltip("Left IK while attacking")]
		public bool useIkAttacking;

		public bool disableIkOnShot;

		public bool useIKOnAiming = true;

		public bool canAimWithoutAmmo = true;

		[vHelpBox("HeadTrack Offsets", vHelpBoxAttribute.MessageType.None)]
		public Vector2 headOffset;

		public Vector2 spineOffset;

		public Vector2 headOffsetCrouch;

		public Vector2 spineOffsetCrouch;

		public Transform handIKTarget;

		[vEditorToolbar("Projectile", false, "", false, false)]
		public vShooterWeapon secundaryWeapon;

		[Tooltip("Assign the aimReference of your weapon")]
		public Transform aimReference;

		[Tooltip("how much precision the weapon have, 1 means no cameraSway and 0 means maxCameraSway from the ShooterManager")]
		[Range(0f, 1f)]
		public float precision = 0.5f;

		[Tooltip("Creates a right recoil on the camera")]
		public float recoilRight = 1f;

		[Tooltip("Creates a left recoil on the camera")]
		public float recoilLeft = -1f;

		[Tooltip("Creates a up recoil on the camera")]
		public float recoilUp = 1f;

		[vEditorToolbar("Audio & VFX", false, "", false, false)]
		public AudioClip reloadClip;

		public AudioClip finishReloadClip;

		[vEditorToolbar("Scope UI", false, "", false, false)]
		[vHelpBox("Third Person Controller Only", vHelpBoxAttribute.MessageType.Info)]
		public bool onlyUseScopeUIView;

		[Tooltip("Check this bool to use an UI image for the scope, ex: snipers")]
		public bool useUI;

		[Tooltip("You can create different Aim sprites and use for different weapons")]
		public int scopeID;

		[Tooltip("change the FOV of the scope view\n **The calc is default value (60)-scopeZoom**")]
		[Range(-118f, 60f)]
		public float scopeZoom = 60f;

		[Tooltip("Used with the TPCamera to use a custom CameraState, if it's empty it will use the 'Aiming' CameraState.")]
		public string customAimCameraState;

		[Tooltip("assign an empty transform with the pos/rot of your scope view")]
		public Transform scopeTarget;

		public Camera zoomScopeCamera;

		[vHelpBox("Keep Scope Camera Z is used to align z rotation of the zoomScopeCamera to z rotation of the weapon muzzle<color=red> (Projectile toolbar)</color>. if you want to align camera with Vector3.up in z rotation enable this.", vHelpBoxAttribute.MessageType.None)]
		public bool keepScopeCameraRotationZ = true;

		[HideInInspector]
		public bool isAiming;

		[HideInInspector]
		public bool usingScope;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onReload;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onFinishReload;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onEnableAim;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onDisableAim;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onEnableScope;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onDisableScope;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent onFullPower;

		public OnChangePowerCharger onChangerPowerCharger;

		[HideInInspector]
		public Transform root;

		[HideInInspector]
		public bool isSecundaryWeapon;

		private float _charge;

		public CheckAmmoHandle checkAmmoHandle;

		public ChangeAmmoHandle changeAmmoHandle;

		[NonSerialized]
		private float testTime;

		public float powerCharge
		{
			get
			{
				return _charge;
			}
			set
			{
				if (value != _charge)
				{
					_charge = value;
					onChangerPowerCharger.Invoke(_charge);
					if (_charge >= 1f)
					{
						onFullPower.Invoke();
					}
				}
			}
		}

		public int ammoCount
		{
			get
			{
				if (checkAmmoHandle != null)
				{
					bool isValid = false;
					int totalAmmo = 0;
					checkAmmoHandle(ref isValid, ref totalAmmo);
					if (isValid)
					{
						return totalAmmo;
					}
					return ammo;
				}
				return ammo;
			}
		}

		protected override float damageMultiplier
		{
			get
			{
				if (!chargeWeapon)
				{
					return base.damageMultiplier;
				}
				return (float)(int)(1f + Mathf.Lerp(0f, chargeDamageMultiplier, _charge)) + damageMultiplierMod;
			}
		}

		protected override float velocityMultiplier
		{
			get
			{
				if (!chargeWeapon || !changeVelocityByCharge)
				{
					return base.velocityMultiplier;
				}
				return 1f + Mathf.Lerp(0f, chargeVelocityMultiplier, _charge) + velocityMultiplierMod;
			}
		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying && testShootEffect)
			{
				if (testTime <= 0f)
				{
					Shootest();
				}
				else
				{
					testTime -= Time.deltaTime;
				}
			}
		}

		private void Start()
		{
			SetScopeZoom(scopeZoom);
		}

		public void Shootest()
		{
			testTime = shootFrequency;
			StartEmitters();
			lightOnShot.enabled = true;
			source.PlayOneShot(fireClip);
			Invoke("StopShootTest", 0.037f);
		}

		private void StopShootTest()
		{
			StopEmitters();
			lightOnShot.enabled = false;
		}

		public void SetPrecision(float value)
		{
			precision = Mathf.Clamp(value, 0f, 1f);
		}

		public override bool HasAmmo()
		{
			if (checkAmmoHandle != null)
			{
				bool isValid = false;
				int totalAmmo = 0;
				bool result = checkAmmoHandle(ref isValid, ref totalAmmo);
				if (isValid)
				{
					return result;
				}
				return ammo > 0;
			}
			return ammo > 0;
		}

		public void AddAmmo(int value)
		{
			if (checkAmmoHandle != null && changeAmmoHandle != null)
			{
				bool isValid = false;
				int totalAmmo = 0;
				checkAmmoHandle(ref isValid, ref totalAmmo);
				if (isValid)
				{
					changeAmmoHandle(value);
				}
				else
				{
					ammo += value;
				}
			}
			else
			{
				ammo += value;
			}
		}

		public override void UseAmmo(int count = 1)
		{
			if (checkAmmoHandle != null && changeAmmoHandle != null)
			{
				bool isValid = false;
				int totalAmmo = 0;
				checkAmmoHandle(ref isValid, ref totalAmmo);
				if (isValid)
				{
					changeAmmoHandle(-count);
				}
				else
				{
					ammo -= count;
				}
			}
			else
			{
				ammo -= count;
			}
		}

		public virtual void ReloadEffect()
		{
			if ((bool)source && (bool)reloadClip)
			{
				source.Stop();
				source.PlayOneShot(reloadClip);
			}
			onReload.Invoke();
		}

		public virtual void FinishReloadEffect()
		{
			if ((bool)source && (bool)finishReloadClip)
			{
				source.Stop();
				source.PlayOneShot(finishReloadClip);
			}
			onFinishReload.Invoke();
		}

		public void SetScopeZoom(float value)
		{
			if ((bool)zoomScopeCamera)
			{
				float fieldOfView = Mathf.Clamp(61f - value, 1f, 179f);
				zoomScopeCamera.fieldOfView = fieldOfView;
			}
		}

		public void SetActiveAim(bool value)
		{
			if (isAiming != value)
			{
				isAiming = value;
				if (isAiming)
				{
					onEnableAim.Invoke();
				}
				else
				{
					onDisableAim.Invoke();
				}
			}
		}

		public void SetActiveScope(bool value)
		{
			if (usingScope != value)
			{
				usingScope = value;
				if (usingScope)
				{
					onEnableScope.Invoke();
				}
				else
				{
					onDisableScope.Invoke();
				}
			}
		}

		public void SetScopeLookTarget(Vector3 point)
		{
			if ((bool)zoomScopeCamera)
			{
				Vector3 eulerAngles = Quaternion.LookRotation(point - zoomScopeCamera.transform.position, Vector3.up).eulerAngles;
				if (keepScopeCameraRotationZ)
				{
					eulerAngles.z = muzzle.transform.eulerAngles.z;
				}
				zoomScopeCamera.transform.eulerAngles = eulerAngles;
			}
		}
	}
}
