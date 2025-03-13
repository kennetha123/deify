using UnityEngine;

namespace Invector.vItemManager
{
	public class vItemCollectionDisplay : MonoBehaviour
	{
		private static vItemCollectionDisplay instance;

		public GameObject HeadsUpText;

		public Transform Contenet;

		public static vItemCollectionDisplay Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType<vItemCollectionDisplay>();
				}
				return instance;
			}
		}

		public void FadeText(string message, float timeToStay, float timeToFadeOut)
		{
			GameObject obj = Object.Instantiate(HeadsUpText);
			obj.transform.SetParent(Contenet, false);
			vItemCollectionTextHUD component = obj.GetComponent<vItemCollectionTextHUD>();
			if (!component.inUse)
			{
				component.transform.SetAsFirstSibling();
				component.Init();
				component.Show(message, timeToStay, timeToFadeOut);
			}
		}
	}
}
