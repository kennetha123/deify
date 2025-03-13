using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
	[Serializable]
	public class vDamageEffect
	{
		public string damageType = "";

		public GameObject effectPrefab;

		public bool rotateToHitDirection = true;

		[Tooltip("Attach prefab in Damage Receiver transform")]
		public bool attachInReceiver;

		public UnityEvent onTriggerEffect;
	}
}
