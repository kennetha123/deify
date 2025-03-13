using System;
using UnityEngine;

[Serializable]
public class EnviroWeatherSettings
{
	[Header("Zones Setup:")]
	[Tooltip("Tag for zone triggers. Create and assign a tag to this gameObject")]
	public bool useTag;

	[Header("Weather Transition Settings:")]
	[Tooltip("Defines the speed of wetness will raise when it is raining.")]
	public float wetnessAccumulationSpeed = 0.05f;

	[Tooltip("Defines the speed of wetness will dry when it is not raining.")]
	public float wetnessDryingSpeed = 0.05f;

	[Tooltip("Defines the speed of snow will raise when it is snowing.")]
	public float snowAccumulationSpeed = 0.05f;

	[Tooltip("Defines the speed of snow will meld when it is not snowing.")]
	public float snowMeltingSpeed = 0.05f;

	[Tooltip("Defines the temperature when snow starts to melt.")]
	public float snowMeltingTresholdTemperature = 1f;

	[Tooltip("Defines the speed of clouds will change when weather conditions changed.")]
	public float cloudTransitionSpeed = 1f;

	[Tooltip("Defines the speed of fog will change when weather conditions changed.")]
	public float fogTransitionSpeed = 1f;

	[Tooltip("Defines the speed of wind intensity will change when weather conditions changed.")]
	public float windIntensityTransitionSpeed = 1f;

	[Tooltip("Defines the speed of particle effects will change when weather conditions changed.")]
	public float effectTransitionSpeed = 1f;

	[Tooltip("Defines the speed of sfx will fade in and out when weather conditions changed.")]
	public float audioTransitionSpeed = 0.1f;

	[Header("Lightning Effect:")]
	public GameObject lightningEffect;

	[Range(500f, 10000f)]
	public float lightningRange = 500f;

	[Range(500f, 5000f)]
	public float lightningHeight = 750f;

	[Header("Temperature:")]
	[Tooltip("Defines the speed of temperature changes.")]
	public float temperatureChangingSpeed = 10f;
}
