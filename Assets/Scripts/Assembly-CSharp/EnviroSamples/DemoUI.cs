using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EnviroSamples
{
	public class DemoUI : MonoBehaviour
	{
		public Slider sliderTime;

		private Slider sliderQuality;

		public Text timeText;

		public Text weatherText;

		public Text temperatureText;

		public Dropdown weatherDropdown;

		private bool started;

		private void Start()
		{
			if (EnviroSkyMgr.instance == null || !EnviroSkyMgr.instance.IsAvailable())
			{
				base.enabled = false;
				return;
			}
			EnviroSkyMgr.instance.OnWeatherChanged += delegate
			{
				UpdateWeatherSlider();
			};
		}

		private IEnumerator setupDrodown()
		{
			started = true;
			yield return new WaitForSeconds(0.1f);
			for (int i = 0; i < EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Count; i++)
			{
				Dropdown.OptionData optionData = new Dropdown.OptionData();
				optionData.text = EnviroSkyMgr.instance.GetCurrentWeatherPresetList()[i].Name;
				weatherDropdown.options.Add(optionData);
			}
			yield return new WaitForSeconds(0.1f);
			UpdateWeatherSlider();
		}

		public void ChangeTimeSlider()
		{
			if (sliderTime.value < 0f)
			{
				sliderTime.value = 0f;
			}
			EnviroSkyMgr.instance.SetTimeOfDay(sliderTime.value * 24f);
		}

		public void ChangeCloudQuality(int value)
		{
			EnviroSky.instance.ApplyVolumeCloudsQualityPreset(value);
		}

		public void ChangeAmbientVolume(float value)
		{
			EnviroSkyMgr.instance.ambientAudioVolume = value;
		}

		public void ChangeWeatherVolume(float value)
		{
			EnviroSkyMgr.instance.weatherAudioVolume = value;
		}

		public void SetWeatherID(int id)
		{
			EnviroSkyMgr.instance.ChangeWeather(id);
		}

		public void SetVolumeClouds(bool b)
		{
			EnviroSkyMgr.instance.useVolumeClouds = b;
		}

		public void SetVolumeLighting(bool b)
		{
			EnviroSkyMgr.instance.useVolumeLighting = b;
		}

		public void SetFlatClouds(bool b)
		{
			EnviroSkyMgr.instance.useFlatClouds = b;
		}

		public void SetParticleClouds(bool b)
		{
			EnviroSkyMgr.instance.useParticleClouds = b;
		}

		public void SetSunShafts(bool b)
		{
			EnviroSkyMgr.instance.useSunShafts = b;
		}

		public void SetMoonShafts(bool b)
		{
			EnviroSkyMgr.instance.useMoonShafts = b;
		}

		public void SetSeason(int id)
		{
			switch (id)
			{
			case 0:
				EnviroSkyMgr.instance.ChangeSeason(EnviroSeasons.Seasons.Spring);
				break;
			case 1:
				EnviroSkyMgr.instance.ChangeSeason(EnviroSeasons.Seasons.Summer);
				break;
			case 2:
				EnviroSkyMgr.instance.ChangeSeason(EnviroSeasons.Seasons.Autumn);
				break;
			case 3:
				EnviroSkyMgr.instance.ChangeSeason(EnviroSeasons.Seasons.Winter);
				break;
			}
		}

		public void SetTimeProgress(int id)
		{
			switch (id)
			{
			case 0:
				EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.None);
				break;
			case 1:
				EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.Simulated);
				break;
			case 2:
				EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.SystemTime);
				break;
			}
		}

		private void UpdateWeatherSlider()
		{
			if (!(EnviroSkyMgr.instance.GetCurrentWeatherPreset() != null))
			{
				return;
			}
			for (int i = 0; i < weatherDropdown.options.Count; i++)
			{
				if (weatherDropdown.options[i].text == EnviroSkyMgr.instance.GetCurrentWeatherPreset().Name)
				{
					weatherDropdown.value = i;
				}
			}
		}

		private void Update()
		{
			if (EnviroSkyMgr.instance.IsStarted())
			{
				if (!started)
				{
					StartCoroutine(setupDrodown());
				}
				timeText.text = EnviroSkyMgr.instance.GetTimeString();
				if (EnviroSkyMgr.instance.GetCurrentWeatherPreset() != null)
				{
					weatherText.text = EnviroSkyMgr.instance.GetCurrentWeatherPreset().Name;
				}
				temperatureText.text = EnviroSkyMgr.instance.GetCurrentTemperatureString();
			}
		}

		private void LateUpdate()
		{
			sliderTime.value = EnviroSkyMgr.instance.GetUniversalTimeOfDay() / 24f;
		}
	}
}
