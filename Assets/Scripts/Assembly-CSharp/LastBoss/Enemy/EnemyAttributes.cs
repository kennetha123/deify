using Invector;
using LastBoss.Character;
using UnityEngine;

namespace LastBoss.Enemy
{
	public class EnemyAttributes : MonoBehaviour, IStrengthen
	{
		[Tooltip("How many sins player would like to have.")]
		public int sinsValue;

		[Tooltip("Prefab object to sign enemy as the target")]
		public GameObject spotLight;

		[Tooltip("Prefab of enemy had sins power")]
		public GameObject revengePower;

		[Tooltip("How many percent stronger the enemy when eating the player souls.")]
		public int percentageUpDamage;

		[HideInInspector]
		public bool isRevengeTarget;

		[HideInInspector]
		public int expContainer;

		[HideInInspector]
		public int expGet;

		[HideInInspector]
		public bool isGetExp;

		[HideInInspector]
		public float damageUpgradeSender;

		private RevengeMode revenge;

		private int expBeforeAdded;

		private float tempValue;

		private bool isDead;

		private void Awake()
		{
			expContainer = PlayerPrefs.GetInt("exp " + base.gameObject.name);
			PlayerPrefs.SetInt("exp " + base.gameObject.name, 0);
		}

		private void Start()
		{
			revenge = Object.FindObjectOfType<RevengeMode>();
			if (expContainer != 0)
			{
				revengePower.SetActive(true);
			}
			damageUpgradeSender = CalculateStrengthWhenPowerUp(percentageUpDamage);
		}

		private void Update()
		{
			if (isRevengeTarget)
			{
				Object.FindObjectOfType<vGameController>().thereIsEnemyInRevenge = true;
				spotLight.SetActive(true);
			}
			else
			{
				spotLight.SetActive(false);
			}
			if (isGetExp)
			{
				PlayerPrefs.SetInt("exp " + base.gameObject.name, expGet + expContainer);
			}
			if (isDead && tempValue <= 1.5f)
			{
				revenge.expToShow = (int)Mathf.Lerp(expBeforeAdded, revenge.experience, tempValue);
				tempValue += 1f * Time.deltaTime;
			}
		}

		public float CalculateStrengthWhenPowerUp(float value)
		{
			return 100f * value / 10000f + 1f;
		}

		public void OnDeath()
		{
			expBeforeAdded = revenge.experience;
			isDead = true;
			if (revenge.isRevengeMode)
			{
				revenge.experience = revenge.experience + sinsValue + expContainer;
			}
			else
			{
				revenge.experience += sinsValue;
			}
			if (isRevengeTarget)
			{
				revenge.isRevengeModeAvailable = false;
				revenge.isRevengeMode = false;
				revenge.StopRevenge();
			}
		}
	}
}
