using System;
using UnityEngine;

[Serializable]
public class EnviroSeasons
{
	public enum Seasons
	{
		Spring = 0,
		Summer = 1,
		Autumn = 2,
		Winter = 3
	}

	[Tooltip("When enabled the system will change seasons automaticly when enough days passed.")]
	public bool calcSeasons;

	[Tooltip("The current season.")]
	public Seasons currentSeasons;

	[HideInInspector]
	public Seasons lastSeason;
}
