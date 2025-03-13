using System;
using System.Collections;
using Invector.vCamera;
using LastBoss.Character;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	[vClassHeader("Input Manager", true, "icon_v2", false, "", iconName = "inputIcon")]
	public class vThirdPersonInput : vMonoBehaviour
	{
		[Serializable]
		public class OnUpdateEvent : UnityEvent<vThirdPersonInput>
		{
		}

		[vEditorToolbar("Inputs", false, "", false, false)]
		[Header("Default Input")]
		public bool lockInput;

		[Header("Uncheck if you need to use the cursor")]
		public bool unlockCursorOnStart;

		public bool showCursorOnStart;

		public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");

		public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");

		public GenericInput jumpInput = new GenericInput("Space", "X", "X");

		public GenericInput rollInput = new GenericInput("Q", "B", "B");

		public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

		public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");

		public GenericInput crouchInput = new GenericInput("C", "Y", "Y");

		[vEditorToolbar("Camera Settings", false, "", false, false)]
		public bool lockCameraInput;

		public bool ignoreCameraRotation;

		[vHelpBox("Character will rotate to the Camera Forward while walking when Strafing", vHelpBoxAttribute.MessageType.None)]
		public bool rotateToCameraFwdWhenMoving = true;

		[vHelpBox("Character will rotate to the Camera Forward at all times when Strafing", vHelpBoxAttribute.MessageType.None)]
		public bool rotateToCameraFwdWhenStanding;

		[vEditorToolbar("Inputs", false, "", false, false)]
		[Header("Camera Input")]
		public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");

		public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");

		public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");

		[HideInInspector]
		public vThirdPersonCamera tpCamera;

		[HideInInspector]
		public Camera cameraMain;

		[HideInInspector]
		public string customCameraState;

		[HideInInspector]
		public string customlookAtPoint;

		[HideInInspector]
		public bool changeCameraState;

		[HideInInspector]
		public bool smoothCameraState;

		protected Vector2 oldInput;

		[vEditorToolbar("Events", false, "", false, false)]
		public UnityEvent OnLateUpdate;

		private RevengeMode revenge;

		[HideInInspector]
		public vThirdPersonController cc;

		[HideInInspector]
		public vHUDController hud;

		protected bool updateIK;

		protected bool isInit;

		[HideInInspector]
		public OnUpdateEvent onUpdateInput;

		private vMeleeCombatInput meleeInput;

		private float timerRollAndSprint = 0.3f;

		public bool sprint;

		protected InputDevice inputDevice
		{
			get
			{
				return vInput.instance.inputDevice;
			}
		}

		public Animator animator
		{
			get
			{
				if (cc == null)
				{
					cc = GetComponent<vThirdPersonController>();
				}
				if (cc.animator == null)
				{
					return GetComponent<Animator>();
				}
				return cc.animator;
			}
		}

		public virtual bool keepDirection
		{
			get
			{
				if ((bool)cc)
				{
					return cc.keepDirection;
				}
				return false;
			}
			set
			{
				if ((bool)cc)
				{
					cc.keepDirection = value;
				}
			}
		}

		protected virtual void Start()
		{
			cc = GetComponent<vThirdPersonController>();
			revenge = GetComponent<RevengeMode>();
			meleeInput = GetComponent<vMeleeCombatInput>();
			if (cc != null)
			{
				cc.Init();
			}
			if (vThirdPersonController.instance == cc || vThirdPersonController.instance == null)
			{
				StartCoroutine(CharacterInit());
			}
			ShowCursor(showCursorOnStart);
			LockCursor(unlockCursorOnStart);
		}

		protected virtual IEnumerator CharacterInit()
		{
			yield return new WaitForEndOfFrame();
			if (tpCamera == null)
			{
				tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
				if ((bool)tpCamera && tpCamera.target != base.transform)
				{
					tpCamera.SetMainTarget(base.transform);
				}
			}
			if (hud == null && vHUDController.instance != null)
			{
				hud = vHUDController.instance;
				hud.Init(cc);
			}
		}

		protected virtual void LateUpdate()
		{
			if (!(cc == null) && Time.timeScale != 0f && (updateIK || animator.updateMode != AnimatorUpdateMode.Fixed))
			{
				CameraInput();
				UpdateCameraStates();
				OnLateUpdate.Invoke();
				updateIK = false;
			}
		}

		protected virtual void FixedUpdate()
		{
			cc.ControlLocomotion();
			cc.AirControl();
			updateIK = true;
		}

		protected virtual void Update()
		{
			Additional();
			if (!(cc == null) && Time.timeScale != 0f)
			{
				InputHandle();
				cc.UpdateMotor();
				UpdateHUD();
			}
		}

		protected virtual void InputHandle()
		{
			if (!lockInput && !cc.lockMovement && !cc.continueRoll && !cc.ragdolled)
			{
				MoveCharacter();
				CrouchInput();
				StrafeInput();
				JumpInput();
			}
		}

		public virtual void SetLockBasicInput(bool value)
		{
			lockInput = value;
			if (value)
			{
				cc.input = Vector2.zero;
				cc.isSprinting = false;
				cc.animator.SetFloat("InputHorizontal", 0f, 0.25f, Time.deltaTime);
				cc.animator.SetFloat("InputVertical", 0f, 0.25f, Time.deltaTime);
				cc.animator.SetFloat("InputMagnitude", 0f, 0.25f, Time.deltaTime);
			}
		}

		public virtual void ShowCursor(bool value)
		{
			Cursor.visible = value;
		}

		public virtual void LockCursor(bool value)
		{
			if (!value)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}

		public virtual void SetLockCameraInput(bool value)
		{
			lockCameraInput = value;
		}

		public virtual void IgnoreCameraRotation(bool value)
		{
			ignoreCameraRotation = value;
		}

		public virtual void SetWalkByDefault(bool value)
		{
			cc.freeSpeed.walkByDefault = value;
			cc.strafeSpeed.walkByDefault = value;
		}

		public virtual void SetStrafeLocomotion(bool value)
		{
			cc.lockInStrafe = value;
			cc.isStrafing = value;
		}

		public void Additional()
		{
			if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f || Input.GetAxis("LeftAnalogHorizontal") != 0f || Input.GetAxis("LeftAnalogVertical") != 0f)
			{
				animator.SetBool("isMoving", true);
			}
			else
			{
				animator.SetBool("isMoving", false);
			}
			if (rollInput.GetButton())
			{
				timerRollAndSprint -= Time.deltaTime;
			}
			if (timerRollAndSprint > 0f)
			{
				cc.RollInput();
			}
			else
			{
				SprintInput();
				sprint = true;
			}
			if (rollInput.GetButtonUp())
			{
				timerRollAndSprint = 0.3f;
				sprint = false;
			}
		}

		public virtual void MoveCharacter(Vector3 position, bool rotateToDirection = true)
		{
			Vector3 vector = position - base.transform.position;
			Vector3 targetDirection = (cc.isStrafing ? base.transform.InverseTransformDirection(vector).normalized : vector.normalized);
			cc.input.x = targetDirection.x;
			cc.input.y = targetDirection.z;
			if (!keepDirection)
			{
				oldInput = cc.input;
			}
			if (rotateToDirection && cc.isStrafing)
			{
				targetDirection.y = 0f;
				cc.RotateToDirection(vector);
				Debug.DrawRay(base.transform.position, vector * 10f, Color.blue);
			}
			else if (rotateToDirection)
			{
				targetDirection.y = 0f;
				cc.targetDirection = targetDirection;
			}
		}

		public virtual void MoveCharacter(Transform _transform, bool rotateToDirection = true)
		{
			MoveCharacter(_transform.position, rotateToDirection);
		}

		protected virtual void MoveCharacter()
		{
			cc.input.x = Mathf.Round(horizontalInput.GetAxis() * 2f) * 0.5f;
			cc.input.y = Mathf.Round(verticallInput.GetAxis() * 2f) * 0.5f;
			if (!keepDirection)
			{
				oldInput = cc.input;
			}
		}

		protected virtual void StrafeInput()
		{
			if (strafeInput.GetButtonDown())
			{
				cc.Strafe();
			}
		}

		protected virtual void SprintInput()
		{
			if (sprint && meleeInput.weakAttackInput.GetButtonDown())
			{
				animator.SetBool("sprintAttack", true);
			}
			else
			{
				cc.Sprint(cc.useContinuousSprint ? rollInput.GetButtonDown() : rollInput.GetButton());
			}
		}

		protected virtual void CrouchInput()
		{
			if (crouchInput.GetButtonDown())
			{
				cc.Crouch();
			}
		}

		protected virtual void JumpInput()
		{
			if (jumpInput.GetButtonDown())
			{
				cc.Jump(true);
			}
		}

		public virtual void CameraInput()
		{
			if (!cameraMain)
			{
				if (!Camera.main)
				{
					Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
				}
				else
				{
					cameraMain = Camera.main;
				}
			}
			if ((bool)cameraMain && !ignoreCameraRotation)
			{
				if (!keepDirection)
				{
					cc.UpdateTargetDirection(cameraMain.transform);
				}
				RotateWithCamera(cameraMain.transform);
			}
			if (!(tpCamera == null))
			{
				float y = (lockCameraInput ? 0f : rotateCameraYInput.GetAxis());
				float x = (lockCameraInput ? 0f : rotateCameraXInput.GetAxis());
				float axis = cameraZoomInput.GetAxis();
				tpCamera.RotateCamera(x, y);
				tpCamera.Zoom(axis);
				if (keepDirection && Vector2.Distance(cc.input, oldInput) > 0.2f)
				{
					keepDirection = false;
				}
			}
		}

		protected virtual void UpdateCameraStates()
		{
			if (tpCamera == null)
			{
				tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
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
				tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
			}
			else if (cc.isCrouching)
			{
				tpCamera.ChangeState("Crouch", true);
			}
			else if (cc.isStrafing)
			{
				tpCamera.ChangeState("Strafing", true);
			}
			else
			{
				tpCamera.ChangeState("Default", true);
			}
		}

		public virtual void ChangeCameraState(string cameraState)
		{
			changeCameraState = true;
			customCameraState = cameraState;
		}

		public virtual void ResetCameraState()
		{
			changeCameraState = false;
			customCameraState = string.Empty;
		}

		public virtual void RotateWithCamera(Transform cameraTransform)
		{
			if (rotateToCameraFwdWhenMoving && cc.isStrafing && !cc.actions && !cc.lockMovement && !cc.continueRoll)
			{
				if (tpCamera != null && (bool)tpCamera.lockTarget)
				{
					cc.RotateToTarget(tpCamera.lockTarget);
				}
				else if (cc.input != Vector2.zero || rotateToCameraFwdWhenStanding)
				{
					cc.RotateWithAnotherTransform(cameraTransform);
				}
			}
		}

		public virtual void UpdateHUD()
		{
			if (hud == null)
			{
				if (!(vHUDController.instance != null))
				{
					return;
				}
				hud = vHUDController.instance;
				hud.Init(cc);
			}
			hud.UpdateHUD(cc);
		}
	}
}
