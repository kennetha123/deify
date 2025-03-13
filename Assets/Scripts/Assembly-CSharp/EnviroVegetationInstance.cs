using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Enviro/Vegetation Growth Object")]
public class EnviroVegetationInstance : MonoBehaviour
{
	[HideInInspector]
	public int id;

	public EnviroVegetationAge Age;

	public EnviroVegetationSeasons Seasons;

	public List<EnviroVegetationStage> GrowStages = new List<EnviroVegetationStage>();

	public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

	public Vector3 maxScale = new Vector3(1f, 1f, 1f);

	public float GrowSpeedMod = 1f;

	public GameObject DeadPrefab;

	public Color GizmoColor = new Color(255f, 0f, 0f, 255f);

	public float GizmoSize = 0.5f;

	private EnviroSeasons.Seasons currentSeason;

	private double ageInHours;

	private double maxAgeInHours;

	private int currentStage;

	private GameObject currentVegetationObject;

	private bool stay;

	private bool reBirth;

	private bool rescale = true;

	private bool canGrow = true;

	private bool shrink;

	private void Start()
	{
		EnviroSkyMgr.instance.RegisterVegetationInstance(this);
		currentSeason = EnviroSkyMgr.instance.GetCurrentSeason();
		maxAgeInHours = EnviroSkyMgr.instance.GetInHours(Age.maxAgeHours, Age.maxAgeDays, Age.maxAgeYears);
		EnviroSkyMgr.instance.OnSeasonChanged += delegate
		{
			SetSeason();
		};
		if (Age.randomStartAge)
		{
			Age.startAgeinHours = Random.Range(0f, (float)maxAgeInHours);
			Age.randomStartAge = false;
		}
		Birth(0, Age.startAgeinHours);
	}

	private void OnEnable()
	{
		if (GrowStages.Count < 1)
		{
			Debug.LogError("Please setup GrowStages!");
			base.enabled = false;
		}
		for (int i = 0; i < GrowStages.Count; i++)
		{
			if (GrowStages[i].GrowGameobjectAutumn == null || GrowStages[i].GrowGameobjectSpring == null || GrowStages[i].GrowGameobjectSummer == null || GrowStages[i].GrowGameobjectWinter == null)
			{
				Debug.LogError("One ore more GrowStages missing GrowPrefabs!");
				base.enabled = false;
			}
		}
	}

	private void SetSeason()
	{
		currentSeason = EnviroSkyMgr.instance.GetCurrentSeason();
		VegetationChange();
	}

	public void KeepVariablesClear()
	{
		GrowStages[0].minAgePercent = 0f;
		for (int i = 0; i < GrowStages.Count; i++)
		{
			if (GrowStages[i].minAgePercent > 100f)
			{
				GrowStages[i].minAgePercent = 100f;
			}
		}
	}

	public void UpdateInstance()
	{
		if (reBirth)
		{
			Birth(0, 0f);
		}
		if (shrink)
		{
			ShrinkAndDeactivate();
		}
		if (canGrow)
		{
			UpdateGrowth();
		}
	}

	public void UpdateGrowth()
	{
		ageInHours = EnviroSkyMgr.instance.GetCurrentTimeInHours() - Age.birthdayInHours;
		KeepVariablesClear();
		if (stay)
		{
			return;
		}
		if (currentStage + 1 < GrowStages.Count)
		{
			if (maxAgeInHours * (double)(GrowStages[currentStage + 1].minAgePercent / 100f) <= ageInHours && ageInHours > 0.0)
			{
				currentStage++;
				VegetationChange();
			}
			else if (GrowStages[currentStage].growAction == EnviroVegetationStage.GrowState.Grow)
			{
				CalculateScale();
			}
		}
		else
		{
			if (stay)
			{
				return;
			}
			if (ageInHours > maxAgeInHours)
			{
				if (Age.Loop)
				{
					currentVegetationObject.SetActive(false);
					if (DeadPrefab != null)
					{
						DeadPrefabLoop();
					}
					else
					{
						Birth(Age.LoopFromGrowStage, 0f);
					}
				}
				else
				{
					stay = true;
				}
			}
			else if (GrowStages[currentStage].growAction == EnviroVegetationStage.GrowState.Grow)
			{
				CalculateScale();
			}
		}
	}

	private void DeadPrefabLoop()
	{
		stay = true;
		Object.Instantiate(DeadPrefab, base.transform.position, base.transform.rotation).transform.localScale = currentVegetationObject.transform.localScale;
		Birth(Age.LoopFromGrowStage, 0f);
		stay = false;
	}

