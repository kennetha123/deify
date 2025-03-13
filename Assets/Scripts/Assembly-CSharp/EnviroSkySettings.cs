using System;
using UnityEngine;

[Serializable]
public class EnviroSkySettings
{
	public enum SunAndMoonCalc
	{
		Simple = 0,
		Realistic = 1
	}

	public enum MoonPhases
	{
		Custom = 0,
		Realistic = 1
	}

	public enum SkyboxModi
	{
		Default = 0,
		Simple = 1,
		CustomSkybox = 2,
		CustomColor = 3
	}

	public enum SkyboxModiLW
	{
		Simple = 0,
		CustomSkybox = 1,
		CustomColor = 2
	}

	[Header("Sky Mode:")]
	[Tooltip("Select if you want to use enviro skybox your custom material.")]
	public SkyboxModi skyboxMode;

	[Tooltip("Select if you want to use enviro skybox your custom material.")]
	public SkyboxModiLW skyboxModeLW;

	[Tooltip("If SkyboxMode == CustomSkybox : Assign your skybox material here!")]
	public Material customSkyboxMaterial;

	[Tooltip("If SkyboxMode == CustomColor : Select your sky color here!")]
	public Color customSkyboxColor;

	[Tooltip("Enable to render black skybox at ground level.")]
	public bool blackGroundMode;

	[Header("Scattering")]
	[Tooltip("Light Wavelength used for atmospheric scattering. Keep it near defaults for earthlike atmospheres, or change for alien or fantasy atmospheres for example.")]
	public Vector3 waveLength = new Vector3(540f, 496f, 437f);

	[Tooltip("Influence atmospheric scattering.")]
	public float rayleigh = 5.15f;

	[Tooltip("Sky turbidity. Particle in air. Influence atmospheric scattering.")]
	public float turbidity = 1f;

	[Tooltip("Influence scattering near sun.")]
	public float mie = 5f;

	[Tooltip("Influence scattering near sun.")]
	public float g = 0.8f;

	[Tooltip("Intensity gradient for atmospheric scattering. Influence atmospheric scattering based on current sun altitude.")]
	public AnimationCurve scatteringCurve = new AnimationCurve();

	[Tooltip("Color gradient for atmospheric scattering. Influence atmospheric scattering based on current sun altitude.")]
	public Gradient scatteringColor;

	[Header("Sun")]
	public SunAndMoonCalc sunAndMoonPosition = SunAndMoonCalc.Realistic;

	[Tooltip("Intensity of Sun Influence Scale and Dropoff of sundisk.")]
	public float sunIntensity = 100f;

	[Tooltip("Scale of rendered sundisk.")]
	public float sunDiskScale = 20f;

	[Tooltip("Intenisty of rendered sundisk.")]
	public float sunDiskIntensity = 3f;

	[Tooltip("Color gradient for sundisk. Influence sundisk color based on current sun altitude")]
	public Gradient sunDiskColor;

	[Tooltip("Top color of simple skybox.")]
	public Gradient simpleSkyColor;

	[Tooltip("Horizon color of simple skybox.")]
	public Gradient simpleHorizonColor;

	[Tooltip("Sun color of simple skybox.")]
	public Gradient simpleSunColor;

	[Tooltip("Size of sun in simple skybox mode.")]
	public AnimationCurve simpleSunDiskSize = new AnimationCurve();

	[Header("Moon")]
	[Tooltip("Whether to render the moon.")]
	public bool renderMoon = true;

	[Tooltip("The Moon phase mode. Custom = for customizable phase.")]
	public MoonPhases moonPhaseMode = MoonPhases.Realistic;

	[Tooltip("The Moon texture.")]
	public Texture moonTexture;

	[Tooltip("The Moon's Glow texture.")]
	public Texture glowTexture;

	[Tooltip("The color of the moon")]
	public Color moonColor;

	[Range(0f, 5f)]
	[Tooltip("Brightness of the moon.")]
	public float moonBrightness = 1f;

	[Range(0f, 20f)]
	[Tooltip("Size of the moon.")]
	public float moonSize = 10f;

	[Range(0f, 20f)]
	[Tooltip("Size of the moon glowing effect.")]
	public float glowSize = 10f;

	[Tooltip("Glow around moon.")]
	public AnimationCurve moonGlow = new AnimationCurve();

	[Tooltip("Glow color around moon.")]
	public Color moonGlowColor;

	[Tooltip("Start moon phase when using custom phase mode.(-1f - 1f)")]
	[Range(-1f, 1f)]
	public float startMoonPhase;

	[Header("Sky Color Corrections")]
	[Tooltip("Higher values = brighter sky.")]
	public AnimationCurve skyLuminence = new AnimationCurve();

	[Tooltip("Higher values = stronger colors applied BEFORE clouds rendered!")]
	public AnimationCurve skyColorPower = new AnimationCurve();

	[Header("Tonemapping - LDR")]
	[Tooltip("Tonemapping when using LDR")]
	public float skyExposure = 1.5f;

	[Header("Stars")]
	[Tooltip("A cubemap for night sky.")]
	public Cubemap starsCubeMap;

	[Tooltip("Intensity of stars based on time of day.")]
	public AnimationCurve starsIntensity = new AnimationCurve();

	[Tooltip("Stars Twinkling Speed")]
	[Range(0f, 10f)]
	public float starsTwinklingRate = 1f;

	[Header("Galaxy")]
	[Tooltip("A cubemap for night galaxy.")]
	public Cubemap galaxyCubeMap;

	[Tooltip("Intensity of galaxy based on time of day.")]
	public AnimationCurve galaxyIntensity = new AnimationCurve();

	[Header("Sky Dithering")]
	public bool dithering = true;
}
