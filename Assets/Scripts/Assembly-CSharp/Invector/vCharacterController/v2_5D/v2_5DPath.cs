using System;
using Invector.vCamera;
using UnityEngine;

namespace Invector.vCharacterController.v2_5D
{
	[vClassHeader("2.5D PathNode", false, "icon_v2", false, "")]
	public class v2_5DPath : vMonoBehaviour
	{
		public class v2_5DPathPoint
		{
			public Transform center;

			public Transform forward;

			public Transform backward;
		}

		public bool autoUpdateCameraAngle = true;

		public bool loopPath = true;

		public Transform reference;

		public Transform[] points;

		public v2_5DPathPoint currentPoint;

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying && points.Length != base.transform.childCount)
			{
				points = new Transform[base.transform.childCount];
			}
			if (base.transform.childCount > 1)
			{
				Transform child = base.transform.GetChild(0);
				if (!Application.isPlaying)
				{
					points[0] = base.transform.GetChild(0);
				}
				for (int i = 1; i < base.transform.childCount; i++)
				{
					if (!Application.isPlaying)
					{
						points[i] = base.transform.GetChild(i);
					}
					Gizmos.DrawLine(child.position, base.transform.GetChild(i).position);
					child = base.transform.GetChild(i);
				}
				if (loopPath)
				{
					Gizmos.DrawLine(points[0].position, points[points.Length - 1].position);
				}
			}
			if (currentPoint != null)
			{
				Gizmos.color = Color.red;
				if ((bool)currentPoint.center)
				{
					Gizmos.DrawSphere(currentPoint.center.position, 0.2f);
				}
				Gizmos.color = Color.yellow;
				if ((bool)currentPoint.forward)
				{
					Gizmos.DrawSphere(currentPoint.forward.position, 0.2f);
				}
				Gizmos.color = Color.yellow;
				if ((bool)currentPoint.backward)
				{
					Gizmos.DrawSphere(currentPoint.backward.position, 0.2f);
				}
			}
		}

		private v2_5DPathPoint GetStartPoint(Vector3 position)
		{
			float num = float.PositiveInfinity;
			v2_5DPathPoint v2_5DPathPoint = new v2_5DPathPoint();
			for (int i = 0; i < points.Length; i++)
			{
				float num2 = Vector3.Distance(points[i].position, position);
				if (num2 < num)
				{
					num = num2;
					v2_5DPathPoint.center = points[i];
					if (i + 1 < points.Length)
					{
						v2_5DPathPoint.forward = points[i + 1];
					}
					else if (i == points.Length - 1 && loopPath)
					{
						v2_5DPathPoint.forward = points[0];
					}
					if (i - 1 > -1)
					{
						v2_5DPathPoint.backward = points[i - 1];
					}
					else if (i == 0 && loopPath)
					{
						v2_5DPathPoint.backward = points[points.Length - 1];
					}
				}
			}
			return v2_5DPathPoint;
		}

		public bool isNearForward(Vector3 position)
		{
			if (currentPoint == null || !currentPoint.forward)
			{
				return false;
			}
			return Vector3.Distance(currentPoint.forward.position, position) < 0.1f;
		}

		public bool isNearBackward(Vector3 position)
		{
			if (!currentPoint.backward)
			{
				return false;
			}
			return Vector3.Distance(currentPoint.backward.position, position) < 0.1f;
		}

		private v2_5DPathPoint GetNextPoint(Transform center)
		{
			v2_5DPathPoint v2_5DPathPoint = new v2_5DPathPoint();
			v2_5DPathPoint.center = center;
			int num = Array.IndexOf(points, center);
			if (num + 1 < points.Length)
			{
				v2_5DPathPoint.forward = points[num + 1];
			}
			else if (num == points.Length - 1 && loopPath)
			{
				v2_5DPathPoint.forward = points[0];
			}
			if (num - 1 > -1)
			{
				v2_5DPathPoint.backward = points[num - 1];
			}
			else if (num == 0 && loopPath)
			{
				v2_5DPathPoint.backward = points[points.Length - 1];
			}
			return v2_5DPathPoint;
		}

		public void Init()
		{
			currentPoint = null;
		}

		public Vector3 ConstraintPosition(Vector3 pos, bool checkChangePoint = true)
		{
			Vector3 vector = pos;
			if (currentPoint == null)
			{
				currentPoint = GetStartPoint(pos);
			}
			if ((bool)currentPoint.center)
			{
				if (!reference)
				{
					GameObject gameObject = new GameObject("Reference");
					reference = gameObject.transform;
				}
				vector.y = currentPoint.center.position.y;
				if (checkChangePoint)
				{
					if (isNearBackward(vector))
					{
						currentPoint = GetNextPoint(currentPoint.backward);
					}
					if (isNearForward(vector))
					{
						currentPoint = GetNextPoint(currentPoint.forward);
					}
				}
				if (currentPoint.forward != null)
				{
					Vector3 vector2 = (currentPoint.backward ? (currentPoint.backward.position - currentPoint.center.position) : (-reference.right));
					Vector3 a = currentPoint.center.position + vector2;
					Vector3 vector3 = (currentPoint.forward ? (currentPoint.forward.position - currentPoint.center.position) : reference.right);
					Vector3 a2 = currentPoint.center.position + vector3;
					reference.position = currentPoint.center.position;
					float num = (currentPoint.backward ? Vector3.Distance(currentPoint.center.position, currentPoint.backward.position) : float.PositiveInfinity);
					float num2 = (currentPoint.forward ? Vector3.Distance(currentPoint.center.position, currentPoint.forward.position) : float.PositiveInfinity);
					if (Vector3.Distance(a, vector) > num + 0.1f)
					{
						reference.right = vector3;
					}
					else if (Vector3.Distance(a2, vector) > num2 + 0.1f)
					{
						reference.right = -vector2;
					}
				}
				if (autoUpdateCameraAngle && (bool)vThirdPersonCamera.instance)
				{
					float y = Quaternion.LookRotation(reference.forward, Vector3.up).eulerAngles.NormalizeAngle().y;
					vThirdPersonCamera.instance.lerpState.fixedAngle.x = y;
				}
				Vector3 position = reference.InverseTransformPoint(pos);
				position.z = 0f;
				return reference.TransformPoint(position);
			}
			return vector;
		}
	}
}
