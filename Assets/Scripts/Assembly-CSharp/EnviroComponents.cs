using System;
using UnityEngine;

[Serializable]
public class EnviroComponents
{
	[Tooltip("The Enviro sun object.")]
	public GameObject Sun;

	[Tooltip("The Enviro moon object.")]
	public GameObject Moon;

	[Tooltip("The directional light for directional sun lighting when using dual mode. Used for sun and moon in single mode.")]
	public Transform DirectLight;

	[Tooltip("The directional light for directional moon lighting when using the dual mode.")]
	public Transform AdditionalDirectLight;

	[Tooltip("The Enviro global reflection probe for dynamic reflections.")]
	public EnviroReflectionProbe GlobalReflectionProbe;

	[Tooltip("Your WindZone that reflect our weather wind settings.")]
	public WindZone windZone;

	[Tooltip("The Enviro Lighting Flash Component.")]
	public EnviroLightning LightningGenerator;

	[Tooltip("Link to the object that hold all additional satellites as childs.")]
	public Transform satellites;

	[Tooltip("Just a transform for stars rotation calculations. ")]
	public Transform starsRotation;

	[Tooltip("Plane to cast cloud shadows.")]
	public GameObject particleClouds;
}
