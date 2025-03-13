using System;
using UnityEngine;

namespace Invector.vCharacterController.vActions
{
	public abstract class vActionListener : vMonoBehaviour, IActionListener, IActionEnterListener, IActionController, IActionExitListener, IActionStayListener
	{
		public bool actionEnter;

		public bool actionStay;

		public bool actionExit;

		public bool _doingAction;

		public vOnActionHandle OnDoAction = new vOnActionHandle();

		public bool useActionEnter
		{
			get
			{
				return actionEnter;
			}
		}

		public bool useActionExit
		{
			get
			{
				return actionExit;
			}
		}

		public bool useActionStay
		{
			get
			{
				return actionStay;
			}
		}

		public bool doingAction
		{
			get
			{
				return _doingAction;
			}
			protected set
			{
				_doingAction = value;
			}
		}

		bool IActionController.enabled
		{
			get
			{
				return base.enabled;
			}
			set
			{
				base.enabled = value;
			}
		}

		GameObject IActionController.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		Transform IActionController.transform
		{
			get
			{
				return base.transform;
			}
		}

		string IActionController.name
		{
			get
			{
				return base.name;
			}
		}

		protected virtual void Start()
		{
			IActionReceiver[] components = GetComponents<IActionReceiver>();
			for (int i = 0; i < components.Length; i++)
			{
				OnDoAction.AddListener(components[i].OnReceiveAction);
			}
		}

		public virtual void OnActionEnter(Collider other)
		{
		}

		public virtual void OnActionStay(Collider other)
		{
		}

		public virtual void OnActionExit(Collider other)
		{
		}

		Type IActionController.GetType()
		{
			return GetType();
		}
	}
}
