using System;
using UnityEngine;

[Serializable]
public class EnviroCustomRenderingSettings
{
	[Header("Feature Control")]
	public bool useVolumeClouds = true;

	public bool useVolumeLighting = true;

	public bool useDistanceBlur = true;

	public bool useFog = true;

	public EnviroVolumeCloudsQuality customCloudsQuality;
}
