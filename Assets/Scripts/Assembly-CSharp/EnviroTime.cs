using System;
using UnityEngine;

[Serializable]
public class EnviroTime
{
	public enum TimeProgressMode
	{
		None = 0,
		Simulated = 1,
		OneDay = 2,
		SystemTime = 3
	}

	[Tooltip("None = No time auto time progressing, Simulated = Time calculated with DayLenghtInMinutes, SystemTime = uses your systemTime.")]
	public TimeProgressMode ProgressTime = TimeProgressMode.Simulated;

	[Tooltip("Current Time: minutes")]
	[Range(0f, 60f)]
	public int Seconds;

	[Tooltip("Current Time: minutes")]
	[Range(0f, 60f)]
	public int Minutes;

	[Tooltip("Current Time: hours")]
	[Range(0f, 24f)]
	public int Hours = 12;

	[Tooltip("Current Time: Days")]
	public int Days = 1;

	[Tooltip("Current Time: Years")]
	public int Years = 1;

	[Space(20f)]
	[Tooltip("How many days in one year?")]
	public int DaysInYear = 365;

	[Tooltip("Day lenght in realtime minutes.")]
	public float DayLengthInMinutes = 5f;

	[Tooltip("Night lenght in realtime minutes.")]
	public float NightLengthInMinutes = 5f;

	[Range(-13f, 13f)]
	[Tooltip("Time offset for timezones")]
	public int utcOffset;

	[Range(-90f, 90f)]
	[Tooltip("-90,  90   Horizontal earth lines")]
	public float Latitude;

	[Range(-180f, 180f)]
	[Tooltip("-180, 180  Vertical earth line")]
	public float Longitude;

	[HideInInspector]
	public float solarTime;

	[HideInInspector]
	public float lunarTime;

	[Range(0.3f, 0.7f)]
	public float dayNightSwitch = 0.45f;
}
