using System;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController
{
	public abstract class vThirdPersonAnimator : vThirdPersonMotor
	{
		[HideInInspector]
		public Transform matchTarget;

		public vAnimatorStateInfos animatorStateInfos;

		private float randomIdleCount;

		private float randomIdle;

		private Vector3 lookPosition;

		private float _speed;

		private float _direction;

		private bool triggerDieBehaviour;

		private int baseLayer
		{
			get
			{
				return base.animator.GetLayerIndex("Base Layer");
			}
		}

		private int underBodyLayer
		{
			get
			{
				return base.animator.GetLayerIndex("UnderBody");
			}
		}

		private int rightArmLayer
		{
			get
			{
				return base.animator.GetLayerIndex("RightArm");
			}
		}

		private int leftArmLayer
		{
			get
			{
				return base.animator.GetLayerIndex("LeftArm");
			}
		}

		private int upperBodyLayer
		{
			get
			{
				return base.animator.GetLayerIndex("UpperBody");
			}
		}

		private int fullbodyLayer
		{
			get
			{
				return base.animator.GetLayerIndex("FullBody");
			}
		}

		protected virtual Quaternion rollRotation
		{
			get
			{
				return Quaternion.LookRotation(Vector3.RotateTowards(base.transform.forward, targetDirection, 180f * Time.fixedDeltaTime, 0f));
			}
		}

		protected override void Start()
		{
			base.Start();
			animatorStateInfos = new vAnimatorStateInfos(GetComponent<Animator>());
			animatorStateInfos.RegisterListener();
		}

		protected virtual void OnEnable()
		{
			if (animatorStateInfos != null)
			{
				animatorStateInfos.RegisterListener();
			}
		}

		protected virtual void OnAnimatorMove()
		{
			if (!base.enabled)
			{
				return;
			}
			UpdateAnimator();
			if (!base.isGrounded)
			{
				return;
			}
			if ((customAction && !isRolling) || continueRoll)
			{
				base.transform.rotation = base.animator.rootRotation;
			}
			if (base.isStrafing)
			{
				float num = Mathf.Abs(strafeMagnitude);
				if (num <= 0.5f)
				{
					ControlSpeed(strafeSpeed.walkSpeed);
				}
				else if (num > 0.5f && num <= 1f)
				{
					ControlSpeed(strafeSpeed.runningSpeed);
				}
				else
				{
					ControlSpeed(strafeSpeed.sprintSpeed);
				}
				if (isCrouching)
				{
					ControlSpeed(strafeSpeed.crouchSpeed);
				}
			}
			else if (!base.isStrafing)
			{
				if (speed <= 0.5f)
				{
					ControlSpeed(freeSpeed.walkSpeed);
				}
				else if ((double)speed > 0.5 && speed <= 1f)
				{
					ControlSpeed(freeSpeed.runningSpeed);
				}
				else
				{
					ControlSpeed(freeSpeed.sprintSpeed);
				}
				if (isCrouching)
				{
					ControlSpeed(freeSpeed.crouchSpeed);
				}
			}
		}

		protected virtual void UpdateAnimator()
		{
			if (!(base.animator == null) && base.animator.enabled)
			{
				LayerControl();
				ActionsControl();
				TriggerRandomIdle();
				RollAnimation();
				StartRollAnimation();
				LocomotionAnimation();
				DeadAnimation();
			}
		}

		public virtual void LayerControl()
		{
			baseLayerInfo = base.animator.GetCurrentAnimatorStateInfo(baseLayer);
			underBodyInfo = base.animator.GetCurrentAnimatorStateInfo(underBodyLayer);
			rightArmInfo = base.animator.GetCurrentAnimatorStateInfo(rightArmLayer);
			leftArmInfo = base.animator.GetCurrentAnimatorStateInfo(leftArmLayer);
			upperBodyInfo = base.animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
			fullBodyInfo = base.animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
		}

		public virtual void ActionsControl()
		{
			landHigh = baseLayerInfo.IsName("LandHigh");
			quickStop = baseLayerInfo.IsName("QuickStop");
			startRolling = baseLayerInfo.IsName("Start Roll");
			isQuickStep = baseLayerInfo.IsName("QuickStep");
			isRolling = baseLayerInfo.IsName("Roll");
			inTurn = IsAnimatorTag("TurnOnSpot");
			isStepBack = baseLayerInfo.IsName("StepBack");
			lockMovement = IsAnimatorTag("LockMovement");
			customAction = IsAnimatorTag("CustomAction");
			continueRoll = IsAnimatorTag("ContinueRoll");
		}

		protected virtual void TriggerRandomIdle()
		{
			if (input != Vector2.zero || base.actions || !(randomIdleTime > 0f))
			{
				return;
			}
			if (input.sqrMagnitude == 0f && !isCrouching && _capsuleCollider.enabled && base.isGrounded)
			{
				randomIdleCount += Time.fixedDeltaTime;
				if (randomIdleCount > 6f)
				{
					randomIdleCount = 0f;
					base.animator.SetTrigger("IdleRandomTrigger");
					base.animator.SetInteger("IdleRandom", UnityEngine.Random.Range(1, 4));
				}
			}
			else
			{
				randomIdleCount = 0f;
				base.animator.SetInteger("IdleRandom", 0);
			}
		}

		protected virtual void LocomotionAnimation()
		{
			base.animator.SetBool("IsStrafing", base.isStrafing);
			base.animator.SetBool("IsCrouching", isCrouching);
			base.animator.SetBool("IsGrounded", base.isGrounded);
			base.animator.SetBool("isDead", base.isDead);
			base.animator.SetFloat("GroundDistance", groundDistance);
			if (!base.isGrounded)
			{
				base.animator.SetFloat("VerticalVelocity", verticalVelocity);
			}
			if (base.isStrafing)
			{
				base.animator.SetFloat("InputHorizontal", (!base.stopMove && !lockMovement && !continueRoll) ? direction : 0f, 0.25f, Time.deltaTime);
				base.animator.SetFloat("InputVertical", (!base.stopMove && !lockMovement && !continueRoll) ? speed : 0f, 0.25f, Time.deltaTime);
			}
			else
			{
				Vector3 vector = base.transform.InverseTransformDirection(targetDirection);
				vector.z *= speed;
				base.animator.SetFloat("InputVertical", (!base.stopMove && !lockMovement && !continueRoll) ? vector.z : 0f, 0.25f, Time.deltaTime);
				base.animator.SetFloat("InputHorizontal", (!base.stopMove && !lockMovement && !continueRoll) ? vector.x : 0f, 0.25f, Time.deltaTime);
			}
			if (turnOnSpotAnim)
			{
				GetTurnOnSpotDirection(base.transform, Camera.main.transform, ref _speed, ref _direction, input);
				FreeTurnOnSpot(_direction * 180f);
				StrafeTurnOnSpot();
			}
		}

		public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
		{
			return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
		}

		protected virtual void StrafeTurnOnSpot()
		{
			if (!base.isStrafing || input.sqrMagnitude >= 0.25f || inTurn || customAction)
			{
				base.animator.SetFloat("TurnOnSpotDirection", 0f);
				return;
			}
			double num = Math.Round(base.transform.InverseTransformDirection(Camera.main.transform.forward).x, 1);
			if (num >= 0.10000000149011612 && !inTurn)
			{
				base.animator.SetFloat("TurnOnSpotDirection", 1f);
			}
			else if (num <= -0.10000000149011612 && !inTurn)
			{
				base.animator.SetFloat("TurnOnSpotDirection", -1f);
			}
			else
			{
				base.animator.SetFloat("TurnOnSpotDirection", 0f);
			}
		}

		protected virtual void FreeTurnOnSpot(float direction)
		{
			if (!base.isStrafing)
			{
				bool flag = base.animator.IsInTransition(0);
				float dampTime = ((inTurn || flag) ? 1000000 : 0);
				base.animator.SetFloat("TurnOnSpotDirection", direction, dampTime, Time.deltaTime);
			}
		}

		protected virtual void GetTurnOnSpotDirection(Transform root, Transform camera, ref float _speed, ref float _direction, Vector2 input)
		{
			Vector3 forward = root.forward;
			Vector3 vector = new Vector3(input.x, 0f, input.y);
			Vector3 forward2 = camera.forward;
			forward2.y = 0f;
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, forward2);
			Vector3 vector2 = (rotateByWorld ? vector : (quaternion * vector));
			_speed = Mathf.Clamp(new Vector2(input.x, input.y).magnitude, 0f, 1f);
			if (_speed > 0.01f)
			{
				Vector3 vector3 = Vector3.Cross(forward, vector2);
				_direction = Vector3.Angle(forward, vector2) / 180f * (float)((!(vector3.y < 0f)) ? 1 : (-1));
			}
			else
			{
				_direction = 0f;
			}
		}

		protected virtual void StartRollAnimation()
		{
			if (startRolling)
			{
				base.autoCrouch = true;
				if (base.isStrafing && (input != Vector2.zero || speed > 0.25f))
				{
					Quaternion quaternion = rollRotation;
					Vector3 eulerAngles = new Vector3(base.transform.eulerAngles.x, quaternion.eulerAngles.y, base.transform.eulerAngles.z);
					base.transform.eulerAngles = eulerAngles;
				}
				else
				{
					Quaternion quaternion2 = rollRotation;
					Vector3 eulerAngles2 = new Vector3(input.x, quaternion2.eulerAngles.y, input.y);
					base.transform.eulerAngles = eulerAngles2;
				}
			}
		}

		protected virtual void RollAnimation()
		{
			if (isRolling)
			{
				base.autoCrouch = true;
				if (base.isStrafing && (input != Vector2.zero || speed > 0.25f))
				{
					Quaternion quaternion = rollRotation;
					Vector3 eulerAngles = new Vector3(base.transform.eulerAngles.x, quaternion.eulerAngles.y, base.transform.eulerAngles.z);
					base.transform.eulerAngles = eulerAngles;
				}
			}
		}

		protected virtual void DeadAnimation()
		{
			if (!base.isDead)
			{
				return;
			}
			if (!triggerDieBehaviour)
			{
				triggerDieBehaviour = true;
				DeathBehaviour();
			}
			if (deathBy == DeathBy.Animation)
			{
				if (fullBodyInfo.IsName("Dead") && fullBodyInfo.normalizedTime >= 0.99f && groundDistance <= 0.15f)
				{
					RemoveComponents();
				}
			}
			else if (deathBy == DeathBy.AnimationWithRagdoll)
			{
				if (fullBodyInfo.IsName("Dead") && fullBodyInfo.normalizedTime >= 0.8f)
				{
					base.onActiveRagdoll.Invoke();
				}
			}
			else if (deathBy == DeathBy.Ragdoll)
			{
				base.onActiveRagdoll.Invoke();
			}
		}

		public virtual void SetActionState(int value)
		{
			base.animator.SetInteger("ActionState", value);
		}

		public virtual void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
		{
			if (!base.animator.isMatchingTarget && !base.animator.IsInTransition(0) && !(Mathf.Repeat(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f) > normalisedEndTime))
			{
				base.animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
			}
		}

		public virtual void TriggerAnimationState(string animationClip, float transition)
		{
			base.animator.CrossFadeInFixedTime(animationClip, transition);
		}

		public virtual bool IsAnimatorTag(string tag)
		{
			if (base.animator == null)
			{
				return false;
			}
			if (animatorStateInfos != null && animatorStateInfos.HasTag(tag))
			{
				return true;
			}
			if (baseLayerInfo.IsTag(tag))
			{
				return true;
			}
			if (underBodyInfo.IsTag(tag))
			{
				return true;
			}
			if (rightArmInfo.IsTag(tag))
			{
				return true;
			}
			if (leftArmInfo.IsTag(tag))
			{
				return true;
			}
			if (upperBodyInfo.IsTag(tag))
			{
				return true;
			}
			if (fullBodyInfo.IsTag(tag))
			{
				return true;
			}
			return false;
		}
	}
}
