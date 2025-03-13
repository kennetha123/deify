using System;
using UnityEngine;

namespace Invector
{
	[Serializable]
	public abstract class vAnimatorSetValue<T> : StateMachineBehaviour where T : IConvertible
	{
		public bool setOnEnter;

		[vHideInInspector("setOnEnter", false)]
		public T enterValue;

		public bool setOnExit;

		[vHideInInspector("setOnExit", false)]
		public T exitValue;

		public string animatorParameter = "My Animator Parameter";

		protected virtual T GetEnterValue()
		{
			return enterValue;
		}

		protected virtual T GetExitValue()
		{
			return exitValue;
		}

		public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (setOnEnter)
			{
				if (typeof(T).Equals(typeof(int)))
				{
					animator.SetInteger(animatorParameter, (int)(object)GetEnterValue());
				}
				else if (typeof(T).Equals(typeof(float)))
				{
					animator.SetFloat(animatorParameter, (float)(object)GetEnterValue());
				}
				else if (typeof(T).Equals(typeof(bool)))
				{
					animator.SetBool(animatorParameter, (bool)(object)GetEnterValue());
				}
			}
		}

		public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (setOnExit)
			{
				if (typeof(T).Equals(typeof(int)))
				{
					animator.SetInteger(animatorParameter, (int)(object)GetExitValue());
				}
				else if (typeof(T).Equals(typeof(float)))
				{
					animator.SetFloat(animatorParameter, (float)(object)GetExitValue());
				}
				else if (typeof(T).Equals(typeof(bool)))
				{
					animator.SetBool(animatorParameter, (bool)(object)GetExitValue());
				}
			}
		}
	}
}
