using System;
using UnityEngine;

[Serializable]
public class EnviroInteriorZoneSettings
{
	[HideInInspector]
	public Color currentInteriorDirectLightMod;

	[HideInInspector]
	public Color currentInteriorAmbientLightMod;

	[HideInInspector]
	public Color currentInteriorAmbientEQLightMod;

	[HideInInspector]
	public Color currentInteriorAmbientGRLightMod;

	[HideInInspector]
	public Color currentInteriorSkyboxMod;

	[HideInInspector]
	public Color currentInteriorFogColorMod = new Color(0f, 0f, 0f, 0f);

	[HideInInspector]
	public float currentInteriorFogMod = 1f;

	[HideInInspector]
	public float currentInteriorWeatherEffectMod = 1f;

	[HideInInspector]
	public float currentInteriorZoneAudioVolume = 1f;

	[HideInInspector]
	public float currentInteriorZoneAudioFadingSpeed = 1f;
}
