using UnityEngine;

public class SimpleHumanoidIKSampleThirdPersonCamera : MonoBehaviour
{
	[SerializeField]
	private GameObject target;

	[SerializeField]
	private GameObject lookAt;

	[SerializeField]
	private float cameraMoveSpeed = 15f;

	[SerializeField]
	private float minCameraAngle = 10f;

	[SerializeField]
	private float maxCameraAngle = 30f;

	[SerializeField]
	private GameObject pivotCamera;

	[SerializeField]
	private float wheelSpeed = 5f;

	[SerializeField]
	private float distance = 2.6f;

	[SerializeField]
	private float minDistanceLimit = 0.6f;

	[SerializeField]
	private float maxDistanceLimit = 10f;

	[SerializeField]
	private float horizontalOffset;

	[SerializeField]
	private float verticalOffset;

	[SerializeField]
	private float floorCheckHeight = 0.5f;

	private bool rightclicked = true;

	private bool mouseWheeled = true;

	private float x;

	private float y;

	private void Start()
	{
		RotateByPositionAndLookAt();
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (Input.GetMouseButton(1))
		{
			rightclicked = true;
		}
		else
		{
			rightclicked = false;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			mouseWheeled = true;
		}
		else
		{
			mouseWheeled = false;
		}
		RotateByPositionAndLookAt();
		SetCameraOffset(horizontalOffset, verticalOffset, distance);
	}

	private void RotateByPositionAndLookAt()
	{
		if (!target)
		{
			return;
		}
		if (mouseWheeled)
		{
			distance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
			if (distance < minDistanceLimit)
			{
				distance = minDistanceLimit;
			}
			if (distance > maxDistanceLimit)
			{
				distance = maxDistanceLimit;
			}
		}
		bool flag = false;
		RaycastHit hitInfo;
		if (distance > minDistanceLimit && Physics.Linecast(base.transform.position + Vector3.down * floorCheckHeight, target.transform.position, out hitInfo) && hitInfo.collider.gameObject.layer == 8)
		{
			flag = true;
		}
		if (rightclicked)
		{
			y += Input.GetAxis("Mouse Y") * cameraMoveSpeed;
			x += Input.GetAxis("Mouse X") * cameraMoveSpeed;
			if (x > 180f)
			{
				x -= 360f;
			}
			else if (x <= -180f)
			{
				x += 360f;
			}
			if (y < -1f * maxCameraAngle)
			{
				y = -1f * maxCameraAngle;
			}
			else if (y > -1f * minCameraAngle)
			{
				y = -1f * minCameraAngle;
			}
		}
		Vector3 b = Quaternion.Euler(y, x, 0f) * new Vector3(0f, 0f, distance) + target.transform.position;
		if (flag && base.transform.position.y > b.y)
		{
			b.y = base.transform.position.y;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, b, cameraMoveSpeed * Time.deltaTime);
		base.transform.LookAt(lookAt.transform.position);
	}

	private void SetCameraOffset(float horizontalOffsetValue, float verticalOffsetValue, float currentDistance)
	{
		if ((bool)pivotCamera)
		{
			pivotCamera.transform.localPosition = Vector3.Lerp(pivotCamera.transform.localPosition, new Vector3(horizontalOffsetValue * currentDistance, verticalOffsetValue * currentDistance, 0f), cameraMoveSpeed * Time.deltaTime);
		}
	}
}
