using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vCharacterController.vActions
{
	[vClassHeader("Trigger Generic Action", false, "icon_v2", false, "", iconName = "triggerIcon")]
	public class vTriggerGenericAction : vMonoBehaviour
	{
		public enum InputType
		{
			GetButtonDown = 0,
			GetDoubleButton = 1,
			GetButtonTimer = 2,
			AutoAction = 3
		}

		[Serializable]
		public class OnUpdateValue : UnityEvent<float>
		{
		}

		[vEditorToolbar("Input", false, "", false, false, order = 1)]
		[Tooltip("Input to make the action")]
		public GenericInput actionInput = new GenericInput("E", "A", "A");

		public InputType inputType;

		[Header("- GetButtonTimer Settings:")]
		[vHelpBox("How much time you have to press the button", vHelpBoxAttribute.MessageType.None)]
		public float buttonTimer = 3f;

		[vHelpBox("Needs a delay to work correctly", vHelpBoxAttribute.MessageType.None)]
		public float inputDelay = 0.1f;

		[vHelpBox("<b>TRUE: </b> The action starts as soon as you press the input, if you release before it finishes the buttomTimer counter it will stop the action and reset the counter. \n<b>FALSE: </b> it will perform the action/animation only after the buttonTimer is finished", vHelpBoxAttribute.MessageType.None)]
		public bool doActionWhilePressingButton = true;

		[Header("- DoubleButtonDown Settings:")]
		[vHelpBox("Time to press the button twice", vHelpBoxAttribute.MessageType.None)]
		public float doubleButtomTime = 0.25f;

		[vEditorToolbar("Trigger", false, "", false, false, order = 2)]
		public string actionTag = "Action";

		[vHelpBox("Disable this trigger OnStart", vHelpBoxAttribute.MessageType.None)]
		public bool disableOnStart;

		[vHelpBox("Disable the Player's Capsule Collider Collision, useful for animations with closer interactions", vHelpBoxAttribute.MessageType.None)]
		public bool disableCollision;

		[vHelpBox("Disable the Player's Rigidbody Gravity, useful for on air animations", vHelpBoxAttribute.MessageType.None)]
		public bool disableGravity;

		[vHelpBox("It will only use the trigger if the forward of the character is close to the forward of this transform", vHelpBoxAttribute.MessageType.None)]
		public bool activeFromForward;

		[vHelpBox("Rotate Character to the Forward Rotation of this Trigger", vHelpBoxAttribute.MessageType.None)]
		public bool useTriggerRotation;

		[vHelpBox("Destroy this Trigger after pressing the Input or AutoAction or finishing the Action", vHelpBoxAttribute.MessageType.None)]
		public bool destroyAfter;

		[vHideInInspector("destroyAfter", false)]
		public float destroyDelay;

		[vHelpBox("Change your CameraState to a Custom State while playing the animation", vHelpBoxAttribute.MessageType.None)]
		public string customCameraState;

		[vEditorToolbar("Animation", false, "", false, false, order = 2)]
		[vHelpBox("Trigger a Animation - Use the exactly same name of the AnimationState you want to trigger, don't forget to add a vAnimatorTag to your State", vHelpBoxAttribute.MessageType.None)]
		public string playAnimation;

		[vHelpBox("Check the Exit Time of your animation (if it doesn't loop) and insert here. \n\nFor example if your Exit Time is 0.8 and the Transition Duration is 0.2 you need to insert 0.5 or lower as the final value. \n\nAlways check with the Debug of the GenericAction if your animation is finishing correctly, otherwise the controller won't reset to the default physics and collision.", vHelpBoxAttribute.MessageType.Warning)]
		public float endExitTimeAnimation = 0.8f;

		[vHelpBox("Use a ActionState value to apply special conditions for your AnimatorController transitions", vHelpBoxAttribute.MessageType.None)]
		public int animatorActionState;

		[vHelpBox("Reset the ActionState parameter to 0 after playing the animation", vHelpBoxAttribute.MessageType.None)]
		public bool resetAnimatorActionState = true;

		[vHelpBox("Select the bone you want to use as reference to the Match Target", vHelpBoxAttribute.MessageType.None)]
		public AvatarTarget avatarTarget;

		[vHelpBox("Check what positions XYZ you want the matchTarget to work", vHelpBoxAttribute.MessageType.None)]
		[FormerlySerializedAs("matchTargetMask")]
		public Vector3 matchPos;

		[vHelpBox("Rotate Weight for your character to use the matchTarget rotation", vHelpBoxAttribute.MessageType.None)]
		[Range(0f, 1f)]
		public float matchRot;

		[vHelpBox("Use a empty transform as reference for the MatchTarget", vHelpBoxAttribute.MessageType.None)]
		public Transform matchTarget;

		[vHelpBox("Time of the animation to start the MatchTarget goes from 0 to 1", vHelpBoxAttribute.MessageType.None)]
		public float startMatchTarget;

		[vHelpBox("Time of the animation to end the MatchTarget goes from 0 to 1", vHelpBoxAttribute.MessageType.None)]
		public float endMatchTarget;

		[vEditorToolbar("Events", false, "", false, false, order = 3)]
		[Tooltip("Delay to run the OnDoAction Event")]
		[FormerlySerializedAs("onDoActionDelay")]
		public float onPressActionDelay;

		[Header("--- INPUT EVENTS ---")]
		[FormerlySerializedAs("OnDoAction")]
		public UnityEvent OnPressActionInput;

		public OnDoActionWithTarget onPressActionInputWithTarget;

		[Header("--- ONLY FOR GET BUTTON TIMER ---")]
		public UnityEvent OnCancelActionInput;

		public UnityEvent OnFinishActionInput;

		public OnUpdateValue OnUpdateButtonTimer;

		[Header("--- ANIMATION EVENTS ---")]
		public UnityEvent OnStartAnimation;

		public UnityEvent OnEndAnimation;

		[Header("--- PLAYER AND TRIGGER DETECTION ---")]
		public UnityEvent OnPlayerEnter;

		public UnityEvent OnPlayerStay;

		public UnityEvent OnPlayerExit;

		private float currentButtonTimer;

		internal Collider _collider;

		protected virtual void Start()
		{
			base.gameObject.tag = actionTag;
			base.gameObject.layer = LayerMask.NameToLayer("Triggers");
			_collider = GetComponent<Collider>();
			_collider.isTrigger = true;
			if (disableOnStart)
			{
				base.enabled = false;
			}
		}

		public virtual IEnumerator OnDoActionDelay(GameObject obj)
		{
			yield return new WaitForSeconds(onPressActionDelay);
			OnPressActionInput.Invoke();
			if ((bool)obj)
			{
				onPressActionInputWithTarget.Invoke(obj);
			}
		}

		public void UpdateButtonTimer(float value)
		{
			if (value != currentButtonTimer)
			{
				currentButtonTimer = value;
				OnUpdateButtonTimer.Invoke(value);
			}
		}
	}
}
