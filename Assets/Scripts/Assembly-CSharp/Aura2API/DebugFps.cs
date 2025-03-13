using UnityEngine;

namespace Aura2API
{
	public class DebugFps : MonoBehaviour
	{
		public float interval = 1f;

		private float _accumulationValue;

		private int _framesCount;

		private float _timestamp;

		private float _rawFps;

		private float _meanFps;

		private void Update()
		{
			if (Time.time - _timestamp > interval)
			{
				_meanFps = _accumulationValue / (float)_framesCount;
				_timestamp = Time.time;
				_framesCount = 0;
				_accumulationValue = 0f;
			}
			_framesCount++;
			_rawFps = 1f / Time.deltaTime;
			_accumulationValue += _rawFps;
		}

		private void OnGUI()
		{
			GUI.color = Color.white;
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Mean FPS over " + interval + " second(s) = " + _meanFps + "\nRaw FPS = " + _rawFps);
		}
	}
}
