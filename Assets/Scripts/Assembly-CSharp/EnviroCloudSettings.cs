using System;
using UnityEngine;

[Serializable]
public class EnviroCloudSettings
{
	public enum FlatCloudResolution
	{
		R512 = 0,
		R1024 = 1,
		R2048 = 2,
		R4096 = 3
	}

	public EnviroVolumeCloudsQualitySettings cloudsQualitySettings;

	[Range(10000f, 486000f)]
	[Tooltip("Clouds world scale. This settings will influece rendering of clouds at horizon.")]
	public float cloudsWorldScale = 113081f;

	[Tooltip("Change Clouds Height.")]
	[Range(-2000f, 2000f)]
	public float cloudsHeightMod;

	[Header("Clouds Wind Animation")]
	public bool useWindZoneDirection;

	[Range(-1f, 1f)]
	[Tooltip("Time scale / wind animation speed of clouds.")]
	public float cloudsTimeScale = 1f;

	[Range(0f, 1f)]
	[Tooltip("Global clouds wind speed modificator.")]
	public float cloudsWindIntensity = 0.001f;

	[Range(0f, 1f)]
	[Tooltip("Global clouds upwards wind speed modificator.")]
	public float cloudsUpwardsWindIntensity = 0.001f;

	[Range(0f, 1f)]
	[Tooltip("Cirrus clouds wind speed modificator.")]
	public float cirrusWindIntensity = 0.001f;

	[Range(-1f, 1f)]
	[Tooltip("Global clouds wind direction X axes.")]
	public float cloudsWindDirectionX = 1f;

	[Range(-1f, 1f)]
	[Tooltip("Global clouds wind direction Y axes.")]
	public float cloudsWindDirectionY = 1f;

	[Header("Clouds Lighting")]
	[Tooltip("Sun highlight in near of sun.")]
	[Range(0.01f, 1f)]
	public float hgPhase = 0.5f;

	[Tooltip("Sun highlight in away from sun.")]
	[Range(0.01f, 1f)]
	public float silverLiningIntensity = 0.5f;

	[Tooltip("Sun highlight in away from sun.")]
	[Range(0.01f, 0.98f)]
	public float silverLiningSpread = 0.5f;

	[Tooltip("Global Color for volume clouds based sun positon.")]
	public Gradient volumeCloudsColor;

	[Tooltip("Global Color for clouds based moon positon.")]
	public Gradient volumeCloudsMoonColor;

	[Tooltip("Raie or lower the light intensity based on sun altitude.")]
	public AnimationCurve lightIntensity = new AnimationCurve();

	[Tooltip("Tweak the ambient lighting from sky based on sun altitude.")]
	public AnimationCurve ambientLightIntensity = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[Header("Tonemapping")]
	[Tooltip("Use color tonemapping?")]
	public bool tonemapping;

	[Tooltip("Tonemapping exposure")]
	public float cloudsExposure = 1f;

	[Header("Weather Map")]
	[Tooltip("Tiling of the generated weather map.")]
	public int weatherMapTiling = 5;

	[Tooltip("Option to add own weather map. Red Channel = Coverage, Blue = Clouds Height")]
	public Texture2D customWeatherMap;

	[Tooltip("Weathermap sampling offset.")]
	public Vector2 locationOffset;

	[Range(0f, 1f)]
	[Tooltip("Weathermap animation speed.")]
	public float weatherAnimSpeedScale = 0.33f;

	[Header("Global Clouds Control")]
	[Range(0f, 2f)]
	public float globalCloudCoverage = 1f;

	[Tooltip("Texture for cirrus clouds.")]
	public Texture cirrusCloudsTexture;

	[Tooltip("Global Color for flat clouds based sun positon.")]
	public Gradient cirrusCloudsColor;

	[Range(5f, 15f)]
	[Tooltip("Flat Clouds Altitude")]
	public float cirrusCloudsAltitude = 10f;

	[Tooltip("Texture for flat procedural clouds.")]
	public Texture flatCloudsNoiseTexture;

	[Tooltip("Resolution of generated flat clouds texture.")]
	public FlatCloudResolution flatCloudsResolution = FlatCloudResolution.R2048;

	[Tooltip("Global Color for flat clouds based sun positon.")]
	public Gradient flatCloudsColor;

	[Tooltip("Scale/Tiling of flat clouds.")]
	public float flatCloudsScale = 2f;

	[Range(1f, 12f)]
	[Tooltip("Flat Clouds texture generation iterations.")]
	public int flatCloudsNoiseOctaves = 6;

	[Range(30f, 100f)]
	[Tooltip("Flat Clouds Altitude")]
	public float flatCloudsAltitude = 70f;

	[Range(0.01f, 1f)]
	[Tooltip("Flat Clouds morphing animation speed.")]
	public float flatCloudsMorphingSpeed = 0.2f;

	[Tooltip("Clouds Shadowcast Intensity. 0 = disabled")]
	[Range(0f, 1f)]
	public float shadowIntensity;

	[Tooltip("Size of the shadow cookie.")]
	[Range(1000f, 100000f)]
	public int shadowCookieSize = 100000;

	public EnviroParticleClouds ParticleCloudsLayer1 = new EnviroParticleClouds();

	public EnviroParticleClouds ParticleCloudsLayer2 = new EnviroParticleClouds();
}
