using System;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	[Serializable]
	public class vAISimpleTarget
	{
		[SerializeField]
		protected Transform _transform;

		[SerializeField]
		[HideInInspector]
		protected Collider _collider;

		public Transform transform
		{
			get
			{
				return _transform;
			}
			protected set
			{
				_transform = value;
			}
		}

		public Collider collider
		{
			get
			{
				return _collider;
			}
			protected set
			{
				_collider = value;
			}
		}

		public virtual void InitTarget(Transform target)
		{
			if ((bool)target)
			{
				transform = target;
				collider = transform.GetComponent<Collider>();
			}
		}

		public virtual void ClearTarget()
		{
			transform = null;
			collider = null;
		}

		public static implicit operator Transform(vAISimpleTarget m)
		{
			try
			{
				return m.transform;
			}
			catch
			{
				return null;
			}
		}
	}
}
