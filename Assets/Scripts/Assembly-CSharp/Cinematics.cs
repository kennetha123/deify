using Invector.vCharacterController;
using UnityEngine;

public class Cinematics : MonoBehaviour
{
	public vThirdPersonInput input;

	private void Start()
	{
		Cursor.visible = false;
	}

	private void OnEnable()
	{
		input.enabled = false;
	}

	private void OnDisable()
	{
		input.enabled = true;
	}
}
