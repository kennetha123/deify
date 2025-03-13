using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	[vClassHeader("HEAD TRACK", true, "icon_v2", false, "")]
	public class vHeadTrack : vMonoBehaviour
	{
		[vEditorToolbar("Settings", false, "", false, false)]
		public float strafeHeadWeight = 0.8f;

		public float strafeBodyWeight = 0.3f;

		public float aimingHeadWeight = 0.8f;

		public float aimingBodyWeight = 0.8f;

		public float freeHeadWeight = 1f;

		public float freeBodyWeight = 0.4f;

		public float smooth = 12f;

		public Vector2 offsetSpine;

		public Vector2 offsetHead;

		public bool followCamera = true;

		[vHideInInspector("followCamera", false)]
		public bool awaysFollowCamera;

		public bool cancelTrackOutOfAngle = true;

		[vMinMax(minLimit = -180f, maxLimit = 180f)]
		public Vector2 horizontalAngleLimit = new Vector2(-90f, 90f);

		[vMinMax(minLimit = -90f, maxLimit = 90f)]
		public Vector2 verticalAngleLimit = new Vector2(-90f, 90f);

		[vHelpBox("vAnimatorTags in Animator States to ignore the HeadTrack", vHelpBoxAttribute.MessageType.None)]
		public List<string> animatorIgnoreTags = new List<string> { "Attack", "LockMovement", "CustomAction", "IsEquipping" };

		[vEditorToolbar("Bones", false, "", false, false)]
		[vHelpBox("Auto Find Bones using Humanoid", vHelpBoxAttribute.MessageType.None)]
		public bool autoFindBones = true;

		public Transform head;

		public List<Transform> spine = new List<Transform>();

		[vEditorToolbar("Detection", false, "", false, false)]
		public float updateTargetInteration = 1f;

		public float distanceToDetect = 10f;

		public LayerMask obstacleLayer = 1;

		[vHelpBox("Gameobjects Tags to detect", vHelpBoxAttribute.MessageType.None)]
		public List<string> tagsToDetect = new List<string> { "LookAt" };

		internal UnityEvent onInitUpdate = new UnityEvent();

		internal UnityEvent onFinishUpdate = new UnityEvent();

		internal Camera cameraMain;

		internal vLookTarget currentLookTarget;

		internal vLookTarget lastLookTarget;

		internal Vector3 currentLookPosition;

		internal List<vLookTarget> targetsInArea = new List<vLookTarget>();

		private float yRotation;

		private float xRotation;

		private float _currentHeadWeight;

		private float _currentbodyWeight;

		private Animator animator;

		private vAnimatorStateInfos animatorStateInfos;

		private float headHeight;

		private Transform simpleTarget;

		private Vector3 temporaryLookPoint;

		private float temporaryLookTime;

		private vHeadTrackSensor sensor;

		private float interation;

		private vICharacter vchar;

		private float yAngle;

		private float xAngle;

		private float _yAngle;

		private float _xAngle;

		private Vector3 headPoint
		{
			get
			{
				return base.transform.position + base.transform.up * headHeight;
			}
		}

		private bool lookConditions
		{
			get
			{
				if (!cameraMain)
				{
					cameraMain = Camera.main;
				}
				if ((!(head != null) || !followCamera || !(cameraMain != null)) && (followCamera || (!currentLookTarget && !simpleTarget)))
				{
					return temporaryLookTime > 0f;
				}
				return true;
			}
		}

		protected virtual void OnEnable()
		{
			if (animatorStateInfos != null)
			{
				animatorStateInfos.RegisterListener();
			}
		}

		private void Start()
		{
			if (!sensor)
			{
				GameObject gameObject = new GameObject("HeadTrackSensor");
				sensor = gameObject.AddComponent<vHeadTrackSensor>();
			}
			vchar = GetComponent<vICharacter>();
			sensor.headTrack = this;
			animator = GetComponentInChildren<Animator>();
			animatorStateInfos = new vAnimatorStateInfos(animator);
			animatorStateInfos.RegisterListener();
			if (autoFindBones)
			{
				head = animator.GetBoneTransform(HumanBodyBones.Head);
				if ((bool)head)
				{
					Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
					if ((bool)boneTransform)
					{
						Transform parent = head;
						for (int i = 0; i < 4; i++)
						{
							if (!parent.parent)
							{
								break;
							}
							if (!(parent.parent.gameObject != boneTransform.gameObject))
							{
								break;
							}
							spine.Add(parent.parent);
							parent = parent.parent;
						}
					}
				}
			}
			cameraMain = Camera.main;
			if ((bool)head)
			{
				headHeight = Vector3.Distance(base.transform.position, head.position);
				sensor.transform.position = head.transform.position;
			}
			else
			{
				headHeight = 1f;
				sensor.transform.position = base.transform.position;
			}
			if (spine.Count == 0)
			{
				Debug.Log("Headtrack Spines missing");
			}
			int layer = LayerMask.NameToLayer("HeadTrack");
			sensor.transform.parent = base.transform;
			sensor.gameObject.layer = layer;
			sensor.gameObject.tag = "Untagged";
			GetLookPoint();
		}

		public virtual void UpdateHeadTrack()
		{
			if (!(animator == null) && vchar != null && vchar.currentHealth > 0f && animator != null && animator.enabled)
			{
				onInitUpdate.Invoke();
				currentLookPosition = GetLookPoint();
				SetLookAtPosition(currentLookPosition, _currentHeadWeight, _currentbodyWeight);
				onFinishUpdate.Invoke();
			}
		}

		public virtual void SetLookAtPosition(Vector3 point, float headWeight, float spineWeight)
		{
			Vector3 vector = Quaternion.LookRotation(point - headPoint).eulerAngles - base.transform.eulerAngles;
			float b = NormalizeAngle(vector.y);
			float b2 = NormalizeAngle(vector.x);
			xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, b2, smooth * Time.fixedDeltaTime), verticalAngleLimit.x, verticalAngleLimit.y);
			yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, b, smooth * Time.fixedDeltaTime), horizontalAngleLimit.x, horizontalAngleLimit.y);
			float num = NormalizeAngle(xAngle + Quaternion.Euler(offsetSpine).eulerAngles.x);
			float num2 = NormalizeAngle(yAngle + Quaternion.Euler(offsetSpine).eulerAngles.y);
			foreach (Transform item in spine)
			{
				Quaternion quaternion = Quaternion.AngleAxis(num * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.right));
				Quaternion quaternion2 = Quaternion.AngleAxis(num2 * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.up));
				item.rotation *= quaternion * quaternion2;
			}
			if ((bool)head)
			{
				float num3 = NormalizeAngle(xAngle - num * spineWeight + Quaternion.Euler(offsetHead).eulerAngles.x);
				float num4 = NormalizeAngle(yAngle - num2 * spineWeight + Quaternion.Euler(offsetHead).eulerAngles.y);
				Quaternion quaternion3 = Quaternion.AngleAxis(num3 * headWeight, head.InverseTransformDirection(base.transform.right));
				Quaternion quaternion4 = Quaternion.AngleAxis(num4 * headWeight, head.InverseTransformDirection(base.transform.up));
				head.rotation *= quaternion3 * quaternion4;
			}
		}

		private Vector3 GetLookPoint()
		{
			int num = 100;
			if (lookConditions && !IgnoreHeadTrack())
			{
				Vector3 forward = base.transform.forward;
				if (temporaryLookTime <= 0f)
				{
					Vector3 vector = headPoint + base.transform.forward * num;
					if (followCamera)
					{
						vector = cameraMain.transform.position + cameraMain.transform.forward * num;
					}
					forward = vector - headPoint;
					if ((followCamera && !awaysFollowCamera) || !followCamera)
					{
						if (currentLookTarget != null && (currentLookTarget.ignoreHeadTrackAngle || TargetIsOnRange(currentLookTarget.lookPoint - headPoint)) && currentLookTarget.IsVisible(headPoint, obstacleLayer))
						{
							forward = currentLookTarget.lookPoint - headPoint;
							if (currentLookTarget != lastLookTarget)
							{
								currentLookTarget.EnterLook(this);
								lastLookTarget = currentLookTarget;
							}
						}
						else if (simpleTarget != null)
						{
							forward = simpleTarget.position - headPoint;
							if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
							{
								currentLookTarget.ExitLook(this);
								lastLookTarget = null;
							}
						}
						else if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
						{
							currentLookTarget.ExitLook(this);
							lastLookTarget = null;
						}
					}
				}
				else
				{
					forward = temporaryLookPoint - headPoint;
					temporaryLookTime -= Time.deltaTime;
					if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
					{
						currentLookTarget.ExitLook(this);
						lastLookTarget = null;
					}
				}
				Vector2 targetAngle = GetTargetAngle(forward);
				if (cancelTrackOutOfAngle && (lastLookTarget == null || !lastLookTarget.ignoreHeadTrackAngle))
				{
					if (TargetIsOnRange(forward))
					{
						if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
						{
							SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
						}
						else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
						{
							SmoothValues(aimingHeadWeight, aimingBodyWeight, targetAngle.x, targetAngle.y);
						}
						else
						{
							SmoothValues(freeHeadWeight, freeBodyWeight, targetAngle.x, targetAngle.y);
						}
					}
					else
					{
						SmoothValues();
					}
				}
				else if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
				{
					SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
				}
				else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
				{
					SmoothValues(aimingHeadWeight, aimingBodyWeight, targetAngle.x, targetAngle.y);
				}
				else
				{
					SmoothValues(freeHeadWeight, freeBodyWeight, targetAngle.x, targetAngle.y);
				}
				if (targetsInArea.Count > 1)
				{
					SortTargets();
				}
			}
			else
			{
				SmoothValues();
				if (targetsInArea.Count > 1)
				{
					SortTargets();
				}
			}
			Quaternion quaternion = Quaternion.AngleAxis(yRotation, base.transform.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(xRotation, base.transform.right);
			Vector3 vector2 = quaternion * quaternion2 * base.transform.forward;
			return headPoint + vector2 * num;
		}

		private Vector2 GetTargetAngle(Vector3 direction)
		{
			Vector3 eulerAngle = Quaternion.LookRotation(direction, base.transform.up).eulerAngles - base.transform.eulerAngles;
			return new Vector2(eulerAngle.NormalizeAngle().x, eulerAngle.NormalizeAngle().y);
		}

		private bool TargetIsOnRange(Vector3 direction)
		{
			Vector2 targetAngle = GetTargetAngle(direction);
			if (targetAngle.x >= verticalAngleLimit.x && targetAngle.x <= verticalAngleLimit.y && targetAngle.y >= horizontalAngleLimit.x)
			{
				return targetAngle.y <= horizontalAngleLimit.y;
			}
			return false;
		}

		public virtual void SetLookTarget(vLookTarget target, bool priority = false)
		{
			if (!targetsInArea.Contains(target))
			{
				targetsInArea.Add(target);
			}
			if (priority)
			{
				currentLookTarget = target;
			}
		}

		public virtual void SetLookTarget(Transform target)
		{
			simpleTarget = target;
		}

		public virtual void SetTemporaryLookPoint(Vector3 point, float time = 1f)
		{
			temporaryLookPoint = point;
			temporaryLookTime = time;
		}

		public virtual void RemoveLookTarget(vLookTarget target)
		{
			if (targetsInArea.Contains(target))
			{
				targetsInArea.Remove(target);
			}
			if (currentLookTarget == target)
			{
				currentLookTarget = null;
			}
		}

		public virtual void RemoveLookTarget(Transform target)
		{
			if (simpleTarget == target)
			{
				simpleTarget = null;
			}
		}

		private float NormalizeAngle(float angle)
		{
			if (angle < -180f)
			{
				return angle + 360f;
			}
			if (angle > 180f)
			{
				return angle - 360f;
			}
			return angle;
		}

		private void ResetValues()
		{
			_currentHeadWeight = 0f;
			_currentbodyWeight = 0f;
			yRotation = 0f;
			xRotation = 0f;
		}

		private void SmoothValues(float _headWeight = 0f, float _bodyWeight = 0f, float _x = 0f, float _y = 0f)
		{
			_currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, smooth * Time.deltaTime);
			_currentbodyWeight = Mathf.Lerp(_currentbodyWeight, _bodyWeight, smooth * Time.deltaTime);
			yRotation = Mathf.Lerp(yRotation, _y, smooth * Time.deltaTime);
			xRotation = Mathf.Lerp(xRotation, _x, smooth * Time.deltaTime);
			yRotation = Mathf.Clamp(yRotation, horizontalAngleLimit.x, horizontalAngleLimit.y);
			xRotation = Mathf.Clamp(xRotation, verticalAngleLimit.x, verticalAngleLimit.y);
		}

		private void SortTargets()
		{
			interation += Time.deltaTime;
			if (!(interation > updateTargetInteration))
			{
				return;
			}
			interation -= updateTargetInteration;
			if (targetsInArea == null || targetsInArea.Count < 2)
			{
				if (targetsInArea != null && targetsInArea.Count > 0)
				{
					currentLookTarget = targetsInArea[0];
				}
				return;
			}
			for (int num = targetsInArea.Count - 1; num >= 0; num--)
			{
				if (targetsInArea[num] == null)
				{
					targetsInArea.RemoveAt(num);
				}
			}
			targetsInArea.Sort((vLookTarget c1, vLookTarget c2) => Vector3.Distance(base.transform.position, (c1 != null) ? c1.transform.position : (Vector3.one * float.PositiveInfinity)).CompareTo(Vector3.Distance(base.transform.position, (c2 != null) ? c2.transform.position : (Vector3.one * float.PositiveInfinity))));
			if (targetsInArea.Count > 0)
			{
				currentLookTarget = targetsInArea[0];
			}
		}

		public virtual void OnDetect(Collider other)
		{
			if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponent<vLookTarget>() != null)
			{
				currentLookTarget = other.GetComponent<vLookTarget>();
				vHeadTrack componentInParent = other.GetComponentInParent<vHeadTrack>();
				if (!targetsInArea.Contains(currentLookTarget) && (componentInParent == null || componentInParent != this))
				{
					targetsInArea.Add(currentLookTarget);
					SortTargets();
					currentLookTarget = targetsInArea[0];
				}
			}
		}

		public virtual void OnLost(Collider other)
		{
			if (!tagsToDetect.Contains(other.gameObject.tag) || !(other.GetComponentInParent<vLookTarget>() != null))
			{
				return;
			}
			vLookTarget componentInParent = other.GetComponentInParent<vLookTarget>();
			if (targetsInArea.Contains(componentInParent))
			{
				targetsInArea.Remove(componentInParent);
				if (componentInParent == lastLookTarget)
				{
					componentInParent.ExitLook(this);
				}
			}
			SortTargets();
			if (targetsInArea.Count > 0)
			{
				currentLookTarget = targetsInArea[0];
			}
			else
			{
				currentLookTarget = null;
			}
		}

		public virtual bool IgnoreHeadTrack()
		{
			if (animatorIgnoreTags.Exists((string tag) => IsAnimatorTag(tag)))
			{
				return true;
			}
			return false;
		}

		public virtual bool IsAnimatorTag(string tag)
		{
			if (animator == null)
			{
				return false;
			}
			if (animatorStateInfos != null && animatorStateInfos.HasTag(tag))
			{
				return true;
			}
			return false;
		}
	}
}
