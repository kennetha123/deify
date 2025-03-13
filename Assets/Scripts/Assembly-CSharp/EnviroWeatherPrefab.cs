using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnviroWeatherPrefab : MonoBehaviour
{
	public EnviroWeatherPreset weatherPreset;

	[HideInInspector]
	public List<ParticleSystem> effectSystems = new List<ParticleSystem>();

	[HideInInspector]
	public List<float> effectEmmisionRates = new List<float>();
}
