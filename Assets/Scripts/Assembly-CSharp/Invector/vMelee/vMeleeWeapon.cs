using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee
{
	public class vMeleeWeapon : vMeleeAttackObject
	{
		[Header("Melee Weapon Settings")]
		public vMeleeType meleeType = vMeleeType.OnlyAttack;

		[Header("Attack Settings")]
		public bool useStrongAttack = true;

		[Tooltip("Simple AI Only")]
		public float distanceToAttack = 1f;

		[Tooltip("Trigger a Attack Animation")]
		public int attackID;

		[Tooltip("Change the MoveSet when using this Weapon")]
		public int movesetID;

		[Header("* Third Person Controller Only *")]
		[Tooltip("How much stamina will be consumed when attack")]
		public float staminaCost;

		[Tooltip("How much time the stamina will wait to start recover")]
		public float staminaRecoveryDelay;

		[Header("Defense Settings")]
		[Range(0f, 100f)]
		public int defenseRate = 100;

		[Range(0f, 180f)]
		public float defenseRange = 90f;

		[Tooltip("Trigger a Defense Animation")]
		public int defenseID;

		[Tooltip("What recoil animatil will trigger")]
		public int recoilID;

		[Tooltip("Can break the oponent attack, will trigger a recoil animation")]
		public bool breakAttack;

		[HideInInspector]
		public UnityEvent onDefense;

		public void OnDefense()
		{
			onDefense.Invoke();
		}
	}
}
