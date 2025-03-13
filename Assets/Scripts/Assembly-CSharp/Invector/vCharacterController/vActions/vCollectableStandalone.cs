using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Invector.vMelee;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions
{
	[vClassHeader("Collectable Standalone", "Use this component when your character doesn't have a ItemManager", openClose = false)]
	public class vCollectableStandalone : vTriggerGenericAction
	{
		public string targetEquipPoint;

		public GameObject weapon;

		public Sprite weaponIcon;

		public string weaponText;

		public UnityEvent OnEquip;

		public UnityEvent OnDrop;

		private vCollectMeleeControl manager;

		public override IEnumerator OnDoActionDelay(GameObject cc)
		{
			yield return StartCoroutine(_003C_003En__0(cc));
			manager = cc.GetComponent<vCollectMeleeControl>();
			if (manager != null)
			{
				manager.HandleCollectableInput(this);
			}
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0(GameObject obj)
		{
			return base.OnDoActionDelay(obj);
		}
	}
}
