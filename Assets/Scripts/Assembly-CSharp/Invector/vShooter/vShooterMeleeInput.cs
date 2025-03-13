using Invector.IK;
using Invector.vCamera;
using Invector.vCharacterController;
using UnityEngine;

namespace Invector.vShooter
{
	[vClassHeader("SHOOTER/MELEE INPUT", true, "icon_v2", false, "", iconName = "inputIcon")]
	public class vShooterMeleeInput : vMeleeCombatInput
	{
		[vEditorToolbar("Inputs", false, "", false, false)]
		[Header("Shooter Inputs")]
		public GenericInput aimInput = new GenericInput("Mouse1", false, "LT", true, "LT", false);

		public GenericInput shotInput = new GenericInput("Mouse0", false, "RT", true, "RT", false);

		public GenericInput secundaryShotInput = new GenericInput("Mouse2", false, "X", true, "X", false);

		public GenericInput reloadInput = new GenericInput("R", "LB", "LB");

		public GenericInput switchCameraSideInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

		public GenericInput scopeViewInput = new GenericInput("Z", "RB", "RB");

		internal vShooterManager shooterManager;

		internal bool isAiming;

		internal bool isReloading;

		internal bool isEquipping;

		internal bool defaultStrafeWalk;

		internal Transform leftHand;

		internal Transform rightHand;

		internal Transform rightLowerArm;

		internal Transform leftLowerArm;

		internal Transform rightUpperArm;

		internal Transform leftUpperArm;

		internal Vector3 aimPosition;

		internal float aimTimming;

		protected int onlyArmsLayer;

		protected int shootCountA;

		protected int shootCountB;

		protected bool allowAttack;

		protected bool aimConditions;

		protected bool isUsingScopeView;

		protected bool isCameraRightSwitched;

		protected float onlyArmsLayerWeight;

		protected float lIKWeight;

		protected float armAlignmentWeight;

		protected float aimWeight;

		protected float lastAimDistance;

		protected Quaternion handRotation;

		protected Quaternion upperArmRotation;

		protected vIKSolver leftIK;

		protected vIKSolver rightIK;

		protected vHeadTrack headTrack;

		private vControlAimCanvas _controlAimCanvas;

		private GameObject aimAngleReference;

		private Vector3 lastUpperArmRotation;

		private Vector3 lastLowerArmRotation;

		private Vector3 lastHandRotation;

		private Vector3 lastIKHandPosition;

		private Vector3 lastIKHandRotation;

		private Vector3 currentUpperArmOffset;

		private Vector3 currentLowerArmOffset;

		private Vector3 currentHandOffset;

		private Vector3 ikRotationOffset;

		private Vector3 ikPositionOffset;

		private Quaternion upperArmRotationAlignment;

		private Quaternion handRotationAlignment;

		internal bool lockShooterInput;

		public vControlAimCanvas controlAimCanvas
		{
			get
			{
				if (!_controlAimCanvas)
				{
					_controlAimCanvas = Object.FindObjectOfType<vControlAimCanvas>();
					if ((bool)_controlAimCanvas)
					{
						_controlAimCanvas.Init(cc);
					}
				}
				return _controlAimCanvas;
			}
		}

		public override bool lockInventory
		{
			get
			{
				if (!base.lockInventory)
				{
					return isReloading;
				}
				return true;
			}
		}

		public virtual vShooterWeapon CurrentActiveWeapon
		{
			get
			{
				if (!shooterManager.CurrentWeapon || !shooterManager.IsCurrentWeaponActive())
				{
					return null;
				}
				return shooterManager.CurrentWeapon;
			}
		}

		protected virtual Vector3 targetArmAlignmentPosition
		{
			get
			{
				if (!isUsingScopeView || !controlAimCanvas.scopeCamera)
				{
					return aimPosition;
				}
				return cameraMain.transform.position + cameraMain.transform.forward * lastAimDistance;
			}
		}

