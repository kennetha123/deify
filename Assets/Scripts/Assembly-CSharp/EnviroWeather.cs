using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnviroWeather
{
	[Tooltip("If disabled the weather will never change.")]
	public bool updateWeather = true;

	public List<EnviroWeatherPreset> weatherPresets = new List<EnviroWeatherPreset>();

	public List<EnviroWeatherPrefab> WeatherPrefabs = new List<EnviroWeatherPrefab>();

	[Tooltip("List of additional zones. Will be updated on startup!")]
	public List<EnviroZone> zones = new List<EnviroZone>();

	public EnviroWeatherPreset startWeatherPreset;

	[Tooltip("The current active zone.")]
	public EnviroZone currentActiveZone;

	[Tooltip("The current active weather conditions.")]
	public EnviroWeatherPrefab currentActiveWeatherPrefab;

	public EnviroWeatherPreset currentActiveWeatherPreset;

	[HideInInspector]
	public EnviroWeatherPrefab lastActiveWeatherPrefab;

	[HideInInspector]
	public EnviroWeatherPreset lastActiveWeatherPreset;

	[HideInInspector]
	public GameObject VFXHolder;

	[HideInInspector]
	public float wetness;

	[HideInInspector]
	public float curWetness;

	[HideInInspector]
	public float snowStrength;

	[HideInInspector]
	public float curSnowStrength;

	[HideInInspector]
	public int thundersfx;

	[HideInInspector]
	public EnviroAudioSource currentAudioSource;

	[HideInInspector]
	public bool weatherFullyChanged;

	[HideInInspector]
	public float currentTemperature;
}
