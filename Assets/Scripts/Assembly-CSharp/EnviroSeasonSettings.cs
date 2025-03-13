using System;
using UnityEngine;

[Serializable]
public class EnviroSeasonSettings
{
	[Header("Spring")]
	[Tooltip("Start Day of Year for Spring")]
	[Range(0f, 366f)]
	public int SpringStart = 60;

	[Tooltip("End Day of Year for Spring")]
	[Range(0f, 366f)]
	public int SpringEnd = 92;

	[Tooltip("Base Temperature in Spring")]
	public AnimationCurve springBaseTemperature = new AnimationCurve();

	[Header("Summer")]
	[Tooltip("Start Day of Year for Summer")]
	[Range(0f, 366f)]
	public int SummerStart = 93;

	[Tooltip("End Day of Year for Summer")]
	[Range(0f, 366f)]
	public int SummerEnd = 185;

	[Tooltip("Base Temperature in Summer")]
	public AnimationCurve summerBaseTemperature = new AnimationCurve();

	[Header("Autumn")]
	[Tooltip("Start Day of Year for Autumn")]
	[Range(0f, 366f)]
	public int AutumnStart = 186;

	[Tooltip("End Day of Year for Autumn")]
	[Range(0f, 366f)]
	public int AutumnEnd = 276;

	[Tooltip("Base Temperature in Autumn")]
	public AnimationCurve autumnBaseTemperature = new AnimationCurve();

	[Header("Winter")]
	[Tooltip("Start Day of Year for Winter")]
	[Range(0f, 366f)]
	public int WinterStart = 277;

	[Tooltip("End Day of Year for Winter")]
	[Range(0f, 366f)]
	public int WinterEnd = 59;

	[Tooltip("Base Temperature in Winter")]
	public AnimationCurve winterBaseTemperature = new AnimationCurve();
}
