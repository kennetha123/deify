using LastBoss.Character;
using UnityEngine;

namespace LastBoss.Weapon
{
	public class ModelSwapper : MonoBehaviour
	{
		private RevengeMode revengeMode;

		public MeshFilter weaponMesh;

		public Mesh normalWeaponMesh;

		public Mesh revengeModeWeaponMesh;

		private void Start()
		{
			revengeMode = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>();
		}

		private void Update()
		{
			if (revengeMode.isRevengeMode)
			{
				weaponMesh.mesh = revengeModeWeaponMesh;
			}
			else
			{
				weaponMesh.mesh = normalWeaponMesh;
			}
		}
	}
}
