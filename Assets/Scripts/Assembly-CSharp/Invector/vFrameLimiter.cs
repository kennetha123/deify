using System;
using UnityEngine;

namespace Invector
{
	[vClassHeader("Frame Limiter", false, "icon_v2", false, "")]
	public class vFrameLimiter : vMonoBehaviour
	{
		public int desiredFPS = 60;

		private void Awake()
		{
			Application.targetFrameRate = -1;
			QualitySettings.vSyncCount = 0;
		}

		private void Update()
		{
			long ticks = DateTime.Now.Ticks;
			float num = 1f / (float)desiredFPS;
			if (desiredFPS > 0)
			{
				while (!((float)TimeSpan.FromTicks(DateTime.Now.Ticks - ticks).TotalSeconds >= num))
				{
				}
			}
		}
	}
}
