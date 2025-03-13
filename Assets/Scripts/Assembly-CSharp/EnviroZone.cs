using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Enviro/Weather Zone")]
public class EnviroZone : MonoBehaviour
{
	public enum WeatherUpdateMode
	{
		GameTimeHours = 0,
		RealTimeMinutes = 1
	}

	[Tooltip("Defines the zone name.")]
	public string zoneName;

	[Tooltip("Uncheck to remove OnTriggerExit call when using overlapping zone layout.")]
	public bool ExitToDefault = true;

	public List<EnviroWeatherPrefab> zoneWeather = new List<EnviroWeatherPrefab>();

	public List<EnviroWeatherPrefab> curPossibleZoneWeather;

	[Header("Zone weather settings:")]
	[Tooltip("Add all weather prefabs for this zone here.")]
	public List<EnviroWeatherPreset> zoneWeatherPresets = new List<EnviroWeatherPreset>();

	[Tooltip("Shall weather changes occure based on gametime or realtime?")]
	public WeatherUpdateMode updateMode;

	[Tooltip("Defines how often (gametime hours or realtime minutes) the system will heck to change the current weather conditions.")]
	public float WeatherUpdateIntervall = 6f;

	[Header("Zone scaling and gizmo:")]
	[Tooltip("Enable this to use a mesh for zone trigger.")]
	public bool useMeshZone;

	[Tooltip("Custom Zone Mesh")]
	public Mesh zoneMesh;

	[Tooltip("Defines the zone scale.")]
	public Vector3 zoneScale = new Vector3(100f, 100f, 100f);

	[Tooltip("Defines the color of the zone's gizmo in editor mode.")]
	public Color zoneGizmoColor = Color.gray;

	[Header("Current active weather:")]
	[Tooltip("The current active weather conditions.")]
	public EnviroWeatherPrefab currentActiveZoneWeatherPrefab;

	public EnviroWeatherPreset currentActiveZoneWeatherPreset;

	[HideInInspector]
	public EnviroWeatherPrefab lastActiveZoneWeatherPrefab;

	[HideInInspector]
	public EnviroWeatherPreset lastActiveZoneWeatherPreset;

	private BoxCollider zoneCollider;

	private MeshCollider zoneMeshCollider;

	private double nextUpdate;

	private float nextUpdateRealtime;

	public bool init;

	private bool isDefault;

	private void Start()
	{
		if (zoneWeatherPresets.Count > 0)
		{
			if (!useMeshZone)
			{
				zoneCollider = base.gameObject.AddComponent<BoxCollider>();
				zoneCollider.isTrigger = true;
			}
			else
			{
				zoneMeshCollider = base.gameObject.AddComponent<MeshCollider>();
				zoneMeshCollider.sharedMesh = zoneMesh;
				zoneMeshCollider.convex = true;
				zoneMeshCollider.isTrigger = true;
			}
			if (!EnviroSkyMgr.instance.IsDefaultZone(base.gameObject))
			{
				EnviroSkyMgr.instance.RegisterZone(this);
			}
			else
			{
				isDefault = true;
			}
			UpdateZoneScale();
			nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + (double)WeatherUpdateIntervall;
			nextUpdateRealtime = Time.time + WeatherUpdateIntervall * 60f;
		}
		else
		{
			Debug.Log("Please add Weather Prefabs to Zone:" + base.gameObject.name);
		}
	}

	public void UpdateZoneScale()
	{
		if (!isDefault && !useMeshZone)
		{
			zoneCollider.size = zoneScale;
		}
		else if (!isDefault && useMeshZone)
		{
			base.transform.localScale = zoneScale;
		}
		else if (isDefault && !useMeshZone)
		{
			zoneCollider.size = Vector3.one * (1f / base.transform.localScale.y) * 0.25f;
		}
	}

