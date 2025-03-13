using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnviroWeatherPreset : ScriptableObject
{
	public string version;

	public string Name;

	[Header("Season Settings")]
	public bool Spring = true;

	[Range(1f, 100f)]
	public float possibiltyInSpring = 50f;

	public bool Summer = true;

	[Range(1f, 100f)]
	public float possibiltyInSummer = 50f;

	public bool Autumn = true;

	[Range(1f, 100f)]
	public float possibiltyInAutumn = 50f;

	public bool winter = true;

	[Range(1f, 100f)]
	public float possibiltyInWinter = 50f;

	[Header("Cloud Settings")]
	public EnviroWeatherCloudsConfig cloudsConfig;

	[Header("Linear Fog")]
	public float fogStartDistance;

	public float fogDistance = 1000f;

	[Header("Exp Fog")]
	public float fogDensity = 0.0001f;

	[Tooltip("Used to modify sky, direct, ambient light and fog color. The color alpha value defines the intensity")]
	public Gradient weatherSkyMod;

	public Gradient weatherLightMod;

	public Gradient weatherFogMod;

	[Range(0f, 2f)]
	public float volumeLightIntensity = 1f;

	[Range(-1f, 1f)]
	public float shadowIntensityMod;

	[Range(0f, 100f)]
	[Tooltip("The density of height based fog for this weather.")]
	public float heightFogDensity = 1f;

	[Range(0f, 2f)]
	[Tooltip("Define the height of fog rendered in sky.")]
	public float SkyFogHeight = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("Define the start height of fog rendered in sky.")]
	public float skyFogStart;

	[Tooltip("Define the intensity of fog rendered in sky.")]
	[Range(0f, 2f)]
	public float SkyFogIntensity = 1f;

	[Range(1f, 10f)]
	[Tooltip("Define the scattering intensity of fog.")]
	public float FogScatteringIntensity = 1f;

	[Range(0f, 1f)]
	[Tooltip("Block the sundisk with fog.")]
	public float fogSunBlocking = 0.25f;

	[Range(0f, 1f)]
	[Tooltip("Block the moon with fog.")]
	public float moonIntensity = 1f;

	[Header("Weather Settings")]
	public List<EnviroWeatherEffects> effectSystems = new List<EnviroWeatherEffects>();

	[Range(0f, 1f)]
	[Tooltip("Wind intensity that will applied to wind zone.")]
	public float WindStrenght = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("The maximum wetness level that can be reached.")]
	public float wetnessLevel;

	[Range(0f, 1f)]
	[Tooltip("The maximum snow level that can be reached.")]
	public float snowLevel;

	[Range(-50f, 50f)]
	[Tooltip("The temperature modifcation for this weather type. (Will be added or substracted)")]
	public float temperatureLevel;

	[Tooltip("Activate this to enable thunder and lightning.")]
	public bool isLightningStorm;

	[Range(0f, 2f)]
	[Tooltip("The Intervall of lightning in seconds. Random(lightningInterval,lightningInterval * 2). ")]
	public float lightningInterval = 10f;

	[Header("Aurora Settings")]
	[Range(0f, 1f)]
	public float auroraIntensity;

	[Header("Audio Settings - SFX")]
	[Tooltip("Define an sound effect for this weather preset.")]
	public AudioClip weatherSFX;

	[Header("Audio Settings - Ambient")]
	[Tooltip("This sound wil be played in spring at day.(looped)")]
	public AudioClip SpringDayAmbient;

	[Tooltip("This sound wil be played in spring at night.(looped)")]
	public AudioClip SpringNightAmbient;

	[Tooltip("This sound wil be played in summer at day.(looped)")]
	public AudioClip SummerDayAmbient;

	[Tooltip("This sound wil be played in summer at night.(looped)")]
	public AudioClip SummerNightAmbient;

	[Tooltip("This sound wil be played in autumn at day.(looped)")]
	public AudioClip AutumnDayAmbient;

	[Tooltip("This sound wil be played in autumn at night.(looped)")]
	public AudioClip AutumnNightAmbient;

	[Tooltip("This sound wil be played in winter at day.(looped)")]
	public AudioClip WinterDayAmbient;

	[Tooltip("This sound wil be played in winter at night.(looped)")]
	public AudioClip WinterNightAmbient;

	public float blurDistance = 100f;

	public float blurIntensity = 1f;

	public float blurSkyIntensity = 1f;

	public EnviroAura2Config enviroAura2Config;
}
