using UnityEngine;

public class Tutorial : MonoBehaviour
{
	private void Start()
	{
		Time.timeScale = 0f;
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			Time.timeScale = 1f;
			Object.Destroy(base.gameObject);
		}
	}
}
