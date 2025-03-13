using UnityEngine;

namespace LastBoss.Character
{
	public class CharacterBase : MonoBehaviour
	{
		public int experience;

		private int Experience
		{
			get
			{
				return experience;
			}
			set
			{
				experience = value;
			}
		}

		public virtual void Start()
		{
		}

		public virtual void Update()
		{
		}

		public void OnDamage()
		{
		}
	}
}
