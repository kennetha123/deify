using System;
using UnityEngine;

[Serializable]
public class EnviroAudio
{
	[Tooltip("The prefab with AudioSources used by Enviro. Will be instantiated at runtime.")]
	public GameObject SFXHolderPrefab;

	[Header("Volume Settings:")]
	[Range(0f, 1f)]
	[Tooltip("The volume of ambient sounds played by enviro.")]
	public float ambientSFXVolume = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("The volume of weather sounds played by enviro.")]
	public float weatherSFXVolume = 1f;

	[HideInInspector]
	public EnviroAudioSource currentAmbientSource;

	[HideInInspector]
	public float ambientSFXVolumeMod;

	[HideInInspector]
	public float weatherSFXVolumeMod;

	[HideInInspector]
	public EnviroAudioSource AudioSourceWeather;

	[HideInInspector]
	public EnviroAudioSource AudioSourceWeather2;

	[HideInInspector]
	public EnviroAudioSource AudioSourceAmbient;

	[HideInInspector]
	public EnviroAudioSource AudioSourceAmbient2;

	[HideInInspector]
	public EnviroAudioSource AudioSourceThunder;

	[HideInInspector]
	public EnviroAudioSource AudioSourceZone;
}
