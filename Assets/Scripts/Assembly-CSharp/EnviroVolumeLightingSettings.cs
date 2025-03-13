using System;
using UnityEngine;

[Serializable]
public class EnviroVolumeLightingSettings
{
	[Tooltip("Downsampling of volume light rendering.")]
	public EnviroSkyRendering.VolumtericResolution Resolution = EnviroSkyRendering.VolumtericResolution.Quarter;

	[Tooltip("Activate or deactivate directional volume light rendering.")]
	public bool dirVolumeLighting = true;

	[Header("Quality")]
	[Range(1f, 64f)]
	public int SampleCount = 8;

	[Header("Light Settings")]
	public AnimationCurve ScatteringCoef = new AnimationCurve();

	[Range(0f, 0.1f)]
	public float ExtinctionCoef = 0.05f;

	[Range(0f, 0.999f)]
	public float Anistropy = 0.1f;

	public float MaxRayLength = 10f;

	[Header("3D Noise")]
	[Tooltip("Use 3D noise for directional lighting. Attention: Expensive operation for directional lights with high sample count!")]
	public bool directLightNoise;

	[Range(0f, 1f)]
	[Tooltip("The noise intensity volume lighting.")]
	public float noiseIntensity = 1f;

	[Tooltip("The noise intensity offset of volume lighting.")]
	[Range(0f, 1f)]
	public float noiseIntensityOffset = 0.3f;

	[Range(0f, 0.1f)]
	[Tooltip("The noise scaling of volume lighting.")]
	public float noiseScale = 0.001f;

	[Tooltip("The speed and direction of volume lighting.")]
	public Vector2 noiseVelocity = new Vector2(3f, 1.5f);
}
