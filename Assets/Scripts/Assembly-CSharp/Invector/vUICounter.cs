using UnityEngine;
using UnityEngine.UI;

namespace Invector
{
	public class vUICounter : MonoBehaviour
	{
		public Text displayCounter;

		[HideInInspector]
		public float currentCounter;

		public void ResetCounter()
		{
			currentCounter = 0f;
			UpdateCounter();
		}

		public void AddCounter(int value)
		{
			currentCounter += value;
			UpdateCounter();
		}

		public void AddCounter(float value)
		{
			currentCounter += value;
			UpdateCounter();
		}

		private void UpdateCounter()
		{
			if ((bool)displayCounter)
			{
				displayCounter.text = currentCounter.ToString("00");
			}
		}
	}
}
