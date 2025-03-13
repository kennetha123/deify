using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("THIRD PERSON CONTROLLER", true, "icon_v2", false, "", iconName = "controllerIcon")]
	public class vThirdPersonController : vThirdPersonAnimator
	{
		[vHelpBox("Check this option to transfer your character from one scene to another, uncheck if you're planning to use the controller with any kind of Multiplayer local or online", vHelpBoxAttribute.MessageType.None)]
		public bool useInstance = true;

		public static vThirdPersonController instance;

		protected virtual void Awake()
		{
			StartCoroutine(UpdateRaycast());
		}

		protected override void Start()
		{
			base.Start();
			if (useInstance)
			{
				if (instance == null)
				{
					instance = this;
					Object.DontDestroyOnLoad(base.gameObject);
					base.gameObject.name = base.gameObject.name + " Instance";
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
			}
		}

		public virtual void Sprint(bool value)
		{
			bool flag = currentStamina > 0f && input.sqrMagnitude > 0.1f && !isCrouching && base.isGrounded && !base.actions && (!base.isStrafing || strafeSpeed.walkByDefault || (!((double)direction >= 0.5) && !((double)direction <= -0.5) && !(speed <= 0f)));
			if (value && flag)
			{
				if (currentStamina > (finishStaminaOnSprint ? sprintStamina : 0f) && input.sqrMagnitude > 0.1f)
				{
					finishStaminaOnSprint = false;
					if (base.isGrounded && !isCrouching && useContinuousSprint)
					{
						base.isSprinting = !base.isSprinting;
						Object.FindObjectOfType<vHUDController>().SetStaminaNotDelay();
						OnStartSprinting.Invoke();
					}
					else if (!base.isSprinting)
					{
						base.isSprinting = true;
						Object.FindObjectOfType<vHUDController>().SetStaminaNotDelay();
						OnStartSprinting.Invoke();
					}
				}
				else if (!useContinuousSprint && base.isSprinting)
				{
					if (currentStamina <= 0f)
					{
						finishStaminaOnSprint = true;
						OnFinishSprintingByStamina.Invoke();
					}
					base.isSprinting = false;
					OnFinishSprinting.Invoke();
				}
			}
			else if (base.isSprinting && (!useContinuousSprint || !flag))
			{
				if (currentStamina <= 0f)
				{
					finishStaminaOnSprint = true;
					OnFinishSprintingByStamina.Invoke();
				}
				base.isSprinting = false;
				OnFinishSprinting.Invoke();
			}
		}

		public virtual void Crouch()
		{
			if (base.isGrounded && !base.actions)
			{
				if (isCrouching && CanExitCrouch())
				{
					isCrouching = false;
				}
				else
				{
					isCrouching = true;
				}
			}
		}

		public virtual void Strafe()
		{
			base.isStrafing = !base.isStrafing;
		}

		public virtual void Jump(bool consumeStamina = false)
		{
			if (customAction || GroundAngle() > slopeLimit)
			{
				return;
			}
			bool flag = currentStamina > jumpStamina;
			if (!isCrouching && base.isGrounded && !base.actions && flag && !isJumping)
			{
				jumpCounter = jumpTimer;
				isJumping = true;
				OnJump.Invoke();
				if (input.sqrMagnitude < 0.1f)
				{
					base.animator.CrossFadeInFixedTime("Jump", 0.1f);
				}
				else
				{
					base.animator.CrossFadeInFixedTime("JumpMove", 0.2f);
				}
				if (consumeStamina)
				{
					ReduceStamina(jumpStamina, false);
					currentStaminaRecoveryDelay = 1f;
				}
			}
		}

		public virtual void Roll()
		{
			bool flag = currentStamina != 0f;
			if (!flag)
			{
				base.animator.SetBool("isRolling", false);
			}
			if (!((!base.actions || (base.actions && quickStop)) && base.isGrounded && flag) || isJumping)
			{
				return;
			}
			bool flag2 = input == Vector2.zero;
			startRolling = true;
			if (base.animator.GetBool("isMoving"))
			{
				if (isRolling)
				{
					base.animator.CrossFadeInFixedTime("Roll", 0.2f);
				}
				else
				{
					base.animator.CrossFadeInFixedTime("Start Roll", 0.1f);
				}
				ReduceStamina(rollStamina, false);
				currentStaminaRecoveryDelay = 2f;
			}
			else if (!base.animator.GetBool("isMoving") && !isStepBack)
			{
				base.animator.CrossFadeInFixedTime("StepBack", 0.1f);
				ReduceStamina(rollStamina, false);
				currentStaminaRecoveryDelay = 2f;
			}
		}

		public virtual void QuickStep()
		{
			if (base.animator.GetBool("isRolling"))
			{
				return;
			}
			bool flag = currentStamina != 0f;
			if (!flag)
			{
				base.animator.SetBool("isRolling", false);
			}
			if ((!base.actions || (base.actions && quickStop)) && base.isGrounded && flag && !isJumping)
			{
				bool num = input == Vector2.zero;
				if (!num && !isQuickStep)
				{
					base.animator.CrossFadeInFixedTime("QuickStep", 0.2f);
					ReduceStamina(rollStamina, false);
					currentStaminaRecoveryDelay = 2f;
				}
				if (num && !isStepBack)
				{
					base.animator.CrossFadeInFixedTime("StepBack", 0.1f);
					ReduceStamina(rollStamina, false);
					currentStaminaRecoveryDelay = 2f;
				}
			}
		}

		public virtual void FrontStep()
		{
			if (base.animator.GetBool("isRolling") && !fullBodyInfo.IsName("Null"))
			{
				return;
			}
			bool flag = currentStamina != 0f;
			if ((!base.actions || (base.actions && quickStop)) && base.isGrounded && flag && !isJumping)
			{
				bool num = input == Vector2.zero;
				if (!num && !isQuickStep)
				{
					base.animator.CrossFadeInFixedTime("FrontStep", 0.2f);
					ReduceStamina(rollStamina, false);
					currentStaminaRecoveryDelay = 2f;
					isQuickStep = true;
				}
				if (num && !isStepBack)
				{
					base.animator.CrossFadeInFixedTime("StepBack", 0.1f);
					ReduceStamina(rollStamina, false);
					currentStaminaRecoveryDelay = 2f;
				}
			}
		}

		public virtual void RotateWithAnotherTransform(Transform referenceTransform)
		{
			Vector3 euler = new Vector3(base.transform.eulerAngles.x, referenceTransform.eulerAngles.y, base.transform.eulerAngles.z);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(euler), strafeSpeed.rotationSpeed * Time.deltaTime);
			targetRotation = base.transform.rotation;
		}

		protected override void OnTriggerStay(Collider other)
		{
			try
			{
				CheckForAutoCrouch(other);
			}
			catch (UnityException ex)
			{
				Debug.LogWarning(ex.Message);
			}
			base.OnTriggerStay(other);
		}

		protected override void OnTriggerExit(Collider other)
		{
			AutoCrouchExit(other);
			base.OnTriggerExit(other);
		}

		protected IEnumerator UpdateRaycast()
		{
			while (true)
			{
				yield return new WaitForEndOfFrame();
				AutoCrouch();
			}
		}

		public virtual void UseAutoCrouch(bool value)
		{
			base.autoCrouch = value;
		}

		protected virtual void AutoCrouch()
		{
			if (base.autoCrouch)
			{
				isCrouching = true;
			}
			if (base.autoCrouch && !base.inCrouchArea && CanExitCrouch())
			{
				base.autoCrouch = false;
				isCrouching = false;
			}
		}

		public virtual bool CanExitCrouch()
		{
			float radius = _capsuleCollider.radius * 0.9f;
			Vector3 origin = base.transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
			if (Physics.SphereCast(new Ray(origin, Vector3.up), radius, out groundHit, headDetect - colliderRadius * 0.1f, autoCrouchLayer))
			{
				return false;
			}
			return true;
		}

		protected virtual void AutoCrouchExit(Collider other)
		{
			if (other.CompareTag("AutoCrouch"))
			{
				base.inCrouchArea = false;
			}
		}

		protected virtual void CheckForAutoCrouch(Collider other)
		{
			if (other.gameObject.CompareTag("AutoCrouch"))
			{
				base.autoCrouch = true;
				base.inCrouchArea = true;
			}
		}
	}
}