	public void CreateZoneWeatherTypeList()
	{
		for (int i = 0; i < zoneWeatherPresets.Count; i++)
		{
			if (zoneWeatherPresets[i] == null)
			{
				Debug.Log("Warning! Missing Weather Preset in Zone: " + zoneName);
				return;
			}
			bool flag = true;
			for (int j = 0; j < EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Count; j++)
			{
				if (zoneWeatherPresets[i] == EnviroSkyMgr.instance.GetCurrentWeatherPresetList()[j])
				{
					flag = false;
					zoneWeather.Add(EnviroSkyMgr.instance.GetCurrentWeatherPrefabList()[j]);
				}
			}
			if (!flag)
			{
				continue;
			}
			GameObject gameObject = new GameObject();
			EnviroWeatherPrefab enviroWeatherPrefab = gameObject.AddComponent<EnviroWeatherPrefab>();
			enviroWeatherPrefab.weatherPreset = zoneWeatherPresets[i];
			gameObject.name = enviroWeatherPrefab.weatherPreset.Name;
			for (int k = 0; k < enviroWeatherPrefab.weatherPreset.effectSystems.Count; k++)
			{
				if (enviroWeatherPrefab.weatherPreset.effectSystems[k] == null || enviroWeatherPrefab.weatherPreset.effectSystems[k].prefab == null)
				{
					Debug.Log("Warning! Missing Particle System Entry: " + enviroWeatherPrefab.weatherPreset.Name);
					Object.Destroy(gameObject);
					return;
				}
				GameObject gameObject2 = Object.Instantiate(enviroWeatherPrefab.weatherPreset.effectSystems[k].prefab, gameObject.transform);
				gameObject2.transform.localPosition = enviroWeatherPrefab.weatherPreset.effectSystems[k].localPositionOffset;
				gameObject2.transform.localEulerAngles = enviroWeatherPrefab.weatherPreset.effectSystems[k].localRotationOffset;
				ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
				if (component != null)
				{
					enviroWeatherPrefab.effectSystems.Add(component);
					continue;
				}
				component = gameObject2.GetComponentInChildren<ParticleSystem>();
				if (component != null)
				{
					enviroWeatherPrefab.effectSystems.Add(component);
					continue;
				}
				Debug.Log("No Particle System found in prefab in weather preset: " + enviroWeatherPrefab.weatherPreset.Name);
				Object.Destroy(gameObject);
				return;
			}
			enviroWeatherPrefab.effectEmmisionRates.Clear();
			gameObject.transform.parent = EnviroSkyMgr.instance.GetVFXHolder().transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			zoneWeather.Add(enviroWeatherPrefab);
			EnviroSkyMgr.instance.GetCurrentWeatherPrefabList().Add(enviroWeatherPrefab);
			EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Add(zoneWeatherPresets[i]);
		}
		for (int l = 0; l < zoneWeather.Count; l++)
		{
			for (int m = 0; m < zoneWeather[l].effectSystems.Count; m++)
			{
				zoneWeather[l].effectEmmisionRates.Add(EnviroSkyMgr.instance.GetEmissionRate(zoneWeather[l].effectSystems[m]));
				EnviroSkyMgr.instance.SetEmissionRate(zoneWeather[l].effectSystems[m], 0f);
			}
		}
		if (isDefault && EnviroSkyMgr.instance.GetStartWeatherPreset() != null)
		{
			EnviroSkyMgr.instance.ChangeWeatherInstant(EnviroSkyMgr.instance.GetStartWeatherPreset());
			for (int n = 0; n < zoneWeather.Count; n++)
			{
				if (zoneWeather[n].weatherPreset == EnviroSkyMgr.instance.GetStartWeatherPreset())
				{
					currentActiveZoneWeatherPrefab = zoneWeather[n];
					lastActiveZoneWeatherPrefab = zoneWeather[n];
				}
			}
			currentActiveZoneWeatherPreset = EnviroSkyMgr.instance.GetStartWeatherPreset();
			lastActiveZoneWeatherPreset = EnviroSkyMgr.instance.GetStartWeatherPreset();
		}
		else
		{
			currentActiveZoneWeatherPrefab = zoneWeather[0];
			lastActiveZoneWeatherPrefab = zoneWeather[0];
			currentActiveZoneWeatherPreset = zoneWeatherPresets[0];
			lastActiveZoneWeatherPreset = zoneWeatherPresets[0];
		}
		nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + (double)WeatherUpdateIntervall;
	}

	private void BuildNewWeatherList()
	{
		curPossibleZoneWeather = new List<EnviroWeatherPrefab>();
		for (int i = 0; i < zoneWeather.Count; i++)
		{
			switch (EnviroSkyMgr.instance.GetCurrentSeason())
			{
			case EnviroSeasons.Seasons.Spring:
				if (zoneWeather[i].weatherPreset.Spring)
				{
					curPossibleZoneWeather.Add(zoneWeather[i]);
				}
				break;
			case EnviroSeasons.Seasons.Summer:
				if (zoneWeather[i].weatherPreset.Summer)
				{
					curPossibleZoneWeather.Add(zoneWeather[i]);
				}
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (zoneWeather[i].weatherPreset.Autumn)
				{
					curPossibleZoneWeather.Add(zoneWeather[i]);
				}
				break;
			case EnviroSeasons.Seasons.Winter:
				if (zoneWeather[i].weatherPreset.winter)
				{
					curPossibleZoneWeather.Add(zoneWeather[i]);
				}
				break;
			}
		}
	}

