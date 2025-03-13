using System;
using UnityEngine;

[Serializable]
public class EnviroSatelliteVariables
{
	[Tooltip("Name of this satellite")]
	public string name;

	[Tooltip("Prefab with model that get instantiated.")]
	public GameObject prefab;

	[Tooltip("This value will influence the satellite orbitpositions.")]
	public float orbit_X;

	[Tooltip("This value will influence the satellite orbitpositions.")]
	public float orbit_Y;

	[Tooltip("The speed of the satellites orbit.")]
	public float speed;
}
