using System.Collections;
using LastBoss.Character;
using UnityEngine;
using UnityEngine.UI;

namespace LastBoss.UI
{
	public class UIReader : MonoBehaviour
	{
		public Slider revengeSlider;

		public Slider distanceSlider;

		public Text experienceText;

		public Text timerSpirit;

		public Image background;

		private RevengeMode revenge;

		private SpiritModeController spirit;

		public GameObject[] listHiddenItem;

		private void Start()
		{
			DisableSpiritMode();
			revenge = Object.FindObjectOfType<RevengeMode>();
			revengeSlider.maxValue = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>().revengeModeFullPoints;
		}

		private void Update()
		{
			revengeSlider.value = revenge.revengeModePoints;
			experienceText.text = string.Concat(revenge.expToShow);
			if (revenge.isSpiritMode)
			{
				spirit = Object.FindObjectOfType<SpiritModeController>();
				distanceSlider.maxValue = spirit.maxDistance;
				EnableSpiritMode();
				timerSpirit.text = string.Concat(spirit.timerDecrease);
				distanceSlider.value = spirit.distancePower;
			}
		}

		private void EnableSpiritMode()
		{
			for (int i = 0; i < listHiddenItem.Length; i++)
			{
				listHiddenItem[i].SetActive(true);
			}
		}

		public void DisableSpiritMode()
		{
			for (int i = 0; i < listHiddenItem.Length; i++)
			{
				listHiddenItem[i].SetActive(false);
			}
		}

		public void UIGotDamage()
		{
			StartCoroutine(GotHit());
		}

		private IEnumerator GotHit()
		{
			background.CrossFadeColor(Color.red, 0.2f, false, true);
			yield return new WaitForSeconds(0.2f);
			background.CrossFadeColor(Color.white, 0.2f, false, true);
		}

		public void UIStaminaOut()
		{
		}
	}
}
