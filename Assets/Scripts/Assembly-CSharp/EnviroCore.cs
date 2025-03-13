using System;
using System.Collections;
using System.Collections.Generic;
using Aura2API;
using UnityEngine;
using UnityEngine.Rendering;

public class EnviroCore : MonoBehaviour
{
	public enum EnviroStartMode
	{
		Started = 0,
		Paused = 1,
		PausedButTimeProgress = 2
	}

	[Header("Profile")]
	public EnviroProfile profile;

	[HideInInspector]
	public bool profileLoaded;

	[Tooltip("Assign your player gameObject here. Required Field! or enable AssignInRuntime!")]
	public GameObject Player;

	[Tooltip("Assign your main camera here. Required Field! or enable AssignInRuntime!")]
	public Camera PlayerCamera;

	[Tooltip("If enabled Enviro will search for your Player and Camera by Tag!")]
	public bool AssignInRuntime;

	[Tooltip("Your Player Tag")]
	public string PlayerTag = "";

	[Tooltip("Your CameraTag")]
	public string CameraTag = "MainCamera";

	[Header("Camera Settings")]
	[Tooltip("Enable HDR Rendering. You want to use a third party tonemapping effect for best results!")]
	public bool HDR = true;

	public EnviroStartMode startMode;

	[HideInInspector]
	public bool started;

	[HideInInspector]
	public bool serverMode;

	[HideInInspector]
	public EnviroWeatherCloudsConfig cloudsConfig;

	[HideInInspector]
	public float thunder;

	[HideInInspector]
	public bool isNight = true;

	[HideInInspector]
	public List<GameObject> satellites = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> satellitesRotation = new List<GameObject>();

	[HideInInspector]
	public List<EnviroVegetationInstance> EnviroVegetationInstances = new List<EnviroVegetationInstance>();

	[HideInInspector]
	public EnviroLightSettings lightSettings = new EnviroLightSettings();

	[HideInInspector]
	public EnviroVolumeLightingSettings volumeLightSettings = new EnviroVolumeLightingSettings();

	[HideInInspector]
	public EnviroSkySettings skySettings = new EnviroSkySettings();

	[HideInInspector]
	public EnviroReflectionSettings reflectionSettings = new EnviroReflectionSettings();

	[HideInInspector]
	public EnviroCloudSettings cloudsSettings = new EnviroCloudSettings();

	[HideInInspector]
	public EnviroWeatherSettings weatherSettings = new EnviroWeatherSettings();

	[HideInInspector]
	public EnviroFogSettings fogSettings = new EnviroFogSettings();

	[HideInInspector]
	public EnviroLightShaftsSettings lightshaftsSettings = new EnviroLightShaftsSettings();

	[HideInInspector]
	public EnviroSeasonSettings seasonsSettings = new EnviroSeasonSettings();

	[HideInInspector]
	public EnviroAudioSettings audioSettings = new EnviroAudioSettings();

	[HideInInspector]
	public EnviroSatellitesSettings satelliteSettings = new EnviroSatellitesSettings();

	[HideInInspector]
	public EnviroQualitySettings qualitySettings = new EnviroQualitySettings();

	[HideInInspector]
	public EnviroInteriorZoneSettings interiorZoneSettings = new EnviroInteriorZoneSettings();

	[HideInInspector]
	public EnviroDistanceBlurSettings distanceBlurSettings = new EnviroDistanceBlurSettings();

	[HideInInspector]
	public EnviroAuroraSettings auroraSettings = new EnviroAuroraSettings();

	[HideInInspector]
	public DateTime dateTime;

	[HideInInspector]
	public float internalHour;

	[HideInInspector]
	public float currentHour;

	[HideInInspector]
	public float currentDay;

	[HideInInspector]
	public float currentYear;

	[HideInInspector]
	public double currentTimeInHours;

	[HideInInspector]
	public float LST;

	[HideInInspector]
	public float lastHourUpdate;

	[HideInInspector]
	public float hourTime;

	[HideInInspector]
	public Vector3 cloudAnim;

	[HideInInspector]
	public Vector2 cloudAnimNonScaled;

	[HideInInspector]
	public Vector2 cirrusAnim;

	[HideInInspector]
	public float windIntensity;

	[HideInInspector]
	public float shadowIntensityMod;

	[HideInInspector]
	public bool interiorMode;

	[HideInInspector]
	public EnviroInterior lastInteriorZone;

	[HideInInspector]
	public bool updateFogDensity = true;

	[HideInInspector]
	public Color customFogColor = Color.black;

	[HideInInspector]
	public float customFogIntensity;

	[HideInInspector]
	public Color currentWeatherSkyMod;

	[HideInInspector]
	public Color currentWeatherLightMod;

	[HideInInspector]
	public Color currentWeatherFogMod;

	[HideInInspector]
	[Range(0f, 2f)]
	public float customMoonPhase;

	public Light MainLight;

	public Light AdditionalLight;

	public Transform MoonTransform;

	public Renderer MoonRenderer;

	public Material MoonShader;

	[HideInInspector]
	public float lastAmbientSkyUpdate;

	[HideInInspector]
	public double lastRelfectionUpdate;

	[HideInInspector]
	public Vector3 lastRelfectionPositionUpdate;

	[HideInInspector]
	public GameObject EffectsHolder;

	public ParticleSystem lightningEffect;

	public const float pi = (float)Math.PI;

	private Vector3 K = new Vector3(686f, 678f, 666f);

	private const float n = 1.0003f;

	private const float N = 2.545E+25f;

	private const float pn = 0.035f;

	public EnviroTime GameTime;

	public EnviroAudio Audio;

	public EnviroWeather Weather;

	public EnviroSeasons Seasons;

	public EnviroComponents Components;

	public EnviroFogging Fog;

	public EnviroLightshafts LightShafts;

	public EnviroParticleCloud particleClouds;

	[HideInInspector]
	public EnviroPostProcessing EnviroPostProcessing;

	[Header("Layer Setup")]
	[Tooltip("This is the layer id forfor the moon.")]
	public int moonRenderingLayer = 29;

	[Tooltip("This is the layer id for additional satellites like moons, planets.")]
	public int satelliteRenderingLayer = 30;

	[Tooltip("Activate to set recommended maincamera clear flag.")]
	public bool setCameraClearFlags = true;

	public EnviroAura2Config enviroAura2Config = new EnviroAura2Config();

	public void UpdateEnviroment()
	{
		if (Seasons.calcSeasons)
		{
			UpdateSeason();
		}
		if (EnviroVegetationInstances.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < EnviroVegetationInstances.Count; i++)
		{
			if (EnviroVegetationInstances[i] != null)
			{
				EnviroVegetationInstances[i].UpdateInstance();
			}
		}
	}

	public void PlayAmbient(AudioClip sfx)
	{
		if (sfx == Audio.currentAmbientSource.audiosrc.clip)
		{
			if (!Audio.currentAmbientSource.audiosrc.isPlaying)
			{
				Audio.currentAmbientSource.audiosrc.Play();
			}
		}
		else if (Audio.currentAmbientSource == Audio.AudioSourceAmbient)
		{
			Audio.AudioSourceAmbient.FadeOut();
			Audio.AudioSourceAmbient2.FadeIn(sfx);
			Audio.currentAmbientSource = Audio.AudioSourceAmbient2;
		}
		else if (Audio.currentAmbientSource == Audio.AudioSourceAmbient2)
		{
			Audio.AudioSourceAmbient2.FadeOut();
			Audio.AudioSourceAmbient.FadeIn(sfx);
			Audio.currentAmbientSource = Audio.AudioSourceAmbient;
		}
	}

