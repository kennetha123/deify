using UnityEngine;

namespace EnviroSamples
{
	public class EventTest : MonoBehaviour
	{
		private void Start()
		{
			EnviroSkyMgr.instance.OnWeatherChanged += delegate(EnviroWeatherPreset type)
			{
				DoOnWeatherChange(type);
				Debug.Log("Weather changed to: " + type.Name);
			};
			EnviroSkyMgr.instance.OnZoneChanged += delegate(EnviroZone z)
			{
				DoOnZoneChange(z);
				Debug.Log("ChangedZone: " + z.zoneName);
			};
			EnviroSkyMgr.instance.OnSeasonChanged += delegate
			{
				Debug.Log("Season changed");
			};
			EnviroSkyMgr.instance.OnHourPassed += delegate
			{
				Debug.Log("Hour Passed!");
			};
			EnviroSkyMgr.instance.OnDayPassed += delegate
			{
				Debug.Log("New Day!");
			};
			EnviroSkyMgr.instance.OnYearPassed += delegate
			{
				Debug.Log("New Year!");
			};
		}

		private void DoOnWeatherChange(EnviroWeatherPreset type)
		{
			bool flag = type.Name == "Light Rain";
		}

		private void DoOnZoneChange(EnviroZone type)
		{
			bool flag = type.zoneName == "Swamp";
		}

		public void TestEventsWWeather()
		{
			MonoBehaviour.print("Weather Changed though interface!");
		}

		public void TestEventsNight()
		{
			MonoBehaviour.print("Night now!!");
		}

		public void TestEventsDay()
		{
			MonoBehaviour.print("Day now!!");
		}
	}
}
