using UnityEngine;

[AddComponentMenu("Enviro/Utility/Seasons for GameObjects")]
public class EnviroSeasonObjectSwitcher : MonoBehaviour
{
	public GameObject SpringObject;

	public GameObject SummerObject;

	public GameObject AutumnObject;

	public GameObject WinterObject;

	private void Start()
	{
		SwitchSeasonObject();
		EnviroSkyMgr.instance.OnSeasonChanged += delegate
		{
			SwitchSeasonObject();
		};
	}

	private void OnEnable()
	{
		if (SpringObject == null)
		{
			Debug.LogError("Please assign a spring Object in Inspector!");
			base.enabled = false;
		}
		if (SummerObject == null)
		{
			Debug.LogError("Please assign a summer Object in Inspector!");
			base.enabled = false;
		}
		if (AutumnObject == null)
		{
			Debug.LogError("Please assign a autumn Object in Inspector!");
			base.enabled = false;
		}
		if (WinterObject == null)
		{
			Debug.LogError("Please assign a winter Object in Inspector!");
			base.enabled = false;
		}
	}

	private void SwitchSeasonObject()
	{
		switch (EnviroSkyMgr.instance.GetCurrentSeason())
		{
		case EnviroSeasons.Seasons.Spring:
			SummerObject.SetActive(false);
			AutumnObject.SetActive(false);
			WinterObject.SetActive(false);
			SpringObject.SetActive(true);
			break;
		case EnviroSeasons.Seasons.Summer:
			SpringObject.SetActive(false);
			AutumnObject.SetActive(false);
			WinterObject.SetActive(false);
			SummerObject.SetActive(true);
			break;
		case EnviroSeasons.Seasons.Autumn:
			SpringObject.SetActive(false);
			SummerObject.SetActive(false);
			WinterObject.SetActive(false);
			AutumnObject.SetActive(true);
			break;
		case EnviroSeasons.Seasons.Winter:
			SpringObject.SetActive(false);
			SummerObject.SetActive(false);
			AutumnObject.SetActive(false);
			WinterObject.SetActive(true);
			break;
		}
	}
}
