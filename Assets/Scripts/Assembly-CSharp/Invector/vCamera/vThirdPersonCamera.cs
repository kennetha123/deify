using System.Collections;
using LastBoss.Character;
using UnityEngine;

namespace Invector.vCamera
{
	public class vThirdPersonCamera : MonoBehaviour
	{
		private static vThirdPersonCamera _instance;

		public Transform target;

		[Tooltip("Lerp speed between Camera States")]
		public float smoothBetweenState = 6f;

		public float smoothCameraRotation = 6f;

		public float scrollSpeed = 10f;

		[Tooltip("What layer will be culled")]
		public LayerMask cullingLayer = 1;

		[Tooltip("Change this value If the camera pass through the wall")]
		public float clipPlaneMargin;

		public float checkHeightRadius;

		public bool showGizmos;

		public bool startUsingTargetRotation = true;

		public bool startSmooth;

		[Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
		public bool lockCamera;

		public Vector2 offsetMouse;

		[HideInInspector]
		public int indexList;

		[HideInInspector]
		public int indexLookPoint;

		[HideInInspector]
		public float offSetPlayerPivot;

		[HideInInspector]
		public float distance = 5f;

		[HideInInspector]
		public string currentStateName;

		[HideInInspector]
		public Transform currentTarget;

		[HideInInspector]
		public vThirdPersonCameraState currentState;

		[HideInInspector]
		public vThirdPersonCameraListData CameraStateList;

		[HideInInspector]
		public Transform lockTarget;

		[HideInInspector]
		public Vector2 movementSpeed;

		[HideInInspector]
		public vThirdPersonCameraState lerpState;

		private Transform targetLookAt;

		private Vector3 currentTargetPos;

		private Vector3 lookPoint;

		private Vector3 current_cPos;

		private Vector3 desired_cPos;

		private Vector3 lookTargetAdjust;

		private Camera targetCamera;

		private float mouseY;

		private float mouseX;

		private float currentHeight;

		private float currentZoom;

		private float cullingHeight;

		private float cullingDistance;

		private float switchRight;

		private float currentSwitchRight;

		private float heightOffset;

		private bool isInit;

		private bool useSmooth;

		private bool isNewTarget;

		private bool firstStateIsInit;

		private Quaternion fixedRotation;

		private RevengeMode revenge;

		[HideInInspector]
		public bool isSpiritMode;

		[HideInInspector]
		public bool isSpiritDone;

		private Vector3 cameraVelocityDamp;

		private bool firstUpdated;

		public static vThirdPersonCamera instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Object.FindObjectOfType<vThirdPersonCamera>();
				}
				return _instance;
			}
		}

		public float CullingDistance
		{
			get
			{
				return cullingDistance;
			}
		}

		private bool isValidFixedPoint
		{
			get
			{
				if (currentState.lookPoints != null && currentState.cameraMode.Equals(TPCameraMode.FixedPoint))
				{
					if (indexLookPoint >= currentState.lookPoints.Count)
					{
						return currentState.lookPoints.Count > 0;
					}
					return true;
				}
				return false;
			}
		}

		private void OnDrawGizmos()
		{
			if (showGizmos && (bool)currentTarget)
			{
				Vector3 vector = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
				Gizmos.DrawWireSphere(vector + Vector3.up * cullingHeight, checkHeightRadius);
				Gizmos.DrawLine(vector, vector + Vector3.up * cullingHeight);
			}
		}

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			revenge = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>();
			if (target == null)
			{
				target = GameObject.FindGameObjectWithTag("Player").transform;
			}
			firstUpdated = true;
			useSmooth = true;
			if (!targetLookAt)
			{
				targetLookAt = new GameObject("targetLookAt").transform;
			}
			targetLookAt.rotation = base.transform.rotation;
			targetLookAt.hideFlags = HideFlags.HideInHierarchy;
			if (startSmooth)
			{
				distance = Vector3.Distance(targetLookAt.position, base.transform.position);
			}
			if (!targetCamera)
			{
				targetCamera = Camera.main;
			}
			currentTarget = target;
			switchRight = 1f;
			currentSwitchRight = 1f;
			if (startUsingTargetRotation)
			{
				base.transform.rotation = currentTarget.rotation;
				mouseY = currentTarget.root.eulerAngles.x;
				mouseX = currentTarget.root.eulerAngles.y;
			}
			else
			{
				mouseY = base.transform.root.eulerAngles.x;
				mouseX = base.transform.root.eulerAngles.y;
			}
			ChangeState("Default", startSmooth);
			currentZoom = currentState.defaultDistance;
			currentHeight = currentState.height;
			currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z) + currentTarget.transform.up * lerpState.height;
			targetLookAt.position = currentTargetPos;
			isInit = true;
		}

		private void FixedUpdate()
		{
			if (revenge.isSpiritMode && !isSpiritMode)
			{
				isSpiritMode = true;
				target = GameObject.FindGameObjectWithTag("Spirit").transform;
				Init();
			}
			if (!revenge.isSpiritMode && revenge.isRevengeMode && !isSpiritDone)
			{
				isSpiritDone = true;
				target = GameObject.FindGameObjectWithTag("Player").transform;
				Init();
			}
			if (target == null)
			{
				target = GameObject.FindGameObjectWithTag("Player").transform;
				Init();
			}
			if (!(target == null) && !(targetLookAt == null) && currentState != null && lerpState != null && isInit)
			{
				switch (currentState.cameraMode)
				{
				case TPCameraMode.FreeDirectional:
					CameraMovement();
					break;
				case TPCameraMode.FixedAngle:
					CameraMovement();
					break;
				case TPCameraMode.FixedPoint:
					CameraFixed();
					break;
				}
			}
		}

		public void SetLockTarget(Transform _lockTarget, float heightOffset)
		{
			if (!(lockTarget != null) || !(lockTarget == _lockTarget))
			{
				isNewTarget = _lockTarget != lockTarget;
				lockTarget = _lockTarget;
				this.heightOffset = heightOffset;
			}
		}

		public void RemoveLockTarget()
		{
			lockTarget = null;
		}

		public void SetTarget(Transform newTarget)
		{
			currentTarget = (newTarget ? newTarget : target);
		}

		public void SetMainTarget(Transform newTarget)
		{
			target = newTarget;
			currentTarget = newTarget;
			if (!isInit)
			{
				Init();
			}
		}

		public Ray ScreenPointToRay(Vector3 Point)
		{
			return GetComponent<Camera>().ScreenPointToRay(Point);
		}

		public void ChangeState(string stateName)
		{
			ChangeState(stateName, true);
		}

		public void ChangeState(string stateName, bool hasSmooth)
		{
			if ((currentState != null && currentState.Name.Equals(stateName)) || (!isInit && firstStateIsInit))
			{
				if (firstStateIsInit)
				{
					useSmooth = hasSmooth;
				}
				return;
			}
			useSmooth = ((!firstStateIsInit) ? startSmooth : hasSmooth);
			vThirdPersonCameraState vThirdPersonCameraState = ((CameraStateList != null) ? CameraStateList.tpCameraStates.Find((vThirdPersonCameraState obj) => obj.Name.Equals(stateName)) : new vThirdPersonCameraState("Default"));
			if (vThirdPersonCameraState != null)
			{
				currentStateName = stateName;
				currentState.cameraMode = vThirdPersonCameraState.cameraMode;
				lerpState = vThirdPersonCameraState;
				if (!firstStateIsInit)
				{
					currentState.defaultDistance = Vector3.Distance(targetLookAt.position, base.transform.position);
					currentState.forward = lerpState.forward;
					currentState.height = vThirdPersonCameraState.height;
					currentState.fov = vThirdPersonCameraState.fov;
					if (useSmooth)
					{
						StartCoroutine(ResetFirstState());
					}
					else
					{
						distance = lerpState.defaultDistance;
						firstStateIsInit = true;
					}
				}
				if (currentState != null && !useSmooth)
				{
					currentState.CopyState(vThirdPersonCameraState);
				}
			}
			else if (CameraStateList != null && CameraStateList.tpCameraStates.Count > 0)
			{
				vThirdPersonCameraState = CameraStateList.tpCameraStates[0];
				currentStateName = vThirdPersonCameraState.Name;
				currentState.cameraMode = vThirdPersonCameraState.cameraMode;
				lerpState = vThirdPersonCameraState;
				if (currentState != null && !useSmooth)
				{
					currentState.CopyState(vThirdPersonCameraState);
				}
			}
			if (currentState == null)
			{
				currentState = new vThirdPersonCameraState("Null");
				currentStateName = currentState.Name;
			}
			if (CameraStateList != null)
			{
				indexList = CameraStateList.tpCameraStates.IndexOf(vThirdPersonCameraState);
			}
			currentZoom = vThirdPersonCameraState.defaultDistance;
			if (currentState.cameraMode == TPCameraMode.FixedAngle)
			{
				mouseX = currentState.fixedAngle.x;
				mouseY = currentState.fixedAngle.y;
			}
			currentState.fixedAngle = new Vector3(mouseX, mouseY);
			indexLookPoint = 0;
			if (!isInit)
			{
				CameraMovement(true);
			}
		}

		public void ChangeState(string stateName, string pointName, bool hasSmooth)
		{
			useSmooth = hasSmooth;
			if (!currentState.Name.Equals(stateName))
			{
				vThirdPersonCameraState vThirdPersonCameraState = CameraStateList.tpCameraStates.Find((vThirdPersonCameraState obj) => obj.Name.Equals(stateName));
				if (vThirdPersonCameraState != null)
				{
					currentStateName = stateName;
					currentState.cameraMode = vThirdPersonCameraState.cameraMode;
					lerpState = vThirdPersonCameraState;
					if (currentState != null && !hasSmooth)
					{
						currentState.CopyState(vThirdPersonCameraState);
					}
				}
				else if (CameraStateList.tpCameraStates.Count > 0)
				{
					vThirdPersonCameraState = CameraStateList.tpCameraStates[0];
					currentStateName = vThirdPersonCameraState.Name;
					currentState.cameraMode = vThirdPersonCameraState.cameraMode;
					lerpState = vThirdPersonCameraState;
					if (currentState != null && !hasSmooth)
					{
						currentState.CopyState(vThirdPersonCameraState);
					}
				}
				if (currentState == null)
				{
					currentState = new vThirdPersonCameraState("Null");
					currentStateName = currentState.Name;
				}
				indexList = CameraStateList.tpCameraStates.IndexOf(vThirdPersonCameraState);
				currentZoom = vThirdPersonCameraState.defaultDistance;
				currentState.fixedAngle = new Vector3(mouseX, mouseY);
				indexLookPoint = 0;
			}
			if (currentState.cameraMode == TPCameraMode.FixedPoint)
			{
				LookPoint lookPoint = currentState.lookPoints.Find((LookPoint obj) => obj.pointName.Equals(pointName));
				if (lookPoint != null)
				{
					indexLookPoint = currentState.lookPoints.IndexOf(lookPoint);
				}
				else
				{
					indexLookPoint = 0;
				}
			}
		}

		private IEnumerator ResetFirstState()
		{
			yield return new WaitForEndOfFrame();
			firstStateIsInit = true;
		}

		public void ChangePoint(string pointName)
		{
			if (currentState != null && currentState.cameraMode == TPCameraMode.FixedPoint && currentState.lookPoints != null)
			{
				LookPoint lookPoint = currentState.lookPoints.Find((LookPoint obj) => obj.pointName.Equals(pointName));
				if (lookPoint != null)
				{
					indexLookPoint = currentState.lookPoints.IndexOf(lookPoint);
				}
				else
				{
					indexLookPoint = 0;
				}
			}
		}

		public void Zoom(float scroolValue)
		{
			currentZoom -= scroolValue * scrollSpeed;
		}

		public void RotateCamera(float x, float y)
		{
			if (currentState.cameraMode.Equals(TPCameraMode.FixedPoint) || !isInit)
			{
				return;
			}
			if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
			{
				if (!lockTarget)
				{
					mouseX += x * currentState.xMouseSensitivity;
					mouseY -= y * currentState.yMouseSensitivity;
					movementSpeed.x = x;
					movementSpeed.y = 0f - y;
					if (!lockCamera)
					{
						mouseY = vExtensions.ClampAngle(mouseY, lerpState.yMinLimit, lerpState.yMaxLimit);
						mouseX = vExtensions.ClampAngle(mouseX, lerpState.xMinLimit, lerpState.xMaxLimit);
					}
					else
					{
						mouseY = currentTarget.root.eulerAngles.NormalizeAngle().x;
						mouseX = currentTarget.root.eulerAngles.NormalizeAngle().y;
					}
				}
			}
			else
			{
				float x2 = lerpState.fixedAngle.x;
				float y2 = lerpState.fixedAngle.y;
				mouseX = (useSmooth ? Mathf.LerpAngle(mouseX, x2, smoothBetweenState * Time.deltaTime) : x2);
				mouseY = (useSmooth ? Mathf.LerpAngle(mouseY, y2, smoothBetweenState * Time.deltaTime) : y2);
			}
		}

		public void SwitchRight(bool value = false)
		{
			switchRight = ((!value) ? 1 : (-1));
		}

		private void CalculeLockOnPoint()
		{
			if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle) || !lockTarget)
			{
				Collider component = lockTarget.GetComponent<Collider>();
				if (!(component == null))
				{
					Quaternion quaternion = Quaternion.LookRotation(component.bounds.center - desired_cPos);
					float num = 0f;
					float y = quaternion.eulerAngles.y;
					num = ((quaternion.eulerAngles.x < -180f) ? (quaternion.eulerAngles.x + 360f) : ((!(quaternion.eulerAngles.x > 180f)) ? quaternion.eulerAngles.x : (quaternion.eulerAngles.x - 360f)));
					mouseY = vExtensions.ClampAngle(num, currentState.yMinLimit, currentState.yMaxLimit);
					mouseX = vExtensions.ClampAngle(y, currentState.xMinLimit, currentState.xMaxLimit);
				}
			}
		}

		private void CameraMovement(bool forceUpdate = false)
		{
			if (currentTarget == null || targetCamera == null || (!firstStateIsInit && !forceUpdate))
			{
				return;
			}
			if (useSmooth)
			{
				currentState.Slerp(lerpState, smoothBetweenState * Time.deltaTime);
			}
			else
			{
				currentState.CopyState(lerpState);
			}
			if (currentState.useZoom)
			{
				currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
				distance = (useSmooth ? Mathf.Lerp(distance, currentZoom, lerpState.smooth * Time.deltaTime) : currentZoom);
			}
			else
			{
				distance = (useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance, lerpState.smooth * Time.deltaTime) : currentState.defaultDistance);
				currentZoom = currentState.defaultDistance;
			}
			targetCamera.fieldOfView = currentState.fov;
			cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.deltaTime);
			currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothBetweenState * Time.deltaTime);
			Vector3 normalized = (currentState.forward * targetLookAt.forward + currentState.right * currentSwitchRight * targetLookAt.right).normalized;
			Vector3 vector = (currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, currentTarget.position.z) + currentTarget.transform.up * offSetPlayerPivot);
			desired_cPos = vector + currentTarget.transform.up * currentState.height;
			current_cPos = (firstUpdated ? (vector + currentTarget.transform.up * currentHeight) : Vector3.SmoothDamp(current_cPos, vector + currentTarget.transform.up * currentHeight, ref cameraVelocityDamp, lerpState.smoothDamp * Time.deltaTime));
			firstUpdated = false;
			ClipPlanePoints to = targetCamera.NearClipPlanePoints(current_cPos + normalized * distance, clipPlaneMargin);
			ClipPlanePoints to2 = targetCamera.NearClipPlanePoints(desired_cPos + normalized * currentZoom, clipPlaneMargin);
			RaycastHit hitInfo;
			if (Physics.SphereCast(vector, checkHeightRadius, currentTarget.transform.up, out hitInfo, currentState.cullingHeight + 0.2f, cullingLayer))
			{
				float num = hitInfo.distance - 0.2f;
				num -= currentState.height;
				num /= currentState.cullingHeight - currentState.height;
				cullingHeight = Mathf.Lerp(currentState.height, currentState.cullingHeight, Mathf.Clamp(num, 0f, 1f));
			}
			else
			{
				cullingHeight = (useSmooth ? Mathf.Lerp(cullingHeight, currentState.cullingHeight, smoothBetweenState * Time.deltaTime) : currentState.cullingHeight);
			}
			if (CullingRayCast(desired_cPos, to2, out hitInfo, currentZoom + 0.2f, cullingLayer, Color.blue))
			{
				distance = hitInfo.distance - 0.2f;
				if (distance < currentState.defaultDistance)
				{
					float num2 = hitInfo.distance;
					num2 -= currentState.cullingMinDist;
					num2 /= currentZoom - currentState.cullingMinDist;
					currentHeight = Mathf.Lerp(cullingHeight, currentState.height, Mathf.Clamp(num2, 0f, 1f));
					current_cPos = vector + currentTarget.transform.up * currentHeight;
				}
			}
			else
			{
				currentHeight = (useSmooth ? Mathf.Lerp(currentHeight, currentState.height, smoothBetweenState * Time.deltaTime) : currentState.height);
			}
			if (CullingRayCast(current_cPos, to, out hitInfo, distance, cullingLayer, Color.cyan))
			{
				distance = Mathf.Clamp(cullingDistance, 0f, currentState.defaultDistance);
			}
			Vector3 vector2 = current_cPos + targetLookAt.forward * 2f + targetLookAt.right * Vector3.Dot(normalized * distance, targetLookAt.right);
			targetLookAt.position = current_cPos;
			Quaternion quaternion = Quaternion.Euler(mouseY + offsetMouse.y, mouseX + offsetMouse.x, 0f);
			targetLookAt.rotation = (useSmooth ? Quaternion.Lerp(targetLookAt.rotation, quaternion, smoothCameraRotation * Time.deltaTime) : quaternion);
			base.transform.position = current_cPos + normalized * distance;
			Quaternion quaternion2 = Quaternion.LookRotation(vector2 - base.transform.position);
			if ((bool)lockTarget)
			{
				CalculeLockOnPoint();
				if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
				{
					Collider component = lockTarget.GetComponent<Collider>();
					if (component != null)
					{
						Vector3 b = Quaternion.LookRotation(component.bounds.center + Vector3.up * heightOffset - base.transform.position).eulerAngles - quaternion2.eulerAngles;
						if (isNewTarget)
						{
							lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, b.x, currentState.smooth * Time.deltaTime);
							lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, b.y, currentState.smooth * Time.deltaTime);
							lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, b.z, currentState.smooth * Time.deltaTime);
							if (Vector3.Distance(lookTargetAdjust, b) < 0.5f)
							{
								isNewTarget = false;
							}
						}
						else
						{
							lookTargetAdjust = b;
						}
					}
				}
			}
			else
			{
				lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, 0f, currentState.smooth * Time.deltaTime);
				lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, 0f, currentState.smooth * Time.deltaTime);
				lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, 0f, currentState.smooth * Time.deltaTime);
			}
			Vector3 vector3 = quaternion2.eulerAngles + lookTargetAdjust;
			vector3.z = 0f;
			Quaternion rotation = Quaternion.Euler(vector3 + currentState.rotationOffSet);
			base.transform.rotation = rotation;
			movementSpeed = Vector2.zero;
		}

		private void CameraFixed()
		{
			if (useSmooth)
			{
				currentState.Slerp(lerpState, smoothBetweenState);
			}
			else
			{
				currentState.CopyState(lerpState);
			}
			Vector3 vector = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot + currentState.height, currentTarget.position.z);
			currentTargetPos = (useSmooth ? Vector3.MoveTowards(currentTargetPos, vector, currentState.smooth * Time.deltaTime) : vector);
			current_cPos = currentTargetPos;
			Vector3 vector2 = (isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : base.transform.position);
			base.transform.position = (useSmooth ? Vector3.Lerp(base.transform.position, vector2, currentState.smooth * Time.deltaTime) : vector2);
			targetLookAt.position = current_cPos;
			if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
			{
				Quaternion quaternion = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
				base.transform.rotation = (useSmooth ? Quaternion.Slerp(base.transform.rotation, quaternion, currentState.smooth * 0.5f * Time.deltaTime) : quaternion);
			}
			else if (isValidFixedPoint)
			{
				Quaternion quaternion2 = Quaternion.LookRotation(currentTargetPos - base.transform.position);
				base.transform.rotation = (useSmooth ? Quaternion.Slerp(base.transform.rotation, quaternion2, currentState.smooth * Time.deltaTime) : quaternion2);
			}
			targetCamera.fieldOfView = currentState.fov;
		}

		private bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
		{
			bool flag = false;
			if (showGizmos)
			{
				Debug.DrawRay(from, _to.LowerLeft - from, color);
				Debug.DrawLine(_to.LowerLeft, _to.LowerRight, color);
				Debug.DrawLine(_to.UpperLeft, _to.UpperRight, color);
				Debug.DrawLine(_to.UpperLeft, _to.LowerLeft, color);
				Debug.DrawLine(_to.UpperRight, _to.LowerRight, color);
				Debug.DrawRay(from, _to.LowerRight - from, color);
				Debug.DrawRay(from, _to.UpperLeft - from, color);
				Debug.DrawRay(from, _to.UpperRight - from, color);
			}
			if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
			{
				flag = true;
				cullingDistance = hitInfo.distance;
			}
			if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
			{
				flag = true;
				if (cullingDistance > hitInfo.distance)
				{
					cullingDistance = hitInfo.distance;
				}
			}
			if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
			{
				flag = true;
				if (cullingDistance > hitInfo.distance)
				{
					cullingDistance = hitInfo.distance;
				}
			}
			if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
			{
				flag = true;
				if (cullingDistance > hitInfo.distance)
				{
					cullingDistance = hitInfo.distance;
				}
			}
			return (bool)hitInfo.collider && flag;
		}
	}
}
