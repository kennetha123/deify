using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Enviro/Reflection Probe")]
[RequireComponent(typeof(ReflectionProbe))]
[ExecuteInEditMode]
public class EnviroReflectionProbe : MonoBehaviour
{
	public bool standalone;

	public bool updateReflectionOnGameTime = true;

	public float reflectionsUpdateTreshhold = 0.025f;

	public bool useTimeSlicing = true;

	[HideInInspector]
	public bool rendering;

	[HideInInspector]
	public ReflectionProbe myProbe;

	public bool customRendering;

	public EnviroVolumeCloudsQuality customCloudsQuality;

	private EnviroSkyRendering eSky;

	public bool useFog;

	private Camera bakingCam;

	private bool currentMode;

	private int currentRes;

	private RenderTexture cubemap;

	private RenderTexture mirrorTexture;

	private RenderTexture renderTexture;

	private GameObject renderCamObj;

	private Camera renderCam;

	private Material mirror;

	private Material bakeMat;

	private Coroutine refreshing;

	private static Quaternion[] orientations = new Quaternion[6]
	{
		Quaternion.LookRotation(Vector3.right, Vector3.down),
		Quaternion.LookRotation(Vector3.left, Vector3.down),
		Quaternion.LookRotation(Vector3.up, Vector3.forward),
		Quaternion.LookRotation(Vector3.down, Vector3.back),
		Quaternion.LookRotation(Vector3.forward, Vector3.down),
		Quaternion.LookRotation(Vector3.back, Vector3.down)
	};

	private double lastRelfectionUpdate;

	private void OnEnable()
	{
		myProbe = GetComponent<ReflectionProbe>();
		if (!standalone && myProbe != null)
		{
			myProbe.enabled = true;
		}
		if (customRendering)
		{
			myProbe.mode = ReflectionProbeMode.Custom;
			myProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			CreateCubemap();
			CreateTexturesAndMaterial();
			CreateRenderCamera();
			currentRes = myProbe.resolution;
			rendering = false;
			StartCoroutine(RefreshFirstTime());
		}
		else
		{
			myProbe.mode = ReflectionProbeMode.Realtime;
			myProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			myProbe.RenderProbe();
		}
	}

	private void OnDisable()
	{
		Cleanup();
		if (!standalone && myProbe != null)
		{
			myProbe.enabled = false;
		}
	}

	private void Cleanup()
	{
		if (refreshing != null)
		{
			StopCoroutine(refreshing);
		}
		if (cubemap != null)
		{
			Object.DestroyImmediate(cubemap);
		}
		if (renderCamObj != null)
		{
			Object.DestroyImmediate(renderCamObj);
		}
		if (mirrorTexture != null)
		{
			Object.DestroyImmediate(mirrorTexture);
		}
		if (renderTexture != null)
		{
			Object.DestroyImmediate(renderTexture);
		}
	}

	private void CreateRenderCamera()
	{
		if (!(renderCamObj == null))
		{
			return;
		}
		renderCamObj = new GameObject();
		renderCamObj.name = "Reflection Probe Cam";
		renderCamObj.hideFlags = HideFlags.HideAndDontSave;
		renderCam = renderCamObj.AddComponent<Camera>();
		renderCam.gameObject.SetActive(true);
		renderCam.cameraType = CameraType.Reflection;
		renderCam.fieldOfView = 90f;
		renderCam.farClipPlane = myProbe.farClipPlane;
		renderCam.nearClipPlane = myProbe.nearClipPlane;
		renderCam.clearFlags = (CameraClearFlags)myProbe.clearFlags;
		renderCam.backgroundColor = myProbe.backgroundColor;
		renderCam.allowHDR = myProbe.hdr;
		renderCam.targetTexture = cubemap;
		renderCam.enabled = false;
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
		{
			eSky = renderCamObj.AddComponent<EnviroSkyRendering>();
			eSky.isAddionalCamera = true;
			eSky.useGlobalRenderingSettings = false;
			eSky.customRenderingSettings.useVolumeClouds = EnviroSkyMgr.instance.useVolumeClouds;
			eSky.customRenderingSettings.useVolumeLighting = false;
			eSky.customRenderingSettings.useDistanceBlur = false;
			eSky.customRenderingSettings.useFog = true;
			if (customCloudsQuality != null)
			{
				eSky.customRenderingSettings.customCloudsQuality = customCloudsQuality;
			}
		}
	}

