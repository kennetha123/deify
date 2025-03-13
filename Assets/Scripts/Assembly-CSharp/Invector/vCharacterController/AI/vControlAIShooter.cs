using System.Collections;
using Invector.IK;
using Invector.vShooter;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI
{
	[vClassHeader(" AI SHOOTER CONTROLLER", true, "icon_v2", false, "", iconName = "AI-icon")]
	public class vControlAIShooter : vControlAICombat, vIControlAIShooter, vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver
	{
		[vEditorToolbar("Shooter Settings", false, "", false, false, order = 10)]
		[Header("Shooter Settings")]
		public float minTimeShooting = 2f;

		public float maxTimeShooting = 5f;

		public float minShotWaiting = 3f;

		public float maxShotWaiting = 6f;

		public float aimTargetHeight = 0.35f;

		public bool doReloadWhileWaiting = true;

		public float aimSmoothDamp = 10f;

		public float smoothArmAlignmentWeight = 4f;

		public float aimTurnAngle = 60f;

		public float maxAngleToShot = 60f;

		protected float _timeShotting;

		protected float _waitingToShot;

		protected float _upperBodyID;

		protected float _shotID;

		protected Vector3 aimPosition;

		protected Quaternion handRotationAlignment;

		protected Quaternion upperArmRotationAlignment;

		protected float armAlignmentWeight;

		private Transform leftUpperArm;

		private Transform rightUpperArm;

		private Transform leftHand;

		private Transform rightHand;

		private GameObject aimAngleReference;

		private Quaternion upperArmRotation;

		private Quaternion handRotation;

		private float rightRotationWeight;

		private float _onlyArmsLayerWeight;

		private float handIKWeight;

		private float weaponIKWeight;

		private float aimTime;

		private float delayEnableAimAfterRagdolled;

		private int onlyArmsLayer;

		private int _moveSetID;

		private int _attackID;

		private bool aimEnable;

		[vEditorToolbar("Debug", false, "", false, true, order = 100)]
		[SerializeField]
		[vReadOnly(false)]
		protected bool _canAiming;

		[SerializeField]
		[vReadOnly(false)]
		protected bool _canShot;

		[SerializeField]
		[vReadOnly(false)]
		protected bool _waitingReload;

		[SerializeField]
		[vReadOnly(false)]
		protected int shots;

		[SerializeField]
		[vReadOnly(false)]
		protected int secundaryShots;

		public bool debugAim;

		public bool lockAimDebug;

		[SerializeField]
		[vHideInInspector("lockAimDebug", false)]
		private Transform aimDebugTarget;

		[SerializeField]
		[vHideInInspector("lockAimDebug", false)]
		private bool debugShoots;

		private Vector3 aimVelocity;

		private Vector3 aimTarget;

		private Vector3 _lastaValidAimLocal;

		protected bool forceCanShot;

		private UnityEvent onShot;

		private UnityEvent onSecundayShot;

		public Vector3 _debugAimPosition;

		public bool IsReloading { get; protected set; }

		public bool IsEquipping { get; protected set; }

		public bool IsInShotAngle { get; protected set; }

		public vAIShooterManager shooterManager { get; set; }

		public vIKSolver leftIK { get; set; }

		public vIKSolver rightIK { get; set; }

		protected int MoveSetID
		{
			get
			{
				return _moveSetID;
			}
			set
			{
				if (value != _moveSetID || base.animator.GetFloat("MoveSet_ID") != (float)value)
				{
					_moveSetID = value;
					base.animator.SetFloat("MoveSet_ID", _moveSetID, 0.25f, Time.deltaTime);
				}
			}
		}

		protected int AttackID
		{
			get
			{
				return _attackID;
			}
			set
			{
				if (value != _attackID)
				{
					_attackID = value;
					base.animator.SetInteger("AttackID", _attackID);
				}
			}
		}

		public Vector3 defaultValidAimLocal
		{
			get
			{
				return Vector3.forward * 10f + Vector3.up * (_capsuleCollider.height * 0.5f + aimTargetHeight);
			}
		}

		protected float UpperBodyID
		{
			get
			{
				return _upperBodyID;
			}
			set
			{
				if (_upperBodyID != value || base.animator.GetFloat("UpperBody_ID") != value)
				{
					_upperBodyID = value;
					base.animator.SetFloat("UpperBody_ID", _upperBodyID);
				}
			}
		}

		protected float ShotID
		{
			get
			{
				return _shotID;
			}
			set
			{
				if (_shotID != value || base.animator.GetFloat("Shot_ID") != value)
				{
					_shotID = value;
					base.animator.SetFloat("Shot_ID", _shotID);
				}
			}
		}

		private Vector3 DebugAimPosition
		{
			get
			{
				if ((bool)aimDebugTarget)
				{
					return aimDebugTarget.position;
				}
				return base.transform.position + base.transform.forward * (2f + _debugAimPosition.z) + base.transform.right * _debugAimPosition.x + base.transform.up * (1.5f + _debugAimPosition.y);
			}
		}

		public virtual vShooterWeapon CurrentActiveWeapon
		{
			get
			{
				if (!shooterManager.currentWeapon || !shooterManager.currentWeapon.gameObject.activeInHierarchy)
				{
					return null;
				}
				return shooterManager.currentWeapon;
			}
		}

		Transform vIDamageReceiver.transform
		{
			get
			{
				return base.transform;
			}
		}

		GameObject vIDamageReceiver.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		public override void CreateSecondaryComponents()
		{
			base.CreateSecondaryComponents();
			if (GetComponent<vAIShooterManager>() == null)
			{
				base.gameObject.AddComponent<vAIShooterManager>();
			}
			if (GetComponent<vAIHeadtrack>() == null)
			{
				base.gameObject.AddComponent<vAIHeadtrack>();
			}
		}

		public void CheckCanShot()
		{
			if ((isAiming && _waitingToShot < Time.time && !inTurn && (base.isStrafing || debugShoots || input.magnitude < 0.1f)) || forceCanShot)
			{
				_timeShotting = Random.Range(minTimeShooting, maxTimeShooting) + Time.time;
			}
			_canShot = _timeShotting > Time.time;
			if (_canShot)
			{
				_waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
			}
		}

		protected override void Start()
		{
			base.Start();
			_lastaValidAimLocal = defaultValidAimLocal;
			_waitingReload = false;
			InitShooter();
		}

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();
			if (lockAimDebug)
			{
				Gizmos.DrawSphere(DebugAimPosition, 0.1f);
			}
			if (debugAim && (bool)currentTarget.transform)
			{
				Gizmos.DrawSphere(aimPosition, 0.1f);
				Gizmos.DrawWireCube(currentTarget.collider.bounds.center, currentTarget.collider.bounds.size);
			}
		}

		public virtual void SetShooterHitLayer(LayerMask mask)
		{
			if ((bool)shooterManager)
			{
				shooterManager.SetDamageLayer(mask);
			}
		}

		public override void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false)
		{
			if (base.ragdolled)
			{
				return;
			}
			if ((bool)shooterManager && attackID != -1)
			{
				AttackID = attackID;
			}
			else
			{
				AttackID = shooterManager.GetAttackID();
			}
			if (!currentTarget.transform && (!debugShoots || !lockAimDebug))
			{
				return;
			}
			forceCanShot = forceCanAttack;
			if (!_canShot)
			{
				return;
			}
			if (!strongAttack)
			{
				secundaryShots = 0;
				if (shots == 0)
				{
					shots++;
				}
			}
			else
			{
				shots = 0;
				if (secundaryShots == 0)
				{
					secundaryShots++;
				}
			}
		}

		public override void InitAttackTime()
		{
			base.InitAttackTime();
			_waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
			_waitingReload = false;
		}

		public override void ResetAttackTime()
		{
			base.ResetAttackTime();
			_waitingToShot = Time.time + Random.Range(minShotWaiting, maxShotWaiting);
		}

		protected virtual void InitShooter()
		{
			if ((bool)_headtrack)
			{
				_headtrack.onPreUpdateSpineIK.AddListener(HandleAim);
				_headtrack.onPosUpdateSpineIK.AddListener(IKBehaviour);
			}
			shooterManager = GetComponent<vAIShooterManager>();
			leftHand = base.animator.GetBoneTransform(HumanBodyBones.LeftHand);
			rightHand = base.animator.GetBoneTransform(HumanBodyBones.RightHand);
			leftUpperArm = base.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			rightUpperArm = base.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			onlyArmsLayer = base.animator.GetLayerIndex("OnlyArms");
			aimAngleReference = new GameObject("aimAngleReference");
			aimAngleReference.transform.rotation = base.transform.rotation;
			Transform boneTransform = base.animator.GetBoneTransform(HumanBodyBones.Head);
			aimAngleReference.transform.SetParent(boneTransform);
			aimAngleReference.transform.localPosition = Vector3.zero;
			aimPosition = DebugAimPosition;
		}

		protected virtual void HandleAim()
		{
			if (base.ragdolled)
			{
				aimTime = 0f;
				isAiming = false;
				delayEnableAimAfterRagdolled = 2f;
			}
			else if (delayEnableAimAfterRagdolled <= 0f)
			{
				ControlAimTime();
				if (isAiming)
				{
					_headtrack.LookAtPoint(AimPositionClamped(), 1f, 0f);
				}
			}
			else
			{
				aimTime = 0f;
				isAiming = false;
				delayEnableAimAfterRagdolled -= Time.deltaTime;
			}
		}

		protected virtual void IKBehaviour()
		{
			if (lockAimDebug)
			{
				if (!base.IsStrafingAnim)
				{
					base.isStrafing = true;
					base.IsStrafingAnim = true;
				}
				AimTo(DebugAimPosition, 0.5f);
			}
			UpdateAimBehaviour();
			if (lockAimDebug && debugShoots)
			{
				Attack();
			}
		}

		protected override void UpdateAnimator()
		{
			base.UpdateAnimator();
			UpdateCombatAnimator();
		}

		protected override void UpdateCombatAnimator()
		{
			base.UpdateCombatAnimator();
			UpdateShooterAnimator();
		}

		protected virtual void UpdateShooterAnimator()
		{
			if ((bool)shooterManager.currentWeapon)
			{
				IsReloading = IsAnimatorTag("IsReloading");
				IsEquipping = IsAnimatorTag("IsEquipping");
				bool flag = isAiming && !IsReloading;
				if (flag && !aimEnable)
				{
					shooterManager.currentWeapon.onEnableAim.Invoke();
					aimEnable = true;
				}
				else if (!flag && aimEnable)
				{
					shooterManager.currentWeapon.onDisableAim.Invoke();
					aimEnable = false;
				}
				base.animator.SetBool("CanAim", flag && _canAiming);
				ShotID = shooterManager.GetShotID();
				UpperBodyID = shooterManager.GetUpperBodyID();
				MoveSetID = shooterManager.GetMoveSetID();
				base.animator.SetBool("IsAiming", flag);
			}
			else
			{
				IsReloading = false;
				base.animator.SetBool("IsAiming", false);
				base.animator.SetBool("CanAim", false);
				if (aimEnable)
				{
					shooterManager.currentWeapon.onDisableAim.Invoke();
					aimEnable = false;
				}
			}
			_onlyArmsLayerWeight = Mathf.Lerp(_onlyArmsLayerWeight, (isAiming || base.isRolling) ? 0f : (((bool)shooterManager && (bool)shooterManager.currentWeapon) ? 1f : 0f), 6f * Time.deltaTime);
			base.animator.SetLayerWeight(onlyArmsLayer, _onlyArmsLayerWeight);
		}

		protected virtual void UpdateAimBehaviour()
		{
			UpdateHeadTrack();
			CheckCanAiming();
			CheckCanShot();
			HandleShots();
			UpdateValidAim();
			ValidateShotAngle();
		}

		protected virtual void HandleShots()
		{
			if (IsAnimatorTag("IgnoreIK"))
			{
				return;
			}
			if ((bool)shooterManager && (bool)shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeSelf)
			{
				UpdateIKAdjust(false);
				RotateAimArm();
				RotateAimHand();
				if (!shooterManager.lWeapon || !shooterManager.lWeapon.gameObject.activeSelf)
				{
					UpdateSupportHandIK();
				}
			}
			if ((bool)shooterManager && (bool)shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeSelf)
			{
				UpdateIKAdjust(true);
				RotateAimArm(true);
				RotateAimHand(true);
				if (!shooterManager.rWeapon || !shooterManager.rWeapon.gameObject.activeSelf)
				{
					UpdateSupportHandIK(true);
				}
			}
			UpdateShotTime();
			if (shots > 0 || secundaryShots > 0)
			{
				Shot();
			}
		}

		protected virtual void UpdateIKAdjust(bool isUsingLeftHand)
		{
			vWeaponIKAdjust currentWeaponIK = shooterManager.CurrentWeaponIK;
			if (!currentWeaponIK || IsAnimatorTag("IgnoreIK"))
			{
				weaponIKWeight = 0f;
				return;
			}
			weaponIKWeight = Mathf.Lerp(weaponIKWeight, (!IsReloading && !IsEquipping) ? 1 : 0, 25f * Time.deltaTime);
			if (!(weaponIKWeight <= 0f))
			{
				if (leftIK == null)
				{
					leftIK = new vIKSolver(base.animator, AvatarIKGoal.LeftHand);
					leftIK.UpdateIK();
				}
				if (rightIK == null)
				{
					rightIK = new vIKSolver(base.animator, AvatarIKGoal.RightHand);
					rightIK.UpdateIK();
				}
				if (isUsingLeftHand)
				{
					ApplyOffsets(currentWeaponIK, leftIK, rightIK);
				}
				else
				{
					ApplyOffsets(currentWeaponIK, rightIK, leftIK);
				}
			}
		}

		protected virtual void ApplyOffsets(vWeaponIKAdjust weaponIKAdjust, vIKSolver weaponHand, vIKSolver supportHand)
		{
			bool flag = weaponIKAdjust != null;
			weaponHand.SetIKWeight(weaponIKWeight);
			IKAdjust iKAdjust = (flag ? weaponIKAdjust.GetIKAdjust(isAiming, base.isCrouching) : null);
			ApplyOffsetToTargetBone(flag ? iKAdjust.weaponHandOffset : null, weaponHand.endBoneOffset, flag);
			ApplyOffsetToTargetBone(flag ? iKAdjust.weaponHintOffset : null, weaponHand.middleBoneOffset, flag);
			ApplyOffsetToTargetBone(flag ? iKAdjust.supportHandOffset : null, supportHand.endBoneOffset, flag);
			ApplyOffsetToTargetBone(flag ? iKAdjust.supportHintOffset : null, supportHand.middleBoneOffset, flag);
			if (flag)
			{
				weaponHand.AnimationToIK();
			}
		}

		protected virtual void ApplyOffsetToTargetBone(IKOffsetTransform iKOffset, Transform target, bool isValid)
		{
			target.localPosition = Vector3.Lerp(target.localPosition, isValid ? iKOffset.position : Vector3.zero, 10f * Time.deltaTime);
			target.localRotation = Quaternion.Lerp(target.localRotation, isValid ? Quaternion.Euler(iKOffset.eulerAngles) : Quaternion.Euler(Vector3.zero), 10f * Time.deltaTime);
		}

		private void UpdateValidAim()
		{
			if (isAiming && _canAiming)
			{
				aimPosition = Vector3.SmoothDamp(aimPosition, aimTarget, ref aimVelocity, aimSmoothDamp * Time.deltaTime);
				_lastaValidAimLocal = base.transform.InverseTransformPoint(aimPosition);
				return;
			}
			if (!isAiming)
			{
				_lastaValidAimLocal = defaultValidAimLocal;
			}
			aimPosition = base.transform.TransformPoint(_lastaValidAimLocal);
		}

		protected virtual Vector3 AimPositionClamped()
		{
			Vector3 position = base.transform.InverseTransformPoint(aimPosition);
			if (position.z < 0.1f)
			{
				position.z = 0.1f;
			}
			return base.transform.TransformPoint(position);
		}

		protected virtual void UpdateHeadTrack()
		{
			if (!shooterManager || !_headtrack)
			{
				if ((bool)_headtrack)
				{
					_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
					_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
				}
			}
			else if (!CurrentActiveWeapon || !_headtrack || !shooterManager.CurrentWeaponIK)
			{
				if ((bool)_headtrack)
				{
					_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
					_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
				}
			}
			else if (isAiming)
			{
				Vector2 b = (base.isCrouching ? shooterManager.CurrentWeaponIK.crouchingAiming.spineOffset.spine : shooterManager.CurrentWeaponIK.standingAiming.spineOffset.spine);
				Vector2 b2 = (base.isCrouching ? shooterManager.CurrentWeaponIK.crouchingAiming.spineOffset.head : shooterManager.CurrentWeaponIK.standingAiming.spineOffset.head);
				_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, b, _headtrack.smooth * Time.deltaTime);
				_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, b2, _headtrack.smooth * Time.deltaTime);
			}
			else
			{
				Vector2 b3 = (base.isCrouching ? shooterManager.CurrentWeaponIK.crouching.spineOffset.spine : shooterManager.CurrentWeaponIK.standing.spineOffset.spine);
				Vector2 b4 = (base.isCrouching ? shooterManager.CurrentWeaponIK.crouching.spineOffset.head : shooterManager.CurrentWeaponIK.standing.spineOffset.head);
				_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, b3, _headtrack.smooth * Time.deltaTime);
				_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, b4, _headtrack.smooth * Time.deltaTime);
			}
		}

		protected virtual void ValidateShotAngle()
		{
			if ((bool)shooterManager && isAiming && _canAiming)
			{
				vShooterWeapon vShooterWeapon = (shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon);
				if ((bool)vShooterWeapon)
				{
					float num = Vector3.Angle(vShooterWeapon.aimReference.forward, (aimTarget - vShooterWeapon.aimReference.position).normalized);
					IsInShotAngle = num <= maxAngleToShot;
					if (debugAim)
					{
						Debug.DrawRay(vShooterWeapon.aimReference.position, vShooterWeapon.aimReference.forward * 100f, IsInShotAngle ? Color.green : Color.red);
						Debug.DrawRay(vShooterWeapon.aimReference.position, aimTarget - vShooterWeapon.aimReference.position, Color.yellow);
						Debug.DrawRay(vShooterWeapon.muzzle.position, vShooterWeapon.muzzle.forward * 100f, Color.blue);
					}
					return;
				}
			}
			IsInShotAngle = false;
		}

		protected virtual void UpdateShotTime()
		{
			shooterManager.UpdateShotTime();
		}

		protected virtual void ControlAimTime()
		{
			if (aimTime > 0f)
			{
				aimTime -= Time.deltaTime;
			}
			else if (isAiming)
			{
				isAiming = false;
			}
		}

		protected virtual void UpdateSupportHandIK(bool isUsingLeftHand = false)
		{
			if (base.ragdolled)
			{
				return;
			}
			vShooterWeapon vShooterWeapon = (shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon);
			if (!shooterManager || !vShooterWeapon || !vShooterWeapon.gameObject.activeInHierarchy || !shooterManager.useLeftIK)
			{
				return;
			}
			if (IsAnimatorTag("Shot") && vShooterWeapon.disableIkOnShot)
			{
				handIKWeight = 0f;
				return;
			}
			bool flag = false;
			float num = (base.isStrafing ? new Vector2(base.animator.GetFloat("InputVertical"), base.animator.GetFloat("InputHorizontal")).magnitude : base.animator.GetFloat("InputVertical"));
			if (!isAiming)
			{
				flag = ((num < 0.1f) ? vShooterWeapon.useIkOnIdle : ((!base.isStrafing) ? vShooterWeapon.useIkOnFree : vShooterWeapon.useIkOnStrafe));
			}
			else if (isAiming)
			{
				flag = vShooterWeapon.useIKOnAiming;
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
			Vector3 euler = Vector3.zero;
			Vector3 vector = Vector3.zero;
			if ((bool)shooterManager.weaponIKAdjustList)
			{
				if (isUsingLeftHand)
				{
					euler = shooterManager.weaponIKAdjustList.ikRotationOffsetR;
					vector = shooterManager.weaponIKAdjustList.ikPositionOffsetR;
				}
				else
				{
					euler = shooterManager.weaponIKAdjustList.ikRotationOffsetL;
					vector = shooterManager.weaponIKAdjustList.ikPositionOffsetL;
				}
			}
			if ((bool)vShooterWeapon && (bool)vShooterWeapon.handIKTarget && Time.timeScale > 0f && !IsReloading && !actions && !customAction && !IsEquipping && (base.isGrounded || isAiming) && !lockMovement && flag)
			{
				handIKWeight = Mathf.Lerp(handIKWeight, 1f, 10f * Time.deltaTime);
			}
			else
			{
				handIKWeight = Mathf.Lerp(handIKWeight, 0f, 10f * Time.deltaTime);
			}
			if (handIKWeight <= 0f)
			{
				return;
			}
			vIKSolver.SetIKWeight(handIKWeight);
			if ((bool)shooterManager && (bool)vShooterWeapon && (bool)vShooterWeapon.handIKTarget)
			{
				Vector3 vector2 = vShooterWeapon.handIKTarget.forward * vector.z + vShooterWeapon.handIKTarget.right * vector.x + vShooterWeapon.handIKTarget.up * vector.y;
				vIKSolver.SetIKPosition(vShooterWeapon.handIKTarget.position + vector2);
				Quaternion quaternion = Quaternion.Euler(euler);
				vIKSolver.SetIKRotation(vShooterWeapon.handIKTarget.rotation * quaternion);
				if ((bool)shooterManager.CurrentWeaponIK)
				{
					vIKSolver.AnimationToIK();
				}
			}
		}

		protected virtual void RotateAimArm(bool isUsingLeftHand = false)
		{
			if (!shooterManager)
			{
				return;
			}
			armAlignmentWeight = ((isAiming && _canAiming) ? Mathf.Lerp(armAlignmentWeight, 1f, smoothArmAlignmentWeight * Time.deltaTime) : 0f);
			if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightUpperArmToAim)
			{
				Vector3 vector = AimPositionClamped() - CurrentActiveWeapon.aimReference.position;
				Vector3 forward = CurrentActiveWeapon.aimReference.forward;
				Transform transform = (isUsingLeftHand ? leftUpperArm : rightUpperArm);
				Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(vector));
				if (!shooterManager.isShooting && !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
				{
					upperArmRotationAlignment = (inTurn ? upperArmRotation : quaternion);
				}
				float num = Vector3.Angle(AimPositionClamped() - aimAngleReference.transform.position, aimAngleReference.transform.forward);
				if (!(num > shooterManager.maxHandAngle) && !(num < 0f - shooterManager.maxHandAngle))
				{
					upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, shooterManager.smoothHandRotation * Time.deltaTime);
				}
				else
				{
					upperArmRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
				{
					transform.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);
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
			if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && _canAiming && CurrentActiveWeapon.alignRightHandToAim)
			{
				Vector3 vector = AimPositionClamped();
				Vector3 vector2 = vector - CurrentActiveWeapon.aimReference.position;
				Vector3 forward = CurrentActiveWeapon.aimReference.forward;
				Transform transform = (isUsingLeftHand ? leftHand : rightHand);
				Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(vector2));
				if (!shooterManager.isShooting && !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
				{
					handRotationAlignment = (inTurn ? handRotation : quaternion);
				}
				float num = Vector3.Angle(AimPositionClamped() - aimAngleReference.transform.position, aimAngleReference.transform.forward);
				if (!(num > shooterManager.maxHandAngle) && !(num < 0f - shooterManager.maxHandAngle))
				{
					handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, shooterManager.smoothHandRotation * Time.deltaTime);
				}
				else
				{
					handRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
				{
					transform.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);
				}
				CurrentActiveWeapon.SetScopeLookTarget(vector);
			}
			else
			{
				handRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}

		protected virtual void CheckCanAiming()
		{
			if (base.ragdolled || (!base.isStrafing && !lockAimDebug) || customAction || IsReloading)
			{
				_canAiming = false;
			}
			Vector3 vector = aimTarget;
			vector.y = base.transform.position.y;
			float num = Vector3.Angle(base.transform.forward, vector - base.transform.position);
			_canAiming = num < aimTurnAngle;
			if (!_canAiming && isAiming)
			{
				RotateTo(aimTarget - base.transform.position);
			}
		}

		public virtual void Shot()
		{
			if (base.isDead || !shooterManager || !shooterManager.currentWeapon || customAction)
			{
				return;
			}
			if (_canShot && !IsReloading && !_waitingReload && _canAiming && IsInShotAngle && isAiming && !inTurn)
			{
				if (shooterManager.weaponHasAmmo)
				{
					if (shots > 0)
					{
						shooterManager.Shoot(CurrentActiveWeapon.muzzle.position + CurrentActiveWeapon.muzzle.forward * 100f);
						shots--;
					}
					if (secundaryShots > 0)
					{
						if ((bool)CurrentActiveWeapon.secundaryWeapon)
						{
							shooterManager.Shoot(CurrentActiveWeapon.secundaryWeapon.muzzle.position + CurrentActiveWeapon.secundaryWeapon.muzzle.forward * 100f, true);
						}
						secundaryShots--;
					}
				}
				else if (!IsReloading && !_waitingReload)
				{
					StartCoroutine(Reload());
				}
			}
			if (!_canShot && !IsReloading && !_waitingReload && doReloadWhileWaiting && shooterManager.currentWeapon.ammoCount < shooterManager.currentWeapon.clipSize)
			{
				shooterManager.ReloadWeapon();
			}
		}

		protected virtual IEnumerator Reload()
		{
			_waitingReload = true;
			yield return new WaitForSeconds(0.5f);
			shooterManager.ReloadWeapon();
			float minTimeToStartReload = 2f;
			while (!IsReloading)
			{
				minTimeToStartReload -= Time.deltaTime;
				if (minTimeToStartReload <= 0f)
				{
					break;
				}
				yield return null;
			}
			while (IsReloading)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			_waitingReload = false;
		}

		protected override void TryBlockAttack(vDamage damage)
		{
			if (shooterManager.currentWeapon != null)
			{
				isBlocking = false;
			}
			else
			{
				base.TryBlockAttack(damage);
			}
		}

		public override void Blocking()
		{
			if (shooterManager.currentWeapon != null)
			{
				isBlocking = false;
			}
			else
			{
				base.Blocking();
			}
		}

		public override void AimTo(Vector3 point, float timeToCancelAim = 1f, object sender = null)
		{
			aimTime = timeToCancelAim;
			isAiming = true;
			aimTarget = point;
		}

		public override void AimToTarget(float stayLookTime = 1f, object sender = null)
		{
			aimTime = stayLookTime;
			isAiming = true;
			if ((bool)currentTarget.transform && (bool)currentTarget.collider)
			{
				aimTarget = _lastTargetPosition + Vector3.up * (currentTarget.collider.bounds.size.y * 0.5f + aimTargetHeight);
			}
			else
			{
				aimTarget = _lastTargetPosition + Vector3.up * aimTargetHeight;
			}
			if (!base.isStrafing && input.magnitude > 0.1f)
			{
				base.isStrafing = true;
			}
		}

		public override void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection)
		{
			if (isAiming)
			{
				if (useNavMeshAgent && (bool)navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped)
				{
					navMeshAgent.isStopped = false;
				}
				SetStrafeLocomotion();
				destination = newDestination;
				if (input.magnitude > 0.1f)
				{
					temporaryDirection = targetDirection;
					temporaryDirectionTime = 1f;
				}
			}
			else
			{
				base.StrafeMoveTo(newDestination, targetDirection);
			}
		}
	}
}
