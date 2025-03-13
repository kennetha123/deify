using System.Collections;
using Invector;
using Invector.vCamera;
using Invector.vCharacterController;
using LastBoss.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace LastBoss.Character
{
	public class RevengeMode : CharacterBase, IStrengthen
	{
		public GenericInput revengeManualButton = new GenericInput("E", "RB", "RB");

		[Tooltip("How many points to reach maximum revenge mode.")]
		public int revengeModeFullPoints;

		[Tooltip("Object of revenge mode.")]
		public GameObject revengePrefab;

		[Tooltip("Custom revenge prefab.")]
		public GameObject eyeRevenge;

		[Tooltip("How many health do you want to decrease / second.")]
		public int healthDecreasePerSec = 100;

		[Tooltip("How many percent stronger the enemy when eating the player souls.")]
		public int percentageUpDamage;

		public PostProcessVolume postProfile;

		public PostProcessProfile normalProfile;

		public PostProcessProfile revengeProfile;

		[HideInInspector]
		public float damageUpgradeSender;

		[HideInInspector]
		public bool isRevengeMode;

		[HideInInspector]
		public bool isRevengeModeAvailable;

		[HideInInspector]
		public int revengeModePoints;

		private vThirdPersonCamera cam;

		private vThirdPersonController character;

		private SpiritModeController spiritController;

		public GameObject spiritEntrance;

		public GameObject spiritModeObject;

		private IEnumerator decreaseHealth;

		public int expToShow;

		[HideInInspector]
		public bool isDead;

		[HideInInspector]
		public bool isSpiritMode;

		private UIReader ui;

		public SkinnedMeshRenderer[] meshBody;

		public Material revengeShader;

		public Material[] normalShader;

		public override void Start()
		{
			Init();
		}

		private void ChangeMaterialInRevenge(bool isStart)
		{
			for (int i = 0; i < meshBody.Length; i++)
			{
				if (isStart)
				{
					meshBody[i].material = revengeShader;
				}
				else
				{
					meshBody[i].material = normalShader[i];
				}
			}
		}

		public override void Update()
		{
			if (revengeModePoints >= revengeModeFullPoints)
			{
				isRevengeModeAvailable = true;
			}
			if (isSpiritMode && revengeManualButton.GetButtonDown())
			{
				GetoutSpiritMode();
			}
		}

		public void Init()
		{
			ui = Object.FindObjectOfType<UIReader>();
			cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<vThirdPersonCamera>();
			character = base.gameObject.GetComponent<vThirdPersonController>();
			if (postProfile == null)
			{
				postProfile = Object.FindObjectOfType<PostProcessVolume>();
			}
			revengeModePoints = 0;
			isDead = false;
			damageUpgradeSender = CalculateStrengthWhenPowerUp(percentageUpDamage);
			StopRevenge();
		}

		public void GetoutSpiritMode()
		{
			cam.isSpiritMode = false;
			cam.isSpiritDone = false;
			base.transform.position = spiritController.transform.position;
			base.transform.rotation = spiritController.transform.rotation;
			spiritController = GameObject.FindGameObjectWithTag("Spirit").GetComponent<SpiritModeController>();
			StartRevenge();
			Object.Destroy(GameObject.FindGameObjectWithTag("Spirit"));
			spiritController.timerDecrease = spiritController.maxTimer;
			spiritController.distancePower = spiritController.maxDistance;
			ui.DisableSpiritMode();
			isSpiritMode = false;
		}

		public void EnterSpiritMode()
		{
			isSpiritMode = true;
			Object.Instantiate(spiritModeObject, base.transform.position, base.transform.rotation);
			spiritController = GameObject.FindGameObjectWithTag("Spirit").GetComponent<SpiritModeController>();
		}

		public void StartRevenge()
		{
			character.animator.SetBool("IsRevengeMode", true);
			PostProcessingRevenge(true);
			Object.Instantiate(spiritEntrance, base.transform.position, base.transform.rotation);
			isSpiritMode = false;
			isRevengeMode = true;
			character.ResetHealth();
			revengePrefab.SetActive(true);
			eyeRevenge.SetActive(true);
			decreaseHealth = GetComponent<vHealthController>().DecreaseHealthCauseRevenge();
			StartCoroutine(decreaseHealth);
			revengeModePoints = 0;
			isRevengeModeAvailable = false;
			ChangeMaterialInRevenge(true);
		}

		public void StopRevenge()
		{
			PostProcessingRevenge(false);
			if (decreaseHealth != null)
			{
				StopCoroutine(decreaseHealth);
				decreaseHealth = null;
			}
			revengePrefab.SetActive(false);
			eyeRevenge.SetActive(false);
			isRevengeMode = false;
			isRevengeModeAvailable = false;
			revengeModePoints = 0;
			ChangeMaterialInRevenge(false);
			Object.FindObjectOfType<vGameController>().thereIsEnemyInRevenge = false;
			character.Init();
			character.animator.SetBool("IsRevengeMode", false);
		}

		private void PostProcessingRevenge(bool isActive)
		{
			if (isActive)
			{
				postProfile.profile = revengeProfile;
			}
			else
			{
				postProfile.profile = normalProfile;
			}
		}

		public float CalculateStrengthWhenPowerUp(float value)
		{
			return 100f * value / 10000f + 1f;
		}
	}
}
