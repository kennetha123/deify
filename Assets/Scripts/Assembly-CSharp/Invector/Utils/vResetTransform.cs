using UnityEngine;

namespace Invector.Utils
{
	[vClassHeader("Reset Transform", true, "icon_v2", false, "", useHelpBox = true, helpBoxText = "Use this to Reset transformation values<b><color=red>\nPosition Zero\nRotation Zero\nScale One</color> </b>", openClose = false)]
	public class vResetTransform : vMonoBehaviour
	{
		public bool resetPositionOnStart;

		public bool resetRotationOnStart;

		public bool resetScaleOnStart;

		private void Start()
		{
			if (resetPositionOnStart)
			{
				ResetPosition();
			}
			if (resetRotationOnStart)
			{
				ResetRotation();
			}
			if (resetScaleOnStart)
			{
				ResetScale();
			}
		}

		public void ResetRotation()
		{
			base.transform.eulerAngles = Vector3.zero;
		}

		public void ResetPosition()
		{
			base.transform.position = Vector3.zero;
		}

		public void ResetScale()
		{
			base.transform.localScale = Vector3.one;
		}
	}
}
