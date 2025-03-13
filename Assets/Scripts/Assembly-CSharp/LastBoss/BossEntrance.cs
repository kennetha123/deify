using Invector.vCharacterController.AI;
using UnityEngine;

namespace LastBoss
{
	public class BossEntrance : MonoBehaviour
	{
		public GameObject[] activatedWhenEnter;

		public GameObject Boss;

		private void Start()
		{
			for (int i = 0; i < activatedWhenEnter.Length; i++)
			{
				activatedWhenEnter[i].SetActive(false);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.tag == "Player")
			{
				for (int i = 0; i < activatedWhenEnter.Length; i++)
				{
					activatedWhenEnter[i].SetActive(true);
				}
			}
		}

		private void Update()
		{
			if (Boss.GetComponent<vControlAIMelee>().isDead)
			{
				for (int i = 0; i < activatedWhenEnter.Length; i++)
				{
					activatedWhenEnter[i].SetActive(false);
				}
				Object.Destroy(base.gameObject);
			}
		}
	}
}
