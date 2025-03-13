using System;
using UnityEngine;

[Serializable]
public class EnviroWeatherCloudsConfig
{
	[Tooltip("Ambient Light Intensity.")]
	[Range(0f, 1f)]
	public float ambientSkyColorIntensity = 1f;

	[Tooltip("Light extinction factor.")]
	[Range(0f, 2f)]
	public float scatteringCoef = 1f;

	[Tooltip("Darkens the edges of clouds from in-out scattering.")]
	[Range(1f, 3f)]
	public float edgeDarkness = 2f;

	public float baseErosionIntensity;

	public float detailErosionIntensity = 0.2f;

	[Tooltip("Density factor of clouds.")]
	public float density = 1f;

	[Tooltip("Density factor of clouds lighting.")]
	public float densityLightning = 1f;

	[Tooltip("Global coverage multiplicator of clouds.")]
	[Range(0f, 1f)]
	public float coverage = 1f;

	[Tooltip("Add or remove Coverage at the top")]
	[Range(0f, 2f)]
	public float coverageModTop = 2f;

	[Tooltip("Add or remove Coverage at the bottom")]
	[Range(0f, 1f)]
	public float coverageModBottom = 1f;

	[Tooltip("Coverage type of clouds. 1 = more round scattered shapes , 0 = connected islands style")]
	[Range(0f, 1f)]
	public float coverageType = 1f;

	[Tooltip("Clouds raynarching step modifier.")]
	[Range(0.25f, 1f)]
	public float raymarchingScale = 1f;

	[Tooltip("Clouds modelling type.")]
	[Range(0f, 1f)]
	public float cloudType = 1f;

	[Tooltip("Cirrus Clouds Alpha")]
	[Range(0f, 1f)]
	public float cirrusAlpha;

	[Tooltip("Cirrus Clouds Coverage")]
	[Range(0f, 1f)]
	public float cirrusCoverage;

	[Tooltip("Cirrus Clouds Color Power")]
	[Range(0f, 1f)]
	public float cirrusColorPow = 2f;

	[Tooltip("Flat Clouds Alpha")]
	[Range(0f, 1f)]
	public float flatAlpha;

	[Tooltip("Flat Clouds Coverage")]
	[Range(0f, 1f)]
	public float flatCoverage;

	[Tooltip("Flat Clouds Softness")]
	[Range(0f, 1f)]
	public float flatSoftness = 0.75f;

	[Tooltip("Flat Clouds Brightness")]
	[Range(0f, 1f)]
	public float flatBrightness = 0.75f;

	[Tooltip("Flat Clouds Color Power")]
	[Range(0f, 1f)]
	public float flatColorPow = 2f;

	[Tooltip("Particle Clouds Alpha")]
	[Range(0f, 1f)]
	public float particleLayer1Alpha;

	[Tooltip("Particle Clouds Brightness")]
	[Range(0f, 1f)]
	public float particleLayer1Brightness = 0.75f;

	[Tooltip("Particle Clouds Color Power")]
	[Range(0f, 1f)]
	public float particleLayer1ColorPow = 2f;

	[Tooltip("Particle Clouds Alpha")]
	[Range(0f, 1f)]
	public float particleLayer2Alpha;

	[Tooltip("Particle Clouds Brightness")]
	[Range(0f, 1f)]
	public float particleLayer2Brightness = 0.75f;

	[Tooltip("Particle Clouds Color Power")]
	[Range(0f, 1f)]
	public float particleLayer2ColorPow = 2f;

	[Tooltip("Use particle clouds here even when it is disabled!")]
	public bool particleCloudsOverwrite;
}
