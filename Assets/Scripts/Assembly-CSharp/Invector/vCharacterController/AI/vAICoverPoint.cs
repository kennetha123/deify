using UnityEngine;
using UnityEngine.AI;

namespace Invector.vCharacterController.AI
{
	[RequireComponent(typeof(BoxCollider))]
	[vClassHeader("AI Cover Point", true, "icon_v2", false, "")]
	public class vAICoverPoint : vMonoBehaviour
	{
		public float posePositionZ = 0.5f;

		public BoxCollider boxCollider;

		public bool isOccuped;

		public Vector3 posePosition
		{
			get
			{
				return base.transform.position + base.transform.forward * posePositionZ;
			}
		}

		private void Awake()
		{
			if ((bool)boxCollider)
			{
				boxCollider.isTrigger = true;
			}
		}

		private void Start()
		{
			NavMeshHit hit;
			if (!NavMesh.SamplePosition(posePosition, out hit, 0.2f, -1))
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
