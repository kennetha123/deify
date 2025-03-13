using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("Character Standalone", true, "icon_v2", false, "")]
	public class vCharacterStandalone : vCharacter
	{
		[HideInInspector]
		public v_SpriteHealth healthSlider;

		protected override void Start()
		{
			base.Start();
			Init();
		}

		public override void TakeDamage(vDamage damage)
		{
			if (!base.isDead)
			{
				base.TakeDamage(damage);
			}
		}
	}
}
