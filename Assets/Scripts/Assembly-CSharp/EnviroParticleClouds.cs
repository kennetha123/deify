using System;
using UnityEngine;

[Serializable]
public class EnviroParticleClouds
{
	[Tooltip("Particle clouds relative height.")]
	[Range(0f, 0.2f)]
	public float height = 0.05f;

	[Tooltip("Global Color for flat clouds based sun positon.")]
	public Gradient particleCloudsColor;
}
