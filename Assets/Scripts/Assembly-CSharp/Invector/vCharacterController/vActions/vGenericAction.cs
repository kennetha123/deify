using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions
{
	[vClassHeader("GENERIC ACTION", "Use the vTriggerGenericAction to trigger a simple animation.", iconName = "triggerIcon")]
	public class vGenericAction : vActionListener
	{
		[Tooltip("Tag of the object you want to access")]
		public string actionTag = "Action";

		[Tooltip("Use root motion of the animation")]
		public bool useRootMotion = true;

		[Header("--- Debug Only ---")]
		public vTriggerGenericAction triggerAction;

		[Tooltip("Check this to enter the debug mode")]
		public bool debugMode;

		public bool canTriggerAction;

		public bool triggerActionOnce;

		public Camera mainCamera;

		public UnityEvent OnStartAction;

		public UnityEvent OnCancelAction;

		public UnityEvent OnEndAction;

		internal vThirdPersonInput tpInput;

		private float _currentInputDelay;

		private bool _playingAnimation;

		private Vector3 screenCenter;

		public Dictionary<Collider, vTriggerGenericAction> actions;

		private float timeInTrigger;

		private bool animationStarted;

		protected virtual bool inActionAnimation
		{
			get
			{
				if (!string.IsNullOrEmpty(triggerAction.playAnimation))
				{
					return tpInput.cc.baseLayerInfo.IsName(triggerAction.playAnimation);
				}
				return false;
			}
		}

		public virtual bool playingAnimation
		{
			get
			{
				if (triggerAction == null)
				{
					return _playingAnimation = false;
				}
				if (!_playingAnimation && inActionAnimation)
				{
					_playingAnimation = true;
					DisablePlayerGravityAndCollision();
				}
				else if (_playingAnimation && !inActionAnimation)
				{
					_playingAnimation = false;
				}
				return _playingAnimation;
			}
			protected set
			{
				_playingAnimation = true;
			}
		}

		public virtual bool actionConditions
		{
			get
			{
				if (!tpInput.cc.isJumping || !tpInput.cc.customAction || (canTriggerAction && !triggerActionOnce) || !playingAnimation)
				{
					return !tpInput.cc.animator.IsInTransition(0);
				}
				return false;
			}
		}

		private void Awake()
		{
			actionEnter = true;
			actionStay = true;
			actionExit = true;
			actions = new Dictionary<Collider, vTriggerGenericAction>();
		}

		protected override void Start()
		{
			base.Start();
			tpInput = GetComponent<vThirdPersonInput>();
			screenCenter = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			if (!mainCamera)
			{
				mainCamera = Camera.main;
			}
		}

		public override void OnActionEnter(Collider other)
		{
			if (other.gameObject.CompareTag(actionTag) && !actions.ContainsKey(other))
			{
				vTriggerGenericAction component = other.GetComponent<vTriggerGenericAction>();
				if ((bool)component && component.enabled)
				{
					actions.Add(other, component);
				}
			}
		}

		public override void OnActionExit(Collider other)
		{
			if (other.gameObject.CompareTag(actionTag) && actions.ContainsKey(other))
			{
				actions[other].OnPlayerExit.Invoke();
				triggerActionOnce = false;
				canTriggerAction = false;
				actions.Remove(other);
			}
		}

		public override void OnActionStay(Collider other)
		{
			if (other.gameObject.CompareTag(actionTag))
			{
				timeInTrigger = 0.5f;
			}
		}

		protected virtual void CheckForTriggerAction()
		{
			if (actions.Count == 0 && !triggerAction)
			{
				return;
			}
			vTriggerGenericAction nearAction = GetNearAction();
			if (!nearAction || !nearAction.enabled || !nearAction.gameObject.activeInHierarchy)
			{
				return;
			}
			float num = Vector3.Distance(base.transform.forward, nearAction.transform.forward);
			if (!nearAction.activeFromForward || num <= 0.8f)
			{
				if (!tpInput.cc.customAction && !canTriggerAction)
				{
					triggerAction = nearAction;
					canTriggerAction = true;
					triggerAction.OnPlayerEnter.Invoke();
				}
			}
			else if (canTriggerAction)
			{
				if (triggerAction != null)
				{
					triggerAction.OnPlayerExit.Invoke();
				}
				canTriggerAction = false;
			}
			TriggerActionInput();
		}

		private void Update()
		{
			if (!mainCamera)
			{
				mainCamera = Camera.main;
			}
			if (!mainCamera)
			{
				return;
			}
			AnimationBehaviour();
			CheckForTriggerAction();
			if (!base.doingAction)
			{
				if (timeInTrigger <= 0f)
				{
					actions.Clear();
					triggerAction = null;
					triggerActionOnce = false;
					canTriggerAction = false;
				}
				else
				{
					timeInTrigger -= Time.deltaTime;
				}
			}
		}

		protected virtual void TriggerActionInput()
		{
			if (triggerAction == null || !triggerAction.gameObject.activeInHierarchy || !canTriggerAction)
			{
				return;
			}
			if (triggerAction.inputType == vTriggerGenericAction.InputType.AutoAction && actionConditions)
			{
				TriggerAnimation();
			}
			else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetButtonDown && actionConditions)
			{
				if (triggerAction.actionInput.GetButtonDown())
				{
					TriggerAnimation();
				}
			}
			else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetDoubleButton && actionConditions)
			{
				if (triggerAction.actionInput.GetDoubleButtonDown(triggerAction.doubleButtomTime))
				{
					TriggerAnimation();
				}
			}
			else
			{
				if (triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer)
				{
					return;
				}
				if (_currentInputDelay <= 0f)
				{
					if (triggerAction.doActionWhilePressingButton)
					{
						bool upAfterPressed = false;
						float currentTimer = 0f;
						if (triggerAction.actionInput.GetButtonTimer(ref currentTimer, ref upAfterPressed, triggerAction.buttonTimer))
						{
							triggerAction.OnFinishActionInput.Invoke();
							ResetActionState();
							ResetTriggerSettings();
						}
						if ((bool)triggerAction && triggerAction.actionInput.inButtomTimer)
						{
							triggerAction.UpdateButtonTimer(currentTimer);
							TriggerAnimation();
						}
						if (upAfterPressed && (bool)triggerAction)
						{
							triggerAction.OnCancelActionInput.Invoke();
							_currentInputDelay = triggerAction.inputDelay;
							triggerAction.UpdateButtonTimer(0f);
							ResetActionState();
							ResetTriggerSettings();
						}
					}
					else
					{
						if (!triggerAction.actionInput.GetButtonTimer(triggerAction.buttonTimer))
						{
							return;
						}
						TriggerAnimation();
						if (playingAnimation)
						{
							if (debugMode)
							{
								Debug.Log("call OnFinishInput Event");
							}
							triggerAction.OnFinishActionInput.Invoke();
						}
					}
				}
				else
				{
					_currentInputDelay -= Time.deltaTime;
				}
			}
		}

		protected virtual void TriggerAnimation()
		{
			if (triggerActionOnce || playingAnimation)
			{
				return;
			}
			OnDoAction.Invoke(triggerAction);
			base.doingAction = true;
			if (debugMode)
			{
				Debug.Log("TriggerAnimation", base.gameObject);
			}
			if (triggerAction.animatorActionState != 0)
			{
				if (debugMode)
				{
					Debug.Log("Applied ActionState: " + triggerAction.animatorActionState, base.gameObject);
				}
				tpInput.cc.SetActionState(triggerAction.animatorActionState);
			}
			if (!string.IsNullOrEmpty(triggerAction.playAnimation))
			{
				playingAnimation = true;
				tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, 0.1f);
				if (!string.IsNullOrEmpty(triggerAction.customCameraState))
				{
					tpInput.ChangeCameraState(triggerAction.customCameraState);
				}
			}
			StartCoroutine(triggerAction.OnDoActionDelay(base.gameObject));
			triggerActionOnce = true;
			if (triggerAction.destroyAfter)
			{
				StartCoroutine(DestroyActionDelay(triggerAction));
			}
		}

		protected virtual void AnimationBehaviour()
		{
			if (playingAnimation)
			{
				if (!animationStarted)
				{
					animationStarted = true;
				}
				OnStartAction.Invoke();
				triggerAction.OnStartAnimation.Invoke();
				if (triggerAction.matchTarget != null)
				{
					if (debugMode)
					{
						Debug.Log("Match Target...");
					}
					tpInput.cc.MatchTarget(triggerAction.matchTarget.transform.position, triggerAction.matchTarget.transform.rotation, triggerAction.avatarTarget, new MatchTargetWeightMask(triggerAction.matchPos, triggerAction.matchRot), triggerAction.startMatchTarget, triggerAction.endMatchTarget);
				}
				if (triggerAction.useTriggerRotation)
				{
					if (debugMode)
					{
						Debug.Log("Rotate to Target...");
					}
					Vector3 euler = new Vector3(base.transform.eulerAngles.x, triggerAction.transform.eulerAngles.y, base.transform.eulerAngles.z);
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(euler), tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
				}
				if (triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer && tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= triggerAction.endExitTimeAnimation)
				{
					if (debugMode)
					{
						Debug.Log("Finish Animation");
					}
					triggerAction.OnEndAnimation.Invoke();
					ResetTriggerSettings();
				}
			}
			else if (base.doingAction && animationStarted && (!(triggerAction != null) || triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer))
			{
				if (debugMode)
				{
					Debug.Log("Force ResetTriggerSettings");
				}
				triggerAction.OnEndAnimation.Invoke();
				ResetTriggerSettings();
			}
		}

		public void ResetActionState()
		{
			if ((bool)triggerAction && triggerAction.resetAnimatorActionState)
			{
				tpInput.cc.SetActionState(0);
			}
		}

		public virtual void ResetTriggerSettings()
		{
			if (debugMode)
			{
				Debug.Log("Reset Trigger Settings");
			}
			animationStarted = false;
			EnablePlayerGravityAndCollision();
			ResetActionState();
			tpInput.ResetCameraState();
			canTriggerAction = false;
			triggerAction = null;
			triggerActionOnce = false;
			base.doingAction = false;
		}

		public virtual void DisablePlayerGravityAndCollision()
		{
			if ((bool)triggerAction && triggerAction.disableGravity)
			{
				if (debugMode)
				{
					Debug.Log("Disable Player's Gravity");
				}
				tpInput.cc._rigidbody.useGravity = false;
				tpInput.cc._rigidbody.linearVelocity = Vector3.zero;
			}
			if ((bool)triggerAction && triggerAction.disableCollision)
			{
				if (debugMode)
				{
					Debug.Log("Disable Player's Collision");
				}
				tpInput.cc._capsuleCollider.isTrigger = true;
			}
		}

		public virtual void EnablePlayerGravityAndCollision()
		{
			if (debugMode)
			{
				Debug.Log("Enable Player's Gravity");
			}
			tpInput.cc._rigidbody.useGravity = true;
			if (debugMode)
			{
				Debug.Log("Enable Player's Collision");
			}
			tpInput.cc._capsuleCollider.isTrigger = false;
		}

		public virtual IEnumerator DestroyActionDelay(vTriggerGenericAction triggerAction)
		{
			yield return new WaitForSeconds(triggerAction.destroyDelay);
			OnEndAction.Invoke();
			ResetTriggerSettings();
			Object.Destroy(triggerAction.gameObject);
		}

		protected vTriggerGenericAction GetNearAction()
		{
			float num = float.PositiveInfinity;
			vTriggerGenericAction vTriggerGenericAction2 = null;
			foreach (Collider key in actions.Keys)
			{
				if ((bool)key)
				{
					Vector3 vector = (mainCamera ? mainCamera.WorldToScreenPoint(key.transform.position) : screenCenter);
					if ((bool)mainCamera)
					{
						if ((vector - screenCenter).magnitude < num)
						{
							num = (vector - screenCenter).magnitude;
							if ((bool)vTriggerGenericAction2 && vTriggerGenericAction2 != actions[key])
							{
								vTriggerGenericAction2.OnPlayerExit.Invoke();
							}
							vTriggerGenericAction2 = actions[key];
						}
						else
						{
							actions[key].OnPlayerExit.Invoke();
						}
					}
					else if (!vTriggerGenericAction2)
					{
						vTriggerGenericAction2 = actions[key];
					}
					else
					{
						actions[key].OnPlayerExit.Invoke();
					}
					continue;
				}
				actions.Remove(key);
				return null;
			}
			return vTriggerGenericAction2;
		}
	}
}
