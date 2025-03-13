using System;
using System.Collections.Generic;
using UnityEngine;

public class EnviroSkyMgr : MonoBehaviour
{
	[Serializable]
	public class EnviroBaking
	{
		public int resolution = 2048;
	}

	public enum EnviroSkyVersion
	{
		None = 0,
		LW = 1,
		HD = 2
	}

	public enum EnviroRenderPipeline
	{
		Legacy = 0
	}

	public delegate void HourPassed();

	public delegate void DayPassed();

	public delegate void YearPassed();

	public delegate void WeatherChanged(EnviroWeatherPreset weatherType);

	public delegate void ZoneWeatherChanged(EnviroWeatherPreset weatherType, EnviroZone zone);

	public delegate void SeasonChanged(EnviroSeasons.Seasons season);

	public delegate void isNightE();

	public delegate void isDay();

	public delegate void ZoneChanged(EnviroZone zone);

	private static EnviroSkyMgr _instance;

	[Header("General")]
	[Tooltip("Enable to make sure thast enviro objects don't get destroyed on scene load.")]
	public bool dontDestroy;

	public bool showSetup = true;

	public bool showInstances = true;

	public bool showThirdParty;

	public bool showUtilities;

	public bool showThirdPartyShaders;

	public bool showThirdPartyMisc;

	public bool showThirdPartyNetwork;

	public bool showUtiliies;

	public RenderTexture cube;

	public bool aura2Support;

	public AnimationCurve aura2DirectionalLightIntensity = new AnimationCurve(new Keyframe(0f, 0.1f), new Keyframe(1f, 0.25f));

	public AnimationCurve aura2DirectionalLightIntensityMoon = new AnimationCurve(new Keyframe(0f, 0.1f), new Keyframe(1f, 0.25f));

	public float aura2TransitionSpeed = 1f;

	public EnviroBaking skyBaking;

	public EnviroRenderPipeline currentRenderPipeline;

	public EnviroSkyVersion currentEnviroSkyVersion = EnviroSkyVersion.HD;

	public EnviroSky enviroHDInstance;

	public EnviroSkyLite enviroLWInstance;

