using UnityEngine;
using UnityEngine.AI;

namespace Invector.vCharacterController.AI
{
	public class v_AIAnimator : v_AIMotor
	{
		private bool triggerDieBehaviour;

		private bool resetState;

		private float strafeInput;

		public AnimatorStateInfo baseLayerInfo;

		public AnimatorStateInfo rightArmInfo;

		public AnimatorStateInfo leftArmInfo;

		public AnimatorStateInfo fullBodyInfo;

		public AnimatorStateInfo upperBodyInfo;

		public AnimatorStateInfo underBodyInfo;

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

		protected virtual float maxSpeed
		{
			get
			{
				if (!base.OnStrafeArea)
				{
					return chaseSpeed;
				}
				return strafeSpeed;
			}
		}

		public void UpdateAnimator(float _speed, float _direction)
		{
			if (!(base.animator == null) && base.animator.enabled)
			{
				LayerControl();
				LocomotionAnimation(_speed, _direction);
				RollAnimation();
				CrouchAnimation();
				ResetAndLockAgent();
				MoveSetIDControl();
				MeleeATK_Animation();
				DEF_Animation();
				DeadAnimation();
			}
		}

		private void LayerControl()
		{
			baseLayerInfo = base.animator.GetCurrentAnimatorStateInfo(baseLayer);
			underBodyInfo = base.animator.GetCurrentAnimatorStateInfo(underBodyLayer);
			rightArmInfo = base.animator.GetCurrentAnimatorStateInfo(rightArmLayer);
			leftArmInfo = base.animator.GetCurrentAnimatorStateInfo(leftArmLayer);
			upperBodyInfo = base.animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
			fullBodyInfo = base.animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
		}

