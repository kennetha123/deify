using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
	public class vControlAimCanvas : MonoBehaviour
	{
		public static vControlAimCanvas instance;

		public RectTransform canvas;

		public List<vAimCanvas> aimCanvasCollection = new List<vAimCanvas>();

		protected vThirdPersonController cc;

		public vAimCanvas currentAimCanvas;

		protected int currentCanvasID;

		protected float scopeCameraTransformWeight;

		private float scopeCameraTargetZoom;

		private float scopeCameraOriginZoom;

		private Quaternion scopeCameraTargetRot;

		private Quaternion scopeCameraOriginRot;

		private Vector3 scopeCameraTargetPos;

		private Vector3 scopeCameraOriginPos;

		private bool updateScopeCameraTransition;

		public Camera scopeCamera
		{
			get
			{
				if (!currentAimCanvas)
				{
					return null;
				}
				return currentAimCanvas.scopeCamera;
			}
		}

		public bool isValid
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.isValid;
			}
			set
			{
				currentAimCanvas.isValid = value;
			}
		}

		public bool isAimActive
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.isAimActive;
			}
			set
			{
				currentAimCanvas.isAimActive = value;
			}
		}

		public bool isScopeCameraActive
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.isScopeCameraActive;
			}
			set
			{
				currentAimCanvas.isScopeCameraActive = value;
			}
		}

		public bool isScopeUIActive
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.isScopeUIActive;
			}
			set
			{
				currentAimCanvas.isScopeUIActive = value;
			}
		}

		public bool useScopeTransition
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.useScopeTransition;
			}
			set
			{
				currentAimCanvas.useScopeTransition = value;
			}
		}

		protected bool scaleAimWithMovement
		{
			get
			{
				if (!currentAimCanvas)
				{
					return false;
				}
				return currentAimCanvas.scaleAimWithMovement;
			}
		}

		protected float movementSensibility
		{
			get
			{
				return currentAimCanvas.movementSensibility;
			}
		}

		protected float scaleWithMovement
		{
			get
			{
				return currentAimCanvas.scaleWithMovement;
			}
		}

		protected float smothChangeScale
		{
			get
			{
				return currentAimCanvas.smothChangeScale;
			}
		}

		protected RectTransform aimTarget
		{
			get
			{
				return currentAimCanvas.aimTarget;
			}
		}

		protected RectTransform aimCenter
		{
			get
			{
				return currentAimCanvas.aimCenter;
			}
		}

		protected Vector2 sizeDeltaTarget
		{
			get
			{
				return currentAimCanvas.sizeDeltaTarget;
			}
		}

		protected Vector2 sizeDeltaCenter
		{
			get
			{
				return currentAimCanvas.sizeDeltaCenter;
			}
		}

		protected UnityEvent onEnableAim
		{
			get
			{
				return currentAimCanvas.onEnableAim;
			}
		}

		protected UnityEvent onDisableAim
		{
			get
			{
				return currentAimCanvas.onDisableAim;
			}
		}

		protected UnityEvent onCheckvalidAim
		{
			get
			{
				return currentAimCanvas.onCheckvalidAim;
			}
		}

		protected UnityEvent onCheckInvalidAim
		{
			get
			{
				return currentAimCanvas.onCheckInvalidAim;
			}
		}

		protected UnityEvent onEnableScopeCamera
		{
			get
			{
				return currentAimCanvas.onEnableScopeCamera;
			}
		}

		protected UnityEvent onDisableScopeCamera
		{
			get
			{
				return currentAimCanvas.onDisableScopeCamera;
			}
		}

		protected UnityEvent onEnableScopeUI
		{
			get
			{
				return currentAimCanvas.onEnableScopeUI;
			}
		}

		protected UnityEvent onDisableScopeUI
		{
			get
			{
				return currentAimCanvas.onDisableScopeUI;
			}
		}

		public virtual void Init(vThirdPersonController cc)
		{
			instance = this;
			this.cc = cc;
			currentAimCanvas = aimCanvasCollection[0];
			isValid = true;
		}

		private void FixedUpdate()
		{
			updateScopeCameraTransition = true;
		}

		private void LateUpdate()
		{
			if (updateScopeCameraTransition && (bool)scopeCamera && scopeCamera.gameObject.activeSelf && useScopeTransition)
			{
				updateScopeCameraTransition = false;
				if (isScopeCameraActive)
				{
					scopeCameraTransformWeight = Mathf.Lerp(scopeCameraTransformWeight, 1.01f, 20f * Time.deltaTime);
				}
				else
				{
					scopeCameraTransformWeight = Mathf.Lerp(scopeCameraTransformWeight, -0.01f, 20f * Time.deltaTime);
				}
				scopeCameraTransformWeight = Mathf.Clamp(scopeCameraTransformWeight, 0f, 1f);
				if (!isScopeCameraActive && scopeCameraTransformWeight == 0f)
				{
					onDisableScopeCamera.Invoke();
				}
				if (scopeCameraTransformWeight < 1f)
				{
					scopeCamera.transform.rotation = Quaternion.Lerp(scopeCameraOriginRot, scopeCameraTargetRot, scopeCameraTransformWeight);
					scopeCamera.transform.position = Vector3.Lerp(scopeCameraOriginPos, scopeCameraTargetPos, scopeCameraTransformWeight);
					scopeCamera.fieldOfView = Mathf.Lerp(scopeCameraOriginZoom, scopeCameraTargetZoom, scopeCameraTransformWeight);
				}
			}
		}

		public void SetAimToCenter(bool validPoint = true)
		{
			if (currentAimCanvas == null)
			{
				return;
			}
			if (validPoint != isValid)
			{
				isValid = validPoint;
				if (isValid)
				{
					onCheckvalidAim.Invoke();
				}
				else
				{
					onCheckInvalidAim.Invoke();
				}
			}
			if ((bool)aimTarget && (bool)aimCenter)
			{
				aimTarget.anchoredPosition = aimCenter.anchoredPosition;
				aimTarget.sizeDelta = sizeDeltaTarget;
			}
		}

		public void SetWordPosition(Vector3 wordPosition, bool validPoint = true)
		{
			if (currentAimCanvas == null)
			{
				return;
			}
			if (validPoint != isValid)
			{
				isValid = validPoint;
				if (isValid)
				{
					onCheckvalidAim.Invoke();
				}
				else
				{
					onCheckInvalidAim.Invoke();
				}
			}
			if (validPoint && (bool)aimTarget && (bool)aimCenter)
			{
				Vector2 vector = Camera.main.WorldToViewportPoint(wordPosition);
				Vector2 anchoredPosition = new Vector2(vector.x * canvas.sizeDelta.x - canvas.sizeDelta.x * 0.5f, vector.y * canvas.sizeDelta.y - canvas.sizeDelta.y * 0.5f);
				aimTarget.anchoredPosition = anchoredPosition;
				if (scaleAimWithMovement && (cc.input.magnitude > movementSensibility || Input.GetAxis("Mouse X") > movementSensibility || Input.GetAxis("Mouse Y") > movementSensibility))
				{
					aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * scaleWithMovement, smothChangeScale * Time.deltaTime);
					aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * scaleWithMovement, smothChangeScale * Time.deltaTime);
				}
				else
				{
					aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * 1f, smothChangeScale * Time.deltaTime);
					aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * 1f, smothChangeScale * Time.deltaTime);
				}
			}
		}

		public void SetActiveAim(bool value)
		{
			if (!(currentAimCanvas == null) && value != isAimActive)
			{
				isAimActive = value;
				if (value)
				{
					isValid = true;
					onEnableAim.Invoke();
				}
				else
				{
					onDisableAim.Invoke();
				}
			}
		}

		public void SetActiveScopeCamera(bool value, bool useUI = false)
		{
			if (currentAimCanvas == null || (isScopeCameraActive == value && isScopeUIActive == useUI))
			{
				return;
			}
			isScopeUIActive = useUI;
			if (value)
			{
				onEnableScopeCamera.Invoke();
				isScopeCameraActive = true;
				if (value && useUI)
				{
					onEnableScopeUI.Invoke();
					isScopeUIActive = true;
				}
				else
				{
					onDisableScopeUI.Invoke();
					isScopeUIActive = false;
				}
			}
			else
			{
				if (!useScopeTransition)
				{
					onDisableScopeCamera.Invoke();
				}
				onDisableScopeUI.Invoke();
				isScopeUIActive = false;
				isScopeCameraActive = false;
			}
		}

		public void UpdateScopeCamera(Vector3 position, Vector3 lookPosition, float zoom = 60f)
		{
			if (currentAimCanvas == null || !scopeCamera)
			{
				return;
			}
			float fieldOfView = Mathf.Clamp(60f - zoom, 1f, 179f);
			if (scopeCameraTransformWeight < 1f && useScopeTransition)
			{
				scopeCameraTargetPos = position;
				scopeCameraTargetRot = Quaternion.LookRotation(lookPosition - scopeCamera.transform.position);
				scopeCameraTargetZoom = fieldOfView;
				scopeCameraOriginPos = Camera.main.transform.position;
				scopeCameraOriginRot = Camera.main.transform.rotation;
				scopeCameraOriginZoom = Camera.main.fieldOfView;
				if (!scopeCamera.isActiveAndEnabled && useScopeTransition)
				{
					scopeCamera.transform.position = Camera.main.transform.position;
					scopeCamera.transform.rotation = Camera.main.transform.rotation;
					scopeCamera.fieldOfView = Camera.main.fieldOfView;
				}
			}
			else
			{
				scopeCamera.fieldOfView = fieldOfView;
				scopeCamera.transform.position = position;
				Quaternion rotation = Quaternion.LookRotation(lookPosition - scopeCamera.transform.position);
				scopeCamera.transform.rotation = rotation;
			}
		}

		public void SetAimCanvasID(int id)
		{
			if (aimCanvasCollection.Count > 0 && currentCanvasID != id)
			{
				if (currentAimCanvas != null)
				{
					currentAimCanvas.DisableAll();
				}
				if (id < aimCanvasCollection.Count)
				{
					currentAimCanvas = aimCanvasCollection[id];
					currentCanvasID = id;
				}
				else
				{
					currentAimCanvas = aimCanvasCollection[0];
					currentCanvasID = 0;
				}
			}
		}
	}
}