		protected virtual Vector3 targetArmAligmentDirection
		{
			get
			{
				return (((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive && (bool)controlAimCanvas.scopeCamera) ? controlAimCanvas.scopeCamera.transform : cameraMain.transform).forward;
			}
		}

		protected override void Start()
		{
			shooterManager = GetComponent<vShooterManager>();
			base.Start();
			leftHand = base.animator.GetBoneTransform(HumanBodyBones.LeftHand);
			rightHand = base.animator.GetBoneTransform(HumanBodyBones.RightHand);
			leftLowerArm = base.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			rightLowerArm = base.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			leftUpperArm = base.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			rightUpperArm = base.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			onlyArmsLayer = base.animator.GetLayerIndex("OnlyArms");
			aimAngleReference = new GameObject("aimAngleReference");
			aimAngleReference.tag = "Ignore Ragdoll";
			aimAngleReference.transform.rotation = base.transform.rotation;
			Transform boneTransform = base.animator.GetBoneTransform(HumanBodyBones.Head);
			aimAngleReference.transform.SetParent(boneTransform);
			aimAngleReference.transform.localPosition = Vector3.zero;
			defaultStrafeWalk = cc.strafeSpeed.walkByDefault;
			headTrack = GetComponent<vHeadTrack>();
			if (!controlAimCanvas)
			{
				Debug.LogWarning("Missing the AimCanvas, drag and drop the prefab to this scene in order to Aim", base.gameObject);
			}
		}

		protected override void LateUpdate()
		{
			if (updateIK || base.animator.updateMode != AnimatorUpdateMode.Fixed)
			{
				base.LateUpdate();
				UpdateAimBehaviour();
			}
		}

		public virtual void SetLockShooterInput(bool value)
		{
			lockShooterInput = value;
			if (value)
			{
				cc.isStrafing = false;
				base.isBlocking = false;
				isAiming = false;
				aimTimming = 0f;
				if ((bool)controlAimCanvas)
				{
					controlAimCanvas.SetActiveAim(false);
					controlAimCanvas.SetActiveScopeCamera(false);
				}
			}
		}

		protected override void InputHandle()
		{
			if ((cc.lockMovement && !cc.isStepBack) || cc == null || lockInput || cc.isDead)
			{
				return;
			}
			if (!base.isAttacking)
			{
				if (!cc.ragdolled)
				{
					MoveCharacter();
					CrouchInput();
					StrafeInput();
					JumpInput();
				}
			}
			else
			{
				cc.input = Vector2.zero;
			}
			if (MeleeAttackConditions && !isAiming && !isReloading && !lockMeleeInput && (shooterManager.CurrentWeapon == null || (CurrentActiveWeapon == null && !shooterManager.hipfireShot)))
			{
				MeleeWeakAttackInput();
				MeleeStrongAttackInput();
				BlockingInput();
			}
			else
			{
				base.isBlocking = false;
			}
			if (lockShooterInput)
			{
				isAiming = false;
			}
			else if (shooterManager == null || CurrentActiveWeapon == null || isEquipping)
			{
				if (isAiming)
				{
					isAiming = false;
					if (cc.isStrafing)
					{
						cc.Strafe();
					}
					if (controlAimCanvas != null)
					{
						controlAimCanvas.SetActiveAim(false);
						controlAimCanvas.SetActiveScopeCamera(false);
					}
					if ((bool)shooterManager && (bool)shooterManager.CurrentWeapon && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
					{
						CurrentActiveWeapon.powerCharge = 0f;
					}
					if ((bool)shooterManager && (bool)shooterManager.CurrentWeapon && shooterManager.CurrentWeapon.secundaryWeapon != null && shooterManager.CurrentWeapon.secundaryWeapon.chargeWeapon && shooterManager.CurrentWeapon.secundaryWeapon.powerCharge != 0f)
					{
						shooterManager.CurrentWeapon.secundaryWeapon.powerCharge = 0f;
					}
					shootCountA = 0;
					shootCountB = 0;
				}
			}
			else
			{
				AimInput();
				ShotInput();
				ReloadInput();
				SwitchCameraSideInput();
				ScopeViewInput();
			}
			onUpdateInput.Invoke(this);
		}

		public virtual void AlwaysAim(bool value)
		{
			shooterManager.alwaysAiming = value;
		}

		public virtual void AimInput()
		{
			if (!shooterManager || base.isAttacking)
			{
				isAiming = false;
				cc.strafeSpeed.walkByDefault = defaultStrafeWalk;
				if ((bool)controlAimCanvas)
				{
					controlAimCanvas.SetActiveAim(false);
					controlAimCanvas.SetActiveScopeCamera(false);
				}
				if (cc.isStrafing)
				{
					cc.Strafe();
				}
				return;
			}
			if (shooterManager.onlyWalkWhenAiming)
			{
				cc.strafeSpeed.walkByDefault = isAiming || defaultStrafeWalk;
			}
			if (cc.locomotionType == vThirdPersonMotor.LocomotionType.OnlyFree)
			{
				Debug.LogWarning("Shooter behaviour needs to be OnlyStrafe or Free with Strafe. \n Please change the Locomotion Type.");
				return;
			}
			if (shooterManager.hipfireShot && aimTimming > 0f)
			{
				aimTimming -= Time.deltaTime;
			}
			if (!shooterManager || !CurrentActiveWeapon)
			{
				if ((bool)controlAimCanvas)
				{
					controlAimCanvas.SetActiveAim(false);
					controlAimCanvas.SetActiveScopeCamera(false);
				}
				isAiming = false;
				if (cc.isStrafing)
				{
					cc.Strafe();
				}
				return;
			}
			if (!cc.isRolling)
			{
				isAiming = (!isReloading && (aimInput.GetButton() || shooterManager.alwaysAiming) && !cc.ragdolled && !cc.actions && !cc.customAction) || (cc.actions && cc.isJumping);
			}
			if ((bool)headTrack)
			{
				headTrack.awaysFollowCamera = isAiming;
			}
			bool flag = isAiming || aimTimming > 0f;
			if (cc.locomotionType == vThirdPersonMotor.LocomotionType.FreeWithStrafe)
			{
				if (flag && !cc.isStrafing)
				{
					cc.Strafe();
				}
				else if (!flag && cc.isStrafing)
				{
					cc.Strafe();
				}
			}
			if (flag && shooterManager.onlyWalkWhenAiming && cc.isSprinting)
			{
				cc.isSprinting = false;
			}
			if ((bool)controlAimCanvas)
			{
				if (flag && !controlAimCanvas.isAimActive)
				{
					controlAimCanvas.SetActiveAim(true);
				}
				if (!flag && controlAimCanvas.isAimActive)
				{
					controlAimCanvas.SetActiveAim(false);
				}
			}
			if ((bool)shooterManager.rWeapon)
			{
				shooterManager.rWeapon.SetActiveAim(flag && aimConditions);
				shooterManager.rWeapon.SetActiveScope(flag && isUsingScopeView);
			}
			else if ((bool)shooterManager.lWeapon)
			{
				shooterManager.lWeapon.SetActiveAim(flag && aimConditions);
				shooterManager.lWeapon.SetActiveScope(flag && isUsingScopeView);
			}
		}

		public virtual void ShotInput()
		{
			if (!shooterManager || CurrentActiveWeapon == null || cc.isDead)
			{
				if ((bool)shooterManager && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
				{
					CurrentActiveWeapon.powerCharge = 0f;
				}
				if ((bool)shooterManager && shooterManager.CurrentWeapon.secundaryWeapon != null && shooterManager.CurrentWeapon.secundaryWeapon.chargeWeapon && shooterManager.CurrentWeapon.secundaryWeapon.powerCharge != 0f)
				{
					shooterManager.CurrentWeapon.secundaryWeapon.powerCharge = 0f;
				}
				shootCountA = 0;
				shootCountB = 0;
				return;
			}
			if (((isAiming && !shooterManager.hipfireShot) || shooterManager.hipfireShot) && !shooterManager.isShooting && aimConditions && !isReloading && !base.isAttacking)
			{
				if ((bool)CurrentActiveWeapon || ((bool)shooterManager.CurrentWeapon && shooterManager.hipfireShot))
				{
					HandleShotCount(shooterManager.CurrentWeapon, false, shotInput.GetButton());
				}
				if (((bool)CurrentActiveWeapon || ((bool)shooterManager.CurrentWeapon && shooterManager.hipfireShot)) && (bool)shooterManager.CurrentWeapon.secundaryWeapon)
				{
					HandleShotCount(shooterManager.CurrentWeapon.secundaryWeapon, true, secundaryShotInput.GetButton());
				}
			}
			else if (!isAiming)
			{
				if (shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
				{
					CurrentActiveWeapon.powerCharge = 0f;
				}
				if (shooterManager.CurrentWeapon.secundaryWeapon != null && shooterManager.CurrentWeapon.secundaryWeapon.chargeWeapon && shooterManager.CurrentWeapon.secundaryWeapon.powerCharge != 0f)
				{
					shooterManager.CurrentWeapon.secundaryWeapon.powerCharge = 0f;
				}
				shootCountA = 0;
				shootCountB = 0;
			}
			shooterManager.UpdateShotTime();
		}

		public virtual void HandleShotCount(vShooterWeapon weapon, bool secundaryShot = false, bool weaponInput = true)
		{
			if (weapon.chargeWeapon)
			{
				if (shooterManager.WeaponHasAmmo(secundaryShot) && weapon.powerCharge < 1f && weaponInput)
				{
					if (shooterManager.hipfireShot)
					{
						aimTimming = shooterManager.HipfireAimTime;
					}
					weapon.powerCharge += Time.deltaTime * weapon.chargeSpeed;
				}
				else if ((weapon.powerCharge >= 1f && weapon.autoShotOnFinishCharge && weaponInput) || (!weaponInput && (isAiming || (shooterManager.hipfireShot && aimTimming > 0f)) && weapon.powerCharge > 0f))
				{
					if (shooterManager.hipfireShot)
					{
						aimTimming = shooterManager.HipfireAimTime;
					}
					if (secundaryShot)
					{
						shootCountB++;
					}
					else
					{
						shootCountA++;
					}
					weapon.powerCharge = 0f;
				}
				base.animator.SetFloat("PowerCharger", weapon.powerCharge);
			}
			else if (weapon.automaticWeapon && weaponInput)
			{
				if (shooterManager.hipfireShot)
				{
					aimTimming = shooterManager.HipfireAimTime;
				}
				if (secundaryShot)
				{
					shootCountB++;
				}
				else
				{
					shootCountA++;
				}
			}
			else if (weaponInput)
			{
				if (!allowAttack)
				{
					if (shooterManager.hipfireShot)
					{
						aimTimming = shooterManager.HipfireAimTime;
					}
					if (secundaryShot)
					{
						shootCountB++;
					}
					else
					{
						shootCountA++;
					}
					allowAttack = true;
				}
			}
			else
			{
				allowAttack = false;
			}
		}

		public virtual void DoShots()
		{
			if (shootCountA > 0 && CanDoShots())
			{
				shootCountA--;
				shooterManager.Shoot(aimPosition, !isAiming);
			}
			if (shootCountB > 0 && CanDoShots())
			{
				shootCountB--;
				shooterManager.Shoot(aimPosition, !isAiming, true);
			}
		}

		public virtual void ShotPrimary(bool inputValue = true)
		{
			if (((isAiming && !shooterManager.hipfireShot) || shooterManager.hipfireShot) && !shooterManager.isShooting && aimConditions && !isReloading && !base.isAttacking && (bool)CurrentActiveWeapon)
			{
				HandleShotCount(CurrentActiveWeapon, false, inputValue);
			}
		}

		public virtual void ShotSecundary(bool inputValue = true)
		{
			if (((isAiming && !shooterManager.hipfireShot) || shooterManager.hipfireShot) && !shooterManager.isShooting && aimConditions && !isReloading && !base.isAttacking && (bool)CurrentActiveWeapon && (bool)CurrentActiveWeapon.secundaryWeapon)
			{
				HandleShotCount(CurrentActiveWeapon.secundaryWeapon, true, inputValue);
			}
		}

		public virtual void ReloadInput()
		{
			if ((bool)shooterManager && !(CurrentActiveWeapon == null) && reloadInput.GetButtonDown() && !isReloading && !cc.actions && !cc.ragdolled)
			{
				aimTimming = 0f;
				shooterManager.ReloadWeapon();
			}
		}

		public virtual void SwitchCameraSideInput()
		{
			if (!(tpCamera == null) && switchCameraSideInput.GetButtonDown())
			{
				SwitchCameraSide();
			}
		}

		public virtual void SwitchCameraSide()
		{
			if (!(tpCamera == null))
			{
				isCameraRightSwitched = !isCameraRightSwitched;
				tpCamera.SwitchRight(isCameraRightSwitched);
			}
		}

		public void CancelAiming()
		{
			isAiming = false;
			aimTimming = 0f;
			if ((bool)controlAimCanvas)
			{
				controlAimCanvas.SetActiveAim(false);
				controlAimCanvas.SetActiveScopeCamera(false);
			}
			if (cc.isStrafing)
			{
				cc.Strafe();
			}
		}

		public virtual void ScopeViewInput()
		{
			if (!shooterManager || CurrentActiveWeapon == null)
			{
				return;
			}
			if (isAiming && aimConditions && (scopeViewInput.GetButtonDown() || CurrentActiveWeapon.onlyUseScopeUIView))
			{
				if ((bool)controlAimCanvas && (bool)CurrentActiveWeapon.scopeTarget)
				{
					if (!isUsingScopeView && CurrentActiveWeapon.onlyUseScopeUIView)
					{
						EnableScorpeView();
					}
					else if (isUsingScopeView && !CurrentActiveWeapon.onlyUseScopeUIView)
					{
						DisableScopeView();
					}
					else if (!isUsingScopeView)
					{
						EnableScorpeView();
					}
				}
			}
			else if (isUsingScopeView && (((bool)controlAimCanvas && !isAiming) || ((bool)controlAimCanvas && !aimConditions) || cc.isRolling || cc.continueRoll))
			{
				DisableScopeView();
			}
		}

		public virtual void EnableScorpeView()
		{
			if (isAiming)
			{
				isUsingScopeView = true;
				controlAimCanvas.SetActiveScopeCamera(true, CurrentActiveWeapon.useUI);
			}
		}

		public virtual void DisableScopeView()
		{
			isUsingScopeView = false;
			controlAimCanvas.SetActiveScopeCamera(false);
		}

		public override void BlockingInput()
		{
			if (shooterManager == null || CurrentActiveWeapon == null)
			{
				base.BlockingInput();
			}
		}

		public override void RotateWithCamera(Transform cameraTransform)
		{
			if (!cc.isStrafing || cc.actions || cc.lockMovement || !rotateToCameraFwdWhenMoving)
			{
				return;
			}
			if (tpCamera != null && (bool)tpCamera.lockTarget)
			{
				if (!cc.continueRoll)
				{
					cc.RotateToTarget(tpCamera.lockTarget);
				}
			}
			else if (cc.input != Vector2.zero || isAiming || aimTimming > 0f || rotateToCameraFwdWhenStanding)
			{
				cc.RotateWithAnotherTransform(cameraTransform);
			}
		}

		protected override void UpdateMeleeAnimations()
		{
			if ((bool)base.animator)
			{
				if ((shooterManager == null || !CurrentActiveWeapon) && (bool)meleeManager)
				{
					base.UpdateMeleeAnimations();
					base.animator.SetFloat("UpperBody_ID", 0f, 0.2f, Time.deltaTime);
					onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0f, 6f * Time.deltaTime);
					base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
					base.animator.SetBool("IsAiming", false);
					isReloading = false;
				}
				else if ((bool)shooterManager && (bool)CurrentActiveWeapon)
				{
					UpdateShooterAnimations();
				}
				else
				{
					base.animator.SetFloat("MoveSet_ID", 0f, 0.1f, Time.deltaTime);
					base.animator.SetFloat("UpperBody_ID", 0f, 0.2f, Time.deltaTime);
					base.animator.SetBool("CanAim", false);
					base.animator.SetBool("IsAiming", false);
					onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0f, 6f * Time.deltaTime);
					base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
				}
			}
		}

		protected virtual void UpdateShooterAnimations()
		{
			if (!(shooterManager == null))
			{
				if (!isAiming && aimTimming <= 0f && (bool)meleeManager)
				{
					base.animator.SetInteger("AttackID", meleeManager.GetAttackID());
				}
				else
				{
					base.animator.SetFloat("Shot_ID", shooterManager.GetShotID());
				}
				onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, CurrentActiveWeapon ? 1f : 0f, 6f * Time.deltaTime);
				base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
				if ((CurrentActiveWeapon != null && !shooterManager.useDefaultMovesetWhenNotAiming) || isAiming || aimTimming > 0f)
				{
					base.animator.SetFloat("MoveSet_ID", shooterManager.GetMoveSetID(), 0.1f, Time.deltaTime);
				}
				else if (shooterManager.useDefaultMovesetWhenNotAiming)
				{
					base.animator.SetFloat("MoveSet_ID", 0f, 0.1f, Time.deltaTime);
				}
				base.animator.SetBool("IsBlocking", false);
				base.animator.SetFloat("UpperBody_ID", shooterManager.GetUpperBodyID(), 0.2f, Time.deltaTime);
				base.animator.SetBool("CanAim", aimConditions);
				base.animator.SetBool("IsAiming", (isAiming || aimTimming > 0f) && !base.isAttacking);
				isReloading = cc.IsAnimatorTag("IsReloading") || shooterManager.isReloadingWeapon;
				isEquipping = cc.IsAnimatorTag("IsEquipping");
			}
		}

		protected override void UpdateCameraStates()
		{
			if (tpCamera == null)
			{
				tpCamera = Object.FindObjectOfType<vThirdPersonCamera>();
				if (tpCamera == null)
				{
					return;
				}
				if ((bool)tpCamera)
				{
					tpCamera.SetMainTarget(base.transform);
					tpCamera.Init();
				}
			}
			if (changeCameraState)
			{
				tpCamera.ChangeState(customCameraState, customlookAtPoint, true);
			}
			else if (cc.isCrouching)
			{
				tpCamera.ChangeState("Crouch", true);
			}
			else if (cc.isStrafing && !isAiming)
			{
				tpCamera.ChangeState("Strafing", true);
			}
			else if (isAiming && (bool)CurrentActiveWeapon)
			{
				if (string.IsNullOrEmpty(CurrentActiveWeapon.customAimCameraState))
				{
					tpCamera.ChangeState("Aiming", true);
				}
				else
				{
					tpCamera.ChangeState(CurrentActiveWeapon.customAimCameraState, true);
				}
			}
			else
			{
				tpCamera.ChangeState("Default", true);
			}
		}

		protected virtual void UpdateAimPosition()
		{
			if (!shooterManager || CurrentActiveWeapon == null)
			{
				return;
			}
			Transform transform = ((!isUsingScopeView || !controlAimCanvas || !controlAimCanvas.scopeCamera) ? cameraMain.transform : (CurrentActiveWeapon.zoomScopeCamera ? CurrentActiveWeapon.zoomScopeCamera.transform : controlAimCanvas.scopeCamera.transform));
			Vector3 position = transform.position;
			if (!controlAimCanvas || !controlAimCanvas.isScopeCameraActive || !controlAimCanvas.scopeCamera)
			{
				position = transform.position;
			}
			Vector3 origin = position;
			origin += (((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive && (bool)controlAimCanvas.scopeCamera) ? transform.forward : Vector3.zero);
			aimPosition = transform.position + transform.forward * 100f;
			if (!isUsingScopeView)
			{
				lastAimDistance = 100f;
			}
			if (shooterManager.raycastAimTarget && CurrentActiveWeapon.raycastAimTarget)
			{
				Ray ray = new Ray(origin, transform.forward);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, cameraMain.farClipPlane, shooterManager.damageLayer))
				{
					if (hitInfo.collider.transform.IsChildOf(base.transform))
					{
						Collider collider = hitInfo.collider;
						RaycastHit[] array = Physics.RaycastAll(ray, cameraMain.farClipPlane, shooterManager.damageLayer);
						float num = cameraMain.farClipPlane;
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i].distance < num && array[i].collider.gameObject != collider.gameObject && !array[i].collider.transform.IsChildOf(base.transform))
							{
								num = array[i].distance;
								hitInfo = array[i];
							}
						}
					}
					if ((bool)hitInfo.collider)
					{
						if (!isUsingScopeView)
						{
							lastAimDistance = Vector3.Distance(transform.position, hitInfo.point);
						}
						aimPosition = hitInfo.point;
					}
				}
				if (shooterManager.showCheckAimGizmos)
				{
					Debug.DrawLine(ray.origin, aimPosition);
				}
			}
			if (isAiming)
			{
				shooterManager.CameraSway();
			}
		}

		private void OnDrawGizmos()
		{
			if ((bool)shooterManager && shooterManager.showCheckAimGizmos)
			{
				int num = ((!isCameraRightSwitched) ? 1 : (-1));
				Ray ray = new Ray(aimAngleReference.transform.position + base.transform.up * shooterManager.blockAimOffsetY + base.transform.right * shooterManager.blockAimOffsetX * num, cameraMain.transform.forward);
				Gizmos.DrawRay(ray.origin, ray.direction * shooterManager.minDistanceToAim);
				Color color = Gizmos.color;
				color = (aimConditions ? Color.green : Color.red);
				color.a = 1f;
				Gizmos.color = color;
				Gizmos.DrawSphere(ray.GetPoint(shooterManager.minDistanceToAim), shooterManager.checkAimRadius);
				Gizmos.DrawSphere(aimPosition, shooterManager.checkAimRadius);
			}
		}

		protected virtual void UpdateAimBehaviour()
		{
			UpdateAimPosition();
			UpdateHeadTrack();
			if ((bool)shooterManager && (bool)CurrentActiveWeapon)
			{
				RotateAimArm(shooterManager.IsLeftWeapon);
				RotateAimHand(shooterManager.IsLeftWeapon);
				UpdateArmsIK(shooterManager.IsLeftWeapon);
			}
			if (isUsingScopeView && (bool)controlAimCanvas && (bool)controlAimCanvas.scopeCamera)
			{
				UpdateAimPosition();
			}
			CheckAimConditions();
			UpdateAimHud();
			DoShots();
		}

		protected virtual void UpdateArmsIK(bool isUsingLeftHand = false)
		{
			if (!shooterManager || !CurrentActiveWeapon || !shooterManager.useLeftIK)
			{
				return;
			}
			if (base.animator.GetCurrentAnimatorStateInfo(6).IsName("Shot Fire") && CurrentActiveWeapon.disableIkOnShot)
			{
				lIKWeight = 0f;
				return;
			}
			bool flag = false;
			float magnitude = cc.input.magnitude;
			if (!isAiming && !base.isAttacking)
			{
				flag = ((magnitude < 1f) ? CurrentActiveWeapon.useIkOnIdle : ((!cc.isStrafing) ? CurrentActiveWeapon.useIkOnFree : CurrentActiveWeapon.useIkOnStrafe));
			}
			else if (isAiming && !base.isAttacking)
			{
				flag = CurrentActiveWeapon.useIKOnAiming;
			}
			else if (base.isAttacking)
			{
				flag = CurrentActiveWeapon.useIkAttacking;
			}
			if (leftIK == null)
			{
				leftIK = new vIKSolver(base.animator, AvatarIKGoal.LeftHand);
			}
			if (rightIK == null)
			{
				rightIK = new vIKSolver(base.animator, AvatarIKGoal.RightHand);
			}
			vIKSolver vIKSolver = null;
			vIKSolver = ((!isUsingLeftHand) ? leftIK : rightIK);
			if (vIKSolver == null)
			{
				return;
			}
			if (isUsingLeftHand)
			{
				ikRotationOffset = shooterManager.ikRotationOffsetR;
				ikPositionOffset = shooterManager.ikPositionOffsetR;
			}
			else
			{
				ikRotationOffset = shooterManager.ikRotationOffsetL;
				ikPositionOffset = shooterManager.ikPositionOffsetL;
			}
			if ((bool)CurrentActiveWeapon && (bool)CurrentActiveWeapon.handIKTarget && Time.timeScale > 0f && !isReloading && !cc.actions && !cc.customAction && (!base.animator.IsInTransition(4) || isAiming) && !isEquipping && (cc.isGrounded || isAiming || aimTimming > 0f) && !cc.lockMovement && !cc.continueRoll && flag)
			{
				lIKWeight = Mathf.Lerp(lIKWeight, 1f, 10f * Time.deltaTime);
			}
			else
			{
				lIKWeight = Mathf.Lerp(lIKWeight, 0f, 25f * Time.deltaTime);
			}
			if (!(lIKWeight <= 0f))
			{
				vIKSolver.SetIKWeight(lIKWeight);
				if ((bool)shooterManager && (bool)CurrentActiveWeapon && (bool)CurrentActiveWeapon.handIKTarget)
				{
					Vector3 vector = CurrentActiveWeapon.handIKTarget.forward * ikPositionOffset.z + CurrentActiveWeapon.handIKTarget.right * ikPositionOffset.x + CurrentActiveWeapon.handIKTarget.up * ikPositionOffset.y;
					vIKSolver.SetIKPosition(CurrentActiveWeapon.handIKTarget.position + vector);
					Quaternion quaternion = Quaternion.Euler(ikRotationOffset);
					vIKSolver.SetIKRotation(CurrentActiveWeapon.handIKTarget.rotation * quaternion);
				}
			}
		}

		protected virtual bool CanRotateAimArm()
		{
			return cc.IsAnimatorTag("Upperbody Pose");
		}

		protected virtual bool CanDoShots()
		{
			return armAlignmentWeight >= 0.5f;
		}

		protected virtual void RotateAimArm(bool isUsingLeftHand = false)
		{
			if (!shooterManager)
			{
				return;
			}
			armAlignmentWeight = (((isAiming || aimTimming > 0f) && aimConditions && CanRotateAimArm()) ? Mathf.Lerp(armAlignmentWeight, 1f, shooterManager.smoothArmAlignWeight * Time.deltaTime) : 0f);
			if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightUpperArmToAim)
			{
				Vector3 direction = targetArmAlignmentPosition - CurrentActiveWeapon.aimReference.position;
				Vector3 forward = CurrentActiveWeapon.aimReference.forward;
				Transform transform = (isUsingLeftHand ? leftUpperArm : rightUpperArm);
				Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(direction));
				if (!shooterManager.isShooting && !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
				{
					upperArmRotationAlignment = quaternion;
				}
				float num = Vector3.Angle(aimPosition - aimAngleReference.transform.position, aimAngleReference.transform.forward);
				if ((!(num > shooterManager.maxAimAngle) && !(num < 0f - shooterManager.maxAimAngle)) || ((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive))
				{
					upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, shooterManager.smoothArmIKRotation * (0.001f + Time.deltaTime));
				}
				else
				{
					upperArmRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
				{
					transform.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight * Mathf.Clamp(cc.upperBodyInfo.normalizedTime, 0f, 1f));
				}
			}
			else
			{
				upperArmRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}

		protected virtual void RotateAimHand(bool isUsingLeftHand = false)
		{
			if (!shooterManager)
			{
				return;
			}
			if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && aimConditions && CurrentActiveWeapon.alignRightHandToAim)
			{
				Vector3 vector = targetArmAlignmentPosition;
				Vector3 direction = vector - CurrentActiveWeapon.aimReference.position;
				Vector3 forward = CurrentActiveWeapon.aimReference.forward;
				Transform transform = (isUsingLeftHand ? leftHand : rightHand);
				Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(direction));
				if (!shooterManager.isShooting && !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
				{
					handRotationAlignment = quaternion;
				}
				float num = Vector3.Angle(aimPosition - aimAngleReference.transform.position, aimAngleReference.transform.forward);
				if ((!(num > shooterManager.maxAimAngle) && !(num < 0f - shooterManager.maxAimAngle)) || ((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive))
				{
					handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, shooterManager.smoothArmIKRotation * (0.001f + Time.deltaTime));
				}
				else
				{
					handRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
				{
					transform.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight * Mathf.Clamp(cc.upperBodyInfo.normalizedTime, 0f, 1f));
				}
				CurrentActiveWeapon.SetScopeLookTarget(vector);
			}
			else
			{
				handRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}

		protected virtual void CheckAimConditions()
		{
			if (!shooterManager)
			{
				return;
			}
			int num = ((!isCameraRightSwitched) ? 1 : (-1));
			if (CurrentActiveWeapon == null)
			{
				aimConditions = false;
				return;
			}
			RaycastHit hitInfo;
			if (!shooterManager.hipfireShot && !IsAimAlignWithForward())
			{
				aimConditions = false;
			}
			else if (Physics.SphereCast(new Ray(aimAngleReference.transform.position + base.transform.up * shooterManager.blockAimOffsetY + base.transform.right * shooterManager.blockAimOffsetX * num, cameraMain.transform.forward), shooterManager.checkAimRadius, out hitInfo, shooterManager.minDistanceToAim, shooterManager.blockAimLayer))
			{
				aimConditions = false;
			}
			else
			{
				aimConditions = true;
			}
			aimWeight = Mathf.Lerp(aimWeight, aimConditions ? 1 : 0, 10f * Time.deltaTime);
		}

		protected virtual bool IsAimAlignWithForward()
		{
			if (!shooterManager)
			{
				return false;
			}
			Vector3 eulerAngle = Quaternion.LookRotation(aimPosition - aimAngleReference.transform.position, Vector3.up).eulerAngles - base.transform.eulerAngles;
			if (eulerAngle.NormalizeAngle().y < 90f)
			{
				return eulerAngle.NormalizeAngle().y > -90f;
			}
			return false;
		}

		protected virtual void UpdateHeadTrack()
		{
			if (!shooterManager || !headTrack)
			{
				if ((bool)headTrack)
				{
					headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, headTrack.smooth * Time.deltaTime);
				}
			}
			else if (!CurrentActiveWeapon || !headTrack)
			{
				if ((bool)headTrack)
				{
					headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, headTrack.smooth * Time.deltaTime);
				}
			}
			else if (isAiming || aimTimming > 0f)
			{
				Vector2 b = (cc.isCrouching ? CurrentActiveWeapon.spineOffsetCrouch : CurrentActiveWeapon.spineOffset);
				headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, b, headTrack.smooth * Time.deltaTime);
			}
			else
			{
				headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, headTrack.smooth * Time.deltaTime);
			}
		}

		protected virtual void UpdateAimHud()
		{
			if (!shooterManager || !controlAimCanvas || CurrentActiveWeapon == null)
			{
				return;
			}
			controlAimCanvas.SetAimCanvasID(CurrentActiveWeapon.scopeID);
			if ((bool)controlAimCanvas.scopeCamera && controlAimCanvas.scopeCamera.gameObject.activeSelf)
			{
				controlAimCanvas.SetAimToCenter();
			}
			else if (isAiming)
			{
				RaycastHit hitInfo;
				if (Physics.Linecast(CurrentActiveWeapon.muzzle.position, aimPosition, out hitInfo, shooterManager.blockAimLayer))
				{
					controlAimCanvas.SetWordPosition(hitInfo.point, aimConditions);
				}
				else
				{
					controlAimCanvas.SetWordPosition(aimPosition, aimConditions);
				}
			}
			else
			{
				controlAimCanvas.SetAimToCenter();
			}
			if ((bool)CurrentActiveWeapon.scopeTarget)
			{
				Vector3 lookPosition = cameraMain.transform.position + cameraMain.transform.forward * (isUsingScopeView ? lastAimDistance : 100f);
				controlAimCanvas.UpdateScopeCamera(CurrentActiveWeapon.scopeTarget.position, lookPosition, CurrentActiveWeapon.zoomScopeCamera ? 0f : CurrentActiveWeapon.scopeZoom);
			}
		}
	}
}