	public void TryPlayAmbientSFX()
	{
		if (Weather.currentActiveWeatherPreset == null)
		{
			return;
		}
		if (isNight)
		{
			switch (Seasons.currentSeasons)
			{
			case EnviroSeasons.Seasons.Spring:
				if (Weather.currentActiveWeatherPreset.SpringNightAmbient != null)
				{
					PlayAmbient(Weather.currentActiveWeatherPreset.SpringNightAmbient);
					break;
				}
				Audio.AudioSourceAmbient.FadeOut();
				Audio.AudioSourceAmbient2.FadeOut();
				break;
			case EnviroSeasons.Seasons.Summer:
				if (Weather.currentActiveWeatherPreset.SummerNightAmbient != null)
				{
					PlayAmbient(Weather.currentActiveWeatherPreset.SummerNightAmbient);
					break;
				}
				Audio.AudioSourceAmbient.FadeOut();
				Audio.AudioSourceAmbient2.FadeOut();
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (Weather.currentActiveWeatherPreset.AutumnNightAmbient != null)
				{
					PlayAmbient(Weather.currentActiveWeatherPreset.AutumnNightAmbient);
					break;
				}
				Audio.AudioSourceAmbient.FadeOut();
				Audio.AudioSourceAmbient2.FadeOut();
				break;
			case EnviroSeasons.Seasons.Winter:
				if (Weather.currentActiveWeatherPreset.WinterNightAmbient != null)
				{
					PlayAmbient(Weather.currentActiveWeatherPreset.WinterNightAmbient);
					break;
				}
				Audio.AudioSourceAmbient.FadeOut();
				Audio.AudioSourceAmbient2.FadeOut();
				break;
			}
			return;
		}
		switch (Seasons.currentSeasons)
		{
		case EnviroSeasons.Seasons.Spring:
			if (Weather.currentActiveWeatherPreset.SpringDayAmbient != null)
			{
				PlayAmbient(Weather.currentActiveWeatherPreset.SpringDayAmbient);
				break;
			}
			Audio.AudioSourceAmbient.FadeOut();
			Audio.AudioSourceAmbient2.FadeOut();
			break;
		case EnviroSeasons.Seasons.Summer:
			if (Weather.currentActiveWeatherPreset.SummerDayAmbient != null)
			{
				PlayAmbient(Weather.currentActiveWeatherPreset.SummerDayAmbient);
				break;
			}
			Audio.AudioSourceAmbient.FadeOut();
			Audio.AudioSourceAmbient2.FadeOut();
			break;
		case EnviroSeasons.Seasons.Autumn:
			if (Weather.currentActiveWeatherPreset.AutumnDayAmbient != null)
			{
				PlayAmbient(Weather.currentActiveWeatherPreset.AutumnDayAmbient);
				break;
			}
			Audio.AudioSourceAmbient.FadeOut();
			Audio.AudioSourceAmbient2.FadeOut();
			break;
		case EnviroSeasons.Seasons.Winter:
			if (Weather.currentActiveWeatherPreset.WinterDayAmbient != null)
			{
				PlayAmbient(Weather.currentActiveWeatherPreset.WinterDayAmbient);
				break;
			}
			Audio.AudioSourceAmbient.FadeOut();
			Audio.AudioSourceAmbient2.FadeOut();
			break;
		}
	}