		private void OnAnimatorMove()
		{
			if (Time.timeScale == 0f)
			{
				return;
			}
			if (agent.enabled && !agent.isOnOffMeshLink && agent.updatePosition)
			{
				agent.velocity = base.animator.deltaPosition / Time.deltaTime;
			}
			if (!_rigidbody.useGravity && !base.actions && !agent.isOnOffMeshLink)
			{
				_rigidbody.linearVelocity = base.animator.deltaPosition;
			}
			if (!agent.updatePosition && !base.actions)
			{
				Vector3 vector = (agent.enabled ? agent.nextPosition : destination);
				if (Vector3.Distance(base.transform.position, vector) > 0.5f)
				{
					desiredRotation = Quaternion.LookRotation(vector - base.transform.position);
					Quaternion to = Quaternion.Euler(base.transform.eulerAngles.x, desiredRotation.eulerAngles.y, base.transform.eulerAngles.z);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, agent.angularSpeed * Time.deltaTime);
				}
				base.transform.position = base.animator.rootPosition;
			}
			else if (base.OnStrafeArea && !base.actions && currentTarget.transform != null && canSeeTarget && base.currentHealth > 0f)
			{
				Vector3 target = currentTarget.transform.position - base.transform.position;
				float maxRadiansDelta = ((meleeManager != null && base.isAttacking) ? (attackRotationSpeed * Time.deltaTime) : (strafeRotationSpeed * Time.deltaTime));
				Quaternion quaternion = Quaternion.LookRotation(Vector3.RotateTowards(base.transform.forward, target, maxRadiansDelta, 0f));
				base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, quaternion.eulerAngles.y, base.transform.eulerAngles.z);
			}
			else if (agent.isOnOffMeshLink && !base.actions)
			{
				Vector3 endPos = agent.nextOffMeshLinkData.endPos;
				targetPos = endPos;
				OffMeshLinkData currentOffMeshLinkData = agent.currentOffMeshLinkData;
				desiredRotation = Quaternion.LookRotation(new Vector3(currentOffMeshLinkData.endPos.x, base.transform.position.y, currentOffMeshLinkData.endPos.z) - base.transform.position);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, desiredRotation, agent.angularSpeed * 2f * Time.deltaTime);
			}
			else if (agent.desiredVelocity.magnitude > 0.1f && !base.actions && agent.enabled && base.currentHealth > 0f)
			{
				if (meleeManager != null && base.isAttacking)
				{
					desiredRotation = Quaternion.LookRotation(agent.desiredVelocity);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, desiredRotation, agent.angularSpeed * attackRotationSpeed * Time.deltaTime);
				}
				else
				{
					desiredRotation = Quaternion.LookRotation(agent.desiredVelocity);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, desiredRotation, agent.angularSpeed * Time.deltaTime);
				}
			}
			else if (base.actions || base.currentHealth <= 0f || base.isAttacking)
			{
				if (isRolling)
				{
					desiredRotation = Quaternion.LookRotation(rollDirection, Vector3.up);
					base.transform.rotation = desiredRotation;
				}
				else
				{
					base.transform.rotation = base.animator.rootRotation;
				}
				if (!agent.enabled)
				{
					destination = base.transform.position;
					base.transform.position = base.animator.rootPosition;
				}
			}
		}

		private void LocomotionAnimation(float _speed, float _direction)
		{
			isGrounded = (agent.enabled ? agent.isOnNavMesh : (isRolling || groundDistance <= groundCheckDistance));
			base.animator.SetBool("IsGrounded", isGrounded);
			_speed = Mathf.Clamp(_speed, 0f - maxSpeed, maxSpeed);
			if (base.OnStrafeArea)
			{
				_direction = Mathf.Clamp(_direction, 0f - strafeSpeed, strafeSpeed);
			}
			strafeInput = Mathf.Clamp(new Vector2(_speed, _direction).magnitude, 0f, 1.5f);
			base.animator.SetFloat("InputMagnitude", strafeInput, 0.2f, Time.deltaTime);
			base.animator.SetFloat("InputVertical", base.actions ? 0f : ((_speed != 0f) ? _speed : 0f), 0.2f, Time.fixedDeltaTime);
			base.animator.SetFloat("InputHorizontal", _direction, 0.2f, Time.fixedDeltaTime);
			base.animator.SetBool("IsStrafing", base.OnStrafeArea);
			base.animator.SetBool("isDead", base.isDead);
		}

		private void DeadAnimation()
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
					RemoveComponents();
				}
			}
			else if (deathBy == DeathBy.Ragdoll)
			{
				base.onActiveRagdoll.Invoke();
				RemoveComponents();
			}
		}

		private void DeathBehaviour()
		{
			base.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
			{
				base.animator.SetBool("isDead", base.isDead);
			}
		}

		private void CrouchAnimation()
		{
			base.animator.SetBool("IsCrouching", isCrouched);
			if (base.animator != null && base.animator.enabled)
			{
				CheckAutoCrouch();
			}
		}

		protected void RollAnimation()
		{
			if (!(base.animator == null) && base.animator.enabled)
			{
				isRolling = baseLayerInfo.IsName("Roll");
				if (isRolling)
				{
					_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
					_rigidbody.useGravity = true;
					agent.enabled = false;
					agent.updatePosition = false;
				}
			}
		}

		private void ResetAIRotation()
		{
			base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
		}

		private void MoveSetIDControl()
		{
			if (!(meleeManager == null))
			{
				base.animator.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID());
			}
		}

		private void MeleeATK_Animation()
		{
			if (!(meleeManager == null))
			{
				if (base.actions)
				{
					attackCount = 0;
				}
				base.animator.SetInteger("AttackID", meleeManager.GetAttackID());
			}
		}

		private void DEF_Animation()
		{
			if (!(meleeManager == null))
			{
				if (base.isBlocking)
				{
					base.animator.SetInteger("DefenseID", meleeManager.GetDefenseID());
				}
				base.animator.SetBool("IsBlocking", base.isBlocking);
			}
		}

		public void MeleeAttack()
		{
			if (base.animator != null && base.animator.enabled && !base.actions)
			{
				base.animator.SetTrigger("WeakAttack");
			}
		}

		private void ResetAndLockAgent()
		{
			lockMovement = fullBodyInfo.IsTag("LockMovement") || upperBodyInfo.IsTag("ResetState");
			if (lockMovement)
			{
				if (attackCount > 0)
				{
					canAttack = false;
					attackCount = 0;
				}
				if (baseLayerInfo.normalizedTime > 0.1f)
				{
					base.animator.ResetTrigger("ResetState");
					_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
					_rigidbody.useGravity = true;
					agent.enabled = false;
					agent.updatePosition = false;
				}
			}
		}

		public void TriggerRecoil(int recoil_id)
		{
			if (base.animator != null && base.animator.enabled && !isRolling)
			{
				base.animator.SetInteger("RecoilID", recoil_id);
				base.animator.SetTrigger("TriggerRecoil");
				base.animator.SetTrigger("ResetState");
			}
		}
	}
}
