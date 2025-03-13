using System;
using UnityEngine;

[Serializable]
public class EnviroFogging
{
	[HideInInspector]
	public float skyFogStart;

	[HideInInspector]
	public float skyFogHeight = 1f;

	[HideInInspector]
	public float skyFogIntensity = 0.1f;

	[HideInInspector]
	public float skyFogStrength = 0.1f;

	[HideInInspector]
	public float scatteringStrenght = 0.5f;

	[HideInInspector]
	public float sunBlocking = 0.5f;

	[HideInInspector]
	public float moonIntensity = 1f;
}