	private IEnumerator BirthColliders()
	{
		Collider[] colliders = currentVegetationObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = false;
		}
		yield return new WaitForSeconds(10f);
		for (int j = 0; j < colliders.Length; j++)
		{
			colliders[j].enabled = true;
		}
	}

	private void CalculateScale()
	{
		if (rescale)
		{
			currentVegetationObject.transform.localScale = minScale;
			rescale = false;
		}
		double num = ageInHours / maxAgeInHours * (double)GrowSpeedMod;
		currentVegetationObject.transform.localScale = minScale + new Vector3((float)num, (float)num, (float)num);
		if (currentVegetationObject.transform.localScale.y > maxScale.y)
		{
			currentVegetationObject.transform.localScale = maxScale;
		}
		if (currentVegetationObject.transform.localScale.y < minScale.y)
		{
			currentVegetationObject.transform.localScale = minScale;
		}
	}

	public void Birth(int stage, float startAge)
	{
		Age.birthdayInHours = EnviroSkyMgr.instance.GetCurrentTimeInHours() - (double)startAge;
		startAge = 0f;
		ageInHours = 0.0;
		currentStage = stage;
		rescale = true;
		reBirth = false;
		VegetationChange();
		StartCoroutine(BirthColliders());
	}

	private void SeasonAction()
	{
		if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.SpawnDeadPrefab)
		{
			if (DeadPrefab != null)
			{
				Object.Instantiate(DeadPrefab, base.transform.position, base.transform.rotation).transform.localScale = currentVegetationObject.transform.localScale;
			}
			currentVegetationObject.SetActive(false);
		}
		else if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.Deactivate)
		{
			shrink = true;
		}
		else if (Seasons.seasonAction == EnviroVegetationSeasons.SeasonAction.Destroy)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void CheckSeason(bool update)
	{
		if (!update && canGrow)
		{
			SeasonAction();
			canGrow = false;
		}
		else if (update && !canGrow)
		{
			canGrow = true;
			reBirth = true;
		}
		else if (!update && !canGrow)
		{
			SeasonAction();
		}
	}

	private void ShrinkAndDeactivate()
	{
		if (currentVegetationObject.transform.localScale.y > minScale.y)
		{
			currentVegetationObject.transform.localScale = new Vector3(currentVegetationObject.transform.localScale.x - 0.1f * Time.deltaTime, currentVegetationObject.transform.localScale.y - 0.1f * Time.deltaTime, currentVegetationObject.transform.localScale.z - 0.1f * Time.deltaTime);
			return;
		}
		shrink = false;
		currentVegetationObject.SetActive(false);
	}

	public void VegetationChange()
	{
		canGrow = true;
		if (currentVegetationObject != null)
		{
			currentVegetationObject.SetActive(false);
		}
		switch (currentSeason)
		{
		case EnviroSeasons.Seasons.Spring:
			currentVegetationObject = GrowStages[currentStage].GrowGameobjectSpring;
			CalculateScale();
			currentVegetationObject.SetActive(true);
			if (!Seasons.GrowInSpring)
			{
				CheckSeason(false);
			}
			else if (Seasons.GrowInSpring)
			{
				CheckSeason(true);
			}
			break;
		case EnviroSeasons.Seasons.Summer:
			currentVegetationObject = GrowStages[currentStage].GrowGameobjectSummer;
			CalculateScale();
			currentVegetationObject.SetActive(true);
			if (!Seasons.GrowInSummer)
			{
				CheckSeason(false);
			}
			else if (Seasons.GrowInSummer)
			{
				CheckSeason(true);
			}
			break;
		case EnviroSeasons.Seasons.Autumn:
			currentVegetationObject = GrowStages[currentStage].GrowGameobjectAutumn;
			CalculateScale();
			currentVegetationObject.SetActive(true);
			if (!Seasons.GrowInAutumn)
			{
				CheckSeason(false);
			}
			else if (Seasons.GrowInAutumn)
			{
				CheckSeason(true);
			}
			break;
		case EnviroSeasons.Seasons.Winter:
			currentVegetationObject = GrowStages[currentStage].GrowGameobjectWinter;
			CalculateScale();
			currentVegetationObject.SetActive(true);
			if (!Seasons.GrowInWinter)
			{
				CheckSeason(false);
			}
			else if (Seasons.GrowInWinter)
			{
				CheckSeason(true);
			}
			break;
		}
	}

	private void LateUpdate()
	{
		if (GrowStages[currentStage].billboard && canGrow)
		{
			base.transform.rotation = Camera.main.transform.rotation;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = GizmoColor;
		Gizmos.DrawCube(base.transform.position, new Vector3(GizmoSize, GizmoSize, GizmoSize));
	}
}
