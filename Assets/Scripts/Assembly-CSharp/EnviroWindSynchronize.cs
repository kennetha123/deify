using System.Collections.Generic;
using UnityEngine;

public class EnviroWindSynchronize : MonoBehaviour
{
	[Header("Terrain Grass")]
	public bool syncTerrainGrassWind = true;

	public List<Terrain> terrains = new List<Terrain>();

	[Header("Speed")]
	[Range(0f, 10f)]
	public float windChangingSpeed = 1f;

	private void Start()
	{
		if (syncTerrainGrassWind && terrains.Count > 0)
		{
			Debug.Log("Please assign Terrain, or deactivate 'syncTerrainGrassWind'!");
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (syncTerrainGrassWind)
		{
			for (int i = 0; i < terrains.Count; i++)
			{
				terrains[i].terrainData.wavingGrassStrength = Mathf.Lerp(terrains[i].terrainData.wavingGrassStrength, EnviroSkyMgr.instance.Components.windZone.windMain, Time.deltaTime * windChangingSpeed);
			}
		}
	}
}
