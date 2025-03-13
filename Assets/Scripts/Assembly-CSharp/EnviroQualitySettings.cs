using System;
using UnityEngine;

[Serializable]
public class EnviroQualitySettings
{
	[Range(0f, 1f)]
	[Tooltip("Modifies the amount of particles used in weather effects.")]
	public float GlobalParticleEmissionRates = 1f;

	[Tooltip("How often Enviro Growth Instances should be updated. Lower value = smoother growth and more frequent updates but more perfomance hungry!")]
	public float UpdateInterval = 0.5f;
}
