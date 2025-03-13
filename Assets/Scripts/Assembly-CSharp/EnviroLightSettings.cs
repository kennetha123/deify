using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class EnviroLightSettings
{
	public enum LightingMode
	{
		Single = 0,
		Dual = 1
	}

	[Header("Direct")]
	[Tooltip("Whether you want to use two direcitonal lights for sun and moon or only one that will switch. Dual mode can be expensive in complex scenes!")]
	public LightingMode directionalLightMode;

	[Tooltip("Color gradient for sun and moon light based on sun position in sky.")]
	public Gradient LightColor;

	[Tooltip("Direct light sun intensity based on sun position in sky")]
	public AnimationCurve directLightSunIntensity = new AnimationCurve();

	[Tooltip("Direct light moon intensity based on moon position in sky")]
	public AnimationCurve directLightMoonIntensity = new AnimationCurve();

	[Tooltip("Set the speed of how fast light intensity will update.")]
	[Range(0.01f, 10f)]
	public float lightIntensityTransitionSpeed = 1f;

	[Tooltip("Realtime shadow strength of the directional light.")]
	public AnimationCurve shadowIntensity = new AnimationCurve();

	[Tooltip("Direct lighting y-offset.")]
	[Range(0f, 5000f)]
	public float directLightAngleOffset;

	[Header("Ambient")]
	[Tooltip("Ambient Rendering Mode.")]
	public AmbientMode ambientMode = AmbientMode.Flat;

	[Tooltip("Ambientlight intensity based on sun position in sky.")]
	public AnimationCurve ambientIntensity = new AnimationCurve();

	[Tooltip("Ambientlight sky color based on sun position in sky.")]
	public Gradient ambientSkyColor;

	[Tooltip("Ambientlight Equator color based on sun position in sky.")]
	public Gradient ambientEquatorColor;

	[Tooltip("Ambientlight Ground color based on sun position in sky.")]
	public Gradient ambientGroundColor;

	[Tooltip("Activate to stop the rotation of sun and moon at 'rotationStopHigh' sun/moon altitude in sky.")]
	public bool stopRotationAtHigh;

	[Range(0f, 1f)]
	[Tooltip("The altitude of sun/moon in sky (Same as 'DayNightSwitch' or the evaluatation of gradients.")]
	public float rotationStopHigh = 0.5f;
}