	public void CreateWeatherEffectHolder(string name)
	{
		if (Weather.VFXHolder == null)
		{
			Weather.VFXHolder = GameObject.Find(name + "/VFX");
		}
		if (Weather.VFXHolder == null)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "VFX";
			gameObject.transform.parent = EffectsHolder.transform;
			gameObject.transform.localPosition = Vector3.zero;
			Weather.VFXHolder = gameObject;
		}
	}

	public void CreateEffects(string name)
	{
		if (EffectsHolder == null)
		{
			EffectsHolder = GameObject.Find(name);
		}
		if (EffectsHolder == null)
		{
			EffectsHolder = new GameObject();
			EffectsHolder.name = name;
			EffectsHolder.transform.parent = base.transform;
			EffectsHolder.transform.parent = null;
		}
		CreateWeatherEffectHolder(name);
		if (Application.isPlaying && EnviroSkyMgr.instance.dontDestroy)
		{
			UnityEngine.Object.DontDestroyOnLoad(EffectsHolder);
		}
		if (Player != null)
		{
			EffectsHolder.transform.position = Player.transform.position;
		}
		else
		{
			EffectsHolder.transform.position = base.transform.position;
		}
		GameObject gameObject = GameObject.Find(name + "/SFX Holder(Clone)");
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Audio.SFXHolderPrefab, Vector3.zero, Quaternion.identity);
			gameObject.transform.parent = EffectsHolder.transform;
		}
		EnviroAudioSource[] componentsInChildren = gameObject.GetComponentsInChildren<EnviroAudioSource>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			switch (componentsInChildren[i].myFunction)
			{
			case EnviroAudioSource.AudioSourceFunction.Weather1:
				Audio.AudioSourceWeather = componentsInChildren[i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Weather2:
				Audio.AudioSourceWeather2 = componentsInChildren[i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Ambient:
				Audio.AudioSourceAmbient = componentsInChildren[i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Ambient2:
				Audio.AudioSourceAmbient2 = componentsInChildren[i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Thunder:
				Audio.AudioSourceThunder = componentsInChildren[i];
				break;
			case EnviroAudioSource.AudioSourceFunction.ZoneAmbient:
				Audio.AudioSourceZone = componentsInChildren[i];
				break;
			}
		}
		Weather.currentAudioSource = Audio.AudioSourceWeather;
		Audio.currentAmbientSource = Audio.AudioSourceAmbient;
		TryPlayAmbientSFX();
	}

	public Transform CreateDirectionalLight(bool additional)
	{
		GameObject gameObject = new GameObject();
		if (!additional)
		{
			gameObject.name = "Enviro Directional Light";
		}
		else
		{
			gameObject.name = "Enviro Directional Light - Moon";
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.parent = null;
		Light light = gameObject.AddComponent<Light>();
		light.type = LightType.Directional;
		light.shadows = LightShadows.Soft;
		return gameObject.transform;
	}

	public Vector3 BetaRay(Vector3 waveLength)
	{
		Vector3 vector = waveLength * 1E-09f;
		Vector3 result = default(Vector3);
		result.x = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.x, 4f) * 5.755f) * 2000f;
		result.y = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.y, 4f) * 5.755f) * 2000f;
		result.z = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.z, 4f) * 5.755f) * 2000f;
		return result;
	}

	public Vector3 BetaMie(float turbidity, Vector3 waveLength)
	{
		float num = 0.2f * turbidity * 10f;
		Vector3 result = default(Vector3);
		result.x = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / waveLength.x, 2f) * K.x;
		result.y = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / waveLength.y, 2f) * K.y;
		result.z = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / waveLength.z, 2f) * K.z;
		result.x = Mathf.Pow(result.x, -1f);
		result.y = Mathf.Pow(result.y, -1f);
		result.z = Mathf.Pow(result.z, -1f);
		return result;
	}

	public Vector3 GetMieG(float g)
	{
		if (g == 1f)
		{
			g = 0.99f;
		}
		return new Vector3(1f - g * g, 1f + g * g, 2f * g);
	}

	public Vector3 GetMieGScene(float g)
	{
		if (g == 1f)
		{
			g = 0.99f;
		}
		return new Vector3(1f - g * g, 1f + g * g, 2f * g);
	}

	public void UpdateTime(int daysInYear)
	{
		if (Application.isPlaying)
		{
			float num = 0f;
			num = (isNight ? (0.4f / GameTime.NightLengthInMinutes) : (0.4f / GameTime.DayLengthInMinutes));
			hourTime = num * Time.deltaTime;
			switch (GameTime.ProgressTime)
			{
			case EnviroTime.TimeProgressMode.None:
				SetTime(GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
				break;
			case EnviroTime.TimeProgressMode.Simulated:
				internalHour += hourTime;
				SetGameTime();
				break;
			case EnviroTime.TimeProgressMode.OneDay:
				internalHour += hourTime;
				SetGameTime();
				break;
			case EnviroTime.TimeProgressMode.SystemTime:
				SetTime(DateTime.Now);
				break;
			}
		}
		else
		{
			SetTime(GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		}
		if (internalHour > lastHourUpdate + 1f)
		{
			lastHourUpdate = internalHour;
			EnviroSkyMgr.instance.NotifyHourPassed();
		}
		if (GameTime.Days >= daysInYear)
		{
			GameTime.Years++;
			GameTime.Days = 0;
			EnviroSkyMgr.instance.NotifyYearPassed();
		}
		currentHour = internalHour;
		currentDay = GameTime.Days;
		currentYear = GameTime.Years;
		currentTimeInHours = GetInHours(internalHour, currentDay, currentYear, daysInYear);
	}

	public void SetInternalTime(int year, int dayOfYear, int hour, int minute, int seconds)
	{
		GameTime.Years = year;
		GameTime.Days = dayOfYear;
		GameTime.Minutes = minute;
		GameTime.Hours = hour;
		internalHour = (float)hour + (float)minute * 0.0166667f + (float)seconds * 0.000277778f;
	}

	public void SetGameTime()
	{
		if (internalHour >= 24f)
		{
			internalHour -= 24f;
			EnviroSkyMgr.instance.NotifyHourPassed();
			lastHourUpdate = internalHour;
			if (GameTime.ProgressTime != EnviroTime.TimeProgressMode.OneDay)
			{
				GameTime.Days++;
				EnviroSkyMgr.instance.NotifyDayPassed();
			}
		}
		else if (internalHour < 0f)
		{
			internalHour = 24f + internalHour;
			lastHourUpdate = internalHour;
			if (GameTime.ProgressTime != EnviroTime.TimeProgressMode.OneDay)
			{
				GameTime.Days--;
				EnviroSkyMgr.instance.NotifyDayPassed();
			}
		}
		float num = internalHour;
		GameTime.Hours = (int)num;
		num -= (float)GameTime.Hours;
		GameTime.Minutes = (int)(num * 60f);
		num -= (float)GameTime.Minutes * 0.0166667f;
		GameTime.Seconds = (int)(num * 3600f);
	}

	public void SetTime(DateTime date)
	{
		GameTime.Years = date.Year;
		GameTime.Days = date.DayOfYear;
		GameTime.Minutes = date.Minute;
		GameTime.Seconds = date.Second;
		GameTime.Hours = date.Hour;
		internalHour = (float)date.Hour + (float)date.Minute * 0.0166667f + (float)date.Second * 0.000277778f;
	}

	public void SetTime(int year, int dayOfYear, int hour, int minute, int seconds)
	{
		GameTime.Years = year;
		GameTime.Days = dayOfYear;
		GameTime.Minutes = minute;
		GameTime.Hours = hour;
		internalHour = (float)hour + (float)minute * 0.0166667f + (float)seconds * 0.000277778f;
	}

	public void SetInternalTimeOfDay(float inHours)
	{
		internalHour = inHours;
		GameTime.Hours = (int)inHours;
		inHours -= (float)GameTime.Hours;
		GameTime.Minutes = (int)(inHours * 60f);
		inHours -= (float)GameTime.Minutes * 0.0166667f;
		GameTime.Seconds = (int)(inHours * 3600f);
	}

	public string GetTimeStringWithSeconds()
	{
		return string.Format("{0:00}:{1:00}:{2:00}", GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
	}

	public string GetTimeString()
	{
		return string.Format("{0:00}:{1:00}", GameTime.Hours, GameTime.Minutes);
	}

	public DateTime CreateSystemDate()
	{
		return default(DateTime).AddYears(GameTime.Years - 1).AddDays(GameTime.Days - 1);
	}

	public float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	public Vector3 OrbitalToLocal(float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float y = Mathf.Cos(theta);
		float num2 = Mathf.Sin(phi);
		float num3 = Mathf.Cos(phi);
		Vector3 result = default(Vector3);
		result.z = num * num3;
		result.y = y;
		result.x = num * num2;
		return result;
	}

	public void CalculateSunPosition(float d, float ecl, bool simpleMoon)
	{
		float num = 282.9404f + 4.70935E-05f * d;
		float num2 = 0.016709f - 1.151E-09f * d;
		float num3 = 356.047f + 0.98560023f * d;
		float num4 = num3 + num2 * 57.29578f * Mathf.Sin((float)Math.PI / 180f * num3) * (1f + num2 * Mathf.Cos((float)Math.PI / 180f * num3));
		float num5 = Mathf.Cos((float)Math.PI / 180f * num4) - num2;
		float num6 = Mathf.Sin((float)Math.PI / 180f * num4) * Mathf.Sqrt(1f - num2 * num2);
		float num7 = 57.29578f * Mathf.Atan2(num6, num5);
		float num8 = Mathf.Sqrt(num5 * num5 + num6 * num6);
		float num9 = num7 + num;
		float num10 = num8 * Mathf.Cos((float)Math.PI / 180f * num9);
		float num11 = num8 * Mathf.Sin((float)Math.PI / 180f * num9);
		float num12 = num10;
		float num13 = num11 * Mathf.Cos((float)Math.PI / 180f * ecl);
		float f = Mathf.Atan2(num11 * Mathf.Sin((float)Math.PI / 180f * ecl), Mathf.Sqrt(num12 * num12 + num13 * num13));
		float num14 = Mathf.Sin(f);
		float num15 = Mathf.Cos(f);
		float num16 = num9 + 180f + GetUniversalTimeOfDay() * 15f;
		LST = num16 + GameTime.Longitude;
		float num17 = LST - 57.29578f * Mathf.Atan2(num13, num12);
		float f2 = (float)Math.PI / 180f * num17;
		float num18 = Mathf.Sin(f2);
		float num19 = Mathf.Cos(f2) * num15;
		float num20 = num18 * num15;
		float num21 = num14;
		float num22 = Mathf.Sin((float)Math.PI / 180f * GameTime.Latitude);
		float num23 = Mathf.Cos((float)Math.PI / 180f * GameTime.Latitude);
		float num24 = num19 * num22 - num21 * num23;
		float num25 = num20;
		float y = num19 * num23 + num21 * num22;
		float num26 = Mathf.Atan2(num25, num24) + (float)Math.PI;
		float num27 = Mathf.Atan2(y, Mathf.Sqrt(num24 * num24 + num25 * num25));
		float num28 = (float)Math.PI / 2f - num27;
		float phi = num26;
		GameTime.solarTime = Mathf.Clamp01(Remap(num28, -1.5f, 0f, 1.5f, 1f));
		Components.Sun.transform.localPosition = OrbitalToLocal(num28, phi);
		Components.Sun.transform.LookAt(base.transform.position);
		if (simpleMoon)
		{
			Components.Moon.transform.localPosition = OrbitalToLocal(num28 - (float)Math.PI, phi);
			GameTime.lunarTime = Mathf.Clamp01(Remap(num28 - (float)Math.PI, -3f, 0f, 0f, 1f));
			Components.Moon.transform.LookAt(base.transform.position);
		}
	}

	public void CalculateMoonPosition(float d, float ecl)
	{
		float num = 125.1228f - 0.05295381f * d;
		float num2 = 5.1454f;
		float num3 = 318.0634f + 0.16435732f * d;
		float num4 = 0.0549f;
		float num5 = 115.3654f + 13.064993f * d;
		float num6 = (float)Math.PI / 180f * num5;
		float f = num6 + num4 * Mathf.Sin(num6) * (1f + num4 * Mathf.Cos(num6));
		float num7 = 60.2666f * (Mathf.Cos(f) - num4);
		float num8 = 60.2666f * (Mathf.Sqrt(1f - num4 * num4) * Mathf.Sin(f));
		float num9 = 57.29578f * Mathf.Atan2(num8, num7);
		float num10 = Mathf.Sqrt(num7 * num7 + num8 * num8);
		float f2 = (float)Math.PI / 180f * num;
		float num11 = Mathf.Sin(f2);
		float num12 = Mathf.Cos(f2);
		float f3 = (float)Math.PI / 180f * (num9 + num3);
		float num13 = Mathf.Sin(f3);
		float num14 = Mathf.Cos(f3);
		float f4 = (float)Math.PI / 180f * num2;
		float num15 = Mathf.Cos(f4);
		float num16 = num10 * (num12 * num14 - num11 * num13 * num15);
		float num17 = num10 * (num11 * num14 + num12 * num13 * num15);
		float num18 = num10 * (num13 * Mathf.Sin(f4));
		float num19 = Mathf.Cos((float)Math.PI / 180f * ecl);
		float num20 = Mathf.Sin((float)Math.PI / 180f * ecl);
		float num21 = num16;
		float num22 = num17 * num19 - num18 * num20;
		float y = num17 * num20 + num18 * num19;
		float num23 = Mathf.Atan2(num22, num21);
		float f5 = Mathf.Atan2(y, Mathf.Sqrt(num21 * num21 + num22 * num22));
		float f6 = (float)Math.PI / 180f * LST - num23;
		float num24 = Mathf.Cos(f6) * Mathf.Cos(f5);
		float num25 = Mathf.Sin(f6) * Mathf.Cos(f5);
		float num26 = Mathf.Sin(f5);
		float f7 = (float)Math.PI / 180f * GameTime.Latitude;
		float num27 = Mathf.Sin(f7);
		float num28 = Mathf.Cos(f7);
		float num29 = num24 * num27 - num26 * num28;
		float num30 = num25;
		float y2 = num24 * num28 + num26 * num27;
		float num31 = Mathf.Atan2(num30, num29) + (float)Math.PI;
		float num32 = Mathf.Atan2(y2, Mathf.Sqrt(num29 * num29 + num30 * num30));
		float num33 = (float)Math.PI / 2f - num32;
		float phi = num31;
		Components.Moon.transform.localPosition = OrbitalToLocal(num33, phi);
		GameTime.lunarTime = Mathf.Clamp01(Remap(num33, -1.5f, 0f, 1.5f, 1f));
		Components.Moon.transform.LookAt(base.transform.position);
	}

	public Vector3 UpdateSatellitePosition(float orbit, float orbit2, float speed)
	{
		float f = (float)Math.PI / 180f * GameTime.Latitude;
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float num3 = (float)Math.PI / 180f * GameTime.Longitude;
		float f2 = orbit2 * Mathf.Sin((float)Math.PI / 184f * ((float)GameTime.Days - 81f));
		float num4 = Mathf.Sin(f2);
		float num5 = Mathf.Cos(f2);
		float num6 = (int)(GameTime.Longitude / 15f);
		float num7 = (float)Math.PI / 12f * num6;
		float num8 = GetUniversalTimeOfDay() + orbit * Mathf.Sin(0.03333255f * ((float)GameTime.Days - 80f)) - speed * Mathf.Sin((float)Math.PI / 355f * ((float)GameTime.Days - 8f)) + 12f / (float)Math.PI * (num7 - num3);
		float f3 = (float)Math.PI / 12f * num8;
		float num9 = Mathf.Sin(f3);
		float num10 = Mathf.Cos(f3);
		float num11 = Mathf.Asin(num * num4 - num2 * num5 * num10);
		float y = (0f - num5) * num9;
		float x = num2 * num4 - num * num5 * num10;
		float num12 = Mathf.Atan2(y, x);
		float theta = (float)Math.PI / 2f - num11;
		float phi = num12;
		return OrbitalToLocal(theta, phi);
	}

	public void CalculateStarsPosition(float siderealTime)
	{
		if (siderealTime > 24f)
		{
			siderealTime -= 24f;
		}
		else if (siderealTime < 0f)
		{
			siderealTime += 24f;
		}
		Quaternion localRotation = Quaternion.Euler(90f - GameTime.Latitude, (float)Math.PI / 180f * GameTime.Longitude, 0f);
		localRotation *= Quaternion.Euler(0f, siderealTime, 0f);
		Components.starsRotation.localRotation = localRotation;
		Shader.SetGlobalMatrix("_StarsMatrix", Components.starsRotation.worldToLocalMatrix);
	}

	public void UpdateSunAndMoonPosition()
	{
		DateTime dateTime = CreateSystemDate();
		float num = 367 * dateTime.Year - 7 * (dateTime.Year + (dateTime.Month / 12 + 9) / 12) / 4 + 275 * dateTime.Month / 9 + dateTime.Day - 730530;
		num += GetUniversalTimeOfDay() / 24f;
		float ecl = 23.4393f - 3.563E-07f * num;
		if (skySettings.sunAndMoonPosition == EnviroSkySettings.SunAndMoonCalc.Realistic)
		{
			CalculateSunPosition(num, ecl, false);
			CalculateMoonPosition(num, ecl);
		}
		else
		{
			CalculateSunPosition(num, ecl, true);
		}
		CalculateStarsPosition(LST);
	}

	public float GetUniversalTimeOfDay()
	{
		return internalHour - (float)GameTime.utcOffset;
	}

	public double GetInHours(float hours, float days, float years, int daysInYear)
	{
		return hours + days * 24f + years * (float)daysInYear * 24f;
	}

	public void UpdateSeason()
	{
		if (currentDay >= (float)seasonsSettings.SpringStart && currentDay <= (float)seasonsSettings.SpringEnd)
		{
			ChangeSeason(EnviroSeasons.Seasons.Spring);
		}
		else if (currentDay >= (float)seasonsSettings.SummerStart && currentDay <= (float)seasonsSettings.SummerEnd)
		{
			ChangeSeason(EnviroSeasons.Seasons.Summer);
		}
		else if (currentDay >= (float)seasonsSettings.AutumnStart && currentDay <= (float)seasonsSettings.AutumnEnd)
		{
			ChangeSeason(EnviroSeasons.Seasons.Autumn);
		}
		else if (currentDay >= (float)seasonsSettings.WinterStart || currentDay <= (float)seasonsSettings.WinterEnd)
		{
			ChangeSeason(EnviroSeasons.Seasons.Winter);
		}
	}

	public void ChangeSeason(EnviroSeasons.Seasons season)
	{
		if (Seasons.lastSeason != season)
		{
			EnviroSkyMgr.instance.NotifySeasonChanged(season);
			Seasons.lastSeason = Seasons.currentSeasons;
			Seasons.currentSeasons = season;
		}
	}

	public void ApplyProfile(EnviroProfile p)
	{
		profile = p;
		lightSettings = JsonUtility.FromJson<EnviroLightSettings>(JsonUtility.ToJson(p.lightSettings));
		volumeLightSettings = JsonUtility.FromJson<EnviroVolumeLightingSettings>(JsonUtility.ToJson(p.volumeLightSettings));
		distanceBlurSettings = JsonUtility.FromJson<EnviroDistanceBlurSettings>(JsonUtility.ToJson(p.distanceBlurSettings));
		skySettings = JsonUtility.FromJson<EnviroSkySettings>(JsonUtility.ToJson(p.skySettings));
		cloudsSettings = JsonUtility.FromJson<EnviroCloudSettings>(JsonUtility.ToJson(p.cloudsSettings));
		weatherSettings = JsonUtility.FromJson<EnviroWeatherSettings>(JsonUtility.ToJson(p.weatherSettings));
		fogSettings = JsonUtility.FromJson<EnviroFogSettings>(JsonUtility.ToJson(p.fogSettings));
		lightshaftsSettings = JsonUtility.FromJson<EnviroLightShaftsSettings>(JsonUtility.ToJson(p.lightshaftsSettings));
		audioSettings = JsonUtility.FromJson<EnviroAudioSettings>(JsonUtility.ToJson(p.audioSettings));
		satelliteSettings = JsonUtility.FromJson<EnviroSatellitesSettings>(JsonUtility.ToJson(p.satelliteSettings));
		qualitySettings = JsonUtility.FromJson<EnviroQualitySettings>(JsonUtility.ToJson(p.qualitySettings));
		seasonsSettings = JsonUtility.FromJson<EnviroSeasonSettings>(JsonUtility.ToJson(p.seasonsSettings));
		reflectionSettings = JsonUtility.FromJson<EnviroReflectionSettings>(JsonUtility.ToJson(p.reflectionSettings));
		auroraSettings = JsonUtility.FromJson<EnviroAuroraSettings>(JsonUtility.ToJson(p.auroraSettings));
		profileLoaded = true;
	}

	public void SaveProfile()
	{
		profile.lightSettings = JsonUtility.FromJson<EnviroLightSettings>(JsonUtility.ToJson(lightSettings));
		profile.volumeLightSettings = JsonUtility.FromJson<EnviroVolumeLightingSettings>(JsonUtility.ToJson(volumeLightSettings));
		profile.distanceBlurSettings = JsonUtility.FromJson<EnviroDistanceBlurSettings>(JsonUtility.ToJson(distanceBlurSettings));
		profile.skySettings = JsonUtility.FromJson<EnviroSkySettings>(JsonUtility.ToJson(skySettings));
		profile.cloudsSettings = JsonUtility.FromJson<EnviroCloudSettings>(JsonUtility.ToJson(cloudsSettings));
		profile.weatherSettings = JsonUtility.FromJson<EnviroWeatherSettings>(JsonUtility.ToJson(weatherSettings));
		profile.fogSettings = JsonUtility.FromJson<EnviroFogSettings>(JsonUtility.ToJson(fogSettings));
		profile.lightshaftsSettings = JsonUtility.FromJson<EnviroLightShaftsSettings>(JsonUtility.ToJson(lightshaftsSettings));
		profile.audioSettings = JsonUtility.FromJson<EnviroAudioSettings>(JsonUtility.ToJson(audioSettings));
		profile.satelliteSettings = JsonUtility.FromJson<EnviroSatellitesSettings>(JsonUtility.ToJson(satelliteSettings));
		profile.qualitySettings = JsonUtility.FromJson<EnviroQualitySettings>(JsonUtility.ToJson(qualitySettings));
		profile.seasonsSettings = JsonUtility.FromJson<EnviroSeasonSettings>(JsonUtility.ToJson(seasonsSettings));
		profile.reflectionSettings = JsonUtility.FromJson<EnviroReflectionSettings>(JsonUtility.ToJson(reflectionSettings));
		profile.auroraSettings = JsonUtility.FromJson<EnviroAuroraSettings>(JsonUtility.ToJson(auroraSettings));
	}

	public void UpdateReflections()
	{
		if (Components.GlobalReflectionProbe == null)
		{
			Debug.Log("Global Reflection Probe not assigned in 'Components' menu of Enviro Sky Instance!");
			return;
		}
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
		{
			Components.GlobalReflectionProbe.customRendering = reflectionSettings.globalReflectionCustomRendering;
			if (reflectionSettings.reflectionCloudsQuality != null)
			{
				Components.GlobalReflectionProbe.customCloudsQuality = reflectionSettings.reflectionCloudsQuality;
			}
			Components.GlobalReflectionProbe.useFog = reflectionSettings.globalReflectionUseFog;
		}
		if (!reflectionSettings.globalReflections)
		{
			Components.GlobalReflectionProbe.enabled = false;
			return;
		}
		if (!Components.GlobalReflectionProbe.isActiveAndEnabled)
		{
			Components.GlobalReflectionProbe.enabled = true;
		}
		Components.GlobalReflectionProbe.myProbe.cullingMask = reflectionSettings.globalReflectionLayers;
		Components.GlobalReflectionProbe.myProbe.intensity = reflectionSettings.globalReflectionsIntensity;
		Components.GlobalReflectionProbe.myProbe.size = base.transform.localScale * reflectionSettings.globalReflectionsScale;
		switch (reflectionSettings.globalReflectionResolution)
		{
		case EnviroReflectionSettings.GlobalReflectionResolution.R16:
			Components.GlobalReflectionProbe.myProbe.resolution = 16;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R32:
			Components.GlobalReflectionProbe.myProbe.resolution = 32;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R64:
			Components.GlobalReflectionProbe.myProbe.resolution = 64;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R128:
			Components.GlobalReflectionProbe.myProbe.resolution = 128;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R256:
			Components.GlobalReflectionProbe.myProbe.resolution = 256;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R512:
			Components.GlobalReflectionProbe.myProbe.resolution = 512;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R1024:
			Components.GlobalReflectionProbe.myProbe.resolution = 1024;
			break;
		case EnviroReflectionSettings.GlobalReflectionResolution.R2048:
			Components.GlobalReflectionProbe.myProbe.resolution = 2048;
			break;
		}
		if ((currentTimeInHours > lastRelfectionUpdate + (double)reflectionSettings.globalReflectionsUpdateTreshhold || currentTimeInHours < lastRelfectionUpdate - (double)reflectionSettings.globalReflectionsUpdateTreshhold) && reflectionSettings.globalReflectionsUpdateOnGameTime)
		{
			lastRelfectionUpdate = currentTimeInHours;
			Components.GlobalReflectionProbe.RefreshReflection(reflectionSettings.globalReflectionTimeSlicing);
		}
		else if ((base.transform.position.magnitude > lastRelfectionPositionUpdate.magnitude + 0.25f || base.transform.position.magnitude < lastRelfectionPositionUpdate.magnitude - 0.25f) && reflectionSettings.globalReflectionsUpdateOnPosition)
		{
			lastRelfectionPositionUpdate = base.transform.position;
			Components.GlobalReflectionProbe.RefreshReflection(reflectionSettings.globalReflectionTimeSlicing);
		}
	}

	public void UpdateAmbientLight()
	{
		switch (lightSettings.ambientMode)
		{
		case AmbientMode.Flat:
			RenderSettings.ambientSkyColor = Color.Lerp(Color.Lerp(lightSettings.ambientSkyColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime), interiorZoneSettings.currentInteriorAmbientLightMod, interiorZoneSettings.currentInteriorAmbientLightMod.a);
			break;
		case AmbientMode.Trilight:
			RenderSettings.ambientSkyColor = Color.Lerp(Color.Lerp(lightSettings.ambientSkyColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime), interiorZoneSettings.currentInteriorAmbientLightMod, interiorZoneSettings.currentInteriorAmbientLightMod.a);
			RenderSettings.ambientEquatorColor = Color.Lerp(Color.Lerp(lightSettings.ambientEquatorColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime), interiorZoneSettings.currentInteriorAmbientEQLightMod, interiorZoneSettings.currentInteriorAmbientEQLightMod.a);
			RenderSettings.ambientGroundColor = Color.Lerp(Color.Lerp(lightSettings.ambientGroundColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime), interiorZoneSettings.currentInteriorAmbientGRLightMod, interiorZoneSettings.currentInteriorAmbientGRLightMod.a);
			break;
		case AmbientMode.Skybox:
			RenderSettings.ambientIntensity = lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
			if (lastAmbientSkyUpdate < internalHour || lastAmbientSkyUpdate > internalHour + 0.101f)
			{
				DynamicGI.UpdateEnvironment();
				lastAmbientSkyUpdate = internalHour + 0.1f;
			}
			break;
		case (AmbientMode)2:
			break;
		}
	}

	public void CalculateDirectLight()
	{
		if (MainLight == null)
		{
			MainLight = Components.DirectLight.GetComponent<Light>();
		}
		if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Single || Components.AdditionalDirectLight == null)
		{
			Color a = Color.Lerp(lightSettings.LightColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a);
			MainLight.color = Color.Lerp(a, interiorZoneSettings.currentInteriorDirectLightMod, interiorZoneSettings.currentInteriorDirectLightMod.a);
			float b;
			if (!isNight)
			{
				b = lightSettings.directLightSunIntensity.Evaluate(GameTime.solarTime);
				Components.Sun.transform.LookAt(new Vector3(base.transform.position.x, base.transform.position.y - lightSettings.directLightAngleOffset, base.transform.position.z));
				if (!lightSettings.stopRotationAtHigh || (lightSettings.stopRotationAtHigh && GameTime.solarTime >= lightSettings.rotationStopHigh))
				{
					Components.DirectLight.rotation = Components.Sun.transform.rotation;
				}
			}
			else
			{
				b = lightSettings.directLightMoonIntensity.Evaluate(GameTime.lunarTime);
				Components.Moon.transform.LookAt(new Vector3(base.transform.position.x, base.transform.position.y - lightSettings.directLightAngleOffset, base.transform.position.z));
				if (!lightSettings.stopRotationAtHigh || (lightSettings.stopRotationAtHigh && GameTime.lunarTime >= lightSettings.rotationStopHigh))
				{
					Components.DirectLight.rotation = Components.Moon.transform.rotation;
				}
			}
			MainLight.intensity = Mathf.Lerp(MainLight.intensity, b, Time.deltaTime * lightSettings.lightIntensityTransitionSpeed);
			MainLight.shadowStrength = Mathf.Clamp01(lightSettings.shadowIntensity.Evaluate(GameTime.solarTime) + shadowIntensityMod);
		}
		else if (lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Dual && Components.AdditionalDirectLight != null)
		{
			if (AdditionalLight == null)
			{
				AdditionalLight = Components.AdditionalDirectLight.GetComponent<Light>();
			}
			Color a2 = Color.Lerp(lightSettings.LightColor.Evaluate(GameTime.solarTime), currentWeatherLightMod, currentWeatherLightMod.a);
			MainLight.color = Color.Lerp(a2, interiorZoneSettings.currentInteriorDirectLightMod, interiorZoneSettings.currentInteriorDirectLightMod.a);
			AdditionalLight.color = MainLight.color;
			float b2 = lightSettings.directLightSunIntensity.Evaluate(GameTime.solarTime);
			float b3 = lightSettings.directLightMoonIntensity.Evaluate(GameTime.lunarTime) * (1f - GameTime.solarTime);
			Components.Sun.transform.LookAt(new Vector3(base.transform.position.x, base.transform.position.y - lightSettings.directLightAngleOffset, base.transform.position.z));
			if (!lightSettings.stopRotationAtHigh || (lightSettings.stopRotationAtHigh && GameTime.solarTime >= lightSettings.rotationStopHigh))
			{
				Components.DirectLight.rotation = Components.Sun.transform.rotation;
			}
			Components.Moon.transform.LookAt(new Vector3(base.transform.position.x, base.transform.position.y - lightSettings.directLightAngleOffset, base.transform.position.z));
			if (!lightSettings.stopRotationAtHigh || (lightSettings.stopRotationAtHigh && GameTime.lunarTime >= lightSettings.rotationStopHigh))
			{
				Components.AdditionalDirectLight.rotation = Components.Moon.transform.rotation;
			}
			MainLight.intensity = Mathf.Lerp(MainLight.intensity, b2, Time.deltaTime * lightSettings.lightIntensityTransitionSpeed);
			MainLight.shadowStrength = Mathf.Clamp01(lightSettings.shadowIntensity.Evaluate(GameTime.solarTime) + shadowIntensityMod);
			AdditionalLight.intensity = Mathf.Lerp(AdditionalLight.intensity, b3, Time.deltaTime * lightSettings.lightIntensityTransitionSpeed);
			AdditionalLight.shadowStrength = Mathf.Clamp01(lightSettings.shadowIntensity.Evaluate(GameTime.solarTime) + shadowIntensityMod);
		}
	}

	public void UpdateSceneView()
	{
		if (Weather.startWeatherPreset != null && !Application.isPlaying)
		{
			currentWeatherSkyMod = Weather.startWeatherPreset.weatherSkyMod.Evaluate(GameTime.solarTime);
			currentWeatherFogMod = Weather.startWeatherPreset.weatherFogMod.Evaluate(GameTime.solarTime);
			currentWeatherLightMod = Weather.startWeatherPreset.weatherLightMod.Evaluate(GameTime.solarTime);
		}
	}

	public void UpdateWind(EnviroWeatherPreset preset)
	{
		if (preset != null)
		{
			windIntensity = Mathf.Lerp(windIntensity, preset.WindStrenght, weatherSettings.windIntensityTransitionSpeed * Time.deltaTime);
		}
		if (cloudsSettings.useWindZoneDirection)
		{
			cloudsSettings.cloudsWindDirectionX = 0f - Components.windZone.transform.forward.x;
			cloudsSettings.cloudsWindDirectionY = 0f - Components.windZone.transform.forward.z;
		}
		cloudAnim += new Vector3(cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionX) * cloudsSettings.cloudsWindIntensity * Time.deltaTime, cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionY) * cloudsSettings.cloudsWindIntensity * Time.deltaTime, cloudsSettings.cloudsTimeScale * -1f * cloudsSettings.cloudsUpwardsWindIntensity * Time.deltaTime);
		cloudAnimNonScaled += new Vector2(cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionX) * cloudsSettings.cloudsWindIntensity * Time.deltaTime * 0.1f, cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionY) * cloudsSettings.cloudsWindIntensity * Time.deltaTime * 0.1f);
		cirrusAnim += new Vector2(cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionX) * cloudsSettings.cirrusWindIntensity * Time.deltaTime * 0.1f, cloudsSettings.cloudsTimeScale * (windIntensity * cloudsSettings.cloudsWindDirectionY) * cloudsSettings.cirrusWindIntensity * Time.deltaTime * 0.1f);
		if (cloudAnim.x > 1f)
		{
			cloudAnim.x = -1f;
		}
		else if (cloudAnim.x < -1f)
		{
			cloudAnim.x = 1f;
		}
		if (cloudAnim.y > 1f)
		{
			cloudAnim.y = -1f;
		}
		else if (cloudAnim.y < -1f)
		{
			cloudAnim.y = 1f;
		}
		if (cloudAnim.z > 1f)
		{
			cloudAnim.z = -1f;
		}
		else if (cloudAnim.z < -1f)
		{
			cloudAnim.z = 1f;
		}
		if (cirrusAnim.x > 1f)
		{
			cirrusAnim.x = -1f;
		}
		else if (cirrusAnim.x < -1f)
		{
			cirrusAnim.x = 1f;
		}
		if (cirrusAnim.y > 1f)
		{
			cirrusAnim.y = -1f;
		}
		else if (cirrusAnim.y < -1f)
		{
			cirrusAnim.y = 1f;
		}
		Components.windZone.windMain = windIntensity;
	}

	public int GetActiveWeatherID()
	{
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
		{
			if (Weather.WeatherPrefabs[i].weatherPreset == Weather.currentActiveWeatherPreset)
			{
				return i;
			}
		}
		return -1;
	}

	public void UpdateWeatherVariables(EnviroWeatherPreset p)
	{
		UpdateWind(p);
		if (Weather.wetness < p.wetnessLevel)
		{
			Weather.wetness = Mathf.Lerp(Weather.curWetness, p.wetnessLevel, weatherSettings.wetnessAccumulationSpeed * Time.deltaTime);
		}
		else
		{
			Weather.wetness = Mathf.Lerp(Weather.curWetness, p.wetnessLevel, weatherSettings.wetnessDryingSpeed * Time.deltaTime);
		}
		Weather.wetness = Mathf.Clamp(Weather.wetness, 0f, 1f);
		Weather.curWetness = Weather.wetness;
		if (Weather.snowStrength < p.snowLevel)
		{
			Weather.snowStrength = Mathf.Lerp(Weather.curSnowStrength, p.snowLevel, weatherSettings.snowAccumulationSpeed * Time.deltaTime);
		}
		else if (Weather.currentTemperature > weatherSettings.snowMeltingTresholdTemperature)
		{
			Weather.snowStrength = Mathf.Lerp(Weather.curSnowStrength, p.snowLevel, weatherSettings.snowMeltingSpeed * Time.deltaTime);
		}
		Weather.snowStrength = Mathf.Clamp(Weather.snowStrength, 0f, 1f);
		Weather.curSnowStrength = Weather.snowStrength;
		Shader.SetGlobalFloat("_EnviroGrassSnow", Weather.curSnowStrength);
		float num = 0f;
		switch (Seasons.currentSeasons)
		{
		case EnviroSeasons.Seasons.Spring:
			num = seasonsSettings.springBaseTemperature.Evaluate(GetUniversalTimeOfDay() / 24f);
			break;
		case EnviroSeasons.Seasons.Summer:
			num = seasonsSettings.summerBaseTemperature.Evaluate(GetUniversalTimeOfDay() / 24f);
			break;
		case EnviroSeasons.Seasons.Autumn:
			num = seasonsSettings.autumnBaseTemperature.Evaluate(GetUniversalTimeOfDay() / 24f);
			break;
		case EnviroSeasons.Seasons.Winter:
			num = seasonsSettings.winterBaseTemperature.Evaluate(GetUniversalTimeOfDay() / 24f);
			break;
		}
		num += p.temperatureLevel;
		Weather.currentTemperature = Mathf.Lerp(Weather.currentTemperature, num, Time.deltaTime * weatherSettings.temperatureChangingSpeed);
	}

	public IEnumerator PlayThunderRandom()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(Weather.currentActiveWeatherPreset.lightningInterval, Weather.currentActiveWeatherPreset.lightningInterval * 2f));
		if (Weather.currentActiveWeatherPrefab.weatherPreset.isLightningStorm)
		{
			if (Weather.weatherFullyChanged)
			{
				PlayLightning();
			}
			StartCoroutine(PlayThunderRandom());
		}
		else
		{
			StopCoroutine(PlayThunderRandom());
			Components.LightningGenerator.StopLightning();
		}
	}

	public IEnumerator PlayLightningEffect(Vector3 position)
	{
		lightningEffect.transform.position = position;
		lightningEffect.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-80f, -100f), 0f, 0f);
		lightningEffect.Play();
		yield return new WaitForSeconds(0.5f);
		lightningEffect.Stop();
	}

	public void PlayLightning()
	{
		if (lightningEffect != null)
		{
			StartCoroutine(PlayLightningEffect(new Vector3(base.transform.position.x + UnityEngine.Random.Range(0f - weatherSettings.lightningRange, weatherSettings.lightningRange), weatherSettings.lightningHeight, base.transform.position.z + UnityEngine.Random.Range(0f - weatherSettings.lightningRange, weatherSettings.lightningRange))));
		}
		int index = UnityEngine.Random.Range(0, audioSettings.ThunderSFX.Count);
		Audio.AudioSourceThunder.PlayOneShot(audioSettings.ThunderSFX[index]);
		Components.LightningGenerator.Lightning();
	}

	public void ForceWeatherUpdate()
	{
		Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
		Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
		Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
		Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
		if (!(Weather.currentActiveWeatherPreset != null))
		{
			return;
		}
		EnviroSkyMgr.instance.NotifyWeatherChanged(Weather.currentActiveWeatherPreset);
		Weather.weatherFullyChanged = false;
		if (!serverMode)
		{
			if (Weather.currentActiveWeatherPrefab.weatherPreset.isLightningStorm)
			{
				StartCoroutine(PlayThunderRandom());
				return;
			}
			StopCoroutine(PlayThunderRandom());
			Components.LightningGenerator.StopLightning();
		}
	}

	public void CalcWeatherTransitionState()
	{
		bool weatherFullyChanged = false;
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
		{
			weatherFullyChanged = ((cloudsConfig.coverage >= Weather.currentActiveWeatherPreset.cloudsConfig.coverage - 0.01f) ? true : false);
		}
		else if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.LW)
		{
			weatherFullyChanged = ((cloudsConfig.particleLayer1Alpha >= Weather.currentActiveWeatherPreset.cloudsConfig.particleLayer1Alpha - 0.01f && cloudsConfig.particleLayer2Alpha >= Weather.currentActiveWeatherPreset.cloudsConfig.particleLayer2Alpha - 0.01f) ? true : false);
		}
		Weather.weatherFullyChanged = weatherFullyChanged;
	}

	public void SetWeatherOverwrite(int weatherId)
	{
		if (weatherId >= 0 && weatherId <= Weather.WeatherPrefabs.Count)
		{
			if (Weather.WeatherPrefabs[weatherId] != Weather.currentActiveWeatherPrefab)
			{
				Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[weatherId];
				Weather.currentActiveZone.currentActiveZoneWeatherPreset = Weather.WeatherPrefabs[weatherId].weatherPreset;
				EnviroSkyMgr.instance.NotifyZoneWeatherChanged(Weather.WeatherPrefabs[weatherId].weatherPreset, Weather.currentActiveZone);
			}
			EnviroSkyMgr.instance.InstantWeatherChange(Weather.currentActiveZone.currentActiveZoneWeatherPreset, Weather.currentActiveZone.currentActiveZoneWeatherPrefab);
		}
	}

	public void SetWeatherOverwrite(EnviroWeatherPreset preset)
	{
		if (preset == null)
		{
			return;
		}
		if (preset != Weather.currentActiveWeatherPreset)
		{
			for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
			{
				if (preset == Weather.WeatherPrefabs[i].weatherPreset)
				{
					Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[i];
					Weather.currentActiveZone.currentActiveZoneWeatherPreset = preset;
					EnviroSkyMgr.instance.NotifyZoneWeatherChanged(preset, Weather.currentActiveZone);
				}
			}
		}
		EnviroSkyMgr.instance.InstantWeatherChange(Weather.currentActiveZone.currentActiveZoneWeatherPreset, Weather.currentActiveZone.currentActiveZoneWeatherPrefab);
	}

	public void ChangeWeather(int weatherId)
	{
		if (weatherId >= 0 && weatherId <= Weather.WeatherPrefabs.Count && Weather.WeatherPrefabs[weatherId] != Weather.currentActiveWeatherPrefab)
		{
			Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[weatherId];
			Weather.currentActiveZone.currentActiveZoneWeatherPreset = Weather.WeatherPrefabs[weatherId].weatherPreset;
			EnviroSkyMgr.instance.NotifyZoneWeatherChanged(Weather.WeatherPrefabs[weatherId].weatherPreset, Weather.currentActiveZone);
		}
	}

	public void ChangeWeather(EnviroWeatherPreset preset)
	{
		if (preset == null || !(preset != Weather.currentActiveWeatherPreset))
		{
			return;
		}
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
		{
			if (preset == Weather.WeatherPrefabs[i].weatherPreset)
			{
				Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[i];
				Weather.currentActiveZone.currentActiveZoneWeatherPreset = preset;
				EnviroSkyMgr.instance.NotifyZoneWeatherChanged(preset, Weather.currentActiveZone);
			}
		}
	}

	public void ChangeWeather(string weatherName)
	{
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
		{
			if (Weather.WeatherPrefabs[i].weatherPreset.Name == weatherName && Weather.WeatherPrefabs[i] != Weather.currentActiveWeatherPrefab)
			{
				ChangeWeather(i);
				EnviroSkyMgr.instance.NotifyZoneWeatherChanged(Weather.WeatherPrefabs[i].weatherPreset, Weather.currentActiveZone);
			}
		}
	}

	public void UpdateAudioSource(EnviroWeatherPreset i)
	{
		if (i != null && i.weatherSFX != null)
		{
			if (i.weatherSFX == Weather.currentAudioSource.audiosrc.clip)
			{
				if (Weather.currentAudioSource.audiosrc.volume < 0.1f)
				{
					Weather.currentAudioSource.FadeIn(i.weatherSFX);
				}
			}
			else if (Weather.currentAudioSource == Audio.AudioSourceWeather)
			{
				Audio.AudioSourceWeather.FadeOut();
				Audio.AudioSourceWeather2.FadeIn(i.weatherSFX);
				Weather.currentAudioSource = Audio.AudioSourceWeather2;
			}
			else if (Weather.currentAudioSource == Audio.AudioSourceWeather2)
			{
				Audio.AudioSourceWeather2.FadeOut();
				Audio.AudioSourceWeather.FadeIn(i.weatherSFX);
				Weather.currentAudioSource = Audio.AudioSourceWeather;
			}
		}
		else
		{
			Audio.AudioSourceWeather.FadeOut();
			Audio.AudioSourceWeather2.FadeOut();
		}
	}

	public void RegisterZone(EnviroZone zoneToAdd)
	{
		Weather.zones.Add(zoneToAdd);
	}

	public void EnterZone(EnviroZone zone)
	{
		Weather.currentActiveZone = zone;
	}

	public void ExitZone()
	{
	}

	public void UpdateParticleClouds(bool active)
	{
		if (particleClouds.layer1System == null || particleClouds.layer2System == null)
		{
			return;
		}
		if (active)
		{
			if (!particleClouds.layer1System.gameObject.activeSelf)
			{
				particleClouds.layer1System.gameObject.SetActive(true);
			}
			if (!particleClouds.layer2System.gameObject.activeSelf)
			{
				particleClouds.layer2System.gameObject.SetActive(true);
			}
			particleClouds.layer1System.transform.localPosition = new Vector3(particleClouds.layer1System.transform.localPosition.x, cloudsSettings.ParticleCloudsLayer1.height, particleClouds.layer1System.transform.localPosition.z);
			particleClouds.layer2System.transform.localPosition = new Vector3(particleClouds.layer2System.transform.localPosition.x, cloudsSettings.ParticleCloudsLayer2.height, particleClouds.layer2System.transform.localPosition.z);
			if (cloudsSettings.ParticleCloudsLayer1.height >= cloudsSettings.ParticleCloudsLayer2.height)
			{
				particleClouds.layer1Material.renderQueue = 3001;
				particleClouds.layer2Material.renderQueue = 3002;
			}
			else
			{
				particleClouds.layer1Material.renderQueue = 3002;
				particleClouds.layer2Material.renderQueue = 3001;
			}
			Color value = cloudsSettings.ParticleCloudsLayer1.particleCloudsColor.Evaluate(GameTime.solarTime) * cloudsConfig.particleLayer1Brightness;
			value.a = cloudsConfig.particleLayer1Alpha;
			particleClouds.layer1Material.SetColor("_CloudsColor", value);
			Color value2 = cloudsSettings.ParticleCloudsLayer2.particleCloudsColor.Evaluate(GameTime.solarTime) * cloudsConfig.particleLayer2Brightness;
			value2.a = cloudsConfig.particleLayer2Alpha;
			particleClouds.layer2Material.SetColor("_CloudsColor", value2);
		}
		else
		{
			if (particleClouds.layer1System != null && particleClouds.layer1System.isPlaying)
			{
				particleClouds.layer1System.gameObject.SetActive(false);
			}
			if (particleClouds.layer2System != null && particleClouds.layer2System.isPlaying)
			{
				particleClouds.layer2System.gameObject.SetActive(false);
			}
		}
	}

	public void CreateSatellite(int id)
	{
		if (satelliteSettings.additionalSatellites[id].prefab == null)
		{
			Debug.Log("Satellite without prefab! Pleae assign a prefab to all satellites.");
			return;
		}
		GameObject gameObject = new GameObject();
		gameObject.name = satelliteSettings.additionalSatellites[id].name;
		gameObject.transform.parent = Components.satellites;
		satellitesRotation.Add(gameObject);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(satelliteSettings.additionalSatellites[id].prefab, gameObject.transform);
		gameObject2.layer = satelliteRenderingLayer;
		satellites.Add(gameObject2);
	}

	public void CheckSatellites()
	{
		satellites = new List<GameObject>();
		for (int num = Components.satellites.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(Components.satellites.GetChild(num).gameObject);
		}
		satellites.Clear();
		satellitesRotation.Clear();
		for (int i = 0; i < satelliteSettings.additionalSatellites.Count; i++)
		{
			CreateSatellite(i);
		}
	}

	public void CalculateSatPositions(float siderealTime)
	{
		for (int i = 0; i < satelliteSettings.additionalSatellites.Count; i++)
		{
			Quaternion localRotation = Quaternion.Euler(90f - GameTime.Latitude, GameTime.Longitude, 0f);
			localRotation *= Quaternion.Euler(satelliteSettings.additionalSatellites[i].yRot, siderealTime, satelliteSettings.additionalSatellites[i].xRot);
			if (satellites.Count >= i)
			{
				satellites[i].transform.localPosition = new Vector3(0f, satelliteSettings.additionalSatellites[i].orbit, 0f);
			}
			if (satellitesRotation.Count >= i)
			{
				satellitesRotation[i].transform.localRotation = localRotation;
			}
		}
	}

	public void SetCameraHDR(Camera cam, bool hdr)
	{
		cam.allowHDR = hdr;
	}

	public bool GetCameraHDR(Camera cam)
	{
		return cam.allowHDR;
	}

	private Quaternion LightLookAt(Quaternion inputRotation, Quaternion newRotation)
	{
		return Quaternion.Lerp(inputRotation, newRotation, 500f * Time.deltaTime);
	}

	public int RegisterMe(EnviroVegetationInstance me)
	{
		EnviroVegetationInstances.Add(me);
		return EnviroVegetationInstances.Count - 1;
	}

	public void Save()
	{
		PlayerPrefs.SetFloat("Time_Hours", internalHour);
		PlayerPrefs.SetInt("Time_Days", GameTime.Days);
		PlayerPrefs.SetInt("Time_Years", GameTime.Years);
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++)
		{
			if (Weather.WeatherPrefabs[i] == Weather.currentActiveWeatherPrefab)
			{
				PlayerPrefs.SetInt("currentWeather", i);
			}
		}
	}

	public void Load()
	{
		if (PlayerPrefs.HasKey("Time_Hours"))
		{
			SetInternalTimeOfDay(PlayerPrefs.GetFloat("Time_Hours"));
		}
		if (PlayerPrefs.HasKey("Time_Days"))
		{
			GameTime.Days = PlayerPrefs.GetInt("Time_Days");
		}
		if (PlayerPrefs.HasKey("Time_Years"))
		{
			GameTime.Years = PlayerPrefs.GetInt("Time_Years");
		}
		if (PlayerPrefs.HasKey("currentWeather"))
		{
			SetWeatherOverwrite(PlayerPrefs.GetInt("currentWeather"));
		}
	}

	public void UpdateAura2(EnviroWeatherPreset id, bool withTransition)
	{
		if (!(id != null) || !(EnviroSkyMgr.instance != null))
		{
			return;
		}
		float t = 500f * Time.deltaTime;
		if (withTransition)
		{
			t = EnviroSkyMgr.instance.aura2TransitionSpeed * Time.deltaTime;
		}
		enviroAura2Config.aura2GlobalDensity = Mathf.Lerp(enviroAura2Config.aura2GlobalDensity, id.enviroAura2Config.aura2GlobalDensity, t);
		enviroAura2Config.aura2GlobalScattering = Mathf.Lerp(enviroAura2Config.aura2GlobalScattering, id.enviroAura2Config.aura2GlobalScattering, t);
		enviroAura2Config.aura2GlobalAmbientLight = Mathf.Lerp(enviroAura2Config.aura2GlobalAmbientLight, id.enviroAura2Config.aura2GlobalAmbientLight, t);
		enviroAura2Config.aura2GlobalExtinction = Mathf.Lerp(enviroAura2Config.aura2GlobalExtinction, id.enviroAura2Config.aura2GlobalExtinction, t);
		AuraCamera[] auraCameras = Aura.GetAuraCameras();
		for (int i = 0; i < auraCameras.Length; i++)
		{
			if (auraCameras[i].frustumSettings.baseSettings == null)
			{
				auraCameras[i].frustumSettings.baseSettings = new AuraBaseSettings();
				Debug.Log("No Aura 2 Basesetting. Enviro will create new one.");
			}
			auraCameras[i].frustumSettings.baseSettings.density = enviroAura2Config.aura2GlobalDensity;
			auraCameras[i].frustumSettings.baseSettings.scattering = enviroAura2Config.aura2GlobalScattering;
			auraCameras[i].frustumSettings.baseSettings.ambientLightingStrength = enviroAura2Config.aura2GlobalAmbientLight;
			auraCameras[i].frustumSettings.baseSettings.extinction = enviroAura2Config.aura2GlobalExtinction;
		}
		AuraLight[] auraLights = Aura.GetAuraLights(LightType.Directional);
		for (int j = 0; j < auraLights.Length; j++)
		{
			if (auraLights[j].gameObject.name == "Enviro Directional Light")
			{
				auraLights[j].strength = EnviroSkyMgr.instance.aura2DirectionalLightIntensity.Evaluate(GameTime.solarTime);
			}
			if (auraLights[j].gameObject.name == "Enviro Directional Light - Moon")
			{
				auraLights[j].strength = EnviroSkyMgr.instance.aura2DirectionalLightIntensityMoon.Evaluate(GameTime.lunarTime);
			}
		}
	}
}
