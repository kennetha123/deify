using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnviroSatellitesSettings
{
	[Tooltip("List of satellites.")]
	public List<EnviroSatellite> additionalSatellites = new List<EnviroSatellite>();
}
