using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Invector.vCharacterController.vActions;
using UnityEngine;

namespace Invector.vItemManager
{
	[vClassHeader("vAmmoStandalone", true, "icon_v2", false, "")]
	public class vAmmoStandalone : vTriggerGenericAction
	{
		[Header("Ammo Standalone Options")]
		[Tooltip("Use the same name as in the AmmoManager")]
		public string weaponName;

		public int ammoID;

		public int ammoAmount;

		private vAmmoManager ammoManager;

		public override IEnumerator OnDoActionDelay(GameObject cc)
		{
			yield return StartCoroutine(_003C_003En__0(cc));
			ammoManager = cc.gameObject.GetComponent<vAmmoManager>();
			if (ammoManager != null)
			{
				ammoManager.AddAmmo(weaponName, ammoID, ammoAmount);
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
