using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class EnviroSky : EnviroCore
{
	private static EnviroSky _instance;

	public string prefabVersion = "2.1.5";

	[Header("Virtual Reality")]
	[Tooltip("Enable this when using singlepass rendering.")]
	public bool singlePassVR;

	[Tooltip("Enable this to activate volume lighing")]
	[HideInInspector]
	public bool useVolumeLighting = true;

	[HideInInspector]
	public bool useVolumeClouds = true;

	[HideInInspector]
	public bool useFog = true;

	[HideInInspector]
	public bool useFlatClouds;

	[HideInInspector]
	public bool useParticleClouds;

	[HideInInspector]
	public bool useDistanceBlur = true;

	[HideInInspector]
	public bool useAurora;

	private bool flatCloudsSkybox;

	[Header("Scene View Preview")]
	public bool showVolumeLightingInEditor = true;

	public bool showVolumeCloudsInEditor = true;

	public bool showFlatCloudsInEditor = true;

	public bool showFogInEditor = true;

	public bool showDistanceBlurInEditor = true;

	public bool showSettings;

	[HideInInspector]
	public Camera satCamera;

	[HideInInspector]
	public EnviroVolumeLight directVolumeLight;

	[HideInInspector]
	public EnviroVolumeLight additionalDirectVolumeLight;

	[HideInInspector]
	public EnviroSkyRendering EnviroSkyRender;

	public float globalVolumeLightIntensity;

	public float auroraIntensity;

	public EnviroVolumeCloudsQuality currentActiveCloudsQualityPreset;

	[HideInInspector]
	public RenderTexture cloudsRenderTarget;

	[HideInInspector]
	public RenderTexture flatCloudsRenderTarget;

	[HideInInspector]
	public RenderTexture weatherMap;

	[HideInInspector]
	public RenderTexture satRenderTarget;

	[HideInInspector]
	public RenderTexture cloudShadowMap;

	[HideInInspector]
	public Material skyMat;

	[HideInInspector]
	public Material flatCloudsMat;

	private Material weatherMapMat;

	private Material cloudShadowMat;

	public List<EnviroVolumeCloudsQuality> cloudsQualityList = new List<EnviroVolumeCloudsQuality>();

	private string[] cloudsQualityPresetsFound;

	public int selectedCloudsQuality;

	private float starsTwinklingRot;

	public float blurDistance = 100f;

	public float blurIntensity = 1f;

	public float blurSkyIntensity = 1f;

	public static EnviroSky instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType<EnviroSky>();
			}
			return _instance;
		}
	}

	private void Start()
	{
		if (EnviroSkyMgr.instance == null)
		{
			Debug.Log("Please use the EnviroSky Manager!");
			base.gameObject.SetActive(false);
			return;
		}
		started = false;
		SetTime(GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		lastHourUpdate = Mathf.RoundToInt(internalHour);
		currentTimeInHours = GetInHours(internalHour, GameTime.Days, GameTime.Years, GameTime.DaysInYear);
		Weather.weatherFullyChanged = false;
		thunder = 0f;
		if (profileLoaded)
		{
			InvokeRepeating("UpdateEnviroment", 0f, qualitySettings.UpdateInterval);
			if (PlayerCamera != null && Player != null && !AssignInRuntime && startMode == EnviroStartMode.Started)
			{
				Init();
			}
		}
	}

	private IEnumerator SetSceneSettingsLate()
	{
		yield return 0;
		if (skyMat != null && RenderSettings.skybox != skyMat)
		{
			SetupSkybox();
		}
		RenderSettings.fogMode = fogSettings.Fogmode;
		RenderSettings.ambientMode = lightSettings.ambientMode;
	}

	private void OnEnable()
	{
		Weather.currentActiveWeatherPreset = Weather.zones[0].currentActiveZoneWeatherPreset;
		Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
		if (weatherMapMat == null)
		{
			weatherMapMat = new Material(Shader.Find("Enviro/Standard/WeatherMap"));
		}
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
		else if (PlayerCamera != null && Player != null && startMode == EnviroStartMode.Started)
		{
			Init();
		}
		PopulateCloudsQualityList();
		if (currentActiveCloudsQualityPreset != null)
		{
			ApplyVolumeCloudsQualityPreset(currentActiveCloudsQualityPreset);
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
		CreateEffects("Enviro Effects");
		if (weatherSettings.lightningEffect != null && lightningEffect == null)
		{
			lightningEffect = UnityEngine.Object.Instantiate(weatherSettings.lightningEffect, EffectsHolder.transform).GetComponent<ParticleSystem>();
		}
		if (serverMode)
		{
			return;
		}
		CheckSatellites();
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
			customMoonPhase = skySettings.startMoonPhase;
		}
		else
		{
			Debug.LogError("Please set moon object in inspector!");
		}
		if (weatherMap != null)
		{
			UnityEngine.Object.DestroyImmediate(weatherMap);
		}
		if (weatherMap == null)
		{
			weatherMap = new RenderTexture(512, 512, 0, RenderTextureFormat.Default);
			weatherMap.wrapMode = TextureWrapMode.Repeat;
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
		if (cloudShadowMap != null)
		{
			UnityEngine.Object.DestroyImmediate(cloudShadowMap);
		}
		cloudShadowMap = new RenderTexture(2048, 2048, 0, RenderTextureFormat.Default);
		cloudShadowMap.wrapMode = TextureWrapMode.Repeat;
		if (cloudShadowMat != null)
		{
			UnityEngine.Object.DestroyImmediate(cloudShadowMat);
		}
		cloudShadowMat = new Material(Shader.Find("Enviro/Standard/ShadowCookie"));
		if (cloudsSettings.shadowIntensity > 0f)
		{
			Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);
			MainLight.cookie = cloudShadowMap;
			MainLight.cookieSize = 10000f;
		}
		else
		{
			MainLight.cookie = null;
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
		if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Simple)
		{
			if (skyMat != null)
			{
				UnityEngine.Object.DestroyImmediate(skyMat);
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
		else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Default)
		{
			if (skyMat != null)
			{
				UnityEngine.Object.DestroyImmediate(skyMat);
			}
			if (!useFlatClouds)
			{
				skyMat = new Material(Shader.Find("Enviro/Standard/Skybox"));
				flatCloudsSkybox = false;
			}
			else
			{
				skyMat = new Material(Shader.Find("Enviro/Standard/SkyboxFlatClouds"));
				flatCloudsSkybox = true;
			}
			if (skySettings.starsCubeMap != null)
			{
				skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
			}
			if (skySettings.galaxyCubeMap != null)
			{
				skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);
			}
			Cubemap cubemap = Resources.Load("cube_enviro_starsNoise") as Cubemap;
			if (cubemap != null)
			{
				skyMat.SetTexture("_StarsTwinklingNoise", cubemap);
			}
			Texture2D texture2D = Resources.Load("tex_enviro_dither") as Texture2D;
			if (texture2D != null)
			{
				skyMat.SetTexture("_DitheringTex", texture2D);
			}
			Texture2D texture2D2 = Resources.Load("tex_enviro_aurora_layer_1") as Texture2D;
			if (texture2D2 != null)
			{
				skyMat.SetTexture("_Aurora_Layer_1", texture2D2);
			}
			Texture2D texture2D3 = Resources.Load("tex_enviro_aurora_layer_2") as Texture2D;
			if (texture2D3 != null)
			{
				skyMat.SetTexture("_Aurora_Layer_2", texture2D3);
			}
			Texture2D texture2D4 = Resources.Load("tex_enviro_aurora_colorshift") as Texture2D;
			if (texture2D4 != null)
			{
				skyMat.SetTexture("_Aurora_Colorshift", texture2D4);
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
		SetupSkybox();
		RenderSettings.fogMode = fogSettings.Fogmode;
		RenderSettings.ambientMode = lightSettings.ambientMode;
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
		if (satelliteSettings.additionalSatellites.Count > 0)
		{
			CreateSatCamera();
		}
		started = true;
	}

	private void InitImageEffects()
	{
		EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRendering>();
		if (EnviroSkyRender == null)
		{
			EnviroSkyRender = PlayerCamera.gameObject.AddComponent<EnviroSkyRendering>();
		}
		EnviroPostProcessing = PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();
		if (EnviroPostProcessing == null)
		{
			EnviroPostProcessing = PlayerCamera.gameObject.AddComponent<EnviroPostProcessing>();
		}
	}

	public void CreateSatCamera()
	{
		Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].cullingMask &= ~(1 << satelliteRenderingLayer);
		}
		UnityEngine.Object.DestroyImmediate(GameObject.Find("Enviro Sat Camera"));
		GameObject gameObject = new GameObject();
		gameObject.name = "Enviro Sat Camera";
		gameObject.transform.position = PlayerCamera.transform.position;
		gameObject.transform.rotation = PlayerCamera.transform.rotation;
		gameObject.hideFlags = HideFlags.DontSave;
		satCamera = gameObject.AddComponent<Camera>();
		satCamera.farClipPlane = PlayerCamera.farClipPlane;
		satCamera.nearClipPlane = PlayerCamera.nearClipPlane;
		satCamera.aspect = PlayerCamera.aspect;
		SetCameraHDR(satCamera, HDR);
		satCamera.useOcclusionCulling = false;
		satCamera.renderingPath = RenderingPath.Forward;
		satCamera.fieldOfView = PlayerCamera.fieldOfView;
		satCamera.clearFlags = CameraClearFlags.Color;
		satCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		satCamera.cullingMask = 1 << satelliteRenderingLayer;
		satCamera.depth = PlayerCamera.depth + 1f;
		satCamera.enabled = true;
		PlayerCamera.cullingMask &= ~(1 << satelliteRenderingLayer);
		RenderTextureFormat format = (GetCameraHDR(satCamera) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		satRenderTarget = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 16, format);
		satCamera.targetTexture = satRenderTarget;
		satCamera.enabled = false;
	}

	private void SetupMainLight()
	{
		if ((bool)Components.DirectLight)
		{
			MainLight = Components.DirectLight.GetComponent<Light>();
			if (directVolumeLight == null)
			{
				directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight>();
			}
			if (directVolumeLight == null)
			{
				directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight>();
			}
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(Components.DirectLight);
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
			if (directVolumeLight == null)
			{
				directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight>();
			}
			if (directVolumeLight == null)
			{
				directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight>();
			}
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(Components.DirectLight);
			}
		}
		if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single && Components.AdditionalDirectLight != null)
		{
			UnityEngine.Object.DestroyImmediate(Components.AdditionalDirectLight.gameObject);
		}
	}

	private void SetupAdditionalLight()
	{
		if ((bool)Components.AdditionalDirectLight)
		{
			AdditionalLight = Components.AdditionalDirectLight.GetComponent<Light>();
			if (additionalDirectVolumeLight == null)
			{
				additionalDirectVolumeLight = Components.AdditionalDirectLight.GetComponent<EnviroVolumeLight>();
			}
			if (additionalDirectVolumeLight == null)
			{
				additionalDirectVolumeLight = Components.AdditionalDirectLight.gameObject.AddComponent<EnviroVolumeLight>();
			}
			if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(Components.AdditionalDirectLight);
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
		if (additionalDirectVolumeLight == null)
		{
			additionalDirectVolumeLight = Components.AdditionalDirectLight.GetComponent<EnviroVolumeLight>();
		}
		if (additionalDirectVolumeLight == null)
		{
			additionalDirectVolumeLight = Components.AdditionalDirectLight.gameObject.AddComponent<EnviroVolumeLight>();
		}
		if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
		{
			UnityEngine.Object.DontDestroyOnLoad(Components.AdditionalDirectLight);
		}
	}

	private void RenderFlatCloudsMap()
	{
		if (flatCloudsMat == null)
		{
			flatCloudsMat = new Material(Shader.Find("Enviro/Standard/FlatCloudMap"));
		}
		flatCloudsRenderTarget = RenderTexture.GetTemporary(512 * (int)(cloudsSettings.flatCloudsResolution + 1), 512 * (int)(cloudsSettings.flatCloudsResolution + 1), 0, RenderTextureFormat.DefaultHDR);
		flatCloudsRenderTarget.wrapMode = TextureWrapMode.Repeat;
		flatCloudsMat.SetVector("_CloudAnimation", cloudAnimNonScaled);
		flatCloudsMat.SetTexture("_NoiseTex", cloudsSettings.flatCloudsNoiseTexture);
		flatCloudsMat.SetFloat("_CloudScale", cloudsSettings.flatCloudsScale);
		flatCloudsMat.SetFloat("_Coverage", cloudsConfig.flatCoverage);
		flatCloudsMat.SetInt("noiseOctaves", cloudsSettings.flatCloudsNoiseOctaves);
		flatCloudsMat.SetFloat("_Softness", cloudsConfig.flatSoftness);
		flatCloudsMat.SetFloat("_Brightness", cloudsConfig.flatBrightness);
		flatCloudsMat.SetFloat("_MorphingSpeed", cloudsSettings.flatCloudsMorphingSpeed);
		Graphics.Blit(null, flatCloudsRenderTarget, flatCloudsMat);
		RenderTexture.ReleaseTemporary(flatCloudsRenderTarget);
	}

	private void RenderWeatherMap()
	{
		if (cloudsSettings.customWeatherMap == null)
		{
			weatherMapMat.SetVector("_WindDir", cloudAnimNonScaled);
			weatherMapMat.SetFloat("_AnimSpeedScale", cloudsSettings.weatherAnimSpeedScale);
			weatherMapMat.SetInt("_Tiling", cloudsSettings.weatherMapTiling);
			weatherMapMat.SetVector("_Location", cloudsSettings.locationOffset);
			double value = cloudsConfig.coverage * cloudsSettings.globalCloudCoverage;
			weatherMapMat.SetFloat("_Coverage", (float)Math.Round(value, 4));
			weatherMapMat.SetFloat("_CloudsType", cloudsConfig.cloudType);
			weatherMapMat.SetFloat("_CoverageType", cloudsConfig.coverageType);
			Graphics.Blit(null, weatherMap, weatherMapMat);
		}
	}

	private void RenderCloudMaps()
	{
		if (Application.isPlaying)
		{
			if (useVolumeClouds)
			{
				RenderWeatherMap();
			}
			if (useFlatClouds)
			{
				RenderFlatCloudsMap();
			}
		}
		else
		{
			if (useVolumeClouds && showVolumeCloudsInEditor)
			{
				RenderWeatherMap();
			}
			if (useFlatClouds && showFlatCloudsInEditor)
			{
				RenderFlatCloudsMap();
			}
		}
	}

	private void Update()
	{
		if (profile == null)
		{
			Debug.Log("No profile applied! Please create and assign a profile.");
			return;
		}
		if (!Application.isPlaying && startMode != 0)
		{
			if (startMode == EnviroStartMode.Paused)
			{
				Stop(true);
			}
			else
			{
				GameTime.ProgressTime = EnviroTime.TimeProgressMode.Simulated;
				Stop(true, false);
			}
		}
		else if (!Application.isPlaying && startMode == EnviroStartMode.Started && !started)
		{
			Play(GameTime.ProgressTime);
		}
		if (!started && !serverMode)
		{
			UpdateTime(GameTime.DaysInYear);
			UpdateSunAndMoonPosition();
			UpdateSceneView();
			CalculateDirectLight();
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
			if (useFlatClouds != flatCloudsSkybox)
			{
				SetupSkybox();
			}
			UpdateSceneView();
			if (!Application.isPlaying && Weather.startWeatherPreset != null && startMode == EnviroStartMode.Started)
			{
				UpdateClouds(Weather.startWeatherPreset, false);
				UpdateFog(Weather.startWeatherPreset, false);
				UpdatePostProcessing(Weather.startWeatherPreset, false);
				UpdateWeatherVariables(Weather.startWeatherPreset);
				if (EnviroSkyMgr.instance.aura2Support)
				{
					UpdateAura2(Weather.startWeatherPreset, true);
				}
			}
			RenderCloudMaps();
			UpdateAmbientLight();
			UpdateReflections();
			UpdateWeather();
			if (Weather.currentActiveWeatherPreset != null && Weather.currentActiveWeatherPreset.cloudsConfig.particleCloudsOverwrite)
			{
				UpdateParticleClouds(true);
			}
			else
			{
				UpdateParticleClouds(useParticleClouds);
			}
			UpdateCloudShadows();
			UpdateSkyRenderingComponent();
			UpdateSunAndMoonPosition();
			CalculateDirectLight();
			SetMaterialsVariables();
			CalculateSatPositions(LST);
			if (directVolumeLight != null && !directVolumeLight.isActiveAndEnabled && volumeLightSettings.dirVolumeLighting)
			{
				directVolumeLight.enabled = true;
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
			base.transform.localScale = new Vector3(PlayerCamera.farClipPlane, PlayerCamera.farClipPlane, PlayerCamera.farClipPlane);
			if (EffectsHolder != null)
			{
				EffectsHolder.transform.position = Player.transform.position;
			}
		}
	}

	private void UpdateCloudShadows()
	{
		if (cloudsSettings.shadowIntensity == 0f || !useVolumeClouds)
		{
			if (MainLight.cookie != null)
			{
				MainLight.cookie = null;
			}
		}
		else if (cloudsSettings.shadowIntensity > 0f)
		{
			cloudShadowMap.DiscardContents(true, true);
			cloudShadowMat.SetFloat("_shadowIntensity", cloudsSettings.shadowIntensity);
			if (useVolumeClouds)
			{
				cloudShadowMat.SetTexture("_MainTex", weatherMap);
				Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);
			}
			if (Application.isPlaying)
			{
				MainLight.cookie = cloudShadowMap;
			}
			else
			{
				MainLight.cookie = null;
			}
			MainLight.cookieSize = cloudsSettings.shadowCookieSize;
		}
	}

	private void SetMaterialsVariables()
	{
		if (skyMat != null)
		{
			if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Simple)
			{
				skyMat.SetColor("_SkyColor", skySettings.simpleSkyColor.Evaluate(GameTime.solarTime));
				skyMat.SetColor("_HorizonColor", skySettings.simpleHorizonColor.Evaluate(GameTime.solarTime));
				skyMat.SetColor("_SunColor", skySettings.simpleSunColor.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_SunDiskSizeSimple", skySettings.simpleSunDiskSize.Evaluate(GameTime.solarTime));
			}
			else
			{
				skyMat.SetVector("_SunDir", -Components.Sun.transform.forward);
				skyMat.SetVector("_MoonDir", Components.Moon.transform.forward);
				skyMat.SetColor("_MoonColor", skySettings.moonColor);
				skyMat.SetColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
				skyMat.SetColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
				skyMat.SetColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
				skyMat.SetColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));
				skyMat.SetVector("_Bm", BetaMie(skySettings.turbidity, skySettings.waveLength) * (skySettings.mie * Fog.scatteringStrenght));
				skyMat.SetVector("_Br", BetaRay(skySettings.waveLength) * skySettings.rayleigh);
				skyMat.SetVector("_mieG", GetMieG(skySettings.g));
				skyMat.SetFloat("_SunIntensity", skySettings.sunIntensity);
				skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
				skyMat.SetFloat("_SunDiskIntensity", skySettings.sunDiskIntensity);
				skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
				skyMat.SetFloat("_Exposure", skySettings.skyExposure);
				skyMat.SetFloat("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_StarsIntensity", skySettings.starsIntensity.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_GalaxyIntensity", skySettings.galaxyIntensity.Evaluate(GameTime.solarTime));
				if (skySettings.dithering)
				{
					skyMat.SetInt("_UseDithering", 1);
				}
				else
				{
					skyMat.SetInt("_UseDithering", 0);
				}
				if (skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Realistic)
				{
					float num = Vector3.SignedAngle(Components.Moon.transform.forward, Components.Sun.transform.forward, base.transform.forward);
					if (GameTime.Latitude >= 0f)
					{
						if (num < 0f)
						{
							customMoonPhase = Remap(num, 0f, -180f, -2f, 0f);
						}
						else
						{
							customMoonPhase = Remap(num, 0f, 180f, 2f, 0f);
						}
					}
					else if (num < 0f)
					{
						customMoonPhase = Remap(num, 0f, -180f, 2f, 0f);
					}
					else
					{
						customMoonPhase = Remap(num, 0f, 180f, -2f, 0f);
					}
				}
				skyMat.SetColor("_moonGlowColor", skySettings.moonGlowColor);
				skyMat.SetVector("_moonParams", new Vector4(skySettings.moonSize, skySettings.glowSize, skySettings.moonGlow.Evaluate(GameTime.solarTime), customMoonPhase));
				if (skySettings.renderMoon)
				{
					skyMat.SetTexture("_MoonTex", skySettings.moonTexture);
					skyMat.SetTexture("_GlowTex", skySettings.glowTexture);
				}
				else
				{
					skyMat.SetTexture("_MoonTex", null);
					skyMat.SetTexture("_GlowTex", null);
				}
				if (skySettings.blackGroundMode)
				{
					skyMat.SetInt("_blackGround", 1);
				}
				else
				{
					skyMat.SetInt("_blackGround", 0);
				}
				float value = (HDR ? 1f : 0f);
				skyMat.SetFloat("_hdr", value);
				skyMat.SetFloat("_StarsTwinkling", skySettings.starsTwinklingRate);
				if (skySettings.starsTwinklingRate > 0f)
				{
					starsTwinklingRot += skySettings.starsTwinklingRate * Time.deltaTime;
					Quaternion q = Quaternion.Euler(starsTwinklingRot, starsTwinklingRot, starsTwinklingRot);
					Matrix4x4 value2 = Matrix4x4.TRS(Vector3.zero, q, new Vector3(1f, 1f, 1f));
					skyMat.SetMatrix("_StarsTwinklingMatrix", value2);
				}
				if (useAurora)
				{
					skyMat.EnableKeyword("ENVIRO_AURORA");
					skyMat.SetFloat("_AuroraIntensity", Mathf.Clamp01(auroraIntensity * auroraSettings.auroraIntensity.Evaluate(GameTime.solarTime)));
					skyMat.SetFloat("_AuroraBrightness", auroraSettings.auroraBrightness);
					skyMat.SetFloat("_AuroraContrast", auroraSettings.auroraContrast);
					skyMat.SetColor("_AuroraColor", auroraSettings.auroraColor);
					skyMat.SetFloat("_AuroraHeight", auroraSettings.auroraHeight);
					skyMat.SetFloat("_AuroraScale", auroraSettings.auroraScale);
					skyMat.SetFloat("_AuroraSpeed", auroraSettings.auroraSpeed);
					skyMat.SetFloat("_AuroraSteps", auroraSettings.auroraSteps);
					skyMat.SetFloat("_AuroraSteps", auroraSettings.auroraSteps);
					skyMat.SetVector("_Aurora_Tiling_Layer1", auroraSettings.auroraLayer1Settings);
					skyMat.SetVector("_Aurora_Tiling_Layer2", auroraSettings.auroraLayer2Settings);
					skyMat.SetVector("_Aurora_Tiling_ColorShift", auroraSettings.auroraColorshiftSettings);
				}
				else
				{
					skyMat.DisableKeyword("ENVIRO_AURORA");
				}
			}
			skyMat.SetVector("_CloudAnimation", cloudAnim);
			skyMat.SetVector("_CloudCirrusAnimation", cirrusAnim);
			if (cloudsSettings.cirrusCloudsTexture != null)
			{
				skyMat.SetTexture("_CloudMap", cloudsSettings.cirrusCloudsTexture);
			}
			skyMat.SetColor("_CloudColor", cloudsSettings.cirrusCloudsColor.Evaluate(GameTime.solarTime));
			skyMat.SetFloat("_CloudAltitude", cloudsSettings.cirrusCloudsAltitude);
			skyMat.SetFloat("_CloudAlpha", cloudsConfig.cirrusAlpha);
			skyMat.SetFloat("_CloudCoverage", cloudsConfig.cirrusCoverage);
			skyMat.SetFloat("_CloudColorPower", cloudsConfig.cirrusColorPow);
			if (flatCloudsRenderTarget != null)
			{
				skyMat.SetTexture("_Cloud1Map", flatCloudsRenderTarget);
				skyMat.SetColor("_Cloud1Color", cloudsSettings.flatCloudsColor.Evaluate(GameTime.solarTime));
				skyMat.SetFloat("_Cloud1Altitude", cloudsSettings.flatCloudsAltitude);
				skyMat.SetFloat("_Cloud1Alpha", cloudsConfig.flatAlpha);
				skyMat.SetFloat("_Cloud1ColorPower", cloudsConfig.flatColorPow);
			}
		}
		Shader.SetGlobalColor("_EnviroLighting", lightSettings.LightColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalVector("_SunDirection", -Components.Sun.transform.forward);
		Shader.SetGlobalVector("_SunPosition", Components.Sun.transform.localPosition + -Components.Sun.transform.forward * 10000f);
		Shader.SetGlobalVector("_MoonPosition", Components.Moon.transform.localPosition);
		Shader.SetGlobalVector("_SunDir", -Components.Sun.transform.forward);
		Shader.SetGlobalVector("_MoonDir", -Components.Moon.transform.forward);
		Shader.SetGlobalColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
		Shader.SetGlobalColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));
		Shader.SetGlobalFloat("_gameTime", Mathf.Clamp(1f - GameTime.solarTime, 0.5f, 1f));
		Shader.SetGlobalVector("_EnviroSkyFog", new Vector4(Fog.skyFogHeight, Fog.skyFogIntensity, Fog.skyFogStart, fogSettings.heightFogIntensity));
		Shader.SetGlobalFloat("_scatteringStrenght", Fog.scatteringStrenght);
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
		Shader.SetGlobalFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat("_distanceFogIntensity", fogSettings.distanceFogIntensity);
		if (Application.isPlaying || showFogInEditor)
		{
			Shader.SetGlobalFloat("_maximumFogDensity", 1f - fogSettings.maximumFogDensity);
		}
		else if (!showFogInEditor)
		{
			Shader.SetGlobalFloat("_maximumFogDensity", 1f);
		}
		Shader.SetGlobalFloat("_lightning", thunder);
		if (fogSettings.useSimpleFog)
		{
			Shader.EnableKeyword("ENVIRO_SIMPLE_FOG");
		}
		else
		{
			Shader.DisableKeyword("ENVIRO_SIMPLE_FOG");
		}
	}

	private void UpdateSkyRenderingComponent()
	{
		if (!(EnviroSkyRender == null) && EnviroSkyRender.fogMat != null)
		{
			EnviroSkyRender.fogMat.SetTexture("_Clouds", cloudsRenderTarget);
			float value = (HDR ? 1f : 0f);
			EnviroSkyRender.fogMat.SetFloat("_hdr", value);
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
			cloudsConfig.coverage = Mathf.Lerp(cloudsConfig.coverage, i.cloudsConfig.coverage, num);
			cloudsConfig.ambientSkyColorIntensity = Mathf.Lerp(cloudsConfig.ambientSkyColorIntensity, i.cloudsConfig.ambientSkyColorIntensity, num);
			if (useVolumeClouds)
			{
				cloudsConfig.coverageModBottom = Mathf.Lerp(cloudsConfig.coverageModBottom, i.cloudsConfig.coverageModBottom, num);
				cloudsConfig.coverageModTop = Mathf.Lerp(cloudsConfig.coverageModTop, i.cloudsConfig.coverageModTop, num);
				cloudsConfig.raymarchingScale = Mathf.Lerp(cloudsConfig.raymarchingScale, i.cloudsConfig.raymarchingScale, num);
				cloudsConfig.ambientSkyColorIntensity = Mathf.Lerp(cloudsConfig.ambientSkyColorIntensity, i.cloudsConfig.ambientSkyColorIntensity, num);
				cloudsConfig.density = Mathf.Lerp(cloudsConfig.density, i.cloudsConfig.density, num);
				cloudsConfig.densityLightning = Mathf.Lerp(cloudsConfig.densityLightning, i.cloudsConfig.densityLightning, num);
				cloudsConfig.scatteringCoef = Mathf.Lerp(cloudsConfig.scatteringCoef, i.cloudsConfig.scatteringCoef, num);
				cloudsConfig.cloudType = Mathf.Lerp(cloudsConfig.cloudType, i.cloudsConfig.cloudType, num);
				cloudsConfig.coverageType = Mathf.Lerp(cloudsConfig.coverageType, i.cloudsConfig.coverageType, num);
				cloudsConfig.edgeDarkness = Mathf.Lerp(cloudsConfig.edgeDarkness, i.cloudsConfig.edgeDarkness, num);
				cloudsConfig.baseErosionIntensity = Mathf.Lerp(cloudsConfig.baseErosionIntensity, i.cloudsConfig.baseErosionIntensity, num);
				cloudsConfig.detailErosionIntensity = Mathf.Lerp(cloudsConfig.detailErosionIntensity, i.cloudsConfig.detailErosionIntensity, num);
			}
			if (useFlatClouds)
			{
				cloudsConfig.flatAlpha = Mathf.Lerp(cloudsConfig.flatAlpha, i.cloudsConfig.flatAlpha, num);
				cloudsConfig.flatCoverage = Mathf.Lerp(cloudsConfig.flatCoverage, i.cloudsConfig.flatCoverage, num);
				cloudsConfig.flatColorPow = Mathf.Lerp(cloudsConfig.flatColorPow, i.cloudsConfig.flatColorPow, num);
				cloudsConfig.flatSoftness = Mathf.Lerp(cloudsConfig.flatSoftness, i.cloudsConfig.flatSoftness, num);
				cloudsConfig.flatBrightness = Mathf.Lerp(cloudsConfig.flatBrightness, i.cloudsConfig.flatBrightness, num);
			}
			cloudsConfig.particleLayer1Alpha = Mathf.Lerp(cloudsConfig.particleLayer1Alpha, i.cloudsConfig.particleLayer1Alpha, num * 0.25f);
			cloudsConfig.particleLayer1Brightness = Mathf.Lerp(cloudsConfig.particleLayer1Brightness, i.cloudsConfig.particleLayer1Brightness, num * 0.25f);
			cloudsConfig.particleLayer2Alpha = Mathf.Lerp(cloudsConfig.particleLayer2Alpha, i.cloudsConfig.particleLayer2Alpha, num * 0.25f);
			cloudsConfig.particleLayer2Brightness = Mathf.Lerp(cloudsConfig.particleLayer2Brightness, i.cloudsConfig.particleLayer2Brightness, num * 0.25f);
			globalVolumeLightIntensity = Mathf.Lerp(globalVolumeLightIntensity, i.volumeLightIntensity, num);
			shadowIntensityMod = Mathf.Lerp(shadowIntensityMod, i.shadowIntensityMod, num);
			currentWeatherSkyMod = Color.Lerp(currentWeatherSkyMod, i.weatherSkyMod.Evaluate(GameTime.solarTime), num);
			currentWeatherFogMod = Color.Lerp(currentWeatherFogMod, i.weatherFogMod.Evaluate(GameTime.solarTime), num * 10f);
			currentWeatherLightMod = Color.Lerp(currentWeatherLightMod, i.weatherLightMod.Evaluate(GameTime.solarTime), num);
			auroraIntensity = Mathf.Lerp(auroraIntensity, i.auroraIntensity, num);
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
			fogSettings.heightDensity = Mathf.Lerp(fogSettings.heightDensity, i.heightFogDensity, t);
			Fog.skyFogStart = Mathf.Lerp(Fog.skyFogStart, i.skyFogStart, t);
			Fog.skyFogHeight = Mathf.Lerp(Fog.skyFogHeight, i.SkyFogHeight, t);
			Fog.skyFogIntensity = Mathf.Lerp(Fog.skyFogIntensity, i.SkyFogIntensity, t);
			fogSettings.skyFogIntensity = Mathf.Lerp(fogSettings.skyFogIntensity, i.SkyFogIntensity, t);
			Fog.scatteringStrenght = Mathf.Lerp(Fog.scatteringStrenght, i.FogScatteringIntensity, t);
			Fog.sunBlocking = Mathf.Lerp(Fog.sunBlocking, i.fogSunBlocking, t);
		}
	}

	private void UpdatePostProcessing(EnviroWeatherPreset i, bool withTransition)
	{
		if (i != null)
		{
			float t = 500f * Time.deltaTime;
			if (withTransition)
			{
				t = 10f * Time.deltaTime;
			}
			blurDistance = Mathf.Lerp(blurDistance, i.blurDistance, t);
			blurIntensity = Mathf.Lerp(blurIntensity, i.blurIntensity, t);
			blurSkyIntensity = Mathf.Lerp(blurSkyIntensity, i.blurSkyIntensity, t);
		}
	}

	private void UpdateEffectSystems(EnviroWeatherPrefab id, bool withTransition)
	{
		if (!(id != null))
		{
			return;
		}
		float num = 500f * Time.deltaTime;
		if (withTransition)
		{
			num = weatherSettings.effectTransitionSpeed * Time.deltaTime;
		}
		for (int i = 0; i < id.effectSystems.Count; i++)
		{
			if (id.effectSystems[i].isStopped)
			{
				id.effectSystems[i].Play();
			}
			float emissionRate = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(id.effectSystems[i]), id.effectEmmisionRates[i] * qualitySettings.GlobalParticleEmissionRates, num) * interiorZoneSettings.currentInteriorWeatherEffectMod;
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
				float num2 = Mathf.Lerp(EnviroSkyMgr.instance.GetEmissionRate(Weather.WeatherPrefabs[j].effectSystems[k]), 0f, num * 10f);
				if (num2 < 1f)
				{
					num2 = 0f;
				}
				EnviroSkyMgr.instance.SetEmissionRate(Weather.WeatherPrefabs[j].effectSystems[k], num2);
				if (num2 == 0f && !Weather.WeatherPrefabs[j].effectSystems[k].isStopped)
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
			UpdatePostProcessing(Weather.currentActiveWeatherPreset, true);
			UpdateEffectSystems(Weather.currentActiveWeatherPrefab, true);
			if (EnviroSkyMgr.instance.aura2Support)
			{
				UpdateAura2(Weather.currentActiveWeatherPreset, true);
			}
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

	public void PopulateCloudsQualityList()
	{
	}

	public void ApplyVolumeCloudsQualityPreset(EnviroVolumeCloudsQuality preset)
	{
		cloudsSettings.cloudsQualitySettings = preset.qualitySettings;
		currentActiveCloudsQualityPreset = preset;
	}

	public void ApplyVolumeCloudsQualityPreset(string name)
	{
		for (int i = 0; i < cloudsQualityList.Count; i++)
		{
			if (cloudsQualityList[i].name == name)
			{
				cloudsSettings.cloudsQualitySettings = cloudsQualityList[i].qualitySettings;
				currentActiveCloudsQualityPreset = cloudsQualityList[i];
				selectedCloudsQuality = i;
			}
		}
	}

	public void ApplyVolumeCloudsQualityPreset(int id)
	{
		if (id < cloudsQualityList.Count && id >= 0)
		{
			cloudsSettings.cloudsQualitySettings = cloudsQualityList[id].qualitySettings;
			currentActiveCloudsQualityPreset = cloudsQualityList[id];
			selectedCloudsQuality = id;
		}
	}

	public void InstantWeatherChange(EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
	{
		UpdateClouds(preset, false);
		UpdateFog(preset, false);
		UpdatePostProcessing(preset, false);
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
		EnviroSkyRendering component = cam.GetComponent<EnviroSkyRendering>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		EnviroPostProcessing component2 = cam.GetComponent<EnviroPostProcessing>();
		if (component2 != null)
		{
			UnityEngine.Object.Destroy(component2);
		}
	}

	public void Play(EnviroTime.TimeProgressMode progressMode = EnviroTime.TimeProgressMode.Simulated)
	{
		StartCoroutine(SetSceneSettingsLate());
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
		if (EnviroPostProcessing != null)
		{
			EnviroPostProcessing.enabled = true;
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
