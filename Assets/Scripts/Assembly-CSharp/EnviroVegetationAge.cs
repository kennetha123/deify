using System;

[Serializable]
public class EnviroVegetationAge
{
	public float maxAgeHours = 24f;

	public float maxAgeDays = 60f;

	public float maxAgeYears;

	public bool randomStartAge;

	public float startAgeinHours;

	public double birthdayInHours;

	public bool Loop = true;

	public int LoopFromGrowStage;
}
