using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.Utils
{
	[vClassHeader("Events With Delay", true, "icon_v2", false, "")]
	public class vEventWithDelay : vMonoBehaviour
	{
		[Serializable]
		public class vEventWithDelayObject
		{
			public float delay;

			public UnityEvent onDoEvent;
		}

		public bool triggerOnStart;

		[vHideInInspector("triggerOnStart", false)]
		public bool all;

		[vHideInInspector("triggerOnStart", false)]
		public int eventIndex;

		[SerializeField]
		private vEventWithDelayObject[] events;

		private void Start()
		{
			if (triggerOnStart)
			{
				if (all)
				{
					DoEvents();
				}
				else
				{
					DoEvent(eventIndex);
				}
			}
		}

		public void DoEvents()
		{
			for (int i = 0; i < events.Length; i++)
			{
				StartCoroutine(DoEventWithDelay(events[i]));
			}
		}

		public void DoEvent(int index)
		{
			if (index < events.Length && events.Length != 0)
			{
				StartCoroutine(DoEventWithDelay(events[index]));
			}
		}

		private IEnumerator DoEventWithDelay(vEventWithDelayObject _event)
		{
			yield return new WaitForSeconds(_event.delay);
			_event.onDoEvent.Invoke();
		}
	}
}
