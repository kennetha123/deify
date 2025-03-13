using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
	[RequireComponent(typeof(LineRenderer))]
	[vClassHeader("THROW MANAGER", true, "icon_v2", false, "")]
	public class vThrowManager : vMonoBehaviour
	{
		public enum CameraStyle
		{
			ThirdPerson = 0,
			TopDown = 1,
			SideScroll = 2
		}

		[Serializable]
		public class ThrowObject
		{
			private Rigidbody objectToThrow;

			private int id;

			private int count;
		}

		public CameraStyle cameraStyle;

		public GenericInput throwInput = new GenericInput("Mouse0", "RB", "RB");

		public GenericInput aimThrowInput = new GenericInput("G", "LB", "LB");

		public Transform throwStartPoint;

		public GameObject throwEnd;

		public Rigidbody objectToThrow;

		public LayerMask obstacles = 1;

		public float throwMaxForce = 15f;

		public float throwDelayTime = 0.25f;

		public float lineStepPerTime = 0.1f;

		public float lineMaxTime = 10f;

		public float exitStrafeModeDelay = 0.5f;

		public string throwAnimation = "ThrowObject";

		public string holdingAnimation = "HoldingObject";

		public string cancelAnimation = "CancelThrow";

		public int maxThrowObjects = 6;

		public int currentThrowObject;

		public bool debug;

		public UnityEvent onEnableAim;

		public UnityEvent onCancelAim;

		public UnityEvent onThrowObject;

		public UnityEvent onCollectObject;

		private int vertexCount;

		private bool isDrawing;

		private bool isAiming;

		private bool inThrow;

		private bool isThrowInput;

		private Transform rightUpperArm;

		private LineRenderer lineRenderer;

		private Quaternion upperArmRotation;

		private Animator animator;

		private vThrowUI _ui;

		private vThirdPersonInput tpInput;

		private RaycastHit hit;

		public vThrowUI ui
		{
			get
			{
				if (!_ui)
				{
					_ui = UnityEngine.Object.FindObjectOfType<vThrowUI>();
					if ((bool)_ui)
					{
						_ui.UpdateCount(this);
					}
				}
				return _ui;
			}
		}

		private Vector3 thirdPersonAimPoint
		{
			get
			{
				return throwStartPoint.position + tpInput.cameraMain.transform.forward * throwMaxForce;
			}
		}

		private Vector3 topdownAimPoint
		{
			get
			{
				Vector3 result = vMousePositionHandler.Instance.WorldMousePosition(obstacles);
				result.y = base.transform.position.y;
				return result;
			}
		}

		private Vector3 sideScrollAimPoint
		{
			get
			{
				Vector3 position = base.transform.InverseTransformPoint(vMousePositionHandler.Instance.WorldMousePosition(obstacles));
				position.x = 0f;
				return base.transform.TransformPoint(position);
			}
		}

		private Vector3 StartVelocity
		{
			get
			{
				float value = Vector3.Distance(base.transform.position, aimPoint);
				Debug.DrawLine(base.transform.position, aimPoint);
				RaycastHit hitInfo;
				if (cameraStyle == CameraStyle.ThirdPerson && Physics.Raycast(throwStartPoint.position, aimDirection.normalized, out hitInfo, (int)obstacles))
				{
					value = hitInfo.distance;
				}
				if (cameraStyle != CameraStyle.SideScroll)
				{
					float num = Mathf.Clamp(value, 0f, throwMaxForce);
					return Quaternion.AngleAxis(Quaternion.LookRotation(aimDirection.normalized, Vector3.up).eulerAngles.NormalizeAngle().x, base.transform.right) * base.transform.forward * num;
				}
				float num2 = Mathf.Clamp(value, 0f, throwMaxForce);
				return aimDirection.normalized * num2;
			}
		}

		public virtual Vector3 aimPoint
		{
			get
			{
				switch (cameraStyle)
				{
				case CameraStyle.ThirdPerson:
					return thirdPersonAimPoint;
				case CameraStyle.TopDown:
					return topdownAimPoint;
				case CameraStyle.SideScroll:
					return sideScrollAimPoint;
				default:
					return throwStartPoint.position + tpInput.cameraMain.transform.forward * throwMaxForce;
				}
			}
		}

		public virtual Vector3 aimDirection
		{
			get
			{
				return aimPoint - rightUpperArm.position;
			}
		}

		private void Start()
		{
			if (ui != null)
			{
				ui.UpdateCount(this);
			}
			tpInput = GetComponentInParent<vThirdPersonInput>();
			lineRenderer = GetComponent<LineRenderer>();
			if ((bool)lineRenderer)
			{
				lineRenderer.useWorldSpace = true;
			}
			animator = GetComponentInParent<Animator>();
			rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		}

		private void Update()
		{
			if (!isAiming)
			{
				return;
			}
			Vector3 direction = aimDirection;
			direction.y = 0f;
			if (cameraStyle != CameraStyle.SideScroll)
			{
				tpInput.cc.RotateToDirection(direction);
				return;
			}
			Vector3 vector = (Quaternion.LookRotation(aimDirection).eulerAngles - base.transform.rotation.eulerAngles).NormalizeAngle();
			if (vector.y > 150f || vector.y < -150f)
			{
				tpInput.cc.RotateToDirection(-base.transform.forward, true);
			}
		}

		private void LateUpdate()
		{
			if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction)
			{
				isAiming = false;
				inThrow = false;
				isThrowInput = false;
			}
			else
			{
				UpdateThrow();
				UpdateInput();
			}
		}

		private void UpdateInput()
		{
			if (aimThrowInput.GetButtonDown() && !isAiming && !inThrow)
			{
				isAiming = true;
				tpInput.cc.lockInStrafe = true;
				animator.CrossFadeInFixedTime(holdingAnimation, 0.2f);
				onEnableAim.Invoke();
			}
			if (aimThrowInput.GetButtonUp())
			{
				isAiming = false;
				tpInput.cc.lockInStrafe = false;
				animator.CrossFadeInFixedTime(cancelAnimation, 0.2f);
				onCancelAim.Invoke();
			}
			if (throwInput.GetButtonDown() && isAiming && !inThrow)
			{
				isAiming = false;
				isThrowInput = true;
			}
		}

		private void LaunchObject(Rigidbody projectily)
		{
			projectily.AddForce(StartVelocity, ForceMode.VelocityChange);
		}

		private void UpdateThrow()
		{
			if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction)
			{
				isAiming = false;
				inThrow = false;
				isThrowInput = false;
				if ((bool)lineRenderer && lineRenderer.enabled)
				{
					lineRenderer.enabled = false;
				}
				if ((bool)throwEnd && throwEnd.activeSelf)
				{
					throwEnd.SetActive(false);
				}
				return;
			}
			if (isAiming)
			{
				DrawTrajectory();
			}
			else
			{
				if ((bool)lineRenderer && lineRenderer.enabled)
				{
					lineRenderer.enabled = false;
				}
				if ((bool)throwEnd && throwEnd.activeSelf)
				{
					throwEnd.SetActive(false);
				}
			}
			if (isThrowInput)
			{
				inThrow = true;
				isThrowInput = false;
				animator.CrossFadeInFixedTime(throwAnimation, 0.2f);
				currentThrowObject--;
				StartCoroutine(Launch());
			}
		}

		private void DrawTrajectory()
		{
			List<Vector3> trajectoryPoints = GetTrajectoryPoints(throwStartPoint.position, StartVelocity, lineStepPerTime, lineMaxTime);
			if ((bool)lineRenderer)
			{
				if (!lineRenderer.enabled)
				{
					lineRenderer.enabled = true;
				}
				lineRenderer.positionCount = trajectoryPoints.Count;
				lineRenderer.SetPositions(trajectoryPoints.ToArray());
			}
			if ((bool)throwEnd)
			{
				if (!throwEnd.activeSelf)
				{
					throwEnd.SetActive(true);
				}
				if (trajectoryPoints.Count > 1)
				{
					throwEnd.transform.position = trajectoryPoints[trajectoryPoints.Count - 1];
				}
			}
		}

		private IEnumerator Launch()
		{
			yield return new WaitForSeconds(throwDelayTime);
			Rigidbody obj = UnityEngine.Object.Instantiate(objectToThrow, throwStartPoint.position, throwStartPoint.rotation);
			obj.isKinematic = false;
			LaunchObject(obj);
			if ((bool)ui)
			{
				ui.UpdateCount(this);
			}
			onThrowObject.Invoke();
			yield return new WaitForSeconds(2f * lineStepPerTime);
			Collider component = obj.GetComponent<Collider>();
			if ((bool)component)
			{
				component.isTrigger = false;
			}
			inThrow = false;
			if (currentThrowObject <= 0)
			{
				objectToThrow = null;
			}
			yield return new WaitForSeconds(exitStrafeModeDelay);
			tpInput.cc.lockInStrafe = false;
			tpInput.cc.isStrafing = false;
		}

		private Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
		{
			return start + startVelocity * time + Physics.gravity * time * time * 0.5f;
		}

		private List<Vector3> GetTrajectoryPoints(Vector3 start, Vector3 startVelocity, float timestep, float maxTime)
		{
			Vector3 vector = start;
			List<Vector3> list = new List<Vector3>();
			list.Add(vector);
			int num = 1;
			while (true)
			{
				float num2 = timestep * (float)num;
				if (num2 > maxTime)
				{
					break;
				}
				Vector3 vector2 = PlotTrajectoryAtTime(start, startVelocity, num2);
				RaycastHit hitInfo;
				if (Physics.Linecast(vector, vector2, out hitInfo, obstacles))
				{
					list.Add(hitInfo.point);
					break;
				}
				if (debug)
				{
					Debug.DrawLine(vector, vector2, Color.red);
				}
				list.Add(vector2);
				vector = vector2;
				num++;
			}
			return list;
		}

		public virtual void SetAmount(int value)
		{
			currentThrowObject += value;
			if ((bool)ui)
			{
				ui.UpdateCount(this);
			}
			onCollectObject.Invoke();
		}
	}
}
