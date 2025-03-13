using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions
{
	[vClassHeader("Ladder Action", "Use the vTriggerLadderAction on your ladder mesh.", iconName = "ladderIcon")]
	public class vLadderAction : vActionListener
	{
		[Tooltip("Tag of the object you want to access")]
		public string actionTag = "LadderTrigger";

		[Tooltip("Input to up/down the ladder")]
		public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");

		[Tooltip("Input to enter the ladder")]
		public GenericInput enterInput = new GenericInput("E", "A", "A");

		[Tooltip("Input to exit the ladder")]
		public GenericInput exitInput = new GenericInput("Space", "B", "B");

		public bool debugMode;

		public UnityEvent OnEnterLadder;

		public UnityEvent OnExitLadder;

		protected vThirdPersonInput tpInput;

		protected vTriggerLadderAction ladderAction;

		protected vTriggerLadderAction ladderActionTemp;

		protected float speed;

		protected bool isUsingLadder;

		protected bool isExitingLadder;

		protected bool triggerEnterOnce;

		protected bool triggerExitOnce;

		private void Awake()
		{
			actionStay = true;
			actionExit = true;
		}

		private void OnEnable()
		{
			tpInput = GetComponent<vThirdPersonInput>();
		}

		private void Update()
		{
			AutoEnterLadder();
			EnterLadderInput();
			ExitLadderInput();
		}

		private void OnAnimatorMove()
		{
			if (isUsingLadder)
			{
				UseLadder();
				if (!tpInput.cc.customAction)
				{
					base.transform.rotation = tpInput.cc.animator.rootRotation;
				}
				base.transform.position = tpInput.cc.animator.rootPosition;
			}
		}

		private void EnterLadderInput()
		{
			if (!(ladderAction == null) && !tpInput.cc.customAction && !tpInput.cc.isJumping && tpInput.cc.isGrounded && enterInput.GetButtonDown() && !isUsingLadder && !ladderAction.autoAction)
			{
				TriggerEnterLadder();
			}
		}

		private void TriggerEnterLadder()
		{
			if (debugMode)
			{
				Debug.Log("Enter Ladder");
			}
			OnEnterLadder.Invoke();
			triggerEnterOnce = true;
			isUsingLadder = true;
			tpInput.cc.animator.SetInteger("ActionState", 1);
			tpInput.enabled = false;
			tpInput.cc.enabled = false;
			tpInput.cc.DisableGravityAndCollision();
			tpInput.cc._rigidbody.linearVelocity = Vector3.zero;
			tpInput.cc.isGrounded = false;
			tpInput.cc.animator.SetBool("IsGrounded", false);
			ladderAction.OnDoAction.Invoke();
			ladderActionTemp = ladderAction;
			if (!string.IsNullOrEmpty(ladderAction.playAnimation))
			{
				tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.playAnimation, 0.1f);
			}
		}

		private void UseLadder()
		{
			tpInput.cc.LayerControl();
			tpInput.cc.ActionsControl();
			tpInput.CameraInput();
			tpInput.cc.input.y = verticallInput.GetAxis();
			speed = Mathf.Clamp(tpInput.cc.input.y, -1f, 1f);
			tpInput.cc.animator.SetFloat("InputVertical", speed, 0.25f, Time.deltaTime);
			if (tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
			{
				if (ladderActionTemp != null)
				{
					ladderActionTemp.OnPlayerExit.Invoke();
				}
				if (ladderActionTemp.useTriggerRotation)
				{
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, ladderActionTemp.matchTarget.transform.rotation, tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
				}
				if (ladderActionTemp.matchTarget != null)
				{
					if (debugMode)
					{
						Debug.Log("Match Target...");
					}
					tpInput.cc.MatchTarget(ladderActionTemp.matchTarget.transform.position, ladderActionTemp.matchTarget.transform.rotation, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 0f), ladderActionTemp.startMatchTarget, ladderActionTemp.endMatchTarget);
				}
			}
			isExitingLadder = tpInput.cc.baseLayerInfo.IsName("ExitLadderTop") || tpInput.cc.baseLayerInfo.IsName("ExitLadderBottom");
			if (isExitingLadder && tpInput.cc.baseLayerInfo.normalizedTime >= 0.8f)
			{
				ResetPlayerSettings();
			}
		}

		private void ExitLadderInput()
		{
			if (!isUsingLadder || tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
			{
				return;
			}
			if (ladderAction == null)
			{
				if (tpInput.cc.baseLayerInfo.IsName("ClimbLadder") && exitInput.GetButtonDown())
				{
					if (debugMode)
					{
						Debug.Log("Quick Exit");
					}
					ResetPlayerSettings();
				}
				return;
			}
			string exitAnimation = ladderAction.exitAnimation;
			if (exitAnimation == "ExitLadderBottom")
			{
				if (exitInput.GetButtonDown() || (speed <= -0.05f && !triggerExitOnce))
				{
					if (debugMode)
					{
						Debug.Log("Exit Bottom");
					}
					triggerExitOnce = true;
					tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.exitAnimation, 0.1f);
				}
			}
			else if (exitAnimation == "ExitLadderTop" && tpInput.cc.baseLayerInfo.IsName("ClimbLadder") && speed >= 0.05f && !triggerExitOnce && !tpInput.cc.animator.IsInTransition(0))
			{
				if (debugMode)
				{
					Debug.Log("Exit Top");
				}
				triggerExitOnce = true;
				tpInput.cc.animator.CrossFadeInFixedTime(ladderAction.exitAnimation, 0.1f);
			}
		}

		private void AutoEnterLadder()
		{
			if (!(ladderAction == null) && ladderAction.autoAction && !tpInput.cc.customAction && !isUsingLadder && !tpInput.cc.animator.IsInTransition(0) && ladderAction.autoAction && tpInput.cc.input != Vector2.zero && !tpInput.cc.actions)
			{
				Vector3 vector = Camera.main.transform.TransformDirection(new Vector3(tpInput.cc.input.x, 0f, tpInput.cc.input.y));
				vector.y = 0f;
				if (Vector3.Distance(vector.normalized, ladderAction.transform.forward) < 0.8f)
				{
					TriggerEnterLadder();
				}
			}
		}

		private void ResetPlayerSettings()
		{
			if (debugMode)
			{
				Debug.Log("Reset Player Settings");
			}
			speed = 0f;
			ladderAction = null;
			isUsingLadder = false;
			OnExitLadder.Invoke();
			triggerExitOnce = false;
			triggerEnterOnce = false;
			tpInput.cc._capsuleCollider.isTrigger = false;
			tpInput.cc._rigidbody.useGravity = true;
			tpInput.cc.animator.SetInteger("ActionState", 0);
			tpInput.cc.enabled = true;
			tpInput.enabled = true;
			tpInput.cc.gameObject.transform.eulerAngles = new Vector3(0f, tpInput.cc.gameObject.transform.localEulerAngles.y, 0f);
		}

		public override void OnActionStay(Collider other)
		{
			if (other.gameObject.CompareTag(actionTag))
			{
				CheckForTriggerAction(other);
			}
		}

		public override void OnActionExit(Collider other)
		{
			if (other.gameObject.CompareTag(actionTag))
			{
				if (ladderAction != null)
				{
					ladderAction.OnPlayerExit.Invoke();
				}
				ladderAction = null;
			}
		}

		private void CheckForTriggerAction(Collider other)
		{
			vTriggerLadderAction component = other.GetComponent<vTriggerLadderAction>();
			if (!component)
			{
				return;
			}
			float num = Vector3.Distance(base.transform.forward, component.transform.forward);
			if (isUsingLadder && component != null)
			{
				ladderAction = component;
				return;
			}
			if (num <= 0.8f && !isUsingLadder)
			{
				ladderAction = component;
				ladderAction.OnPlayerEnter.Invoke();
				return;
			}
			if (ladderAction != null)
			{
				ladderAction.OnPlayerExit.Invoke();
			}
			ladderAction = null;
		}
	}
}