	private void UpdateCameraSettings()
	{
		if (!(renderCam != null))
		{
			return;
		}
		renderCam.cullingMask = myProbe.cullingMask;
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD && eSky != null)
		{
			if (customCloudsQuality != null)
			{
				eSky.customRenderingSettings.customCloudsQuality = customCloudsQuality;
			}
			eSky.customRenderingSettings.useVolumeClouds = EnviroSkyMgr.instance.useVolumeClouds;
			eSky.customRenderingSettings.useFog = useFog;
		}
	}

	private Camera CreateBakingCamera()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "Reflection Probe Cam";
		Camera camera = gameObject.AddComponent<Camera>();
		camera.enabled = false;
		camera.gameObject.SetActive(true);
		camera.cameraType = CameraType.Reflection;
		camera.fieldOfView = 90f;
		camera.farClipPlane = myProbe.farClipPlane;
		camera.nearClipPlane = myProbe.nearClipPlane;
		camera.cullingMask = myProbe.cullingMask;
		camera.clearFlags = (CameraClearFlags)myProbe.clearFlags;
		camera.backgroundColor = myProbe.backgroundColor;
		camera.allowHDR = myProbe.hdr;
		camera.targetTexture = cubemap;
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
		{
			EnviroSkyRendering enviroSkyRendering = gameObject.AddComponent<EnviroSkyRendering>();
			enviroSkyRendering.isAddionalCamera = true;
			enviroSkyRendering.useGlobalRenderingSettings = false;
			enviroSkyRendering.customRenderingSettings.useVolumeClouds = true;
			enviroSkyRendering.customRenderingSettings.useVolumeLighting = false;
			enviroSkyRendering.customRenderingSettings.useDistanceBlur = false;
			enviroSkyRendering.customRenderingSettings.useFog = true;
		}
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		return camera;
	}

	private void CreateCubemap()
	{
		if (!(cubemap != null) || myProbe.resolution != currentRes)
		{
			if (cubemap != null)
			{
				Object.DestroyImmediate(cubemap);
			}
			int num = (currentRes = myProbe.resolution);
			RenderTextureFormat format = (myProbe.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
			cubemap = new RenderTexture(num, num, 16, format, RenderTextureReadWrite.Linear);
			cubemap.dimension = TextureDimension.Cube;
			cubemap.useMipMap = true;
			cubemap.autoGenerateMips = false;
			cubemap.Create();
		}
	}

	private void CreateTexturesAndMaterial()
	{
		if (mirror == null)
		{
			mirror = new Material(Shader.Find("Hidden/Enviro/ReflectionProbe"));
		}
		int resolution = GetComponent<ReflectionProbe>().resolution;
		RenderTextureFormat format = (myProbe.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		if (mirrorTexture == null || mirrorTexture.width != resolution || mirrorTexture.height != resolution)
		{
			if (mirrorTexture != null)
			{
				Object.DestroyImmediate(mirrorTexture);
			}
			mirrorTexture = new RenderTexture(resolution, resolution, 16, format, RenderTextureReadWrite.Linear);
			mirrorTexture.useMipMap = true;
			mirrorTexture.autoGenerateMips = false;
			mirrorTexture.Create();
		}
		if (renderTexture == null || renderTexture.width != resolution || renderTexture.height != resolution)
		{
			if (renderTexture != null)
			{
				Object.DestroyImmediate(renderTexture);
			}
			renderTexture = new RenderTexture(resolution, resolution, 16, format, RenderTextureReadWrite.Linear);
			renderTexture.useMipMap = true;
			renderTexture.autoGenerateMips = false;
			renderTexture.Create();
		}
	}

	public void RefreshReflection(bool timeSlice = false)
	{
		if (customRendering)
		{
			if (!rendering)
			{
				CreateTexturesAndMaterial();
				if (renderCam == null)
				{
					CreateRenderCamera();
				}
				UpdateCameraSettings();
				renderCam.transform.position = base.transform.position;
				renderCam.targetTexture = renderTexture;
				if (!timeSlice)
				{
					refreshing = StartCoroutine(RefreshInstant(renderTexture, mirrorTexture));
				}
				else
				{
					refreshing = StartCoroutine(RefreshOvertime(renderTexture, mirrorTexture));
				}
			}
		}
		else
		{
			myProbe.RenderProbe();
		}
	}

	private IEnumerator RefreshFirstTime()
	{
		yield return null;
		RefreshReflection(true);
	}

	private IEnumerator RefreshInstant(RenderTexture renderTex, RenderTexture mirrorTex)
	{
		yield return null;
		for (int i = 0; i < 6; i++)
		{
			CreateCubemap();
			rendering = true;
			renderCam.transform.rotation = orientations[i];
			renderCam.Render();
			Graphics.Blit(renderTex, mirrorTex, mirror);
			Graphics.CopyTexture(mirrorTex, 0, 0, cubemap, i, 0);
			ClearTextures();
		}
		cubemap.GenerateMips();
		myProbe.customBakedTexture = cubemap;
		rendering = false;
		refreshing = null;
	}

	private IEnumerator RefreshOvertime(RenderTexture renderTex, RenderTexture mirrorTex)
	{
		try
		{
			for (int face = 0; face < 6; face++)
			{
				CreateCubemap();
				rendering = true;
				ClearTextures();
				renderCam.transform.rotation = orientations[face];
				renderCam.Render();
				Graphics.Blit(renderTex, mirrorTex, mirror);
				Graphics.CopyTexture(mirrorTex, 0, 0, cubemap, face, 0);
				yield return null;
			}
			cubemap.GenerateMips();
			myProbe.customBakedTexture = cubemap;
		}
		finally
		{
			EnviroReflectionProbe enviroReflectionProbe = this;
			enviroReflectionProbe.rendering = false;
			enviroReflectionProbe.refreshing = null;
		}
	}

	public RenderTexture BakeCubemapFace(int face, int res)
	{
		if (bakeMat == null)
		{
			bakeMat = new Material(Shader.Find("Hidden/Enviro/BakeCubemap"));
		}
		if (bakingCam == null)
		{
			bakingCam = CreateBakingCamera();
		}
		bakingCam.transform.rotation = orientations[face];
		RenderTexture temporary = RenderTexture.GetTemporary(res, res, 0, RenderTextureFormat.DefaultHDR);
		bakingCam.targetTexture = temporary;
		bakingCam.Render();
		RenderTexture renderTexture = new RenderTexture(res, res, 0, RenderTextureFormat.DefaultHDR);
		Graphics.Blit(temporary, renderTexture, bakeMat);
		RenderTexture.ReleaseTemporary(temporary);
		return renderTexture;
	}

	private void ClearTextures()
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = mirrorTexture;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = active;
	}

	private void UpdateStandaloneReflection()
	{
		if ((EnviroSkyMgr.instance.GetCurrentTimeInHours() > lastRelfectionUpdate + (double)reflectionsUpdateTreshhold || EnviroSkyMgr.instance.GetCurrentTimeInHours() < lastRelfectionUpdate - (double)reflectionsUpdateTreshhold) && updateReflectionOnGameTime)
		{
			lastRelfectionUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours();
			RefreshReflection(!useTimeSlicing);
		}
	}

	private void Update()
	{
		if (currentMode != customRendering)
		{
			currentMode = customRendering;
			if (customRendering)
			{
				OnEnable();
			}
			else
			{
				OnEnable();
				Cleanup();
			}
		}
		if (EnviroSkyMgr.instance != null && standalone)
		{
			UpdateStandaloneReflection();
		}
	}
}
