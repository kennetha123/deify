using System;
using UnityEngine;

[Serializable]
public class EnviroVolumeCloudsQualitySettings
{
	public enum ReprojectionPixelSize
	{
		Off = 0,
		Low = 1,
		Medium = 2,
		High = 3
	}

	public enum CloudDetailQuality
	{
		Low = 0,
		High = 1
	}

	[Header("Clouds Height Settings")]
	[Tooltip("Clouds start height.")]
	public float bottomCloudHeight = 3000f;

	[Tooltip("Clouds end height.")]
	public float topCloudHeight = 7000f;

	[Header("Raymarch Step Settings")]
	[Range(32f, 256f)]
	[Tooltip("Number of raymarching samples.")]
	public int raymarchSteps = 150;

	[Tooltip("Increase performance by using less steps when clouds are hidden by objects.")]
	[Range(0.1f, 1f)]
	public float stepsInDepthModificator = 0.75f;

	[Range(1f, 8f)]
	[Header("Resolution, Upsample and Reprojection")]
	[Tooltip("Downsampling of clouds rendering. 1 = full res, 2 = half Res, ...")]
	public int cloudsRenderResolution = 1;

	public ReprojectionPixelSize reprojectionPixelSize;

	[Tooltip("Enable this to use bilateral upsampling. Deactivate when using Low reprojection settings to save some actions. Activate when using higher settings for sharper image.")]
	public bool bilateralUpsampling;

	[Header("Clouds Modelling")]
	[Tooltip("LOD Distance for using lower res 3d texture for far away clouds. ")]
	[Range(0f, 1f)]
	public float lodDistance = 0.5f;

	[Tooltip("The UV scale of base noise. High Values = Low performance!")]
	[Range(2f, 100f)]
	public float baseNoiseUV = 20f;

	[Tooltip("The UV scale of detail noise. High Values = Low performance!")]
	[Range(2f, 100f)]
	public float detailNoiseUV = 50f;

	[Tooltip("Resolution of Detail Noise Texture.")]
	public CloudDetailQuality detailQuality;
}
