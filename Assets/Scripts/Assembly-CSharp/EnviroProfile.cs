using System;
using UnityEngine;

[Serializable]
public class EnviroProfile : ScriptableObject
{
	public enum settingsMode
	{
		Lighting = 0,
		Sky = 1,
		Reflections = 2,
		Weather = 3,
		Season = 4,
		Clouds = 5,
		Fog = 6,
		VolumeLighting = 7,
		Lightshafts = 8,
		DistanceBlur = 9,
		Aurora = 10,
		Satellites = 11,
		Audio = 12,
		Quality = 13
	}

	public enum settingsModeLW
	{
		Lighting = 0,
		Sky = 1,
		Reflections = 2,
		Weather = 3,
		Season = 4,
		Clouds = 5,
		Fog = 6,
		Lightshafts = 7,
		Satellites = 8,
		Audio = 9,
		Quality = 10
	}

	public string version;

	public EnviroLightSettings lightSettings = new EnviroLightSettings();

	public EnviroReflectionSettings reflectionSettings = new EnviroReflectionSettings();

	public EnviroVolumeLightingSettings volumeLightSettings = new EnviroVolumeLightingSettings();

	public EnviroDistanceBlurSettings distanceBlurSettings = new EnviroDistanceBlurSettings();

	public EnviroSkySettings skySettings = new EnviroSkySettings();

	public EnviroCloudSettings cloudsSettings = new EnviroCloudSettings();

	public EnviroWeatherSettings weatherSettings = new EnviroWeatherSettings();

	public EnviroFogSettings fogSettings = new EnviroFogSettings();

	public EnviroLightShaftsSettings lightshaftsSettings = new EnviroLightShaftsSettings();

	public EnviroSeasonSettings seasonsSettings = new EnviroSeasonSettings();

	public EnviroAudioSettings audioSettings = new EnviroAudioSettings();

	public EnviroSatellitesSettings satelliteSettings = new EnviroSatellitesSettings();

	public EnviroQualitySettings qualitySettings = new EnviroQualitySettings();

	public EnviroAuroraSettings auroraSettings = new EnviroAuroraSettings();

	[HideInInspector]
	public settingsMode viewMode;

	[HideInInspector]
	public settingsModeLW viewModeLW;

	[HideInInspector]
	public bool showPlayerSetup = true;

	[HideInInspector]
	public bool showRenderingSetup;

	[HideInInspector]
	public bool showComponentsSetup;

	[HideInInspector]
	public bool showTimeUI;

	[HideInInspector]
	public bool showWeatherUI;

	[HideInInspector]
	public bool showAudioUI;

	[HideInInspector]
	public bool showEffectsUI;

	[HideInInspector]
	public bool modified;
}
