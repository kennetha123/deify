using System;
using UnityEngine;

[Serializable]
public class EnviroVegetationStage
{
	public enum GrowState
	{
		Grow = 0,
		Stay = 1
	}

	[Range(0f, 100f)]
	public float minAgePercent;

	public GrowState growAction;

	public GameObject GrowGameobjectSpring;

	public GameObject GrowGameobjectSummer;

	public GameObject GrowGameobjectAutumn;

	public GameObject GrowGameobjectWinter;

	public bool billboard;
}
