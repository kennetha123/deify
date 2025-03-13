using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
	public static class vExtensions
	{
		public static void SetLayerRecursively(this GameObject obj, int layer)
		{
			obj.layer = layer;
			foreach (Transform item in obj.transform)
			{
				item.gameObject.SetLayerRecursively(layer);
			}
		}

		public static bool ContainsLayer(this LayerMask layermask, int layer)
		{
			return (int)layermask == ((int)layermask | (1 << layer));
		}

		public static void SetActiveChildren(this GameObject gameObjet, bool value)
		{
			foreach (Transform item in gameObjet.transform)
			{
				item.gameObject.SetActive(value);
			}
		}

		public static bool isChild(this Transform me, Transform target)
		{
			if (!target)
			{
				return false;
			}
			string name = target.gameObject.name;
			Transform transform = me.FindChildByNameRecursive(name);
			if (transform == null)
			{
				return false;
			}
			return transform.Equals(target);
		}

		private static Transform FindChildByNameRecursive(this Transform me, string name)
		{
			if (me.name == name)
			{
				return me;
			}
			for (int i = 0; i < me.childCount; i++)
			{
				Transform transform = me.GetChild(i).FindChildByNameRecursive(name);
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
		{
			Vector3 vector = eulerAngle;
			if (vector.x > 180f)
			{
				vector.x -= 360f;
			}
			else if (vector.x < -180f)
			{
				vector.x += 360f;
			}
			if (vector.y > 180f)
			{
				vector.y -= 360f;
			}
			else if (vector.y < -180f)
			{
				vector.y += 360f;
			}
			if (vector.z > 180f)
			{
				vector.z -= 360f;
			}
			else if (vector.z < -180f)
			{
				vector.z += 360f;
			}
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
		{
			return otherVector - vector;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			do
			{
				if (angle < -360f)
				{
					angle += 360f;
				}
				if (angle > 360f)
				{
					angle -= 360f;
				}
			}
			while (angle < -360f || angle > 360f);
			return Mathf.Clamp(angle, min, max);
		}

		public static T[] Append<T>(this T[] arrayInitial, T[] arrayToAppend)
		{
			if (arrayToAppend == null)
			{
				throw new ArgumentNullException("The appended object cannot be null");
			}
			if (arrayInitial is string || arrayToAppend is string)
			{
				throw new ArgumentException("The argument must be an enumerable");
			}
			T[] array = new T[arrayInitial.Length + arrayToAppend.Length];
			arrayInitial.CopyTo(array, 0);
			arrayToAppend.CopyTo(array, arrayInitial.Length);
			return array;
		}

		public static List<T> vCopy<T>(this List<T> list)
		{
			List<T> list2 = new List<T>();
			if (list == null || list.Count == 0)
			{
				return list;
			}
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		public static List<T> vToList<T>(this T[] array)
		{
			List<T> list = new List<T>();
			if (array == null || array.Length == 0)
			{
				return list;
			}
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i]);
			}
			return list;
		}

		public static T[] vToArray<T>(this List<T> list)
		{
			T[] array = new T[list.Count];
			if (list == null || list.Count == 0)
			{
				return array;
			}
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = list[i];
			}
			return array;
		}

		public static Vector3 BoxSize(this BoxCollider boxCollider)
		{
			float x = boxCollider.transform.lossyScale.x * boxCollider.size.x;
			float z = boxCollider.transform.lossyScale.z * boxCollider.size.z;
			float y = boxCollider.transform.lossyScale.y * boxCollider.size.y;
			return new Vector3(x, y, z);
		}

		public static bool Contains<T>(this Enum value, Enum lookingForFlag) where T : struct
		{
			int num = (int)(object)value;
			int num2 = (int)(object)lookingForFlag;
			return (num & num2) == num2;
		}
	}
}
