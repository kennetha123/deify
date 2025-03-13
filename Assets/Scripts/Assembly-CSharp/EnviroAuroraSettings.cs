using System;
using UnityEngine;

[Serializable]
public class EnviroAuroraSettings
{
	public AnimationCurve auroraIntensity = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0.1f), new Keyframe(1f, 0f));

	[Header("Aurora Color and Brightness")]
	public Color auroraColor = new Color(0.1f, 0.5f, 0.7f);

	public float auroraBrightness = 75f;

	public float auroraContrast = 10f;

	[Header("Aurora Height and Scale")]
	public float auroraHeight = 20000f;

	[Range(0f, 0.025f)]
	public float auroraScale = 0.01f;

	[Header("Aurora Performance")]
	[Range(8f, 32f)]
	public int auroraSteps = 20;

	[Header("Aurora Modelling and Animation")]
	public Vector4 auroraLayer1Settings = new Vector4(0.1f, 0.1f, 0f, 0.5f);

	public Vector4 auroraLayer2Settings = new Vector4(5f, 5f, 0f, 0.5f);

	public Vector4 auroraColorshiftSettings = new Vector4(0.05f, 0.05f, 0f, 5f);

	[Range(0f, 0.1f)]
	public float auroraSpeed = 0.005f;
}
