using System;

[Serializable]
public class EnviroVegetationSeasons
{
	public enum SeasonAction
	{
		SpawnDeadPrefab = 0,
		Deactivate = 1,
		Destroy = 2
	}

	public SeasonAction seasonAction;

	public bool GrowInSpring = true;

	public bool GrowInSummer = true;

	public bool GrowInAutumn = true;

	public bool GrowInWinter = true;
}
