using UnityEngine;

namespace Invector
{
	public class vHealthItem : MonoBehaviour
	{
		[Tooltip("How much health will be recovery")]
		public float value;

		public string tagFilter = "Player";

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag.Equals(tagFilter))
			{
				vHealthController component = other.GetComponent<vHealthController>();
				if (component != null && component.currentHealth < (float)component.maxHealth)
				{
					component.ChangeHealth((int)value);
					Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
