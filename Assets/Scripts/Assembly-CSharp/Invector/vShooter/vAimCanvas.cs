using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
	public class vAimCanvas : MonoBehaviour
	{
		public RectTransform aimTarget;

		public RectTransform aimCenter;

		public Camera scopeCamera;

		public bool useScopeTransition = true;

		public bool scaleAimWithMovement = true;

		public float scaleWithMovement = 2f;

		public float smothChangeScale = 2f;

		[Range(0f, 1f)]
		public float movementSensibility = 0.1f;

		public UnityEvent onEnableAim;

		public UnityEvent onDisableAim;

		public UnityEvent onCheckvalidAim;

		public UnityEvent onCheckInvalidAim;

		public UnityEvent onEnableScopeCamera;

		public UnityEvent onDisableScopeCamera;

		public UnityEvent onEnableScopeUI;

		public UnityEvent onDisableScopeUI;

		[HideInInspector]
		public bool isValid;

		[HideInInspector]
		public bool isAimActive;

		[HideInInspector]
		public bool isScopeCameraActive;

		[HideInInspector]
		public bool isScopeUIActive;

		[HideInInspector]
		public Vector2 sizeDeltaTarget;

		[HideInInspector]
		public Vector2 sizeDeltaCenter;

		protected virtual void Start()
		{
			onDisableScopeCamera.Invoke();
			onDisableScopeUI.Invoke();
			onDisableAim.Invoke();
			if ((bool)aimCenter)
			{
				sizeDeltaCenter = aimCenter.sizeDelta;
			}
			if ((bool)aimTarget)
			{
				sizeDeltaTarget = aimTarget.sizeDelta;
			}
		}

		public void DisableAll()
		{
			onDisableScopeCamera.Invoke();
			onDisableScopeUI.Invoke();
			onDisableAim.Invoke();
			isValid = false;
			isAimActive = false;
			isScopeCameraActive = false;
			isScopeUIActive = false;
		}
	}
}
