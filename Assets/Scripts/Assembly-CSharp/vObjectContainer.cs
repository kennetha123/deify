using UnityEngine;

public class vObjectContainer : MonoBehaviour
{
	private static vObjectContainer instance;

	public static Transform root
	{
		get
		{
			if (!instance)
			{
				instance = new GameObject("Object Container", typeof(vObjectContainer)).GetComponent<vObjectContainer>();
			}
			return instance.transform;
		}
	}
}
