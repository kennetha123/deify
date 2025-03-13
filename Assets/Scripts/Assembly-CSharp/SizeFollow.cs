using UnityEngine;

public class SizeFollow : MonoBehaviour
{
	public RectTransform follow;

	private void Start()
	{
	}

	private void Update()
	{
		GetComponent<RectTransform>().sizeDelta = follow.sizeDelta;
	}
}
