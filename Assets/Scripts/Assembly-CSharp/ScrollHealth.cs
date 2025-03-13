using UnityEngine;
using UnityEngine.UI;

public class ScrollHealth : MonoBehaviour
{
	private RawImage renderer;

	public float speed = 0.5f;

	private void Start()
	{
		renderer = GetComponent<RawImage>();
	}

	private void Update()
	{
		Vector2 position = new Vector2(Time.time * speed, 0f);
		renderer.uvRect = new Rect(position, new Vector2(1f, 1f));
	}
}
