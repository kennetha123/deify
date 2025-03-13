using System;
using System.Collections;
using Invector.vItemManager;
using LastBoss.Character;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	public abstract class vThirdPersonMotor : vCharacter
	{
		public enum LocomotionType
		{
			FreeWithStrafe = 0,
			OnlyStrafe = 1,
			OnlyFree = 2
		}

		public enum GroundCheckMethod
		{
			Low = 0,
			High = 1
		}

		[Serializable]
		public class vMovementSpeed
		{
			[Tooltip("Rotation speed of the character")]
			public float rotationSpeed = 10f;

			[Tooltip("Character will walk by default and run when the sprint input is pressed. The Sprint animation will not play")]
			public bool walkByDefault;

			[Tooltip("Speed to Walk using rigibody force or extra speed if you're using RootMotion")]
			public float walkSpeed = 2f;

			[Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
			public float runningSpeed = 3f;

			[Tooltip("Speed to Sprint using rigibody force or extra speed if you're using RootMotion")]
			public float sprintSpeed = 4f;

			[Tooltip("Speed to Crouch using rigibody force or extra speed if you're using RootMotion")]
			public float crouchSpeed = 2f;
		}

		[vEditorToolbar("Debug", false, "", false, false, order = 5)]
		[Header("--- Debug Info ---")]
		public bool debugWindow;

		[vEditorToolbar("Stamina", false, "", false, false, order = 1)]
		public float maxStamina = 200f;

		public float staminaRecovery = 1.2f;

		internal float currentStamina;

		internal float currentStaminaRecoveryDelay;

		public float sprintStamina = 30f;

		public float jumpStamina = 30f;

		public float rollStamina = 25f;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent OnJump;

		public UnityEvent OnRoll;

		public UnityEvent OnStartSprinting;

		public UnityEvent OnFinishSprinting;

		public UnityEvent OnFinishSprintingByStamina;

		public UnityEvent OnStaminaEnd;

		[vEditorToolbar("Layers", false, "", false, false, order = 2)]
		[Tooltip("Layers that the character can walk on")]
		public LayerMask groundLayer = 1;

		[Tooltip("What objects can make the character auto crouch")]
		public LayerMask autoCrouchLayer = 1;

		[Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
		public float headDetect = 0.95f;

		[Tooltip("Select the layers the your character will stop moving when close to")]
		public LayerMask stopMoveLayer;

		[Tooltip("[RAYCAST] Stopmove Raycast Height")]
		public float stopMoveHeight = 0.65f;

		[Tooltip("[RAYCAST] Stopmove Raycast Distance")]
		public float stopMoveDistance = 0.5f;

		[vEditorToolbar("Locomotion", false, "", false, false, order = 3)]
		[Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
		[vHelpBox("When 'Use RootMotion' is checked, make sure to reset all speeds to zero to use the original root motion velocity.", vHelpBoxAttribute.MessageType.None)]
		public bool useRootMotion;

		public LocomotionType locomotionType;

		public vMovementSpeed freeSpeed;

		public vMovementSpeed strafeSpeed;

		[Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
		public bool rotateByWorld;

		[Tooltip("Check this to use the TurnOnSpot animations")]
		public bool turnOnSpotAnim;

		[Tooltip("Can control the roll direction")]
		public bool rollControl;

		[Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
		public bool useContinuousSprint = true;

		[Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
		public float randomIdleTime;

		[vEditorToolbar("Jump", false, "", false, false, order = 3)]
		[Tooltip("Check to control the character while jumping")]
		public bool jumpAirControl = true;

		[Tooltip("How much time the character will be jumping")]
		public float jumpTimer = 0.3f;

		internal float jumpCounter;

		[Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
		public float jumpForward = 3f;

		[Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
		public float jumpHeight = 4f;

		[vEditorToolbar("Grounded", false, "", false, false, order = 4)]
		[Tooltip("Ground Check Method To check ground Distance and ground angle\n*Simple: Use just a single Raycast\n*Normal: Use Raycast and SphereCast\n*Complex: Use SphereCastAll")]
		public GroundCheckMethod groundCheckMethod = GroundCheckMethod.High;

		[Tooltip("Distance to became not grounded")]
		[SerializeField]
		protected float groundMinDistance = 0.25f;

		[SerializeField]
		protected float groundMaxDistance = 0.5f;

		[Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
		public float stepOffsetEnd = 0.45f;

		[Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
		public float stepOffsetStart = 0.25f;

		[Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
		public float stepSmooth = 4f;

		[Tooltip("Max angle to walk")]
		[Range(30f, 80f)]
		public float slopeLimit = 75f;

		[Tooltip("Velocity to slide when on a slope limit ramp")]
		[Range(0f, 10f)]
		public float slideVelocity = 7f;

		[Tooltip("Apply extra gravity when the character is not grounded")]
		public float extraGravity = -10f;

		[Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
		public float ragdollVel = -16f;

		[Range(1f, 2.5f)]
		public float crouchHeightReduction = 1.5f;

		protected float groundDistance;

		public RaycastHit groundHit;

		internal vMeleeCombatInput meleeInput;

		private Invector.vItemManager.vItemManager inventory;

		internal bool startRolling;

		internal bool isQuickStep;

		internal bool isRolling;

		internal bool isStepBack;

		internal bool isJumping;

		internal bool inTurn;

		internal bool quickStop;

		internal bool landHigh;

		internal bool customAction;

		internal Vector3 targetDirection;

		internal Quaternion targetRotation;

		internal float strafeMagnitude;

		internal Quaternion freeRotation;

		internal bool keepDirection;

		internal Vector2 oldInput;

		internal Rigidbody _rigidbody;

		internal PhysicsMaterial frictionPhysics;

		internal PhysicsMaterial maxFrictionPhysics;

		internal PhysicsMaterial slippyPhysics;

		internal CapsuleCollider _capsuleCollider;

		internal bool lockMovement;

		internal bool continueRoll;

		internal bool lockInStrafe;

		internal bool lockSpeed;

		internal bool lockRotation;

		internal bool forceRootMotion;

		internal float colliderRadius;

		internal float colliderHeight;

		internal Vector3 colliderCenter;

		internal Vector2 input;

		internal float speed;

		internal float direction;

		internal float verticalVelocity;

		internal float velocity;

		internal AnimatorStateInfo baseLayerInfo;

		internal AnimatorStateInfo underBodyInfo;

		internal AnimatorStateInfo rightArmInfo;

		internal AnimatorStateInfo leftArmInfo;

		internal AnimatorStateInfo fullBodyInfo;

		internal AnimatorStateInfo upperBodyInfo;

		internal float jumpMultiplier = 1f;

		private bool _isStrafing;

		protected float timeToResetJumpMultiplier;

		protected bool finishStaminaOnSprint;

		private vThirdPersonInput inp;

		private vThirdPersonController cc;

		private vLockOn isLock;

		private bool isQuick;

		private float timerForQuickInput;

		private bool queueForAttackRoll;

		public bool isStrafing
		{
			get
			{
				if (!_isStrafing)
				{
					return lockInStrafe;
				}
				return true;
			}
			set
			{
				_isStrafing = value;
			}
		}

		public bool isGrounded { get; set; }

		public bool inCrouchArea { get; protected set; }

		public bool isSprinting { get; set; }

		public bool isSliding { get; protected set; }

		public bool stopMove { get; protected set; }

		public bool autoCrouch { get; protected set; }

		internal bool actions
		{
			get
			{
				if (!isRolling && !quickStop && !landHigh)
				{
					return customAction;
				}
				return true;
			}
		}

		protected virtual bool jumpFwdCondition
		{
			get
			{
				Vector3 vector = base.transform.position + _capsuleCollider.center + Vector3.up * (0f - _capsuleCollider.height) * 0.5f;
				Vector3 point = vector + Vector3.up * _capsuleCollider.height;
				return Physics.CapsuleCastAll(vector, point, _capsuleCollider.radius * 0.5f, base.transform.forward, 0.6f, groundLayer).Length == 0;
			}
		}

		protected void RemoveComponents()
		{
			if (removeComponentsAfterDie)
			{
				if (_capsuleCollider != null)
				{
					UnityEngine.Object.Destroy(_capsuleCollider);
				}
				if (_rigidbody != null)
				{
					UnityEngine.Object.Destroy(_rigidbody);
				}
				if (base.animator != null)
				{
					UnityEngine.Object.Destroy(base.animator);
				}
				MonoBehaviour[] components = GetComponents<MonoBehaviour>();
				for (int i = 0; i < components.Length; i++)
				{
					UnityEngine.Object.Destroy(components[i]);
				}
			}
		}

		public override void Init()
		{
			base.Init();
			meleeInput = GetComponent<vMeleeCombatInput>();
			cc = GetComponent<vThirdPersonController>();
			revenge = GetComponent<RevengeMode>();
			isLock = GetComponent<vLockOn>();
			base.animator.updateMode = AnimatorUpdateMode.Fixed;
			frictionPhysics = new PhysicsMaterial();
			frictionPhysics.name = "frictionPhysics";
			frictionPhysics.staticFriction = 0.25f;
			frictionPhysics.dynamicFriction = 0.25f;
			frictionPhysics.frictionCombine = PhysicsMaterialCombine.Multiply;
			maxFrictionPhysics = new PhysicsMaterial();
			maxFrictionPhysics.name = "maxFrictionPhysics";
			maxFrictionPhysics.staticFriction = 1f;
			maxFrictionPhysics.dynamicFriction = 1f;
			maxFrictionPhysics.frictionCombine = PhysicsMaterialCombine.Maximum;
			slippyPhysics = new PhysicsMaterial();
			slippyPhysics.name = "slippyPhysics";
			slippyPhysics.staticFriction = 0f;
			slippyPhysics.dynamicFriction = 0f;
			slippyPhysics.frictionCombine = PhysicsMaterialCombine.Minimum;
			_rigidbody = GetComponent<Rigidbody>();
			_capsuleCollider = GetComponent<CapsuleCollider>();
			colliderCenter = GetComponent<CapsuleCollider>().center;
			colliderRadius = GetComponent<CapsuleCollider>().radius;
			colliderHeight = GetComponent<CapsuleCollider>().height;
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Physics.IgnoreCollision(_capsuleCollider, componentsInChildren[i]);
			}
			if (fillHealthOnStart)
			{
				base.currentHealth = maxHealth;
			}
			currentHealthRecoveryDelay = healthRecoveryDelay;
			currentStamina = maxStamina;
			ResetJumpMultiplier();
			isGrounded = true;
			inp = GetComponent<vThirdPersonInput>();
			inventory = GetComponent<Invector.vItemManager.vItemManager>();
		}

		public virtual void UpdateMotor()
		{
			CheckHealth();
			CheckStamina();
			CheckGround();
			CheckRagdoll();
			StopMove();
			ControlCapsuleHeight();
			ControlJumpBehaviour();
			StaminaRecovery();
			HealthRecovery();
			if (isQuick)
			{
				timerForQuickInput -= Time.deltaTime;
			}
			if (timerForQuickInput <= 0f)
			{
				isQuick = false;
				timerForQuickInput = 0.5f;
			}
		}

		public void RollInput()
		{
			if (!inventory.inventory.isOpen && inp.rollInput.GetButtonUp() && !startRolling && !isQuick)
			{
				base.animator.SetBool("isRolling", true);
				UnityEngine.Object.FindObjectOfType<vHUDController>().SetStaminaDelay();
				OnRoll.Invoke();
			}
			if (fullBodyInfo.IsName("Null") && base.animator.GetBool("isRolling"))
			{
				if (!revenge.isRevengeMode)
				{
					cc.Roll();
				}
				else
				{
					isQuick = true;
					if (isLock.isLockingOn)
					{
						if (cc.input.x > 0f)
						{
							base.animator.SetFloat("QuickHor", Mathf.Ceil(cc.input.x));
						}
						else
						{
							base.animator.SetFloat("QuickHor", Mathf.Floor(cc.input.x));
						}
						if (cc.input.y > 0f)
						{
							base.animator.SetFloat("QuickVer", Mathf.Ceil(cc.input.y));
						}
						else
						{
							base.animator.SetFloat("QuickVer", Mathf.Floor(cc.input.y));
						}
						cc.QuickStep();
					}
					else
					{
						cc.FrontStep();
					}
				}
				CloseRolling();
			}
			if ((meleeInput.weakAttackInput.GetButtonDown() && isRolling) || (meleeInput.weakAttackInput.GetButtonDown() && startRolling) || (meleeInput.weakAttackInput.GetButtonDown() && isQuickStep))
			{
				queueForAttackRoll = true;
			}
			if (meleeInput.weakAttackInput.GetButtonDown() && isStepBack)
			{
				base.animator.SetBool("backstepAttack", true);
			}
			if (continueRoll && queueForAttackRoll)
			{
				base.animator.SetBool("attackAfterRoll", true);
				queueForAttackRoll = false;
			}
		}

		public void CloseRolling()
		{
			base.animator.SetBool("isRolling", false);
			base.animator.SetBool("attackAfterRoll", false);
			base.animator.SetBool("backstepAttack", false);
			base.animator.SetBool("sprintAttack", false);
		}

		public void TriggerAttackAfterRoll()
		{
			if (base.animator.GetBool("isAttacking"))
			{
				meleeInput.TriggerWeakAttack();
				base.animator.SetBool("isAttacking", false);
			}
		}

		public override void TakeDamage(vDamage damage)
		{
			if (!(base.currentHealth <= 0f) && (damage.ignoreDefense || !isRolling))
			{
				base.TakeDamage(damage);
				vInput.instance.GamepadVibration(0.25f);
			}
		}

		protected override void TriggerDamageReaction(vDamage damage)
		{
			if (!actions || !customAction)
			{
				base.TriggerDamageReaction(damage);
			}
			else if (damage.activeRagdoll)
			{
				base.onActiveRagdoll.Invoke();
			}
		}

		public virtual void ReduceStamina(float value, bool accumulative)
		{
			if (accumulative)
			{
				currentStamina -= value * Time.deltaTime;
			}
			else
			{
				currentStamina -= value;
			}
			if (currentStamina < 0f)
			{
				currentStamina = 0f;
				OnStaminaEnd.Invoke();
			}
		}

		public virtual void ChangeStamina(int value)
		{
			currentStamina += value;
			currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
		}

		public virtual void ChangeMaxStamina(int value)
		{
			maxStamina += value;
			if (maxStamina < 0f)
			{
				maxStamina = 0f;
			}
		}

		public virtual void DeathBehaviour()
		{
			lockMovement = true;
			base.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
			{
				base.animator.SetBool("isDead", true);
			}
		}

		private void CheckHealth()
		{
			if (base.isDead && base.currentHealth > 0f)
			{
				base.isDead = false;
			}
		}

		private void CheckStamina()
		{
			if (isSprinting && fullBodyInfo.IsName("Null"))
			{
				currentStaminaRecoveryDelay = 0.25f;
				ReduceStamina(sprintStamina, true);
			}
		}

		public void StaminaRecovery()
		{
			if (currentStaminaRecoveryDelay > 0f)
			{
				currentStaminaRecoveryDelay -= Time.deltaTime;
				return;
			}
			if (currentStamina > maxStamina)
			{
				currentStamina = maxStamina;
			}
			if (currentStamina < maxStamina)
			{
				currentStamina += staminaRecovery;
			}
		}

		public virtual void ControlLocomotion()
		{
			if (!lockMovement && !continueRoll && !(base.currentHealth <= 0f))
			{
				if ((locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing) || locomotionType.Equals(LocomotionType.OnlyFree))
				{
					FreeMovement();
				}
				else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || (locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing))
				{
					StrafeMovement();
				}
			}
		}

		protected virtual void StrafeMovement()
		{
			isStrafing = true;
			if (strafeSpeed.walkByDefault)
			{
				StrafeLimitSpeed(0.5f);
			}
			else
			{
				StrafeLimitSpeed(1f);
			}
			if (stopMove)
			{
				strafeMagnitude = 0f;
			}
			base.animator.SetFloat("InputMagnitude", strafeMagnitude, 0.2f, Time.deltaTime);
		}

		protected virtual void StrafeLimitSpeed(float value)
		{
			float num = (isSprinting ? (value + 0.5f) : value);
			Vector2 vector = input * num;
			float num2 = Mathf.Clamp(vector.y, 0f - num, num);
			float num3 = Mathf.Clamp(vector.x, 0f - num, num);
			speed = num2;
			direction = num3;
			strafeMagnitude = Mathf.Clamp(new Vector2(speed, direction).magnitude, 0f, num);
		}

		public virtual void FreeMovement()
		{
			speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
			if (freeSpeed.walkByDefault)
			{
				speed = Mathf.Clamp(speed, 0f, 0.5f);
			}
			else
			{
				speed = Mathf.Clamp(speed, 0f, 1f);
			}
			if (isSprinting)
			{
				speed += 0.5f;
			}
			if (stopMove || lockSpeed)
			{
				speed = 0f;
			}
			base.animator.SetFloat("InputMagnitude", speed, 0.2f, Time.deltaTime);
			bool flag = !actions || quickStop || (isRolling && rollControl);
			if (!(input != Vector2.zero && targetDirection.magnitude > 0.1f && flag) || lockRotation)
			{
				return;
			}
			Vector3 normalized = targetDirection.normalized;
			freeRotation = Quaternion.LookRotation(normalized, base.transform.up);
			float num = freeRotation.eulerAngles.y - base.transform.eulerAngles.y;
			float y = base.transform.eulerAngles.y;
			if (isGrounded || (!isGrounded && jumpAirControl))
			{
				if (num < 0f || num > 0f)
				{
					y = freeRotation.eulerAngles.y;
				}
				Vector3 euler = new Vector3(base.transform.eulerAngles.x, y, base.transform.eulerAngles.z);
				if (base.animator.IsInTransition(0) && inTurn)
				{
					return;
				}
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(euler), freeSpeed.rotationSpeed * Time.deltaTime);
			}
			if (!keepDirection)
			{
				oldInput = input;
			}
			if (Vector2.Distance(oldInput, input) > 0.9f && keepDirection)
			{
				keepDirection = false;
			}
		}

		public virtual void ControlSpeed(float velocity)
		{
			if (Time.deltaTime == 0f)
			{
				return;
			}
			if (useRootMotion && !actions && !customAction)
			{
				if (input.magnitude > 0.1f)
				{
					this.velocity = velocity;
					Vector3 b = new Vector3(base.animator.deltaPosition.x, base.transform.position.y, base.animator.deltaPosition.z) * ((velocity > 0f) ? velocity : 1f) / Time.deltaTime;
					b.y = _rigidbody.linearVelocity.y;
					_rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, b, 20f * Time.deltaTime);
					return;
				}
				if (!isJumping && !isSliding)
				{
					_rigidbody.linearVelocity = Vector3.zero;
				}
				base.transform.rotation = base.animator.rootRotation;
				base.transform.position = base.animator.rootPosition;
			}
			else if (actions || customAction || lockMovement || forceRootMotion || continueRoll)
			{
				this.velocity = velocity;
				Vector3 zero = Vector3.zero;
				zero.y = _rigidbody.linearVelocity.y;
				_rigidbody.linearVelocity = zero;
				base.transform.position = base.animator.rootPosition;
				if (forceRootMotion)
				{
					base.transform.rotation = base.animator.rootRotation;
				}
			}
			else if (input.magnitude > 0.1f || isJumping || isSliding)
			{
				if (isStrafing)
				{
					StrafeVelocity(velocity);
				}
				else
				{
					FreeVelocity(velocity);
				}
			}
			else
			{
				base.transform.rotation = base.animator.rootRotation;
				base.transform.position = base.animator.rootPosition;
				_rigidbody.linearVelocity = Vector3.zero;
			}
		}

		protected virtual void StrafeVelocity(float velocity)
		{
			Vector3 b = base.transform.TransformDirection(new Vector3(input.x, 0f, input.y)) * ((velocity > 0f) ? velocity : 1f);
			b.y = _rigidbody.linearVelocity.y;
			_rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, b, 20f * Time.deltaTime);
		}

		protected virtual void FreeVelocity(float velocity)
		{
			Vector3 vector = base.transform.forward * velocity * speed;
			vector.y = _rigidbody.linearVelocity.y;
			_rigidbody.linearVelocity = vector;
		}

		protected void StopMove()
		{
			if ((double)input.sqrMagnitude < 0.1)
			{
				return;
			}
			Ray ray = new Ray(base.transform.position + Vector3.up * stopMoveHeight, targetDirection.normalized);
			float num = 0f;
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, _capsuleCollider.radius + stopMoveDistance, stopMoveLayer))
			{
				stopMove = true;
				return;
			}
			if (!isGrounded || isJumping)
			{
				stopMove = isSliding;
				return;
			}
			if (Physics.Linecast(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), base.transform.position + targetDirection.normalized * (_capsuleCollider.radius + 0.2f), out hitInfo, groundLayer))
			{
				num = Vector3.Angle(Vector3.up, hitInfo.normal);
				if (debugWindow)
				{
					Debug.DrawLine(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), base.transform.position + targetDirection.normalized * (_capsuleCollider.radius + 0.2f), (num > slopeLimit) ? Color.yellow : Color.blue, 0.01f);
				}
				Vector3 end = hitInfo.point + targetDirection.normalized * _capsuleCollider.radius;
				if (num > slopeLimit && Physics.Linecast(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), end, out hitInfo, groundLayer))
				{
					if (debugWindow)
					{
						Debug.DrawRay(hitInfo.point, hitInfo.normal);
					}
					num = Vector3.Angle(Vector3.up, hitInfo.normal);
					if (num > slopeLimit && num < 85f)
					{
						if (debugWindow)
						{
							Debug.DrawLine(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), hitInfo.point, Color.red, 0.01f);
						}
						stopMove = true;
						return;
					}
					if (debugWindow)
					{
						Debug.DrawLine(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), hitInfo.point, Color.green, 0.01f);
					}
				}
			}
			else if (debugWindow)
			{
				Debug.DrawLine(base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), base.transform.position + targetDirection.normalized * (_capsuleCollider.radius * 0.2f), Color.blue, 0.01f);
			}
			stopMove = false;
		}

		protected virtual void ControlJumpBehaviour()
		{
			if (isJumping)
			{
				jumpCounter -= Time.deltaTime;
				if (jumpCounter <= 0f)
				{
					jumpCounter = 0f;
					isJumping = false;
				}
				Vector3 vector = _rigidbody.linearVelocity;
				vector.y = jumpHeight * jumpMultiplier;
				_rigidbody.linearVelocity = vector;
			}
		}

		public virtual void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
		{
			this.jumpMultiplier = jumpMultiplier;
			if (timeToResetJumpMultiplier <= 0f)
			{
				timeToResetJumpMultiplier = timeToReset;
				StartCoroutine(ResetJumpMultiplierRoutine());
			}
			else
			{
				timeToResetJumpMultiplier = timeToReset;
			}
		}

		public virtual void ResetJumpMultiplier()
		{
			StopCoroutine("ResetJumpMultiplierRoutine");
			timeToResetJumpMultiplier = 0f;
			jumpMultiplier = 1f;
		}

		protected IEnumerator ResetJumpMultiplierRoutine()
		{
			while (timeToResetJumpMultiplier > 0f && jumpMultiplier != 1f)
			{
				timeToResetJumpMultiplier -= Time.deltaTime;
				yield return null;
			}
			jumpMultiplier = 1f;
		}

		public virtual void AirControl()
		{
			if (isGrounded)
			{
				return;
			}
			Vector3 vector = base.transform.forward * jumpForward * speed;
			vector.y = _rigidbody.linearVelocity.y;
			Vector3 vector2 = base.transform.right * jumpForward * direction;
			vector2.x = _rigidbody.linearVelocity.x;
			EnableGravityAndCollision(0f);
			if (jumpAirControl)
			{
				if (isStrafing)
				{
					_rigidbody.linearVelocity = new Vector3(vector2.x, vector.y, _rigidbody.linearVelocity.z);
					Vector3 vector3 = base.transform.forward * (jumpForward * speed) + base.transform.right * (jumpForward * direction);
					_rigidbody.linearVelocity = new Vector3(vector3.x, _rigidbody.linearVelocity.y, vector3.z);
				}
				else
				{
					Vector3 vector4 = base.transform.forward * (jumpForward * speed);
					_rigidbody.linearVelocity = new Vector3(vector4.x, _rigidbody.linearVelocity.y, vector4.z);
				}
			}
			else
			{
				Vector3 vector5 = base.transform.forward * jumpForward;
				_rigidbody.linearVelocity = new Vector3(vector5.x, _rigidbody.linearVelocity.y, vector5.z);
			}
		}

		protected virtual void CheckGround()
		{
			CheckGroundDistance();
			if (base.isDead || customAction)
			{
				isGrounded = true;
				return;
			}
			_capsuleCollider.material = ((isGrounded && GroundAngle() <= slopeLimit + 1f) ? frictionPhysics : slippyPhysics);
			if (isGrounded && input == Vector2.zero)
			{
				_capsuleCollider.material = maxFrictionPhysics;
			}
			else if (isGrounded && input != Vector2.zero)
			{
				_capsuleCollider.material = frictionPhysics;
			}
			else
			{
				_capsuleCollider.material = slippyPhysics;
			}
			bool num = !isRolling;
			float num2 = Mathf.Clamp((float)Math.Round(new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z).magnitude, 2), 0f, 1f);
			float num3 = groundMinDistance;
			if (num2 > 0.25f)
			{
				num3 = groundMaxDistance;
			}
			if (!num)
			{
				return;
			}
			bool flag = StepOffset();
			if (groundDistance <= 0.05f)
			{
				isGrounded = true;
				Sliding();
			}
			else if (groundDistance >= num3)
			{
				isGrounded = false;
				verticalVelocity = _rigidbody.linearVelocity.y;
				if (!flag && !isJumping)
				{
					_rigidbody.AddForce(base.transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
				}
			}
			else if (!flag && !isJumping)
			{
				_rigidbody.AddForce(base.transform.up * (extraGravity * 2f * Time.deltaTime), ForceMode.VelocityChange);
			}
		}

		protected virtual void CheckGroundDistance()
		{
			if (base.isDead || !(_capsuleCollider != null))
			{
				return;
			}
			float radius = _capsuleCollider.radius * 0.9f;
			float num = 10f;
			if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, colliderHeight / 2f, 0f), Vector3.down), out groundHit, colliderHeight / 2f + 2f, groundLayer) && !groundHit.collider.isTrigger)
			{
				num = base.transform.position.y - groundHit.point.y;
			}
			if (groundCheckMethod == GroundCheckMethod.High)
			{
				Vector3 origin = base.transform.position + Vector3.up * _capsuleCollider.radius;
				if (Physics.SphereCast(new Ray(origin, -Vector3.up), radius, out groundHit, _capsuleCollider.radius + 2f, groundLayer) && !groundHit.collider.isTrigger)
				{
					if (num > groundHit.distance - _capsuleCollider.radius * 0.1f)
					{
						num = groundHit.distance - _capsuleCollider.radius * 0.1f;
					}
					Physics.Linecast(groundHit.point + Vector3.up * 0.1f, groundHit.point + Vector3.down * 0.1f, out groundHit, groundLayer);
				}
			}
			groundDistance = (float)Math.Round(num, 2);
		}

		public virtual float GroundAngle()
		{
			return Vector3.Angle(groundHit.normal, Vector3.up);
		}

		public virtual float GroundAngleFromDirection()
		{
			return Vector3.Angle((isStrafing && input.magnitude > 0f) ? (base.transform.right * input.x + base.transform.forward * input.y).normalized : base.transform.forward, groundHit.normal) - 90f;
		}

		protected virtual void AlignWithSurface()
		{
			Ray ray = new Ray(base.transform.position, -base.transform.up);
			Quaternion b = base.transform.rotation;
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 1.5f, groundLayer))
			{
				b = Quaternion.FromToRotation(base.transform.up, hitInfo.normal) * base.transform.localRotation;
			}
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, 10f * Time.deltaTime);
		}

		protected virtual void Sliding()
		{
			if (GroundAngle() >= slopeLimit + 1f && GroundAngle() <= 85f && groundDistance <= 0.05f)
			{
				isSliding = true;
				isGrounded = false;
				_rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f - slideVelocity, _rigidbody.linearVelocity.z);
				Vector3 velocity2 = _rigidbody.linearVelocity;
			}
			else
			{
				isSliding = false;
				isGrounded = true;
			}
		}

		protected virtual bool StepOffset()
		{
			if ((double)input.sqrMagnitude < 0.1 || !isGrounded || stopMove || isSliding || isJumping)
			{
				return false;
			}
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = Vector3.ProjectOnPlane((isStrafing && input.magnitude > 0f) ? (base.transform.right * input.x + base.transform.forward * input.y).normalized : base.transform.forward, groundHit.normal);
			Ray ray = new Ray(base.transform.position + new Vector3(0f, stepOffsetEnd, 0f) + vector * (_capsuleCollider.radius + 0.05f), Vector3.down);
			if (Physics.Raycast(ray, out hitInfo, stepOffsetEnd - stepOffsetStart, groundLayer) && !hitInfo.collider.isTrigger && hitInfo.point.y >= base.transform.position.y + 0.1f && hitInfo.point.y <= base.transform.position.y + stepOffsetEnd)
			{
				float num = Mathf.Clamp(input.magnitude, 0f, 1f);
				Vector3 normalized = (hitInfo.point - base.transform.position).normalized;
				Vector3 vector2 = _rigidbody.linearVelocity;
				vector2.y = (normalized * stepSmooth * (num * ((velocity > 1f) ? velocity : 1f))).y;
				if (base.transform.position.y > hitInfo.point.y + 0.1f)
				{
					vector2.y = 0f;
				}
				_capsuleCollider.material = slippyPhysics;
				_rigidbody.linearVelocity = vector2;
				return true;
			}
			if (Physics.SphereCast(ray, _capsuleCollider.radius * 0.9f, out hitInfo, stepOffsetEnd - stepOffsetStart, groundLayer) && !hitInfo.collider.isTrigger && hitInfo.point.y >= base.transform.position.y + 0.1f && hitInfo.point.y <= base.transform.position.y + stepOffsetEnd)
			{
				float num2 = Mathf.Clamp(input.magnitude, 0f, 1f);
				Vector3 normalized2 = (hitInfo.point - base.transform.position).normalized;
				Vector3 vector3 = _rigidbody.linearVelocity;
				vector3.y = (normalized2 * stepSmooth * (num2 * ((velocity > 1f) ? velocity : 1f))).y;
				if (base.transform.position.y > hitInfo.point.y + 0.1f)
				{
					vector3.y = 0f;
				}
				_capsuleCollider.material = slippyPhysics;
				_rigidbody.linearVelocity = vector3;
				return true;
			}
			return false;
		}

		protected virtual void ControlCapsuleHeight()
		{
			if (isCrouching || isRolling || landHigh || startRolling)
			{
				_capsuleCollider.center = colliderCenter / crouchHeightReduction;
				_capsuleCollider.height = colliderHeight / crouchHeightReduction;
			}
			else
			{
				_capsuleCollider.center = colliderCenter;
				_capsuleCollider.radius = colliderRadius;
				_capsuleCollider.height = colliderHeight;
			}
		}

		public virtual void DisableGravityAndCollision()
		{
			base.animator.SetFloat("InputHorizontal", 0f);
			base.animator.SetFloat("InputVertical", 0f);
			base.animator.SetFloat("VerticalVelocity", 0f);
			_rigidbody.useGravity = false;
			_capsuleCollider.isTrigger = true;
		}

		public virtual void EnableGravityAndCollision(float normalizedTime)
		{
			if (baseLayerInfo.normalizedTime >= normalizedTime)
			{
				_capsuleCollider.isTrigger = false;
				_rigidbody.useGravity = true;
			}
		}

		public virtual void RotateToTarget(Transform target)
		{
			if ((bool)target)
			{
				Quaternion quaternion = Quaternion.LookRotation(target.position - base.transform.position);
				Vector3 euler = new Vector3(base.transform.eulerAngles.x, quaternion.eulerAngles.y, base.transform.eulerAngles.z);
				targetRotation = Quaternion.Euler(euler);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(euler), strafeSpeed.rotationSpeed * Time.deltaTime);
			}
		}

		public virtual void RotateToDirection(Vector3 direction, bool ignoreLerp = false)
		{
			Quaternion quaternion = Quaternion.LookRotation(direction);
			Vector3 euler = new Vector3(base.transform.eulerAngles.x, quaternion.eulerAngles.y, base.transform.eulerAngles.z);
			targetRotation = Quaternion.Euler(euler);
			if (ignoreLerp)
			{
				base.transform.rotation = Quaternion.Euler(euler);
			}
			else
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(euler), strafeSpeed.rotationSpeed * Time.deltaTime);
			}
			targetDirection = direction;
		}

		public virtual void UpdateTargetDirection(Transform referenceTransform = null)
		{
			if ((bool)referenceTransform && !rotateByWorld)
			{
				Vector3 vector = (keepDirection ? referenceTransform.forward : referenceTransform.TransformDirection(Vector3.forward));
				vector.y = 0f;
				vector = (keepDirection ? vector : referenceTransform.TransformDirection(Vector3.forward));
				vector.y = 0f;
				Vector3 vector2 = (keepDirection ? referenceTransform.right : referenceTransform.TransformDirection(Vector3.right));
				targetDirection = input.x * vector2 + input.y * vector;
			}
			else
			{
				targetDirection = (keepDirection ? targetDirection : new Vector3(input.x, 0f, input.y));
			}
		}

		protected virtual void CheckRagdoll()
		{
			if (ragdollVel != 0f && verticalVelocity <= ragdollVel && groundDistance <= 0.1f)
			{
				base.onActiveRagdoll.Invoke();
			}
		}

		public override void ResetRagdoll()
		{
			lockMovement = false;
			verticalVelocity = 0f;
			base.ragdolled = false;
			_rigidbody.WakeUp();
			_rigidbody.useGravity = true;
			_rigidbody.isKinematic = false;
			_capsuleCollider.isTrigger = false;
		}

		public override void EnableRagdoll()
		{
			base.animator.SetFloat("InputHorizontal", 0f);
			base.animator.SetFloat("InputVertical", 0f);
			base.animator.SetFloat("VerticalVelocity", 0f);
			base.ragdolled = true;
			_capsuleCollider.isTrigger = true;
			_rigidbody.useGravity = false;
			_rigidbody.isKinematic = true;
			lockMovement = true;
		}

		public virtual string DebugInfo(string additionalText = "")
		{
			string result = string.Empty;
			if (debugWindow)
			{
				float smoothDeltaTime = Time.smoothDeltaTime;
				float num = 1f / smoothDeltaTime;
				result = "FPS " + num.ToString("#,##0 fps") + "\ninputVertical = " + input.y.ToString("0.0") + "\ninputHorizontal = " + input.x.ToString("0.0") + "\nverticalVelocity = " + verticalVelocity.ToString("0.00") + "\ngroundDistance = " + groundDistance.ToString("0.00") + "\ngroundAngle = " + GroundAngle().ToString("0.00") + "\nuseGravity = " + _rigidbody.useGravity + "\ncolliderIsTrigger = " + _capsuleCollider.isTrigger + "\n\n--- Movement Bools ---\nonGround = " + isGrounded + "\nlockMovement = " + lockMovement + "\nstopMove = " + stopMove + "\nsliding = " + isSliding + "\nsprinting = " + isSprinting + "\ncrouch = " + isCrouching + "\nstrafe = " + isStrafing + "\nlandHigh = " + landHigh + "\n\n--- Actions Bools ---\nroll = " + isRolling + "\nisJumping = " + isJumping + "\nragdoll = " + base.ragdolled + "\nactions = " + actions + "\ncustomAction = " + customAction + "\n" + additionalText;
			}
			return result;
		}

		protected virtual void OnDrawGizmos()
		{
			if (Application.isPlaying && debugWindow)
			{
				Vector3 origin = base.transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
				Gizmos.DrawWireSphere(new Ray(origin, Vector3.up).GetPoint(headDetect - colliderRadius * 0.1f), colliderRadius * 0.9f);
				Ray ray = new Ray(base.transform.position + new Vector3(0f, stopMoveHeight, 0f), base.transform.forward);
				Debug.DrawRay(ray.origin, ray.direction * (_capsuleCollider.radius + stopMoveDistance), Color.blue);
				Ray ray2 = new Ray(base.transform.position + new Vector3(0f, colliderHeight / 3.5f, 0f), base.transform.forward);
				Debug.DrawRay(ray2.origin, ray2.direction * 1f, Color.cyan);
				Vector3 vector = ((isStrafing && input.magnitude > 0f) ? (base.transform.right * input.x + base.transform.forward * input.y).normalized : base.transform.forward);
				Ray ray3 = new Ray(base.transform.position + new Vector3(0f, stepOffsetEnd, 0f) + vector * (_capsuleCollider.radius + 0.05f), Vector3.down);
				Debug.DrawRay(ray3.origin, ray3.direction * (stepOffsetEnd - stepOffsetStart), Color.yellow);
			}
		}
	}
}
