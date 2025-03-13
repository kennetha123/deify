using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager
{
	public class vItemCollectionTextHUD : MonoBehaviour
	{
		public Text Message;

		[HideInInspector]
		public bool inUse;

		public void Show(string message, float timeToStay = 1f, float timeToFadeOut = 1f)
		{
			inUse = true;
			Message.text = message;
			StartCoroutine(Timer(timeToStay, timeToFadeOut));
		}

		private IEnumerator Timer(float timeToStay = 1f, float timeToFadeOut = 1f)
		{
			Message.CrossFadeAlpha(1f, 0.5f, false);
			yield return new WaitForSeconds(timeToStay);
			Message.CrossFadeAlpha(0f, timeToFadeOut, false);
			yield return new WaitForSeconds(timeToFadeOut + 0.1f);
			Object.Destroy(base.gameObject);
			inUse = false;
		}

		public void Init()
		{
			Message.text = "";
			Message.CrossFadeAlpha(0f, 0f, false);
		}
	}
}
