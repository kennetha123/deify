using UnityEngine;

namespace Invector
{
	public interface vIDamageReceiver
	{
		OnReceiveDamage onReceiveDamage { get; }

		Transform transform { get; }

		GameObject gameObject { get; }

		void TakeDamage(vDamage damage);
	}
}
