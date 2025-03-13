using System;
using UnityEngine;

[Serializable]
public class EnviroLightShaftsSettings
{
	[Header("Quality Settings")]
	[Tooltip("Lightshafts resolution quality setting.")]
	public EnviroPostProcessing.SunShaftsResolution resolution = EnviroPostProcessing.SunShaftsResolution.Normal;

	[Tooltip("Lightshafts blur mode.")]
	public EnviroPostProcessing.ShaftsScreenBlendMode screenBlendMode;

	[Tooltip("Use cameras depth to hide lightshafts?")]
	public bool useDepthTexture = true;

	[Header("Intensity Settings")]
	[Tooltip("Color gradient for lightshafts based on sun position.")]
	public Gradient lightShaftsColorSun;

	[Tooltip("Color gradient for lightshafts based on moon position.")]
	public Gradient lightShaftsColorMoon;

	[Tooltip("Treshhold gradient for lightshafts based on sun position. This will influence lightshafts intensity!")]
	public Gradient thresholdColorSun;

	[Tooltip("Treshhold gradient for lightshafts based on moon position. This will influence lightshafts intensity!")]
	public Gradient thresholdColorMoon;

	[Tooltip("Radius of blurring applied.")]
	public float blurRadius = 6f;

	[Tooltip("Global Lightshafts intensity.")]
	public float intensity = 0.6f;

	[Tooltip("Lightshafts maximum radius.")]
	public float maxRadius = 10f;
}
