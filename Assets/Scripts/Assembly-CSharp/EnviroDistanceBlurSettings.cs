using System;
using UnityEngine;

[Serializable]
public class EnviroDistanceBlurSettings
{
	public bool antiFlicker = true;

	public bool highQuality = true;

	[Range(1f, 7f)]
	public float radius = 7f;
}
