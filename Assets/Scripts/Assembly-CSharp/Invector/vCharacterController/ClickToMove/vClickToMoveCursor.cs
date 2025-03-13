using UnityEngine;

namespace Invector.vCharacterController.ClickToMove
{
	public class vClickToMoveCursor : MonoBehaviour
	{
		public vClickToMoveInput tpInput;

		public GameObject cursorObject;

		private Vector3 _scale;

		private Vector3 currentScale;

		public float scale;

		public float speed;

		private float time;

		private bool enableCursor;

		private void Start()
		{
			if (!tpInput)
			{
				Object.Destroy(base.gameObject);
			}
			tpInput.onEnableCursor.AddListener(Enable);
			tpInput.onDisableCursor.AddListener(Disable);
			_scale = cursorObject.transform.localScale;
		}

		private void Update()
		{
			if (enableCursor)
			{
				time += speed * Time.deltaTime;
				currentScale.x = Mathf.PingPong(time, _scale.x + scale);
				currentScale.x = Mathf.Clamp(currentScale.x, _scale.x, _scale.x + scale);
				currentScale.y = Mathf.PingPong(time, _scale.y + scale);
				currentScale.y = Mathf.Clamp(currentScale.y, _scale.y, _scale.y + scale);
				currentScale.z = Mathf.PingPong(time, _scale.z + scale);
				currentScale.z = Mathf.Clamp(currentScale.z, _scale.z, _scale.z + scale);
				cursorObject.transform.localScale = currentScale;
			}
		}

		public bool Near(Vector3 pos, float dst)
		{
			Vector3 a = new Vector3(pos.x, 0f, pos.z);
			Vector3 b = new Vector3(base.transform.position.x, 0f, base.transform.position.z);
			return Vector3.Distance(a, b) < dst;
		}

		public void Enable(Vector3 position)
		{
			base.transform.position = position;
			cursorObject.SetActive(true);
			enableCursor = true;
		}

		public void Disable()
		{
			cursorObject.SetActive(false);
			enableCursor = false;
		}
	}
}