	public static EnviroSkyMgr instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType<EnviroSkyMgr>();
			}
			return _instance;
		}
	}

	public EnviroComponents Components
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Components;
			}
			return EnviroSkyLite.instance.Components;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Components = value;
			}
			else
			{
				EnviroSkyLite.instance.Components = value;
			}
		}
	}

	public EnviroTime Time
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.GameTime;
			}
			return EnviroSkyLite.instance.GameTime;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.GameTime = value;
			}
			else
			{
				EnviroSkyLite.instance.GameTime = value;
			}
		}
	}

	public EnviroSeasons Seasons
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Seasons;
			}
			return EnviroSkyLite.instance.Seasons;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Seasons = value;
			}
			else
			{
				EnviroSkyLite.instance.Seasons = value;
			}
		}
	}

	public EnviroSeasonSettings SeasonSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.seasonsSettings;
			}
			return EnviroSkyLite.instance.seasonsSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.seasonsSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.seasonsSettings = value;
			}
		}
	}

	public EnviroAuroraSettings AuroraSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.auroraSettings;
			}
			return null;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.auroraSettings = value;
			}
		}
	}

	public EnviroReflectionSettings ReflectionSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.reflectionSettings;
			}
			return EnviroSkyLite.instance.reflectionSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.reflectionSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.reflectionSettings = value;
			}
		}
	}

	public EnviroCloudSettings CloudSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.cloudsSettings;
			}
			return EnviroSkyLite.instance.cloudsSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.cloudsSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.cloudsSettings = value;
			}
		}
	}

	public EnviroInteriorZoneSettings InteriorZoneSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.interiorZoneSettings;
			}
			return EnviroSkyLite.instance.interiorZoneSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.interiorZoneSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.interiorZoneSettings = value;
			}
		}
	}

	public EnviroAudio AudioSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Audio;
			}
			return EnviroSkyLite.instance.Audio;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Audio = value;
			}
			else
			{
				EnviroSkyLite.instance.Audio = value;
			}
		}
	}

	public EnviroWeatherCloudsConfig Clouds
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.cloudsConfig;
			}
			return EnviroSkyLite.instance.cloudsConfig;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.cloudsConfig = value;
			}
			else
			{
				EnviroSkyLite.instance.cloudsConfig = value;
			}
		}
	}

	public EnviroWeatherSettings WeatherSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.weatherSettings;
			}
			return EnviroSkyLite.instance.weatherSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.weatherSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.weatherSettings = value;
			}
		}
	}

	public EnviroLightSettings LightSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.lightSettings;
			}
			return EnviroSkyLite.instance.lightSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.lightSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.lightSettings = value;
			}
		}
	}

	public EnviroVolumeLightingSettings VolumeLightSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.volumeLightSettings;
			}
			return EnviroSkyLite.instance.volumeLightSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.volumeLightSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.volumeLightSettings = value;
			}
		}
	}

	public EnviroLightShaftsSettings LightShaftsSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.lightshaftsSettings;
			}
			return EnviroSkyLite.instance.lightshaftsSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.lightshaftsSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.lightshaftsSettings = value;
			}
		}
	}

	public EnviroSkySettings SkySettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.skySettings;
			}
			return EnviroSkyLite.instance.skySettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.skySettings = value;
			}
			else
			{
				EnviroSkyLite.instance.skySettings = value;
			}
		}
	}

	public EnviroFogSettings FogSettings
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.fogSettings;
			}
			return EnviroSkyLite.instance.fogSettings;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.fogSettings = value;
			}
			else
			{
				EnviroSkyLite.instance.fogSettings = value;
			}
		}
	}

	public GameObject Player
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Player;
			}
			return EnviroSkyLite.instance.Player;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Player = value;
			}
			else
			{
				EnviroSkyLite.instance.Player = value;
			}
		}
	}

	public Camera Camera
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.PlayerCamera;
			}
			return EnviroSkyLite.instance.PlayerCamera;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.PlayerCamera = value;
			}
			else
			{
				EnviroSkyLite.instance.PlayerCamera = value;
			}
		}
	}

	public EnviroWeather Weather
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Weather;
			}
			return EnviroSkyLite.instance.Weather;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Weather = value;
			}
			else
			{
				EnviroSkyLite.instance.Weather = value;
			}
		}
	}

	public float CustomFogIntensity
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.customFogIntensity;
			}
			return EnviroSkyLite.instance.customFogIntensity;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.customFogIntensity = value;
			}
			else
			{
				EnviroSkyLite.instance.customFogIntensity = value;
			}
		}
	}

	public Color CustomFogColor
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.customFogColor;
			}
			return EnviroSkyLite.instance.customFogColor;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.customFogColor = value;
			}
			else
			{
				EnviroSkyLite.instance.customFogColor = value;
			}
		}
	}

	public bool UpdateFogIntensity
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.updateFogDensity;
			}
			return EnviroSkyLite.instance.updateFogDensity;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.updateFogDensity = value;
			}
			else
			{
				EnviroSkyLite.instance.updateFogDensity = value;
			}
		}
	}

	public float ambientAudioVolume
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Audio.ambientSFXVolume;
			}
			return EnviroSkyLite.instance.Audio.ambientSFXVolume;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Audio.ambientSFXVolume = value;
			}
			else
			{
				EnviroSkyLite.instance.Audio.ambientSFXVolume = value;
			}
		}
	}

	public float weatherAudioVolume
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Audio.weatherSFXVolume;
			}
			return EnviroSkyLite.instance.Audio.weatherSFXVolume;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Audio.weatherSFXVolume = value;
			}
			else
			{
				EnviroSkyLite.instance.Audio.weatherSFXVolume = value;
			}
		}
	}

	public float ambientAudioVolumeModifier
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Audio.ambientSFXVolumeMod;
			}
			return EnviroSkyLite.instance.Audio.ambientSFXVolumeMod;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Audio.ambientSFXVolumeMod = value;
			}
			else
			{
				EnviroSkyLite.instance.Audio.ambientSFXVolumeMod = value;
			}
		}
	}

	public float weatherAudioVolumeModifier
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.Audio.weatherSFXVolumeMod;
			}
			return EnviroSkyLite.instance.Audio.weatherSFXVolumeMod;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.Audio.weatherSFXVolumeMod = value;
			}
			else
			{
				EnviroSkyLite.instance.Audio.weatherSFXVolumeMod = value;
			}
		}
	}

	public float audioTransitionSpeed
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.weatherSettings.audioTransitionSpeed;
			}
			return EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.weatherSettings.audioTransitionSpeed = value;
			}
			else
			{
				EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed = value;
			}
		}
	}

	public float interiorZoneAudioVolume
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
			}
			return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
			}
			else
			{
				EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
			}
		}
	}

	public float interiorZoneAudioFadingSpeed
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
			}
			return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
			}
			else
			{
				EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
			}
		}
	}

	public bool useVolumeClouds
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useVolumeClouds;
			}
			return false;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useVolumeClouds = value;
			}
		}
	}

	public bool useAurora
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useAurora;
			}
			return false;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useAurora = value;
			}
		}
	}

	public bool useVolumeLighting
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useVolumeLighting;
			}
			return false;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useVolumeLighting = value;
			}
		}
	}

	public bool useFlatClouds
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useFlatClouds;
			}
			return false;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useFlatClouds = value;
			}
		}
	}

	public bool useParticleClouds
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useParticleClouds;
			}
			return EnviroSkyLite.instance.useParticleClouds;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useParticleClouds = value;
			}
			else
			{
				EnviroSkyLite.instance.useParticleClouds = value;
			}
		}
	}

	public bool useSunShafts
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.LightShafts.sunLightShafts;
			}
			return EnviroSkyLite.instance.LightShafts.sunLightShafts;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.LightShafts.sunLightShafts = value;
			}
			else
			{
				EnviroSkyLite.instance.LightShafts.sunLightShafts = value;
			}
		}
	}

	public bool useMoonShafts
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.LightShafts.moonLightShafts;
			}
			return EnviroSkyLite.instance.LightShafts.moonLightShafts;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.LightShafts.moonLightShafts = value;
			}
			else
			{
				EnviroSkyLite.instance.LightShafts.moonLightShafts = value;
			}
		}
	}

	public bool useDistanceBlur
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.useDistanceBlur;
			}
			return false;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.useDistanceBlur = value;
			}
		}
	}

	public bool interiorMode
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.interiorMode;
			}
			return EnviroSkyLite.instance.interiorMode;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.interiorMode = value;
			}
			else
			{
				EnviroSkyLite.instance.interiorMode = value;
			}
		}
	}

	public EnviroInterior lastInteriorZone
	{
		get
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				return EnviroSky.instance.lastInteriorZone;
			}
			return EnviroSkyLite.instance.lastInteriorZone;
		}
		set
		{
			if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
			{
				EnviroSky.instance.lastInteriorZone = value;
			}
			else
			{
				EnviroSkyLite.instance.lastInteriorZone = value;
			}
		}
	}

	public event HourPassed OnHourPassed;

	public event DayPassed OnDayPassed;

	public event YearPassed OnYearPassed;

	public event WeatherChanged OnWeatherChanged;

	public event ZoneWeatherChanged OnZoneWeatherChanged;

	public event SeasonChanged OnSeasonChanged;

	public event isNightE OnNightTime;

	public event isDay OnDayTime;

	public event ZoneChanged OnZoneChanged;

	private void Start()
	{
		if (Application.isPlaying && dontDestroy)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public void ActivateHDInstance()
	{
		if (enviroHDInstance != null)
		{
			if (enviroLWInstance != null)
			{
				enviroLWInstance.Deactivate();
				enviroLWInstance.gameObject.SetActive(false);
			}
			enviroHDInstance.gameObject.SetActive(true);
			enviroHDInstance.Activate();
			currentEnviroSkyVersion = EnviroSkyVersion.HD;
		}
	}

	public void DeactivateHDInstance()
	{
		if (enviroHDInstance != null)
		{
			enviroHDInstance.Deactivate();
			enviroHDInstance.gameObject.SetActive(false);
			if (enviroLWInstance != null && !enviroLWInstance.gameObject.activeSelf)
			{
				currentEnviroSkyVersion = EnviroSkyVersion.None;
			}
			else
			{
				currentEnviroSkyVersion = EnviroSkyVersion.LW;
			}
		}
	}

	public void ActivateLWInstance()
	{
		if (enviroLWInstance != null)
		{
			if (enviroHDInstance != null)
			{
				enviroHDInstance.Deactivate();
				enviroHDInstance.gameObject.SetActive(false);
			}
			enviroLWInstance.gameObject.SetActive(true);
			enviroLWInstance.Activate();
			currentEnviroSkyVersion = EnviroSkyVersion.LW;
		}
	}

	public void DeactivateLWInstance()
	{
		if (enviroLWInstance != null)
		{
			enviroLWInstance.Deactivate();
			enviroLWInstance.gameObject.SetActive(false);
			if (enviroHDInstance != null && !enviroHDInstance.gameObject.activeSelf)
			{
				currentEnviroSkyVersion = EnviroSkyVersion.None;
			}
			else
			{
				currentEnviroSkyVersion = EnviroSkyVersion.HD;
			}
		}
	}

	public void DeleteHDInstance()
	{
		if (enviroHDInstance != null)
		{
			UnityEngine.Object.DestroyImmediate(enviroHDInstance.EffectsHolder);
			UnityEngine.Object.DestroyImmediate(enviroHDInstance.gameObject);
			if (enviroHDInstance.EnviroSkyRender != null)
			{
				UnityEngine.Object.DestroyImmediate(enviroHDInstance.EnviroSkyRender);
			}
			if (enviroHDInstance.EnviroPostProcessing != null)
			{
				UnityEngine.Object.DestroyImmediate(enviroHDInstance.EnviroPostProcessing);
			}
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	public void DeleteLWInstance()
	{
		if (enviroLWInstance != null)
		{
			UnityEngine.Object.DestroyImmediate(enviroLWInstance.EffectsHolder);
			UnityEngine.Object.DestroyImmediate(enviroLWInstance.gameObject);
			if (enviroLWInstance.EnviroSkyRender != null)
			{
				UnityEngine.Object.DestroyImmediate(enviroLWInstance.EnviroSkyRender);
			}
			if (enviroLWInstance.EnviroPostProcessing != null)
			{
				UnityEngine.Object.DestroyImmediate(enviroLWInstance.EnviroPostProcessing);
			}
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	public void SearchForEnviroInstances()
	{
		enviroHDInstance = GetComponentInChildren<EnviroSky>();
		enviroLWInstance = GetComponentInChildren<EnviroSkyLite>();
	}

	public void CreateEnviroHDInstance()
	{
		GameObject assetPrefab = GetAssetPrefab("Internal_Enviro_HD");
		if (assetPrefab != null && EnviroSky.instance == null)
		{
			DeactivateAllInstances();
			GameObject gameObject = UnityEngine.Object.Instantiate(assetPrefab, Vector3.zero, Quaternion.identity);
			gameObject.name = "EnviroSky Standard";
			gameObject.transform.SetParent(base.transform);
			enviroHDInstance = gameObject.GetComponent<EnviroSky>();
			gameObject.SetActive(false);
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	public void CreateEnviroHDVRInstance()
	{
		GameObject assetPrefab = GetAssetPrefab("Internal_Enviro_HD_VR");
		if (assetPrefab != null && EnviroSky.instance == null)
		{
			DeactivateAllInstances();
			GameObject gameObject = UnityEngine.Object.Instantiate(assetPrefab, Vector3.zero, Quaternion.identity);
			gameObject.name = "EnviroSky Standard for VR";
			gameObject.transform.SetParent(base.transform);
			enviroHDInstance = gameObject.GetComponent<EnviroSky>();
			gameObject.SetActive(false);
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	public void CreateEnviroLWInstance()
	{
		GameObject assetPrefab = GetAssetPrefab("Internal_Enviro_LW");
		if (assetPrefab != null && EnviroSkyLite.instance == null)
		{
			DeactivateAllInstances();
			GameObject gameObject = UnityEngine.Object.Instantiate(assetPrefab, Vector3.zero, Quaternion.identity);
			gameObject.name = "EnviroSky Lite";
			gameObject.transform.SetParent(base.transform);
			enviroLWInstance = gameObject.GetComponent<EnviroSkyLite>();
			gameObject.SetActive(false);
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	public void CreateEnviroLWMobileInstance()
	{
		GameObject assetPrefab = GetAssetPrefab("Internal_Enviro_LW_MOBILE");
		if (assetPrefab != null && EnviroSkyLite.instance == null)
		{
			DeactivateAllInstances();
			GameObject gameObject = UnityEngine.Object.Instantiate(assetPrefab, Vector3.zero, Quaternion.identity);
			gameObject.name = "EnviroSky Lite for Mobiles";
			gameObject.transform.SetParent(base.transform);
			enviroLWInstance = gameObject.GetComponent<EnviroSkyLite>();
			gameObject.SetActive(false);
			currentEnviroSkyVersion = EnviroSkyVersion.None;
		}
	}

	private void DeactivateAllInstances()
	{
		if (enviroHDInstance != null)
		{
			DeactivateHDInstance();
		}
		if (enviroLWInstance != null)
		{
			DeactivateLWInstance();
		}
	}

	public GameObject GetAssetPrefab(string name)
	{
		return null;
	}

	public void ActivateLegacyRP()
	{
		currentRenderPipeline = EnviroRenderPipeline.Legacy;
	}

	public void AssignAndStart(GameObject Player, Camera cam)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.AssignAndStart(Player, cam);
		}
		else
		{
			EnviroSkyLite.instance.AssignAndStart(Player, cam);
		}
	}

	public void ChangeFocus(GameObject Player, Camera cam)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ChangeFocus(Player, cam);
		}
		else
		{
			EnviroSkyLite.instance.ChangeFocus(Player, cam);
		}
	}

	public void StartAsServer()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.StartAsServer();
		}
		else
		{
			EnviroSkyLite.instance.StartAsServer();
		}
	}

	public void ReInit()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ReInit();
		}
		else
		{
			EnviroSkyLite.instance.ReInit();
		}
	}

	public void SetupSkybox()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetupSkybox();
		}
		else
		{
			EnviroSkyLite.instance.SetupSkybox();
		}
	}

	public bool IsNight()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.isNight;
		}
		return EnviroSkyLite.instance.isNight;
	}

	public bool IsStarted()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.started;
		}
		return EnviroSkyLite.instance.started;
	}

	public bool IsInterior()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.interiorMode;
		}
		return EnviroSkyLite.instance.interiorMode;
	}

	public bool IsEnviroSkyAttached(GameObject obj)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return obj.GetComponent<EnviroSky>();
		}
		return obj.GetComponent<EnviroSkyLite>();
	}

	public bool IsDefaultZone(GameObject zone)
	{
		if ((bool)zone.GetComponent<EnviroSky>() || (bool)zone.GetComponent<EnviroSkyLite>())
		{
			return true;
		}
		return false;
	}

	public bool IsAutoWeatherUpdateActive()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.updateWeather;
		}
		return EnviroSkyLite.instance.Weather.updateWeather;
	}

	public bool IsAvailable()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			if (EnviroSky.instance == null)
			{
				return false;
			}
			return true;
		}
		if (EnviroSkyLite.instance == null)
		{
			return false;
		}
		return true;
	}

	public bool GetUseWeatherTag()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.weatherSettings.useTag;
		}
		return EnviroSkyLite.instance.weatherSettings.useTag;
	}

	public string GetEnviroSkyTag()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.tag;
		}
		return EnviroSkyLite.instance.tag;
	}

	public float GetSnowIntensity()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.curSnowStrength;
		}
		return EnviroSkyLite.instance.Weather.curSnowStrength;
	}

	public float GetWetnessIntensity()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.curWetness;
		}
		return EnviroSkyLite.instance.Weather.curWetness;
	}

	public string GetCurrentTemperatureString()
	{
		int num = 0;
		return ((currentEnviroSkyVersion != EnviroSkyVersion.HD) ? ((int)EnviroSkyLite.instance.Weather.currentTemperature) : ((int)EnviroSky.instance.Weather.currentTemperature)) + "°C";
	}

	public EnviroZone GetZoneByID(int id)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.zones[id];
		}
		return EnviroSkyLite.instance.Weather.zones[id];
	}

	public void RegisterZone(EnviroZone z)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.RegisterZone(z);
		}
		else
		{
			EnviroSkyLite.instance.RegisterZone(z);
		}
	}

	public float GetUniversalTimeOfDay()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.internalHour - (float)EnviroSky.instance.GameTime.utcOffset;
		}
		return EnviroSkyLite.instance.internalHour - (float)EnviroSkyLite.instance.GameTime.utcOffset;
	}

	public double GetCurrentTimeInHours()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.currentTimeInHours;
		}
		return EnviroSkyLite.instance.currentTimeInHours;
	}

	public EnviroSeasons.Seasons GetCurrentSeason()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Seasons.currentSeasons;
		}
		return EnviroSkyLite.instance.Seasons.currentSeasons;
	}

	public void SetYears(int year)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.GameTime.Years = year;
		}
		else
		{
			EnviroSkyLite.instance.GameTime.Years = year;
		}
	}

	public void SetDays(int days)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.GameTime.Days = days;
		}
		else
		{
			EnviroSkyLite.instance.GameTime.Days = days;
		}
	}

	public void SetTime(DateTime date)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetTime(date);
		}
		else
		{
			EnviroSkyLite.instance.SetTime(date);
		}
	}

	public void SetTime(int year, int dayOfYear, int hour, int minute, int seconds)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetTime(year, dayOfYear, hour, minute, seconds);
		}
		else
		{
			EnviroSkyLite.instance.SetTime(year, dayOfYear, hour, minute, seconds);
		}
	}

	public void SetTimeOfDay(float timeOfDay)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetInternalTimeOfDay(timeOfDay);
		}
		else
		{
			EnviroSkyLite.instance.SetInternalTimeOfDay(timeOfDay);
		}
	}

	public void ChangeSeason(EnviroSeasons.Seasons s)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ChangeSeason(s);
		}
		else
		{
			EnviroSkyLite.instance.ChangeSeason(s);
		}
	}

	public void SetTimeProgress(EnviroTime.TimeProgressMode tpm)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.GameTime.ProgressTime = tpm;
		}
		else
		{
			EnviroSkyLite.instance.GameTime.ProgressTime = tpm;
		}
	}

	public string GetTimeStringWithSeconds()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return string.Format("{0:00}:{1:00}:{2:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes, EnviroSky.instance.GameTime.Seconds);
		}
		return string.Format("{0:00}:{1:00}:{2:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes, EnviroSkyLite.instance.GameTime.Seconds);
	}

	public string GetTimeString()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return string.Format("{0:00}:{1:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes);
		}
		return string.Format("{0:00}:{1:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes);
	}

	public int GetCurrentYear()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GameTime.Years;
		}
		return EnviroSkyLite.instance.GameTime.Years;
	}

	public int GetCurrentDay()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GameTime.Days;
		}
		return EnviroSkyLite.instance.GameTime.Days;
	}

	public int GetCurrentHour()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GameTime.Hours;
		}
		return EnviroSkyLite.instance.GameTime.Hours;
	}

	public int GetCurrentMinute()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GameTime.Minutes;
		}
		return EnviroSkyLite.instance.GameTime.Minutes;
	}

	public int GetCurrentSecond()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GameTime.Seconds;
		}
		return EnviroSkyLite.instance.GameTime.Seconds;
	}

	public void ChangeWeatherInstant(int weatherId)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetWeatherOverwrite(weatherId);
		}
		else
		{
			EnviroSkyLite.instance.SetWeatherOverwrite(weatherId);
		}
	}

	public void ChangeWeatherInstant(EnviroWeatherPreset preset)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.SetWeatherOverwrite(preset);
		}
		else
		{
			EnviroSkyLite.instance.SetWeatherOverwrite(preset);
		}
	}

	public void ChangeWeather(int weatherId)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ChangeWeather(weatherId);
		}
		else
		{
			EnviroSkyLite.instance.ChangeWeather(weatherId);
		}
	}

	public void ChangeWeather(EnviroWeatherPreset preset)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ChangeWeather(preset);
		}
		else
		{
			EnviroSkyLite.instance.ChangeWeather(preset);
		}
	}

	public void ChangeWeather(string Name)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.ChangeWeather(Name);
		}
		else
		{
			EnviroSkyLite.instance.ChangeWeather(Name);
		}
	}

	public EnviroZone GetCurrentActiveZone()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.currentActiveZone;
		}
		return EnviroSkyLite.instance.Weather.currentActiveZone;
	}

	public void SetCurrentActiveZone(EnviroZone z)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.Weather.currentActiveZone = z;
		}
		else
		{
			EnviroSkyLite.instance.Weather.currentActiveZone = z;
		}
	}

	public void InstantWeatherChange(EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.InstantWeatherChange(preset, prefab);
		}
		else
		{
			EnviroSkyLite.instance.InstantWeatherChange(preset, prefab);
		}
	}

	public void SetToZone(int z)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.Weather.currentActiveZone = EnviroSky.instance.Weather.zones[z];
		}
		else
		{
			EnviroSkyLite.instance.Weather.currentActiveZone = EnviroSkyLite.instance.Weather.zones[z];
		}
	}

	public EnviroWeatherPreset GetCurrentWeatherPreset()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.currentActiveWeatherPreset;
		}
		return EnviroSkyLite.instance.Weather.currentActiveWeatherPreset;
	}

	public EnviroWeatherPreset GetStartWeatherPreset()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.startWeatherPreset;
		}
		return EnviroSkyLite.instance.Weather.startWeatherPreset;
	}

	public List<EnviroWeatherPreset> GetCurrentWeatherPresetList()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.weatherPresets;
		}
		return EnviroSkyLite.instance.Weather.weatherPresets;
	}

	public List<EnviroWeatherPrefab> GetCurrentWeatherPrefabList()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.WeatherPrefabs;
		}
		return EnviroSkyLite.instance.Weather.WeatherPrefabs;
	}

	public List<EnviroZone> GetZoneList()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.zones;
		}
		return EnviroSkyLite.instance.Weather.zones;
	}

	public void ChangeZoneWeather(int zoneId, int weatherId)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSky.instance.Weather.WeatherPrefabs[weatherId];
			EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
		}
		else
		{
			EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId];
			EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
		}
	}

	public void SetAutoWeatherUpdates(bool b)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.Weather.updateWeather = b;
		}
		else
		{
			EnviroSkyLite.instance.Weather.updateWeather = b;
		}
	}

	public GameObject GetVFXHolder()
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.Weather.VFXHolder;
		}
		return EnviroSkyLite.instance.Weather.VFXHolder;
	}

	public void SetLightningFlashTrigger(float trigger)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.thunder = trigger;
		}
		else
		{
			EnviroSkyLite.instance.thunder = trigger;
		}
	}

	public float GetEmissionRate(ParticleSystem system)
	{
		return system.emission.rateOverTime.constantMax;
	}

	public void SetEmissionRate(ParticleSystem sys, float emissionRate)
	{
		ParticleSystem.EmissionModule emission = sys.emission;
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		rateOverTime.constantMax = emissionRate;
		emission.rateOverTime = rateOverTime;
	}

	public void RegisterVegetationInstance(EnviroVegetationInstance v)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			EnviroSky.instance.RegisterMe(v);
		}
		else
		{
			EnviroSkyLite.instance.RegisterMe(v);
		}
	}

	public double GetInHours(float hours, float days, float years)
	{
		if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
		{
			return EnviroSky.instance.GetInHours(hours, days, years, EnviroSky.instance.GameTime.DaysInYear);
		}
		return EnviroSkyLite.instance.GetInHours(hours, days, years, EnviroSkyLite.instance.GameTime.DaysInYear);
	}

	public virtual void NotifyHourPassed()
	{
		if (this.OnHourPassed != null)
		{
			this.OnHourPassed();
		}
	}

	public virtual void NotifyDayPassed()
	{
		if (this.OnDayPassed != null)
		{
			this.OnDayPassed();
		}
	}

	public virtual void NotifyYearPassed()
	{
		if (this.OnYearPassed != null)
		{
			this.OnYearPassed();
		}
	}

	public virtual void NotifyWeatherChanged(EnviroWeatherPreset type)
	{
		if (this.OnWeatherChanged != null)
		{
			this.OnWeatherChanged(type);
		}
	}

	public virtual void NotifyZoneWeatherChanged(EnviroWeatherPreset type, EnviroZone zone)
	{
		if (this.OnZoneWeatherChanged != null)
		{
			this.OnZoneWeatherChanged(type, zone);
		}
	}

	public virtual void NotifySeasonChanged(EnviroSeasons.Seasons season)
	{
		if (this.OnSeasonChanged != null)
		{
			this.OnSeasonChanged(season);
		}
	}

	public virtual void NotifyIsNight()
	{
		if (this.OnNightTime != null)
		{
			this.OnNightTime();
		}
	}

	public virtual void NotifyIsDay()
	{
		if (this.OnDayTime != null)
		{
			this.OnDayTime();
		}
	}

	public virtual void NotifyZoneChanged(EnviroZone zone)
	{
		if (this.OnZoneChanged != null)
		{
			this.OnZoneChanged(zone);
		}
	}
}
