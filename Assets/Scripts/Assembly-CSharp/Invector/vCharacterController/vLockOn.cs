using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	[vClassHeader("MELEE LOCK-ON", true, "icon_v2", false, "")]
	public class vLockOn : vLockOnBehaviour
	{
		[Serializable]
		public class LockOnEvent : UnityEvent<Transform>
		{
		}

		public bool strafeWhileLockOn = true;

		[Tooltip("Create a Image inside the UI and assign here")]
		public RectTransform aimImagePrefab;

		public Vector2 aimImageSize = new Vector2(30f, 30f);

		[Tooltip("True: Hide the sprite when not Lock On, False: Always show the Sprite")]
		public bool hideSprite = true;

		[Tooltip("Create a offset for the sprite based at the center of the target")]
		[Range(-0.5f, 0.5f)]
		public float spriteHeight = 0.25f;

		[Tooltip("Offset for the camera height")]
		public float cameraHeightOffset;

		[Header("LockOn Inputs")]
		public GenericInput lockOnInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

		public GenericInput nexTargetInput = new GenericInput("X", false, false, "RightAnalogHorizontal", true, false, "X", false, false);

		public GenericInput previousTargetInput = new GenericInput("Z", false, false, "RightAnalogHorizontal", true, true, "Z", false, false);

		public bool isLockingOn;

		public LockOnEvent onLockOnTarget;

		public LockOnEvent onUnLockOnTarget;

		private Canvas _aimCanvas;

		private RectTransform _aimImage;

		protected bool inTarget;

		protected vMeleeCombatInput tpInput;

		public Canvas aimCanvas
		{
			get
			{
				if ((bool)_aimCanvas)
				{
					return _aimCanvas;
				}
				_aimCanvas = vHUDController.instance.GetComponentInParent<Canvas>();
				if (_aimCanvas == null)
				{
					UnityEngine.Object.FindObjectOfType<Canvas>();
				}
				return _aimCanvas;
			}
		}

		public RectTransform aimImage
		{
			get
			{
				if ((bool)_aimImage)
				{
					return _aimImage;
				}
				if ((bool)aimCanvas)
				{
					_aimImage = UnityEngine.Object.Instantiate(aimImagePrefab, Vector2.zero, Quaternion.identity);
					_aimImage.SetParent(aimCanvas.transform);
					return _aimImage;
				}
				return null;
			}
		}

		protected virtual void Start()
		{
			Init();
			tpInput = GetComponent<vMeleeCombatInput>();
			if ((bool)tpInput)
			{
				tpInput.onUpdateInput.AddListener(UpdateLockOn);
				GetComponent<vHealthController>().onDead.AddListener(delegate
				{
					isLockingOn = false;
					LockOn(false);
					UpdateLockOn(tpInput);
				});
			}
		}

		protected virtual void UpdateLockOn(vThirdPersonInput tpInput)
		{
			if (!(this.tpInput == null))
			{
				LockOnInput();
				SwitchTargetsInput();
				CheckForCharacterAlive();
				UpdateAimImage();
			}
		}

		protected virtual void LockOnInput()
		{
			if (tpInput.tpCamera == null || tpInput.cc == null)
			{
				return;
			}
			if (lockOnInput.GetButtonDown() && !tpInput.cc.actions)
			{
				isLockingOn = !isLockingOn;
				LockOn(isLockingOn);
			}
			else if (isLockingOn && tpInput.tpCamera.lockTarget == null)
			{
				isLockingOn = false;
				LockOn(false);
			}
			if (strafeWhileLockOn && !tpInput.cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
			{
				if (isLockingOn && tpInput.tpCamera.lockTarget != null)
				{
					tpInput.cc.isStrafing = true;
				}
				else
				{
					tpInput.cc.isStrafing = false;
				}
			}
		}

		protected override void SetTarget()
		{
			if (tpInput.tpCamera != null)
			{
				tpInput.tpCamera.SetLockTarget(currentTarget.transform, cameraHeightOffset);
				onLockOnTarget.Invoke(currentTarget);
			}
		}

		protected virtual void SwitchTargetsInput()
		{
			if (!(tpInput.tpCamera == null) && (bool)tpInput.tpCamera.lockTarget)
			{
				if (previousTargetInput.GetButtonDown())
				{
					PreviousTarget();
				}
				else if (nexTargetInput.GetButtonDown())
				{
					NextTarget();
				}
			}
		}

		protected virtual void CheckForCharacterAlive()
		{
			if (((bool)currentTarget && !isCharacterAlive() && inTarget) || (inTarget && !isCharacterAlive()))
			{
				ResetLockOn();
				inTarget = false;
				LockOn(true);
				StopLockOn();
			}
		}

		protected virtual void LockOn(bool value)
		{
			base.UpdateLockOn(value);
			if (!inTarget && (bool)currentTarget)
			{
				inTarget = true;
				SetTarget();
			}
			else if (inTarget && !currentTarget)
			{
				inTarget = false;
				StopLockOn();
			}
		}

		protected virtual void UpdateAimImage()
		{
			if (!aimCanvas || !aimImage)
			{
				return;
			}
			if (hideSprite)
			{
				aimImage.sizeDelta = aimImageSize;
				if ((bool)currentTarget && !aimImage.transform.gameObject.activeSelf && isCharacterAlive())
				{
					aimImage.transform.gameObject.SetActive(true);
				}
				else if (!currentTarget && aimImage.transform.gameObject.activeSelf)
				{
					aimImage.transform.gameObject.SetActive(false);
				}
				else if (_aimImage.transform.gameObject.activeSelf && !isCharacterAlive())
				{
					aimImage.transform.gameObject.SetActive(false);
				}
			}
			if ((bool)currentTarget && (bool)aimImage && (bool)aimCanvas)
			{
				aimImage.anchoredPosition = currentTarget.GetScreenPointOffBoundsCenter(aimCanvas, Camera.main, spriteHeight);
			}
			else if ((bool)aimCanvas)
			{
				aimImage.anchoredPosition = Vector2.zero;
			}
		}

		public virtual void StopLockOn()
		{
			if (currentTarget == null && tpInput.tpCamera != null)
			{
				tpInput.tpCamera.RemoveLockTarget();
				onUnLockOnTarget.Invoke(currentTarget);
				isLockingOn = false;
				inTarget = false;
			}
		}

		public virtual void NextTarget()
		{
			base.ChangeTarget(1);
		}

		public virtual void PreviousTarget()
		{
			base.ChangeTarget(-1);
		}
	}
}
