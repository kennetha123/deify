using System;
using UnityEngine;

[Serializable]
public class EnviroReflectionSettings
{
	public enum GlobalReflectionResolution
	{
		R16 = 0,
		R32 = 1,
		R64 = 2,
		R128 = 3,
		R256 = 4,
		R512 = 5,
		R1024 = 6,
		R2048 = 7
	}

	[Header("Global Reflections Settings")]
	[Tooltip("Enable/disable enviro reflection probe..")]
	public bool globalReflections = true;

	[Header("Global Reflections Custom Rendering")]
	[Tooltip("Enable/disable if enviro reflection probe should render in custom mode to support clouds and other enviro effects.")]
	public bool globalReflectionCustomRendering = true;

	[Tooltip("Enable/disable if enviro reflection probe should render with fog.")]
	public bool globalReflectionUseFog;

	[Tooltip("Set if enviro reflection probe should update faces individual on different frames.")]
	public bool globalReflectionTimeSlicing = true;

	[Header("Global Reflections Updates Settings")]
	[Tooltip("Enable/disable enviro reflection probe updates based on gametime changes..")]
	public bool globalReflectionsUpdateOnGameTime = true;

	[Tooltip("Enable/disable enviro reflection probe updates based on transform position changes..")]
	public bool globalReflectionsUpdateOnPosition = true;

	[Tooltip("Reflection probe intensity.")]
	[Range(0f, 2f)]
	public float globalReflectionsIntensity = 0.5f;

	[Tooltip("Reflection probe update rate.")]
	public float globalReflectionsUpdateTreshhold = 0.025f;

	[Tooltip("Reflection probe intensity.")]
	[Range(0.1f, 10f)]
	public float globalReflectionsScale = 1f;

	[Tooltip("Reflection probe resolution.")]
	public GlobalReflectionResolution globalReflectionResolution = GlobalReflectionResolution.R256;

	[Tooltip("Reflection probe rendered Layers.")]
	public LayerMask globalReflectionLayers;

	[Tooltip("Set the quality of clouds in reflection rendering. Leave empty to use global settings.")]
	public EnviroVolumeCloudsQuality reflectionCloudsQuality;
}