	private EnviroWeatherPrefab PossibiltyCheck()
	{
		List<EnviroWeatherPrefab> list = new List<EnviroWeatherPrefab>();
		for (int i = 0; i < curPossibleZoneWeather.Count; i++)
		{
			int num = Random.Range(0, 100);
			if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Spring)
			{
				if ((float)num <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSpring)
				{
					list.Add(curPossibleZoneWeather[i]);
				}
			}
			else if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Summer)
			{
				if ((float)num <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSummer)
				{
					list.Add(curPossibleZoneWeather[i]);
				}
			}
			else if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Autumn)
			{
				if ((float)num <= curPossibleZoneWeather[i].weatherPreset.possibiltyInAutumn)
				{
					list.Add(curPossibleZoneWeather[i]);
				}
			}
			else if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Winter && (float)num <= curPossibleZoneWeather[i].weatherPreset.possibiltyInWinter)
			{
				list.Add(curPossibleZoneWeather[i]);
			}
		}
		if (list.Count > 0)
		{
			EnviroSkyMgr.instance.NotifyZoneWeatherChanged(list[0].weatherPreset, this);
			return list[0];
		}
		return currentActiveZoneWeatherPrefab;
	}

	private void WeatherUpdate()
	{
		nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + (double)WeatherUpdateIntervall;
		nextUpdateRealtime = Time.time + WeatherUpdateIntervall * 60f;
		BuildNewWeatherList();
		lastActiveZoneWeatherPrefab = currentActiveZoneWeatherPrefab;
		lastActiveZoneWeatherPreset = currentActiveZoneWeatherPreset;
		currentActiveZoneWeatherPrefab = PossibiltyCheck();
		currentActiveZoneWeatherPreset = currentActiveZoneWeatherPrefab.weatherPreset;
		EnviroSkyMgr.instance.NotifyZoneWeatherChanged(currentActiveZoneWeatherPreset, this);
	}

	private IEnumerator CreateWeatherListLate()
	{
		yield return 0;
		CreateZoneWeatherTypeList();
		init = true;
	}

	private void LateUpdate()
	{
		if (EnviroSkyMgr.instance == null)
		{
			Debug.Log("No EnviroSky instance found!");
			return;
		}
		if (EnviroSkyMgr.instance.IsStarted() && !init)
		{
			if (zoneWeatherPresets.Count < 1)
			{
				Debug.Log("Zone with no Presets! Please assign at least one preset. Deactivated for now!");
				base.enabled = false;
				return;
			}
			if (isDefault)
			{
				CreateZoneWeatherTypeList();
				init = true;
			}
			else
			{
				StartCoroutine(CreateWeatherListLate());
			}
		}
		if (updateMode == WeatherUpdateMode.GameTimeHours)
		{
			if (EnviroSkyMgr.instance.GetCurrentTimeInHours() > nextUpdate && EnviroSkyMgr.instance.IsAutoWeatherUpdateActive() && EnviroSkyMgr.instance.IsStarted())
			{
				WeatherUpdate();
			}
		}
		else if (Time.time > nextUpdateRealtime && EnviroSkyMgr.instance.IsAutoWeatherUpdateActive() && EnviroSkyMgr.instance.IsStarted())
		{
			WeatherUpdate();
		}
		if (!(EnviroSkyMgr.instance.Player == null) && isDefault && init && !useMeshZone)
		{
			zoneCollider.center = new Vector3(0f, (EnviroSkyMgr.instance.Player.transform.position.y - base.transform.position.y) / base.transform.lossyScale.y, 0f);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (EnviroSkyMgr.instance == null)
		{
			return;
		}
		if (EnviroSkyMgr.instance.GetUseWeatherTag())
		{
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag())
			{
				EnviroSkyMgr.instance.SetCurrentActiveZone(this);
				EnviroSkyMgr.instance.NotifyZoneChanged(this);
			}
		}
		else if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject))
		{
			EnviroSkyMgr.instance.SetCurrentActiveZone(this);
			EnviroSkyMgr.instance.NotifyZoneChanged(this);
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (!ExitToDefault || EnviroSkyMgr.instance == null)
		{
			return;
		}
		if (EnviroSkyMgr.instance.GetUseWeatherTag())
		{
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag())
			{
				EnviroSkyMgr.instance.SetToZone(0);
				EnviroSkyMgr.instance.NotifyZoneChanged(EnviroSkyMgr.instance.GetZoneByID(0));
			}
		}
		else if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject))
		{
			EnviroSkyMgr.instance.SetToZone(0);
			EnviroSkyMgr.instance.NotifyZoneChanged(EnviroSkyMgr.instance.GetZoneByID(0));
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = zoneGizmoColor;
		if (useMeshZone && zoneMesh != null)
		{
			Gizmos.DrawMesh(zoneMesh);
		}
		else
		{
			Gizmos.DrawCube(base.transform.position, new Vector3(zoneScale.x, zoneScale.y, zoneScale.z));
		}
	}
}
