using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class EnviroSkyLite : EnviroCore
{
	private static EnviroSkyLite _instance;

	public string prefabVersion = "2.1.2";

	public bool useParticleClouds;

	public bool usePostEffectFog = true;

	[HideInInspector]
	public EnviroSkyRenderingLW EnviroSkyRender;

	[HideInInspector]
	public Material skyMat;

	private double lastMoonUpdate;

	[HideInInspector]
	public bool showSettings;

	public static EnviroSkyLite instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<EnviroSkyLite>();
			}
			return _instance;
		}
	}

	private void Start()
	{
		SetTime(GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		lastHourUpdate = Mathf.RoundToInt(internalHour);
		currentTimeInHours = GetInHours(internalHour, GameTime.Days, GameTime.Years, GameTime.DaysInYear);
		Weather.weatherFullyChanged = false;
		thunder = 0f;
		if (profileLoaded)
		{
			InvokeRepeating("UpdateEnviroment", 0f, qualitySettings.UpdateInterval);
			CreateEffects("Enviro Effects LW");
			if (weatherSettings.lightningEffect != null && lightningEffect == null)
			{
				lightningEffect = Object.Instantiate(weatherSettings.lightningEffect, EffectsHolder.transform).GetComponent<ParticleSystem>();
			}
			if (PlayerCamera != null && Player != null && !AssignInRuntime && profile != null)
			{
				Init();
			}
		}
		StartCoroutine(SetSkyBoxLateAdditive());
	}

	private IEnumerator SetSkyBoxLateAdditive()
	{
		yield return 0;
		if (skyMat != null && RenderSettings.skybox != skyMat)
		{
			SetupSkybox();
		}
	}

	private void OnEnable()
	{
		if (Weather.zones.Count < 1)
		{
			Weather.zones.Add(GetComponent<EnviroZone>());
		}
		Weather.currentActiveWeatherPreset = Weather.zones[0].currentActiveZoneWeatherPreset;
		Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
		if (profile == null)
		{
			Debug.LogError("No profile assigned!");
			return;
		}
		if (!profileLoaded)
		{
			ApplyProfile(profile);
		}
		PreInit();
		if (AssignInRuntime)
		{
			started = false;
		}
		else if (PlayerCamera != null && Player != null)
		{
			Init();
		}
	}

	public void ReInit()
	{
		OnEnable();
	}

	private void PreInit()
	{
		if (GameTime.solarTime < GameTime.dayNightSwitch)
		{
			isNight = true;
		}
		else
		{
			isNight = false;
		}
		if (serverMode)
		{
			return;
		}
		CheckSatellites();
		RenderSettings.fogMode = fogSettings.Fogmode;
		SetupSkybox();
		RenderSettings.ambientMode = lightSettings.ambientMode;
		if (Components.GlobalReflectionProbe == null)
		{
			foreach (Transform item in base.transform)
			{
				if (item.name == "GlobalReflections")
				{
					GameObject gameObject = item.gameObject;
					Components.GlobalReflectionProbe = gameObject.GetComponent<EnviroReflectionProbe>();
					if (Components.GlobalReflectionProbe == null)
					{
						Components.GlobalReflectionProbe = gameObject.AddComponent<EnviroReflectionProbe>();
					}
				}
			}
		}
		if (!Components.Sun)
		{
			Debug.LogError("Please set sun object in inspector!");
		}
		if (!Components.satellites)
		{
			Debug.LogError("Please set satellite object in inspector!");
		}
		if ((bool)Components.Moon)
		{
			MoonTransform = Components.Moon.transform;
			MoonRenderer = Components.Moon.GetComponent<Renderer>();
			if (MoonRenderer == null)
			{
				MoonRenderer = Components.Moon.AddComponent<MeshRenderer>();
			}
			MoonRenderer.shadowCastingMode = ShadowCastingMode.Off;
			MoonRenderer.receiveShadows = false;
			if (MoonRenderer.sharedMaterial != null)
			{
				Object.DestroyImmediate(MoonRenderer.sharedMaterial);
			}
			if (skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Realistic)
			{
				MoonShader = new Material(Shader.Find("Enviro/Lite/MoonShader"));
			}
			else
			{
				MoonShader = new Material(Shader.Find("Enviro/Lite/MoonShaderPhased"));
			}
			MoonShader.SetTexture("_MainTex", skySettings.moonTexture);
			MoonRenderer.sharedMaterial = MoonShader;
			customMoonPhase = skySettings.startMoonPhase;
		}
		else
		{
			Debug.LogError("Please set moon object in inspector!");
		}
		if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single)
		{
			SetupMainLight();
		}
		else
		{
			SetupMainLight();
			SetupAdditionalLight();
		}
		if ((bool)Components.particleClouds)
		{
			ParticleSystem[] componentsInChildren = Components.particleClouds.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren.Length != 0)
			{
				particleClouds.layer1System = componentsInChildren[0];
			}
			if (componentsInChildren.Length > 1)
			{
				particleClouds.layer2System = componentsInChildren[1];
			}
			if (particleClouds.layer1System != null)
			{
				particleClouds.layer1Material = particleClouds.layer1System.GetComponent<ParticleSystemRenderer>().sharedMaterial;
			}
			if (particleClouds.layer2System != null)
			{
				particleClouds.layer2Material = particleClouds.layer2System.GetComponent<ParticleSystemRenderer>().sharedMaterial;
			}
		}
		else
		{
			Debug.LogError("Please set particleCLouds object in inspector!");
		}
	}

	public void SetupSkybox()
	{
		if (skySettings.skyboxModeLW == EnviroSkySettings.SkyboxModiLW.Simple)
		{
			if (skyMat != null)
			{
				Object.DestroyImmediate(skyMat);
			}
			skyMat = new Material(Shader.Find("Enviro/Lite/SkyboxSimple"));
			if (skySettings.starsCubeMap != null)
			{
				skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
			}
			if (skySettings.galaxyCubeMap != null)
			{
				skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);
			}
			RenderSettings.skybox = skyMat;
		}
		else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.CustomSkybox && skySettings.customSkyboxMaterial != null)
		{
			RenderSettings.skybox = skySettings.customSkyboxMaterial;
		}
		if (lightSettings.ambientMode == AmbientMode.Skybox)
		{
			StartCoroutine(UpdateAmbientLightWithDelay());
		}
	}

	private IEnumerator UpdateAmbientLightWithDelay()
	{
		yield return 0;
		DynamicGI.UpdateEnvironment();
	}

	private void Init()
	{
		if (profile == null)
		{
			return;
		}
		if (serverMode)
		{
			started = true;
			return;
		}
		InitImageEffects();
		if (PlayerCamera != null)
		{
			if (setCameraClearFlags)
			{
				PlayerCamera.clearFlags = CameraClearFlags.Skybox;
			}
			if (PlayerCamera.actualRenderingPath == RenderingPath.DeferredShading)
			{
				SetCameraHDR(PlayerCamera, true);
			}
			else
			{
				SetCameraHDR(PlayerCamera, HDR);
			}
			Components.GlobalReflectionProbe.myProbe.farClipPlane = PlayerCamera.farClipPlane;
		}
		started = true;
		if (MoonShader != null)
		{
			MoonShader.SetFloat("_Phase", customMoonPhase);
			MoonShader.SetColor("_Color", skySettings.moonColor);
			MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1f - GameTime.solarTime));
		}
	}

	private void InitImageEffects()
	{
		EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRenderingLW>();
		if (EnviroSkyRender == null)
		{
			EnviroSkyRender = PlayerCamera.gameObject.AddComponent<EnviroSkyRenderingLW>();
		}
		EnviroPostProcessing = PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();
		if (EnviroPostProcessing == null)
		{
			EnviroPostProcessing = PlayerCamera.gameObject.AddComponent<EnviroPostProcessing>();
		}
	}

	private void SetupMainLight()
	{
		if ((bool)Components.DirectLight)
		{
			MainLight = Components.DirectLight.GetComponent<Light>();
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				Object.DontDestroyOnLoad(Components.DirectLight);
			}
		}
		else
		{
			GameObject gameObject = GameObject.Find("Enviro Directional Light");
			if (gameObject != null)
			{
				Components.DirectLight = gameObject.transform;
			}
			else
			{
				Components.DirectLight = CreateDirectionalLight(false);
			}
			MainLight = Components.DirectLight.GetComponent<Light>();
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				Object.DontDestroyOnLoad(Components.DirectLight);
			}
		}
		if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single && Components.AdditionalDirectLight != null)
		{
			Object.DestroyImmediate(Components.AdditionalDirectLight.gameObject);
		}
	}

	private void SetupAdditionalLight()
	{
		if ((bool)Components.AdditionalDirectLight)
		{
			AdditionalLight = Components.AdditionalDirectLight.GetComponent<Light>();
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				Object.DontDestroyOnLoad(Components.AdditionalDirectLight);
			}
			return;
		}
		GameObject gameObject = GameObject.Find("Enviro Directional Light - Moon");
		if (gameObject != null)
		{
			Components.AdditionalDirectLight = gameObject.transform;
		}
		else
		{
			Components.AdditionalDirectLight = CreateDirectionalLight(true);
		}
		AdditionalLight = Components.DirectLight.GetComponent<Light>();
		if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
		{
			Object.DontDestroyOnLoad(Components.AdditionalDirectLight);
		}
	}

	private void UpdateCameraComponents()
	{
		if (EnviroSkyRender != null)
		{
			EnviroSkyRender.simpleFog = fogSettings.useSimpleFog;
			EnviroSkyRender.distanceFog = fogSettings.distanceFog;
			EnviroSkyRender.heightFog = fogSettings.heightFog;
			EnviroSkyRender.height = fogSettings.height;
			EnviroSkyRender.heightDensity = fogSettings.heightDensity;
			EnviroSkyRender.useRadialDistance = fogSettings.useRadialDistance;
			EnviroSkyRender.startDistance = fogSettings.startDistance;
		}
	}

	private void Update()
	{
		if (profile == null)
		{
			Debug.Log("No profile applied! Please create and assign a profile.");
			return;
		}
		if (!started && !serverMode)
		{
			UpdateTime(GameTime.DaysInYear);
			UpdateSunAndMoonPosition();
			UpdateSceneView();
			CalculateDirectLight();
			UpdateAmbientLight();
			UpdateReflections();
			if (!AssignInRuntime || !(PlayerTag != "") || !(CameraTag != "") || !Application.isPlaying)
			{
				started = false;
				return;
			}
			GameObject gameObject = GameObject.FindGameObjectWithTag(PlayerTag);
			if (gameObject != null)
			{
				Player = gameObject;
			}
			for (int i = 0; i < Camera.allCameras.Length; i++)
			{
				if (Camera.allCameras[i].tag == CameraTag)
				{
					PlayerCamera = Camera.allCameras[i];
				}
			}
			if (!(Player != null) || !(PlayerCamera != null))
			{
				started = false;
				return;
			}
			Init();
			started = true;
		}
		UpdateTime(GameTime.DaysInYear);
		ValidateParameters();
		if (!serverMode)
		{
			UpdateSceneView();
			UpdateCameraComponents();
			UpdateAmbientLight();
			UpdateReflections();
			UpdateWeather();
			UpdateParticleClouds(useParticleClouds);
			UpdateSunAndMoonPosition();
			CalculateDirectLight();
			SetMaterialsVariables();
			CalculateSatPositions(LST);
			if (skySettings.renderMoon && !Components.Moon.activeSelf)
			{
				Components.Moon.SetActive(true);
			}
			else if (!skySettings.renderMoon && Components.Moon.activeSelf)
			{
				Components.Moon.SetActive(false);
			}
			if (EnviroSkyRender == null && PlayerCamera != null)
			{
				InitImageEffects();
			}
			if (fogSettings.useUnityFog && PlayerCamera.renderingPath == RenderingPath.Forward)
			{
				RenderSettings.fog = true;
				if (EnviroSkyRender.isActiveAndEnabled)
				{
					EnviroSkyRender.enabled = false;
				}
			}
			else if (usePostEffectFog && !EnviroSkyRender.isActiveAndEnabled)
			{
				EnviroSkyRender.enabled = true;
			}
			else if (!usePostEffectFog && EnviroSkyRender.isActiveAndEnabled)
			{
				EnviroSkyRender.enabled = false;
			}
			if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch)
			{
				isNight = true;
				if (Audio.AudioSourceAmbient != null)
				{
					TryPlayAmbientSFX();
				}
				EnviroSkyMgr.instance.NotifyIsNight();
			}
			else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch)
			{
				isNight = false;
				if (Audio.AudioSourceAmbient != null)
				{
					TryPlayAmbientSFX();
				}
				EnviroSkyMgr.instance.NotifyIsDay();
			}
		}
		else
		{
			UpdateWeather();
			if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch)
			{
				isNight = true;
				EnviroSkyMgr.instance.NotifyIsNight();
			}
			else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch)
			{
				isNight = false;
				EnviroSkyMgr.instance.NotifyIsDay();
			}
		}
	}

	private void LateUpdate()
	{
		if (!serverMode && PlayerCamera != null && Player != null)
		{
			base.transform.position = Player.transform.position;
			float num = PlayerCamera.farClipPlane - PlayerCamera.farClipPlane * 0.1f;
			base.transform.localScale = new Vector3(num, num, num);
			if (EffectsHolder != null)
			{
				EffectsHolder.transform.position = Player.transform.position;
			}
		}
	}

	private void SetMaterialsVariables()
	{
		skyMat.SetColor("_SkyColor", skySettings.simpleSkyColor.Evaluate(GameTime.solarTime));
		skyMat.SetColor("_HorizonColor", skySettings.simpleHorizonColor.Evaluate(GameTime.solarTime));
		skyMat.SetColor("_SunColor", skySettings.simpleSunColor.Evaluate(GameTime.solarTime));
		skyMat.SetFloat("_SunDiskSizeSimple", skySettings.simpleSunDiskSize.Evaluate(GameTime.solarTime));
		skyMat.SetFloat("_StarsIntensity", skySettings.starsIntensity.Evaluate(GameTime.solarTime));
		skyMat.SetVector("_CloudAnimation", cirrusAnim);
		if (cloudsSettings.cirrusCloudsTexture != null)
		{
			skyMat.SetTexture("_CloudMap", cloudsSettings.cirrusCloudsTexture);
		}
		skyMat.SetColor("_CloudColor", cloudsSettings.cirrusCloudsColor.Evaluate(GameTime.solarTime));
		skyMat.SetFloat("_CloudAltitude", cloudsSettings.cirrusCloudsAltitude);
		skyMat.SetFloat("_CloudAlpha", cloudsConfig.cirrusAlpha);
		skyMat.SetFloat("_CloudCoverage", cloudsConfig.cirrusCoverage);
		skyMat.SetFloat("_CloudColorPower", cloudsConfig.cirrusColorPow);
		Shader.SetGlobalVector("_SunDir", -Components.Sun.transform.forward);
		Shader.SetGlobalColor("_EnviroLighting", lightSettings.LightColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalVector("_SunPosition", Components.Sun.transform.localPosition + -Components.Sun.transform.forward * 10000f);
		Shader.SetGlobalVector("_MoonPosition", Components.Moon.transform.localPosition);
		Shader.SetGlobalColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
		Shader.SetGlobalColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));
		Shader.SetGlobalVector("_EnviroSkyFog", new Vector4(Fog.skyFogHeight, Fog.skyFogIntensity, Fog.skyFogStart, fogSettings.heightFogIntensity));
		Shader.SetGlobalFloat("_distanceFogIntensity", fogSettings.distanceFogIntensity);
		if (Application.isPlaying)
		{
			Shader.SetGlobalFloat("_maximumFogDensity", 1f - fogSettings.maximumFogDensity);
		}
		else
		{
			Shader.SetGlobalFloat("_maximumFogDensity", 1f);
		}
		if (fogSettings.useSimpleFog)
		{
			Shader.EnableKeyword("ENVIRO_SIMPLE_FOG");
			Shader.SetGlobalVector("_EnviroParams", new Vector4(Mathf.Clamp(1f - GameTime.solarTime, 0.5f, 1f), fogSettings.distanceFog ? 1f : 0f, 0f, HDR ? 1f : 0f));
		}
		else
		{
			Shader.SetGlobalColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
			Shader.SetGlobalFloat("_scatteringStrenght", Fog.scatteringStrenght);
			Shader.SetGlobalFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
			Shader.SetGlobalFloat("_SunBlocking", Fog.sunBlocking);
			Shader.SetGlobalVector("_EnviroParams", new Vector4(Mathf.Clamp(1f - GameTime.solarTime, 0.5f, 1f), fogSettings.distanceFog ? 1f : 0f, fogSettings.heightFog ? 1f : 0f, HDR ? 1f : 0f));
			Shader.SetGlobalVector("_Bm", BetaMie(skySettings.turbidity, skySettings.waveLength) * (skySettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
			Shader.SetGlobalVector("_BmScene", BetaMie(skySettings.turbidity, skySettings.waveLength) * (fogSettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
			Shader.SetGlobalVector("_Br", BetaRay(skySettings.waveLength) * skySettings.rayleigh);
			Shader.SetGlobalVector("_mieG", GetMieG(skySettings.g));
			Shader.SetGlobalVector("_mieGScene", GetMieGScene(skySettings.g));
			Shader.SetGlobalVector("_SunParameters", new Vector4(skySettings.sunIntensity, skySettings.sunDiskScale, skySettings.sunDiskIntensity, 0f));
			Shader.SetGlobalFloat("_Exposure", skySettings.skyExposure);
			Shader.SetGlobalFloat("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
			Shader.SetGlobalFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
			Shader.SetGlobalFloat("_heightFogIntensity", fogSettings.heightFogIntensity);
			Shader.SetGlobalFloat("_lightning", thunder);
			Shader.DisableKeyword("ENVIRO_SIMPLE_FOG");
		}
		Shader.DisableKeyword("ENVIROVOLUMELIGHT");
		if (MoonShader != null)
		{
			MoonShader.SetFloat("_Phase", customMoonPhase);
			MoonShader.SetColor("_Color", skySettings.moonColor);
			MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1f - GameTime.solarTime));
			MoonShader.SetFloat("_moonFogIntensity", Fog.moonIntensity);
		}
	}

	private void ValidateParameters()
	{
		internalHour = Mathf.Repeat(internalHour, 24f);
		GameTime.Longitude = Mathf.Clamp(GameTime.Longitude, -180f, 180f);
		GameTime.Latitude = Mathf.Clamp(GameTime.Latitude, -90f, 90f);
	}

	private void UpdateClouds(EnviroWeatherPreset i, bool withTransition)
	{
		if (!(i == null))
		{
			float num = 500f * Time.deltaTime;
			if (withTransition)
			{
				num = weatherSettings.cloudTransitionSpeed * Time.deltaTime;
			}
			cloudsConfig.cirrusAlpha = Mathf.Lerp(cloudsConfig.cirrusAlpha, i.cloudsConfig.cirrusAlpha, num);
			cloudsConfig.cirrusCoverage = Mathf.Lerp(cloudsConfig.cirrusCoverage, i.cloudsConfig.cirrusCoverage, num);
			cloudsConfig.cirrusColorPow = Mathf.Lerp(cloudsConfig.cirrusColorPow, i.cloudsConfig.cirrusColorPow, num);
			cloudsConfig.particleLayer1Alpha = Mathf.Lerp(cloudsConfig.particleLayer1Alpha, i.cloudsConfig.particleLayer1Alpha, num);
			cloudsConfig.particleLayer1Brightness = Mathf.Lerp(cloudsConfig.particleLayer1Brightness, i.cloudsConfig.particleLayer1Brightness, num);
			cloudsConfig.particleLayer1ColorPow = Mathf.Lerp(cloudsConfig.particleLayer1ColorPow, i.cloudsConfig.particleLayer1ColorPow, num);
			cloudsConfig.particleLayer2Alpha = Mathf.Lerp(cloudsConfig.particleLayer2Alpha, i.cloudsConfig.particleLayer2Alpha, num);
			cloudsConfig.particleLayer2Brightness = Mathf.Lerp(cloudsConfig.particleLayer2Brightness, i.cloudsConfig.particleLayer2Brightness, num);
			cloudsConfig.particleLayer2ColorPow = Mathf.Lerp(cloudsConfig.particleLayer2ColorPow, i.cloudsConfig.particleLayer2ColorPow, num);
			shadowIntensityMod = Mathf.Lerp(shadowIntensityMod, i.shadowIntensityMod, num);
			currentWeatherSkyMod = Color.Lerp(currentWeatherSkyMod, i.weatherSkyMod.Evaluate(GameTime.solarTime), num);
			currentWeatherFogMod = Color.Lerp(currentWeatherFogMod, i.weatherFogMod.Evaluate(GameTime.solarTime), num * 10f);
			currentWeatherLightMod = Color.Lerp(currentWeatherLightMod, i.weatherLightMod.Evaluate(GameTime.solarTime), num);
		}
	}

	private void UpdateFog(EnviroWeatherPreset i, bool withTransition)
	{
		RenderSettings.fogColor = Color.Lerp(Color.Lerp(fogSettings.simpleFogColor.Evaluate(GameTime.solarTime), customFogColor, customFogIntensity), currentWeatherFogMod, currentWeatherFogMod.a);
		if (i != null)
		{
			float t = 500f * Time.deltaTime;
			if (withTransition)
			{
				t = weatherSettings.fogTransitionSpeed * Time.deltaTime;
			}
			if (fogSettings.Fogmode == FogMode.Linear)
			{
				RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, i.fogDistance, t);
				RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, i.fogStartDistance, t);
			}
			else if (updateFogDensity)
			{
				RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, i.fogDensity, t) * interiorZoneSettings.currentInteriorFogMod;
			}
			Fog.scatteringStrenght = Mathf.Lerp(Fog.scatteringStrenght, i.FogScatteringIntensity, t);
			Fog.sunBlocking = Mathf.Lerp(Fog.sunBlocking, i.fogSunBlocking, t);
			Fog.moonIntensity = Mathf.Lerp(Fog.moonIntensity, i.moonIntensity, t);
			fogSettings.heightDensity = Mathf.Lerp(fogSettings.heightDensity, i.heightFogDensity, t);
			Fog.skyFogStart = Mathf.Lerp(Fog.skyFogStart, i.skyFogStart, t);
			Fog.skyFogHeight = Mathf.Lerp(Fog.skyFogHeight, i.SkyFogHeight, t);
			Fog.skyFogIntensity = Mathf.Lerp(Fog.skyFogIntensity, i.SkyFogIntensity, t);
		}
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
			t = weatherSettings.effectTransitionSpeed * Time.deltaTime;
		}
		for (int i = 0; i < id.effectSystems.Count; i++)
		{
			if (id.effectSystems[i].isStopped)
			{
				id.effectSystems[i].Play();
			}
			float emissionRate = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(id.effectSystems[i]), id.effectEmmisionRates[i] * qualitySettings.GlobalParticleEmissionRates, t) * interiorZoneSettings.currentInteriorWeatherEffectMod;
			EnviroSkyMgr.instance.SetEmissionRate(id.effectSystems[i], emissionRate);
		}
		for (int j = 0; j < Weather.WeatherPrefabs.Count; j++)
		{
			if (!(Weather.WeatherPrefabs[j].gameObject != id.gameObject))
			{
				continue;
			}
			for (int k = 0; k < Weather.WeatherPrefabs[j].effectSystems.Count; k++)
			{
				float num = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(Weather.WeatherPrefabs[j].effectSystems[k]), 0f, t);
				if (num < 1f)
				{
					num = 0f;
				}
				EnviroSkyMgr.instance.SetEmissionRate(Weather.WeatherPrefabs[j].effectSystems[k], num);
				if (num == 0f && !Weather.WeatherPrefabs[j].effectSystems[k].isStopped)
				{
					Weather.WeatherPrefabs[j].effectSystems[k].Stop();
				}
			}
		}
		UpdateWeatherVariables(id.weatherPreset);
	}

	private void UpdateWeather()
	{
		if (Weather.currentActiveWeatherPreset != Weather.currentActiveZone.currentActiveZoneWeatherPreset)
		{
			Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
			Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
			Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
			Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
			if (Weather.currentActiveWeatherPreset != null)
			{
				EnviroSkyMgr.instance.NotifyWeatherChanged(Weather.currentActiveWeatherPreset);
				Weather.weatherFullyChanged = false;
				if (!serverMode)
				{
					TryPlayAmbientSFX();
					UpdateAudioSource(Weather.currentActiveWeatherPreset);
					if (Weather.currentActiveWeatherPreset.isLightningStorm)
					{
						StartCoroutine(PlayThunderRandom());
					}
					else
					{
						StopCoroutine(PlayThunderRandom());
						Components.LightningGenerator.StopLightning();
					}
				}
			}
		}
		if (Weather.currentActiveWeatherPrefab != null && !serverMode)
		{
			UpdateClouds(Weather.currentActiveWeatherPreset, true);
			UpdateFog(Weather.currentActiveWeatherPreset, true);
			UpdateEffectSystems(Weather.currentActiveWeatherPrefab, true);
			if (!Weather.weatherFullyChanged)
			{
				CalcWeatherTransitionState();
			}
		}
		else if (Weather.currentActiveWeatherPrefab != null)
		{
			UpdateWeatherVariables(Weather.currentActiveWeatherPrefab.weatherPreset);
		}
	}

	public void InstantWeatherChange(EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
	{
		UpdateClouds(preset, false);
		UpdateFog(preset, false);
		UpdateEffectSystems(prefab, false);
	}

	public void AssignAndStart(GameObject player, Camera Camera)
	{
		Player = player;
		PlayerCamera = Camera;
		Init();
		started = true;
	}

	public void StartAsServer()
	{
		Player = base.gameObject;
		serverMode = true;
		Init();
	}

	public void ChangeFocus(GameObject player, Camera Camera)
	{
		Player = player;
		RemoveEnviroCameraComponents(PlayerCamera);
		PlayerCamera = Camera;
		InitImageEffects();
	}

	private void RemoveEnviroCameraComponents(Camera cam)
	{
		EnviroSkyRenderingLW component = cam.GetComponent<EnviroSkyRenderingLW>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		EnviroPostProcessing component2 = cam.GetComponent<EnviroPostProcessing>();
		if (component2 != null)
		{
			Object.Destroy(component2);
		}
	}

	public void Play(EnviroTime.TimeProgressMode progressMode = EnviroTime.TimeProgressMode.Simulated)
	{
		SetupSkybox();
		if (!Components.DirectLight.gameObject.activeSelf)
		{
			Components.DirectLight.gameObject.SetActive(true);
		}
		GameTime.ProgressTime = progressMode;
		if (EffectsHolder != null)
		{
			EffectsHolder.SetActive(true);
		}
		if (EnviroSkyRender != null)
		{
			EnviroSkyRender.enabled = true;
		}
		started = true;
	}

	public void Stop(bool disableLight = false, bool stopTime = true)
	{
		if (disableLight)
		{
			Components.DirectLight.gameObject.SetActive(false);
		}
		if (stopTime)
		{
			GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;
		}
		if (EffectsHolder != null)
		{
			EffectsHolder.SetActive(false);
		}
		if (EnviroSkyRender != null)
		{
			EnviroSkyRender.enabled = false;
		}
		if (EnviroPostProcessing != null)
		{
			EnviroPostProcessing.enabled = false;
		}
		started = false;
	}

	public void Deactivate(bool disableLight = false)
	{
		if (disableLight)
		{
			Components.DirectLight.gameObject.SetActive(false);
		}
		if (EffectsHolder != null)
		{
			EffectsHolder.SetActive(false);
		}
		if (EnviroSkyRender != null)
		{
			EnviroSkyRender.enabled = false;
		}
		if (EnviroPostProcessing != null)
		{
			EnviroPostProcessing.enabled = false;
		}
	}

	public void Activate()
	{
		Components.DirectLight.gameObject.SetActive(true);
		if (EffectsHolder != null)
		{
			EffectsHolder.SetActive(true);
		}
		if (EnviroSkyRender != null)
		{
			EnviroSkyRender.enabled = true;
		}
		if (EnviroPostProcessing != null)
		{
			EnviroPostProcessing.enabled = true;
		}
		TryPlayAmbientSFX();
		if (Weather.currentAudioSource != null)
		{
			Weather.currentAudioSource.audiosrc.Play();
		}
	}
}
