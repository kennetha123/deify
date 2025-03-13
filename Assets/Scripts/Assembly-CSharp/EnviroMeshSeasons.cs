using UnityEngine;

[AddComponentMenu("Enviro/Utility/Seasons for Meshes")]
public class EnviroMeshSeasons : MonoBehaviour
{
	public Material SpringMaterial;

	public Material SummerMaterial;

	public Material AutumnMaterial;

	public Material WinterMaterial;

	private MeshRenderer myRenderer;

	private void Start()
	{
		myRenderer = GetComponent<MeshRenderer>();
		if (myRenderer == null)
		{
			Debug.LogError("Please correct script placement! We need a MeshRenderer to work with!");
			base.enabled = false;
		}
		UpdateSeasonMaterial();
		EnviroSkyMgr.instance.OnSeasonChanged += delegate
		{
			UpdateSeasonMaterial();
		};
	}

	private void OnEnable()
	{
		if (SpringMaterial == null)
		{
			Debug.LogError("Please assign a spring material in Inspector!");
			base.enabled = false;
		}
		if (SummerMaterial == null)
		{
			Debug.LogError("Please assign a summer material in Inspector!");
			base.enabled = false;
		}
		if (AutumnMaterial == null)
		{
			Debug.LogError("Please assign a autumn material in Inspector!");
			base.enabled = false;
		}
		if (WinterMaterial == null)
		{
			Debug.LogError("Please assign a winter material in Inspector!");
			base.enabled = false;
		}
	}

	private void UpdateSeasonMaterial()
	{
		switch (EnviroSkyMgr.instance.GetCurrentSeason())
		{
		case EnviroSeasons.Seasons.Spring:
			myRenderer.sharedMaterial = SpringMaterial;
			break;
		case EnviroSeasons.Seasons.Summer:
			myRenderer.sharedMaterial = SummerMaterial;
			break;
		case EnviroSeasons.Seasons.Autumn:
			myRenderer.sharedMaterial = AutumnMaterial;
			break;
		case EnviroSeasons.Seasons.Winter:
			myRenderer.sharedMaterial = WinterMaterial;
			break;
		}
	}
}
