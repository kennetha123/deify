using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Enviro/AddionalCamera")]
public class EnviroAdditionalCamera : MonoBehaviour
{
	public bool addEnviroSkyRendering = true;

	public bool addEnviroSkyPostProcessing = true;

	public bool addWeatherEffects = true;

	private Camera myCam;

	private EnviroSkyRendering skyRender;

	private EnviroPostProcessing enviroPostProcessing;

	private GameObject EffectHolder;

	private GameObject VFX;

	private List<EnviroWeatherPrefab> zoneWeather = new List<EnviroWeatherPrefab>();

	private EnviroWeatherPrefab currentWeather;

	private void OnEnable()
	{
		myCam = GetComponent<Camera>();
		if (myCam != null)
		{
			InitImageEffects();
		}
	}

	private void Start()
	{
		if (addWeatherEffects)
		{
			CreateEffectHolder();
			StartCoroutine(SetupWeatherEffects());
		}
	}

	private void Update()
	{
		if (addWeatherEffects)
		{
			UpdateWeatherEffects();
		}
	}

	private void CreateEffectHolder()
	{
		for (int num = myCam.transform.childCount - 1; num >= 0; num--)
		{
			if (myCam.transform.GetChild(num).gameObject.name == "Effect Holder")
			{
				Object.DestroyImmediate(myCam.transform.GetChild(num).gameObject);
			}
		}
		EffectHolder = new GameObject();
		EffectHolder.name = "Effect Holder";
		EffectHolder.transform.SetParent(myCam.transform, false);
		VFX = new GameObject();
		VFX.name = "VFX";
		VFX.transform.SetParent(EffectHolder.transform, false);
	}

	private IEnumerator SetupWeatherEffects()
	{
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < EnviroSky.instance.Weather.weatherPresets.Count; i++)
		{
			GameObject gameObject = new GameObject();
			EnviroWeatherPrefab enviroWeatherPrefab = gameObject.AddComponent<EnviroWeatherPrefab>();
			enviroWeatherPrefab.weatherPreset = EnviroSky.instance.Weather.weatherPresets[i];
			gameObject.name = enviroWeatherPrefab.weatherPreset.Name;
			for (int j = 0; j < enviroWeatherPrefab.weatherPreset.effectSystems.Count; j++)
			{
				if (enviroWeatherPrefab.weatherPreset.effectSystems[j] == null || enviroWeatherPrefab.weatherPreset.effectSystems[j].prefab == null)
				{
					Debug.Log("Warning! Missing Particle System Entry: " + enviroWeatherPrefab.weatherPreset.Name);
					Object.Destroy(gameObject);
					break;
				}
				GameObject gameObject2 = Object.Instantiate(enviroWeatherPrefab.weatherPreset.effectSystems[j].prefab, gameObject.transform);
				gameObject2.transform.localPosition = enviroWeatherPrefab.weatherPreset.effectSystems[j].localPositionOffset;
				gameObject2.transform.localEulerAngles = enviroWeatherPrefab.weatherPreset.effectSystems[j].localRotationOffset;
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
				break;
			}
			enviroWeatherPrefab.effectEmmisionRates.Clear();
			gameObject.transform.parent = VFX.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			zoneWeather.Add(enviroWeatherPrefab);
		}
		for (int k = 0; k < zoneWeather.Count; k++)
		{
			for (int l = 0; l < zoneWeather[k].effectSystems.Count; l++)
			{
				zoneWeather[k].effectEmmisionRates.Add(EnviroSkyMgr.instance.GetEmissionRate(zoneWeather[k].effectSystems[l]));
				EnviroSkyMgr.instance.SetEmissionRate(zoneWeather[k].effectSystems[l], 0f);
			}
		}
		if (!(EnviroSky.instance.Weather.currentActiveWeatherPrefab != null))
		{
			yield break;
		}
		for (int m = 0; m < zoneWeather.Count; m++)
		{
			if (zoneWeather[m].weatherPreset == EnviroSky.instance.Weather.currentActiveWeatherPrefab.weatherPreset)
			{
				currentWeather = zoneWeather[m];
			}
		}
	}

	private void UpdateWeatherEffects()
	{
		if (EnviroSky.instance.Weather.currentActiveWeatherPrefab == null || currentWeather == null)
		{
			return;
		}
		if (EnviroSky.instance.Weather.currentActiveWeatherPrefab.weatherPreset != currentWeather.weatherPreset)
		{
			for (int i = 0; i < zoneWeather.Count; i++)
			{
				if (zoneWeather[i].weatherPreset == EnviroSky.instance.Weather.currentActiveWeatherPrefab.weatherPreset)
				{
					currentWeather = zoneWeather[i];
				}
			}
		}
		UpdateEffectSystems(currentWeather, true);
	}

	private void UpdateEffectSystems(EnviroWeatherPrefab id, bool withTransition)
	{
		if (!(id != null))
		{
			return;
		}
		float t = 500f * Time.deltaTime;
		if (withTransition)
		{
			t = EnviroSkyMgr.instance.WeatherSettings.effectTransitionSpeed * Time.deltaTime;
		}
		for (int i = 0; i < id.effectSystems.Count; i++)
		{
			if (id.effectSystems[i].isStopped)
			{
				id.effectSystems[i].Play();
			}
			float emissionRate = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(id.effectSystems[i]), id.effectEmmisionRates[i] * EnviroSky.instance.qualitySettings.GlobalParticleEmissionRates, t) * EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod;
			EnviroSkyMgr.instance.SetEmissionRate(id.effectSystems[i], emissionRate);
		}
		for (int j = 0; j < zoneWeather.Count; j++)
		{
			if (!(zoneWeather[j].gameObject != id.gameObject))
			{
				continue;
			}
			for (int k = 0; k < zoneWeather[j].effectSystems.Count; k++)
			{
				float num = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(zoneWeather[j].effectSystems[k]), 0f, t);
				if (num < 1f)
				{
					num = 0f;
				}
				EnviroSkyMgr.instance.SetEmissionRate(zoneWeather[j].effectSystems[k], num);
				if (num == 0f && !zoneWeather[j].effectSystems[k].isStopped)
				{
					zoneWeather[j].effectSystems[k].Stop();
				}
			}
		}
	}

	private void InitImageEffects()
	{
		if (addEnviroSkyRendering)
		{
			skyRender = myCam.gameObject.GetComponent<EnviroSkyRendering>();
			if (skyRender == null)
			{
				skyRender = myCam.gameObject.AddComponent<EnviroSkyRendering>();
			}
			skyRender.isAddionalCamera = true;
		}
		if (addEnviroSkyPostProcessing)
		{
			enviroPostProcessing = myCam.gameObject.GetComponent<EnviroPostProcessing>();
			if (enviroPostProcessing == null)
			{
				enviroPostProcessing = myCam.gameObject.AddComponent<EnviroPostProcessing>();
			}
		}
	}
}
