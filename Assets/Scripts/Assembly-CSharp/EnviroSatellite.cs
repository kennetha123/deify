using System;
using UnityEngine;

[Serializable]
public class EnviroSatellite
{
	[Tooltip("Name of this satellite")]
	public string name;

	[Tooltip("Prefab with model that get instantiated.")]
	public GameObject prefab;

	[Tooltip("Orbit distance.")]
	public float orbit;

	[Tooltip("Orbit modification on x axis.")]
	public float xRot;

	[Tooltip("Orbit modification on y axis.")]
	public float yRot;
}
