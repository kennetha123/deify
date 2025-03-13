using UnityEngine;

namespace LastBoss.System
{
	public class Singleton<T> : MonoBehaviour where T : Component
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType<T>();
					if (instance == null)
					{
						instance = new GameObject
						{
							name = typeof(T).Name
						}.AddComponent<T>();
					}
				}
				return instance;
			}
		}

		public virtual void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
				Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
