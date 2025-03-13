using UnityEngine;

public class RFX1_RotationFreeze : MonoBehaviour
{
	public bool LockX = true;

	public bool LockY = true;

	public bool LockZ = true;

	private Vector3 startRotation;

	private void Start()
	{
		startRotation = base.transform.localRotation.eulerAngles;
	}

	private void Update()
	{
		float x = (LockX ? startRotation.x : base.transform.rotation.eulerAngles.x);
		float y = (LockY ? startRotation.y : base.transform.rotation.eulerAngles.y);
		float z = (LockZ ? startRotation.z : base.transform.rotation.eulerAngles.z);
		base.transform.rotation = Quaternion.Euler(x, y, z);
	}
}
