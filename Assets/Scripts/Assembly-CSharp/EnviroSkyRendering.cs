using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class EnviroSkyRendering : MonoBehaviour
{
	public enum FogType
	{
		Disabled = 0,
		Simple = 1,
		Standard = 2
	}

	public enum VolumtericResolution
	{
		Full = 0,
		Half = 1,
		Quarter = 2
	}

	[HideInInspector]
	public bool aboveClouds;

	[HideInInspector]
	public bool isAddionalCamera;

	private Camera myCam;

	private RenderTexture spSatTex;

	private Camera spSatCam;

	public bool useGlobalRenderingSettings = true;

	public EnviroCustomRenderingSettings customRenderingSettings = new EnviroCustomRenderingSettings();

	private bool useVolumeClouds = true;

	private bool useVolumeLighting = true;

	private bool useDistanceBlur = true;

	private bool useFog = true;

	[HideInInspector]
	public FogType currentFogType;

	private Material cloudsMat;

	private Material blitMat;

	private Material compose;

	private Material downsample;

	private RenderTexture subFrameTex;

	private RenderTexture prevFrameTex;

	private Texture2D curlMap;

	private Texture2D dither;

	private Texture2D blueNoise;

	private Texture3D noiseTexture;

	private Texture3D noiseTextureHigh;

	private Texture3D detailNoiseTexture;

	private Texture3D detailNoiseTextureHigh;

	private Matrix4x4 projection;

	private Matrix4x4 projectionSPVR;

	private Matrix4x4 inverseRotation;

	private Matrix4x4 inverseRotationSPVR;

	private Matrix4x4 rotation;

	private Matrix4x4 rotationSPVR;

	private Matrix4x4 previousRotation;

	private Matrix4x4 previousRotationSPVR;

	[HideInInspector]
	public EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize currentReprojectionPixelSize;

	private int reprojectionPixelSize;

	private bool isFirstFrame;

	private int subFrameNumber;

	private int[] frameList;

	private int renderingCounter;

	private int subFrameWidth;

	private int subFrameHeight;

	private int frameWidth;

	private int frameHeight;

	private bool textureDimensionChanged;

	private EnviroVolumeCloudsQualitySettings usedCloudsQuality;

	private static Mesh _pointLightMesh;

	private static Mesh _spotLightMesh;

	private static Material _lightMaterial;

	private CommandBuffer _preLightPass;

	public CommandBuffer _afterLightPass;

	private Matrix4x4 _viewProj;

	private Matrix4x4 _viewProjSP;

	[HideInInspector]
	public Material fogMat;

	private Material _bilateralBlurMaterial;

	private RenderTexture _volumeLightTexture;

	private RenderTexture _halfVolumeLightTexture;

	private RenderTexture _quarterVolumeLightTexture;

	private static Texture _defaultSpotCookie;

	private RenderTexture _halfDepthBuffer;

	private RenderTexture _quarterDepthBuffer;

	private VolumtericResolution currentVolumeRes;

	[HideInInspector]
	public Texture2D _ditheringTexture;

	private Texture2D blackTexture;

	[HideInInspector]
	public Texture DefaultSpotCookie;

	[HideInInspector]
	public Material volumeLightMat;

	private Material postProcessMat;

	private const int kMaxIterations = 16;

	private RenderTexture[] _blurBuffer1 = new RenderTexture[16];

	private RenderTexture[] _blurBuffer2 = new RenderTexture[16];

	private Texture2D distributionTexture;

	public CommandBuffer GlobalCommandBuffer
	{
		get
		{
			return _preLightPass;
		}
	}

	public CommandBuffer GlobalCommandBufferForward
	{
		get
		{
			return _afterLightPass;
		}
	}

	public float thresholdGamma
	{
		get
		{
			return Mathf.Max(0f, 0f);
		}
	}

	public float thresholdLinear
	{
		get
		{
			return Mathf.GammaToLinearSpace(thresholdGamma);
		}
	}

	public static event Action<EnviroSkyRendering, Matrix4x4, Matrix4x4> PreRenderEvent;

	public static Material GetLightMaterial()
	{
		return _lightMaterial;
	}

	public static Mesh GetPointLightMesh()
	{
		return _pointLightMesh;
	}

	public static Mesh GetSpotLightMesh()
	{
		return _spotLightMesh;
	}

	public RenderTexture GetVolumeLightBuffer()
	{
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
		{
			return _quarterVolumeLightTexture;
		}
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
		{
			return _halfVolumeLightTexture;
		}
		return _volumeLightTexture;
	}

	public RenderTexture GetVolumeLightDepthBuffer()
	{
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
		{
			return _quarterDepthBuffer;
		}
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
		{
			return _halfDepthBuffer;
		}
		return null;
	}

	public static Texture GetDefaultSpotCookie()
	{
		return _defaultSpotCookie;
	}

	private void OnEnable()
	{
		if (myCam == null)
		{
			myCam = GetComponent<Camera>();
		}
		if (EnviroSky.instance == null)
		{
			base.enabled = false;
			return;
		}
		CreateMaterialsAndTextures();
		SetupVolumeFog();
		CreateCommandBuffer();
		CreateFogMaterial();
		UpdateQualitySettings();
		if (EnviroSky.instance != null)
		{
			SetReprojectionPixelSize(usedCloudsQuality.reprojectionPixelSize);
		}
		if (isAddionalCamera && !useGlobalRenderingSettings && !customRenderingSettings.useVolumeLighting && fogMat != null)
		{
			fogMat.DisableKeyword("ENVIROVOLUMELIGHT");
			fogMat.SetTexture("_EnviroVolumeLightingTex", blackTexture);
		}
	}

	private void OnDisable()
	{
		RemoveCommandBuffer();
		CleanupMaterials();
	}

	private void CleanupMaterials()
	{
		if (postProcessMat != null)
		{
			UnityEngine.Object.DestroyImmediate(postProcessMat);
		}
		if (volumeLightMat != null)
		{
			UnityEngine.Object.DestroyImmediate(volumeLightMat);
		}
		if (_bilateralBlurMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(_bilateralBlurMaterial);
		}
		if (cloudsMat != null)
		{
			UnityEngine.Object.DestroyImmediate(cloudsMat);
		}
		if (fogMat != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMat);
		}
		if (blitMat != null)
		{
			UnityEngine.Object.DestroyImmediate(blitMat);
		}
		if (compose != null)
		{
			UnityEngine.Object.DestroyImmediate(compose);
		}
		if (downsample != null)
		{
			UnityEngine.Object.DestroyImmediate(downsample);
		}
	}

	private void CreateCommandBuffer()
	{
		_preLightPass = new CommandBuffer();
		_preLightPass.name = "PreLight";
		_afterLightPass = new CommandBuffer();
		_afterLightPass.name = "AfterLight";
		if (myCam.actualRenderingPath == RenderingPath.Forward)
		{
			myCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
			myCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
		}
		else
		{
			myCam.AddCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
		}
	}

	private void RemoveCommandBuffer()
	{
		if (myCam.actualRenderingPath == RenderingPath.Forward)
		{
			if (_preLightPass != null)
			{
				myCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
			}
			if (_afterLightPass != null)
			{
				myCam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
			}
		}
		else if (_preLightPass != null)
		{
			myCam.RemoveCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
		}
	}

	private void SetupVolumeFog()
	{
		if (EnviroSky.instance == null)
		{
			return;
		}
		currentVolumeRes = EnviroSky.instance.volumeLightSettings.Resolution;
		ChangeResolution();
		if (_pointLightMesh == null)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			_pointLightMesh = obj.GetComponent<MeshFilter>().sharedMesh;
			UnityEngine.Object.DestroyImmediate(obj);
		}
		if (_spotLightMesh == null)
		{
			_spotLightMesh = CreateSpotLightMesh();
		}
		if (_lightMaterial == null)
		{
			Shader shader = Shader.Find("Enviro/Standard/VolumeLight");
			if (shader == null)
			{
				throw new Exception("Critical Error: \"Enviro/VolumeLight\" shader is missing.");
			}
			_lightMaterial = new Material(shader);
		}
		if (_defaultSpotCookie == null)
		{
			_defaultSpotCookie = DefaultSpotCookie;
		}
		GenerateDitherTexture();
	}

	private void ChangeResolution()
	{
		int pixelWidth = myCam.pixelWidth;
		int pixelHeight = myCam.pixelHeight;
		if (pixelWidth <= 0 || pixelHeight <= 0)
		{
			return;
		}
		if (_volumeLightTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(_volumeLightTexture);
		}
		_volumeLightTexture = new RenderTexture(pixelWidth, pixelHeight, 0, RenderTextureFormat.ARGBHalf);
		_volumeLightTexture.name = "VolumeLightBuffer";
		_volumeLightTexture.filterMode = FilterMode.Bilinear;
		if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
		{
			if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half || EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
			{
				_volumeLightTexture.vrUsage = VRTextureUsage.None;
			}
			else
			{
				_volumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
			}
		}
		if (_halfDepthBuffer != null)
		{
			UnityEngine.Object.DestroyImmediate(_halfDepthBuffer);
		}
		if (_halfVolumeLightTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(_halfVolumeLightTexture);
		}
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half || EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
		{
			_halfVolumeLightTexture = new RenderTexture(pixelWidth / 2, pixelHeight / 2, 0, RenderTextureFormat.ARGBHalf);
			_halfVolumeLightTexture.name = "VolumeLightBufferHalf";
			_halfVolumeLightTexture.filterMode = FilterMode.Bilinear;
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				_halfVolumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
			}
			_halfDepthBuffer = new RenderTexture(pixelWidth / 2, pixelHeight / 2, 0, RenderTextureFormat.RFloat);
			_halfDepthBuffer.name = "VolumeLightHalfDepth";
			_halfDepthBuffer.Create();
			_halfDepthBuffer.filterMode = FilterMode.Point;
		}
		if (_quarterVolumeLightTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(_quarterVolumeLightTexture);
		}
		if (_quarterDepthBuffer != null)
		{
			UnityEngine.Object.DestroyImmediate(_quarterDepthBuffer);
		}
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
		{
			_quarterVolumeLightTexture = new RenderTexture(pixelWidth / 4, pixelHeight / 4, 0, RenderTextureFormat.ARGBHalf);
			_quarterVolumeLightTexture.name = "VolumeLightBufferQuarter";
			_quarterVolumeLightTexture.filterMode = FilterMode.Bilinear;
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				_quarterVolumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
			}
			_quarterDepthBuffer = new RenderTexture(pixelWidth / 4, pixelHeight / 4, 0, RenderTextureFormat.RFloat);
			_quarterDepthBuffer.name = "VolumeLightQuarterDepth";
			_quarterDepthBuffer.Create();
			_quarterDepthBuffer.filterMode = FilterMode.Point;
		}
	}

	private void CreateFogMaterial()
	{
		if (EnviroSky.instance == null)
		{
			return;
		}
		if (fogMat != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMat);
		}
		if (!useFog)
		{
			Shader shader = Shader.Find("Enviro/Standard/EnviroFogRenderingDisabled");
			if (shader == null)
			{
				throw new Exception("Critical Error: \"Enviro/EnviroFogRenderingDisabled\" shader is missing.");
			}
			fogMat = new Material(shader);
			currentFogType = FogType.Disabled;
		}
		else if (!EnviroSky.instance.fogSettings.useSimpleFog)
		{
			Shader shader2 = Shader.Find("Enviro/Standard/EnviroFogRendering");
			if (shader2 == null)
			{
				throw new Exception("Critical Error: \"Enviro/EnviroFogRendering\" shader is missing.");
			}
			fogMat = new Material(shader2);
			currentFogType = FogType.Standard;
		}
		else
		{
			Shader shader3 = Shader.Find("Enviro/Standard/EnviroFogRenderingSimple");
			if (shader3 == null)
			{
				throw new Exception("Critical Error: \"Enviro/EnviroFogRenderingSimple\" shader is missing.");
			}
			fogMat = new Material(shader3);
			currentFogType = FogType.Simple;
		}
	}

	private void CreateMaterialsAndTextures()
	{
		if (cloudsMat == null)
		{
			cloudsMat = new Material(Shader.Find("Enviro/Standard/RaymarchClouds"));
		}
		if (blitMat == null)
		{
			blitMat = new Material(Shader.Find("Enviro/Standard/Blit"));
		}
		if (compose == null)
		{
			compose = new Material(Shader.Find("Hidden/Enviro/Upsample"));
		}
		if (downsample == null)
		{
			downsample = new Material(Shader.Find("Hidden/Enviro/DepthDownsample"));
		}
		if (volumeLightMat != null)
		{
			volumeLightMat = new Material(Shader.Find("Enviro/Standard/VolumeLight"));
		}
		if (postProcessMat != null)
		{
			postProcessMat = new Material(Shader.Find("Hidden/EnviroDistanceBlur"));
		}
		if (_bilateralBlurMaterial != null)
		{
			_bilateralBlurMaterial = new Material(Shader.Find("Hidden/EnviroBilateralBlur"));
		}
		if (curlMap == null)
		{
			curlMap = Resources.Load("tex_enviro_curl") as Texture2D;
		}
		if (blackTexture == null)
		{
			blackTexture = Resources.Load("tex_enviro_black") as Texture2D;
		}
		if (noiseTextureHigh == null)
		{
			noiseTextureHigh = Resources.Load("enviro_clouds_base") as Texture3D;
		}
		if (noiseTexture == null)
		{
			noiseTexture = Resources.Load("enviro_clouds_base_low") as Texture3D;
		}
		if (detailNoiseTexture == null)
		{
			detailNoiseTexture = Resources.Load("enviro_clouds_detail_low") as Texture3D;
		}
		if (detailNoiseTextureHigh == null)
		{
			detailNoiseTextureHigh = Resources.Load("enviro_clouds_detail_high") as Texture3D;
		}
		if (dither == null)
		{
			dither = Resources.Load("tex_enviro_dither") as Texture2D;
		}
		if (distributionTexture == null)
		{
			distributionTexture = Resources.Load("tex_enviro_linear", typeof(Texture2D)) as Texture2D;
		}
		if (blueNoise == null)
		{
			blueNoise = Resources.Load("tex_enviro_blueNoise", typeof(Texture2D)) as Texture2D;
		}
	}

	private void OnPreRender()
	{
		if (EnviroSky.instance == null)
		{
			return;
		}
		if (useVolumeLighting)
		{
			if (_bilateralBlurMaterial == null)
			{
				_bilateralBlurMaterial = new Material(Shader.Find("Hidden/EnviroBilateralBlur"));
			}
			Matrix4x4 matrix4x = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);
			Matrix4x4 matrix4x2 = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);
			if (myCam.stereoEnabled)
			{
				matrix4x = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
				matrix4x = GL.GetGPUProjectionMatrix(matrix4x, true);
				matrix4x2 = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
				matrix4x2 = GL.GetGPUProjectionMatrix(matrix4x2, true);
			}
			else
			{
				matrix4x = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);
				matrix4x = GL.GetGPUProjectionMatrix(matrix4x, true);
			}
			if (myCam.stereoEnabled)
			{
				_viewProj = matrix4x * myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
				_viewProjSP = matrix4x2 * myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			}
			else
			{
				_viewProj = matrix4x * myCam.worldToCameraMatrix;
				_viewProjSP = matrix4x2 * myCam.worldToCameraMatrix;
			}
			if (_preLightPass != null)
			{
				_preLightPass.Clear();
			}
			if (_afterLightPass != null)
			{
				_afterLightPass.Clear();
			}
			bool flag = SystemInfo.graphicsShaderLevel > 40;
			if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
			{
				Texture source = null;
				_preLightPass.Blit(source, _halfDepthBuffer, _bilateralBlurMaterial, flag ? 4 : 10);
				_preLightPass.Blit(source, _quarterDepthBuffer, _bilateralBlurMaterial, flag ? 6 : 11);
				_preLightPass.SetRenderTarget(_quarterVolumeLightTexture);
			}
			else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
			{
				Texture source2 = null;
				_preLightPass.Blit(source2, _halfDepthBuffer, _bilateralBlurMaterial, flag ? 4 : 10);
				_preLightPass.SetRenderTarget(_halfVolumeLightTexture);
			}
			else
			{
				_preLightPass.SetRenderTarget(_volumeLightTexture);
			}
			_preLightPass.ClearRenderTarget(false, true, new Color(0f, 0f, 0f, 1f));
			UpdateMaterialParameters();
			if (EnviroSkyRendering.PreRenderEvent != null)
			{
				EnviroSkyRendering.PreRenderEvent(this, _viewProj, _viewProjSP);
			}
		}
		if (myCam.stereoEnabled)
		{
			Matrix4x4 inverse = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
			Matrix4x4 inverse2 = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
			Matrix4x4 stereoProjectionMatrix = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoProjectionMatrix2 = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			Matrix4x4 inverse3 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix, true).inverse;
			Matrix4x4 inverse4 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix2, true).inverse;
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
			{
				inverse3[1, 1] *= -1f;
				inverse4[1, 1] *= -1f;
			}
			Shader.SetGlobalMatrix("_LeftWorldFromView", inverse);
			Shader.SetGlobalMatrix("_RightWorldFromView", inverse2);
			Shader.SetGlobalMatrix("_LeftViewFromScreen", inverse3);
			Shader.SetGlobalMatrix("_RightViewFromScreen", inverse4);
		}
		else
		{
			Matrix4x4 cameraToWorldMatrix = myCam.cameraToWorldMatrix;
			Matrix4x4 inverse5 = GL.GetGPUProjectionMatrix(myCam.projectionMatrix, true).inverse;
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
			{
				inverse5[1, 1] *= -1f;
			}
			Shader.SetGlobalMatrix("_LeftWorldFromView", cameraToWorldMatrix);
			Shader.SetGlobalMatrix("_LeftViewFromScreen", inverse5);
		}
		if (EnviroSky.instance == null || !(myCam != null))
		{
			return;
		}
		switch (myCam.stereoActiveEye)
		{
		case Camera.MonoOrStereoscopicEye.Mono:
			if (EnviroSky.instance.satCamera != null)
			{
				RenderCamera(EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Mono);
			}
			break;
		case Camera.MonoOrStereoscopicEye.Left:
			if (EnviroSky.instance.satCamera != null)
			{
				RenderCamera(EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Left);
			}
			break;
		case Camera.MonoOrStereoscopicEye.Right:
			if (EnviroSky.instance.satCamera != null)
			{
				RenderCamera(EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Right);
			}
			break;
		}
		if (EnviroSky.instance.satCamera != null)
		{
			RenderSettings.skybox.SetTexture("_SatTex", EnviroSky.instance.satCamera.targetTexture);
		}
	}

	private void RenderCamera(Camera targetCam, Camera.MonoOrStereoscopicEye eye)
	{
		targetCam.fieldOfView = EnviroSky.instance.PlayerCamera.fieldOfView;
		targetCam.aspect = EnviroSky.instance.PlayerCamera.aspect;
		switch (eye)
		{
		case Camera.MonoOrStereoscopicEye.Mono:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.worldToCameraMatrix;
			targetCam.Render();
			break;
		case Camera.MonoOrStereoscopicEye.Left:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			targetCam.Render();
			break;
		case Camera.MonoOrStereoscopicEye.Right:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			targetCam.Render();
			break;
		}
	}

	private void UpdateQualitySettings()
	{
		if (useGlobalRenderingSettings)
		{
			useVolumeClouds = EnviroSky.instance.useVolumeClouds;
			useVolumeLighting = EnviroSky.instance.useVolumeLighting;
			useDistanceBlur = EnviroSky.instance.useDistanceBlur;
			useFog = EnviroSky.instance.useFog;
			usedCloudsQuality = EnviroSky.instance.cloudsSettings.cloudsQualitySettings;
			return;
		}
		useVolumeClouds = customRenderingSettings.useVolumeClouds;
		useVolumeLighting = customRenderingSettings.useVolumeLighting;
		useDistanceBlur = customRenderingSettings.useDistanceBlur;
		useFog = customRenderingSettings.useFog;
		if (customRenderingSettings.customCloudsQuality != null)
		{
			usedCloudsQuality = customRenderingSettings.customCloudsQuality.qualitySettings;
		}
		else
		{
			usedCloudsQuality = EnviroSky.instance.cloudsSettings.cloudsQualitySettings;
		}
	}

	private void Update()
	{
		if (!(EnviroSky.instance == null) && !(myCam == null))
		{
			UpdateQualitySettings();
			if (currentReprojectionPixelSize != usedCloudsQuality.reprojectionPixelSize)
			{
				currentReprojectionPixelSize = usedCloudsQuality.reprojectionPixelSize;
				SetReprojectionPixelSize(usedCloudsQuality.reprojectionPixelSize);
			}
			if (currentVolumeRes != EnviroSky.instance.volumeLightSettings.Resolution)
			{
				ChangeResolution();
				currentVolumeRes = EnviroSky.instance.volumeLightSettings.Resolution;
			}
			if (_volumeLightTexture != null && (_volumeLightTexture.width != myCam.pixelWidth || _volumeLightTexture.height != myCam.pixelHeight))
			{
				ChangeResolution();
			}
			if ((!useFog && currentFogType != 0) || (useFog && EnviroSky.instance.fogSettings.useSimpleFog && currentFogType != FogType.Simple) || (useFog && !EnviroSky.instance.fogSettings.useSimpleFog && currentFogType != FogType.Standard))
			{
				CreateFogMaterial();
			}
		}
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (EnviroSky.instance == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (fogMat == null)
		{
			CreateFogMaterial();
		}
		if (myCam.actualRenderingPath == RenderingPath.Forward)
		{
			myCam.depthTextureMode |= DepthTextureMode.Depth;
		}
		int depthBufferBits = source.depth;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			depthBufferBits = 0;
		}
		RenderTextureDescriptor desc = new RenderTextureDescriptor(source.width, source.height, source.format, depthBufferBits);
		desc.msaaSamples = source.antiAliasing;
		if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
		{
			desc.vrUsage = VRTextureUsage.TwoEyes;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(desc);
		RenderTexture temporary2 = RenderTexture.GetTemporary(desc);
		if (!aboveClouds)
		{
			if (useVolumeClouds)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor) || Application.isPlaying)
				{
					RenderVolumeClouds(source, temporary);
				}
				else
				{
					Graphics.Blit(source, temporary);
				}
			}
			else
			{
				Graphics.Blit(source, temporary);
			}
			if (useVolumeLighting)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showVolumeLightingInEditor) || Application.isPlaying)
				{
					RenderVolumeFog(temporary, temporary2);
				}
				else
				{
					RenderFog(temporary, temporary2);
					if (!isAddionalCamera)
					{
						Shader.DisableKeyword("ENVIROVOLUMELIGHT");
						Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
					}
					else
					{
						fogMat.DisableKeyword("ENVIROVOLUMELIGHT");
						fogMat.SetTexture("_EnviroVolumeLightingTex", blackTexture);
					}
				}
			}
			else
			{
				RenderFog(temporary, temporary2);
				if (!isAddionalCamera)
				{
					Shader.DisableKeyword("ENVIROVOLUMELIGHT");
					Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
				}
				else
				{
					fogMat.DisableKeyword("ENVIROVOLUMELIGHT");
					fogMat.SetTexture("_EnviroVolumeLightingTex", blackTexture);
				}
			}
			if (useDistanceBlur)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showDistanceBlurInEditor) || Application.isPlaying)
				{
					RenderDistanceBlur(temporary2, destination);
				}
				else
				{
					Graphics.Blit(temporary2, destination);
				}
			}
			else
			{
				Graphics.Blit(temporary2, destination);
			}
		}
		else
		{
			if (useVolumeLighting)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showVolumeLightingInEditor) || Application.isPlaying)
				{
					RenderVolumeFog(source, temporary);
				}
				else
				{
					RenderFog(source, temporary);
					if (!isAddionalCamera)
					{
						Shader.DisableKeyword("ENVIROVOLUMELIGHT");
						Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
					}
					else
					{
						fogMat.DisableKeyword("ENVIROVOLUMELIGHT");
						fogMat.SetTexture("_EnviroVolumeLightingTex", blackTexture);
					}
				}
			}
			else
			{
				RenderFog(source, temporary);
				if (!isAddionalCamera)
				{
					Shader.DisableKeyword("ENVIROVOLUMELIGHT");
					Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
				}
				else
				{
					fogMat.DisableKeyword("ENVIROVOLUMELIGHT");
					fogMat.SetTexture("_EnviroVolumeLightingTex", blackTexture);
				}
			}
			if (useVolumeClouds)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor) || Application.isPlaying)
				{
					RenderVolumeClouds(temporary, temporary2);
				}
				else
				{
					Graphics.Blit(temporary, temporary2);
				}
			}
			else
			{
				Graphics.Blit(temporary, temporary2);
			}
			if (useDistanceBlur)
			{
				if ((!Application.isPlaying && EnviroSky.instance.showDistanceBlurInEditor) || Application.isPlaying)
				{
					RenderDistanceBlur(temporary2, destination);
				}
				else
				{
					Graphics.Blit(temporary2, destination);
				}
			}
			else
			{
				Graphics.Blit(temporary2, destination);
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}

	private void RenderVolumeClouds(RenderTexture src, RenderTexture dst)
	{
		if (blitMat == null)
		{
			blitMat = new Material(Shader.Find("Enviro/Standard/Blit"));
		}
		StartFrame();
		if (subFrameTex == null || prevFrameTex == null || textureDimensionChanged)
		{
			CreateCloudsRenderTextures(src);
		}
		if (!isAddionalCamera)
		{
			EnviroSky.instance.cloudsRenderTarget = subFrameTex;
		}
		RenderClouds(src, subFrameTex);
		if (isFirstFrame)
		{
			Graphics.Blit(subFrameTex, prevFrameTex);
			isFirstFrame = false;
		}
		int num = ((!usedCloudsQuality.bilateralUpsampling) ? 1 : (reprojectionPixelSize * usedCloudsQuality.cloudsRenderResolution));
		if (num > 1)
		{
			if (compose == null)
			{
				compose = new Material(Shader.Find("Hidden/Enviro/Upsample"));
			}
			if (downsample == null)
			{
				downsample = new Material(Shader.Find("Hidden/Enviro/DepthDownsample"));
			}
			RenderTexture renderTexture = DownsampleDepth(Screen.width, Screen.height, src, downsample, num);
			compose.SetTexture("_CameraDepthLowRes", renderTexture);
			RenderTexture temporary = RenderTexture.GetTemporary(myCam.pixelWidth / num * 2, myCam.pixelHeight / num * 2, 0, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
			temporary.filterMode = FilterMode.Bilinear;
			Vector2 vector = new Vector2(1f / (float)renderTexture.width, 1f / (float)renderTexture.height);
			compose.SetVector("_LowResPixelSize", vector);
			compose.SetVector("_LowResTextureSize", new Vector2(renderTexture.width, renderTexture.height));
			compose.SetFloat("_DepthMult", 32f);
			compose.SetFloat("_Threshold", 0.0005f);
			compose.SetTexture("_LowResTexture", subFrameTex);
			Graphics.Blit(subFrameTex, temporary, compose);
			RenderTexture.ReleaseTemporary(renderTexture);
			blitMat.SetTexture("_MainTex", src);
			blitMat.SetTexture("_SubFrame", temporary);
			blitMat.SetTexture("_PrevFrame", prevFrameTex);
			SetBlitmaterialProperties();
			Graphics.Blit(src, dst, blitMat);
			Graphics.Blit(temporary, prevFrameTex);
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			blitMat.SetTexture("_MainTex", src);
			blitMat.SetTexture("_SubFrame", subFrameTex);
			blitMat.SetTexture("_PrevFrame", prevFrameTex);
			SetBlitmaterialProperties();
			Graphics.Blit(src, dst, blitMat);
			Graphics.Blit(subFrameTex, prevFrameTex);
		}
		FinalizeFrame();
	}

	private void RenderFog(RenderTexture src, RenderTexture dst)
	{
		float num = myCam.transform.position.y - EnviroSky.instance.fogSettings.height;
		float z = ((num <= 0f) ? 1f : 0f);
		FogMode fogMode = RenderSettings.fogMode;
		float fogDensity = RenderSettings.fogDensity;
		float fogStartDistance = RenderSettings.fogStartDistance;
		float fogEndDistance = RenderSettings.fogEndDistance;
		bool flag = fogMode == FogMode.Linear;
		float num2 = (flag ? (fogEndDistance - fogStartDistance) : 0f);
		float num3 = ((Mathf.Abs(num2) > 0.0001f) ? (1f / num2) : 0f);
		Vector4 value = default(Vector4);
		value.x = fogDensity * 1.2011224f;
		value.y = fogDensity * 1.442695f;
		value.z = (flag ? (0f - num3) : 0f);
		value.w = (flag ? (fogEndDistance * num3) : 0f);
		if (!EnviroSky.instance.fogSettings.useSimpleFog)
		{
			Shader.SetGlobalVector("_FogNoiseVelocity", new Vector4((0f - EnviroSky.instance.Components.windZone.transform.forward.x) * EnviroSky.instance.windIntensity * 5f, (0f - EnviroSky.instance.Components.windZone.transform.forward.z) * EnviroSky.instance.windIntensity * 5f) * EnviroSky.instance.fogSettings.noiseScale);
			Shader.SetGlobalVector("_FogNoiseData", new Vector4(EnviroSky.instance.fogSettings.noiseScale, EnviroSky.instance.fogSettings.noiseIntensity, EnviroSky.instance.fogSettings.noiseIntensityOffset));
			Shader.SetGlobalTexture("_FogNoiseTexture", detailNoiseTexture);
		}
		Shader.SetGlobalFloat("_EnviroVolumeDensity", EnviroSky.instance.globalVolumeLightIntensity);
		Shader.SetGlobalVector("_SceneFogParams", value);
		Shader.SetGlobalVector("_SceneFogMode", new Vector4((float)fogMode, EnviroSky.instance.fogSettings.useRadialDistance ? 1 : 0, 0f, Application.isPlaying ? 1f : 0f));
		Shader.SetGlobalVector("_HeightParams", new Vector4(EnviroSky.instance.fogSettings.height, num, z, EnviroSky.instance.fogSettings.heightDensity * 0.5f));
		Shader.SetGlobalVector("_DistanceParams", new Vector4(0f - Mathf.Max(EnviroSky.instance.fogSettings.startDistance, 0f), 0f, 0f, 0f));
		if (dither != null && EnviroSkyMgr.instance.FogSettings.dithering)
		{
			fogMat.SetTexture("_DitheringTex", dither);
			fogMat.SetInt("_UseDithering", 1);
		}
		else
		{
			fogMat.SetInt("_UseDithering", 0);
		}
		fogMat.SetTexture("_MainTex", src);
		Graphics.Blit(src, dst, fogMat);
	}

	private void RenderVolumeFog(RenderTexture src, RenderTexture dst)
	{
		if (volumeLightMat == null)
		{
			volumeLightMat = new Material(Shader.Find("Enviro/Standard/VolumeLight"));
		}
		if (_volumeLightTexture == null || (_halfVolumeLightTexture == null && EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half) || (_quarterVolumeLightTexture == null && EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter))
		{
			ChangeResolution();
		}
		if (EnviroSky.instance.volumeLightSettings.dirVolumeLighting)
		{
			Light component = EnviroSky.instance.Components.DirectLight.GetComponent<Light>();
			int pass = 4;
			volumeLightMat.SetPass(pass);
			if (EnviroSky.instance.volumeLightSettings.directLightNoise)
			{
				volumeLightMat.EnableKeyword("NOISE");
			}
			else
			{
				volumeLightMat.DisableKeyword("NOISE");
			}
			volumeLightMat.SetVector("_LightDir", new Vector4(component.transform.forward.x, component.transform.forward.y, component.transform.forward.z, 1f / (component.range * component.range)));
			volumeLightMat.SetVector("_LightColor", component.color * component.intensity);
			volumeLightMat.SetFloat("_MaxRayLength", EnviroSky.instance.volumeLightSettings.MaxRayLength);
			if (component.cookie == null)
			{
				volumeLightMat.EnableKeyword("DIRECTIONAL");
				volumeLightMat.DisableKeyword("DIRECTIONAL_COOKIE");
			}
			else
			{
				volumeLightMat.EnableKeyword("DIRECTIONAL_COOKIE");
				volumeLightMat.DisableKeyword("DIRECTIONAL");
				volumeLightMat.SetTexture("_LightTexture0", component.cookie);
			}
			volumeLightMat.SetInt("_SampleCount", EnviroSky.instance.volumeLightSettings.SampleCount);
			volumeLightMat.SetVector("_NoiseVelocity", new Vector4((0f - EnviroSky.instance.Components.windZone.transform.forward.x) * EnviroSky.instance.windIntensity * 10f, (0f - EnviroSky.instance.Components.windZone.transform.forward.z) * EnviroSky.instance.windIntensity * 10f) * EnviroSky.instance.volumeLightSettings.noiseScale);
			volumeLightMat.SetVector("_NoiseData", new Vector4(EnviroSky.instance.volumeLightSettings.noiseScale, EnviroSky.instance.volumeLightSettings.noiseIntensity, EnviroSky.instance.volumeLightSettings.noiseIntensityOffset));
			volumeLightMat.SetVector("_MieG", new Vector4(1f - EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy, 1f + EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy, 2f * EnviroSky.instance.volumeLightSettings.Anistropy, 1f / (4f * (float)Math.PI)));
			volumeLightMat.SetVector("_VolumetricLight", new Vector4(EnviroSky.instance.volumeLightSettings.ScatteringCoef.Evaluate(EnviroSky.instance.GameTime.solarTime), EnviroSky.instance.volumeLightSettings.ExtinctionCoef, component.range, 1f));
			volumeLightMat.SetTexture("_CameraDepthTexture", GetVolumeLightDepthBuffer());
			if (component.shadows != 0)
			{
				volumeLightMat.EnableKeyword("SHADOWS_DEPTH");
				Graphics.Blit(null, GetVolumeLightBuffer(), volumeLightMat, pass);
			}
			else
			{
				volumeLightMat.DisableKeyword("SHADOWS_DEPTH");
				Graphics.Blit(null, GetVolumeLightBuffer(), volumeLightMat, pass);
			}
		}
		if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(_quarterDepthBuffer.width, _quarterDepthBuffer.height, 0, RenderTextureFormat.ARGBHalf);
			temporary.filterMode = FilterMode.Bilinear;
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				temporary.vrUsage = VRTextureUsage.TwoEyes;
			}
			Graphics.Blit(_quarterVolumeLightTexture, temporary, _bilateralBlurMaterial, 8);
			Graphics.Blit(temporary, _quarterVolumeLightTexture, _bilateralBlurMaterial, 9);
			Graphics.Blit(_quarterVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 7);
			RenderTexture.ReleaseTemporary(temporary);
		}
		else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(_halfVolumeLightTexture.width, _halfVolumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
			temporary2.filterMode = FilterMode.Bilinear;
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				temporary2.vrUsage = VRTextureUsage.TwoEyes;
			}
			Graphics.Blit(_halfVolumeLightTexture, temporary2, _bilateralBlurMaterial, 2);
			Graphics.Blit(temporary2, _halfVolumeLightTexture, _bilateralBlurMaterial, 3);
			Graphics.Blit(_halfVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 5);
			RenderTexture.ReleaseTemporary(temporary2);
		}
		else
		{
			RenderTexture temporary3 = RenderTexture.GetTemporary(_volumeLightTexture.width, _volumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
			temporary3.filterMode = FilterMode.Bilinear;
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				temporary3.vrUsage = VRTextureUsage.TwoEyes;
			}
			Graphics.Blit(_volumeLightTexture, temporary3, _bilateralBlurMaterial, 0);
			Graphics.Blit(temporary3, _volumeLightTexture, _bilateralBlurMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary3);
		}
		Shader.EnableKeyword("ENVIROVOLUMELIGHT");
		Shader.SetGlobalTexture("_EnviroVolumeLightingTex", _volumeLightTexture);
		RenderFog(src, dst);
	}

	private void RenderDistanceBlur(RenderTexture source, RenderTexture destination)
	{
		bool allowHDR = myCam.allowHDR;
		int num = source.width;
		int num2 = source.height;
		if (!EnviroSky.instance.distanceBlurSettings.highQuality)
		{
			num /= 2;
			num2 /= 2;
		}
		if (postProcessMat == null)
		{
			postProcessMat = new Material(Shader.Find("Hidden/EnviroDistanceBlur"));
		}
		postProcessMat.SetTexture("_DistTex", distributionTexture);
		postProcessMat.SetFloat("_Distance", EnviroSky.instance.blurDistance);
		postProcessMat.SetFloat("_Radius", EnviroSky.instance.distanceBlurSettings.radius);
		RenderTextureFormat format = (allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		float num3 = Mathf.Log(num2, 2f) + EnviroSky.instance.distanceBlurSettings.radius - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float num6 = thresholdLinear;
		postProcessMat.SetFloat("_Threshold", num6);
		float num7 = num6 * 0.5f + 1E-05f;
		Vector3 vector = new Vector3(num6 - num7, num7 * 2f, 0.25f / num7);
		postProcessMat.SetVector("_Curve", vector);
		bool flag = !EnviroSky.instance.distanceBlurSettings.highQuality && EnviroSky.instance.distanceBlurSettings.antiFlicker;
		postProcessMat.SetFloat("_PrefilterOffs", flag ? (-0.5f) : 0f);
		postProcessMat.SetFloat("_SampleScale", 0.5f + num3 - (float)num4);
		postProcessMat.SetFloat("_Intensity", EnviroSky.instance.blurIntensity);
		postProcessMat.SetFloat("_SkyBlurring", EnviroSky.instance.blurSkyIntensity);
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format);
		int pass = (EnviroSky.instance.distanceBlurSettings.antiFlicker ? 1 : 0);
		Graphics.Blit(source, temporary, postProcessMat, pass);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			pass = ((i == 0) ? (EnviroSky.instance.distanceBlurSettings.antiFlicker ? 3 : 2) : 4);
			Graphics.Blit(renderTexture, _blurBuffer1[i], postProcessMat, pass);
			renderTexture = _blurBuffer1[i];
		}
		for (int num8 = num5 - 2; num8 >= 0; num8--)
		{
			RenderTexture renderTexture2 = _blurBuffer1[num8];
			postProcessMat.SetTexture("_BaseTex", renderTexture2);
			_blurBuffer2[num8] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
			pass = (EnviroSky.instance.distanceBlurSettings.highQuality ? 6 : 5);
			Graphics.Blit(renderTexture, _blurBuffer2[num8], postProcessMat, pass);
			renderTexture = _blurBuffer2[num8];
		}
		postProcessMat.SetTexture("_BaseTex", source);
		pass = (EnviroSky.instance.distanceBlurSettings.highQuality ? 8 : 7);
		Graphics.Blit(renderTexture, destination, postProcessMat, pass);
		for (int j = 0; j < 16; j++)
		{
			if (_blurBuffer1[j] != null)
			{
				RenderTexture.ReleaseTemporary(_blurBuffer1[j]);
			}
			if (_blurBuffer2[j] != null)
			{
				RenderTexture.ReleaseTemporary(_blurBuffer2[j]);
			}
			_blurBuffer1[j] = null;
			_blurBuffer2[j] = null;
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void UpdateMaterialParameters()
	{
		if (_bilateralBlurMaterial == null)
		{
			_bilateralBlurMaterial = new Material(Shader.Find("Hidden/EnviroBilateralBlur"));
		}
		_bilateralBlurMaterial.SetTexture("_HalfResDepthBuffer", _halfDepthBuffer);
		_bilateralBlurMaterial.SetTexture("_HalfResColor", _halfVolumeLightTexture);
		_bilateralBlurMaterial.SetTexture("_QuarterResDepthBuffer", _quarterDepthBuffer);
		_bilateralBlurMaterial.SetTexture("_QuarterResColor", _quarterVolumeLightTexture);
		Shader.SetGlobalTexture("_DitherTexture", _ditheringTexture);
		Shader.SetGlobalTexture("_NoiseTexture", detailNoiseTexture);
	}

	private void GenerateDitherTexture()
	{
		if (!(_ditheringTexture != null))
		{
			int num = 8;
			_ditheringTexture = new Texture2D(num, num, TextureFormat.Alpha8, false, true);
			_ditheringTexture.filterMode = FilterMode.Point;
			Color32[] array = new Color32[num * num];
			int num2 = 0;
			byte b = 3;
			array[num2++] = new Color32(b, b, b, b);
			b = 192;
			array[num2++] = new Color32(b, b, b, b);
			b = 51;
			array[num2++] = new Color32(b, b, b, b);
			b = 239;
			array[num2++] = new Color32(b, b, b, b);
			b = 15;
			array[num2++] = new Color32(b, b, b, b);
			b = 204;
			array[num2++] = new Color32(b, b, b, b);
			b = 62;
			array[num2++] = new Color32(b, b, b, b);
			b = 251;
			array[num2++] = new Color32(b, b, b, b);
			b = 129;
			array[num2++] = new Color32(b, b, b, b);
			b = 66;
			array[num2++] = new Color32(b, b, b, b);
			b = 176;
			array[num2++] = new Color32(b, b, b, b);
			b = 113;
			array[num2++] = new Color32(b, b, b, b);
			b = 141;
			array[num2++] = new Color32(b, b, b, b);
			b = 78;
			array[num2++] = new Color32(b, b, b, b);
			b = 188;
			array[num2++] = new Color32(b, b, b, b);
			b = 125;
			array[num2++] = new Color32(b, b, b, b);
			b = 35;
			array[num2++] = new Color32(b, b, b, b);
			b = 223;
			array[num2++] = new Color32(b, b, b, b);
			b = 19;
			array[num2++] = new Color32(b, b, b, b);
			b = 207;
			array[num2++] = new Color32(b, b, b, b);
			b = 47;
			array[num2++] = new Color32(b, b, b, b);
			b = 235;
			array[num2++] = new Color32(b, b, b, b);
			b = 31;
			array[num2++] = new Color32(b, b, b, b);
			b = 219;
			array[num2++] = new Color32(b, b, b, b);
			b = 160;
			array[num2++] = new Color32(b, b, b, b);
			b = 98;
			array[num2++] = new Color32(b, b, b, b);
			b = 145;
			array[num2++] = new Color32(b, b, b, b);
			b = 82;
			array[num2++] = new Color32(b, b, b, b);
			b = 172;
			array[num2++] = new Color32(b, b, b, b);
			b = 109;
			array[num2++] = new Color32(b, b, b, b);
			b = 156;
			array[num2++] = new Color32(b, b, b, b);
			b = 94;
			array[num2++] = new Color32(b, b, b, b);
			b = 11;
			array[num2++] = new Color32(b, b, b, b);
			b = 200;
			array[num2++] = new Color32(b, b, b, b);
			b = 58;
			array[num2++] = new Color32(b, b, b, b);
			b = 247;
			array[num2++] = new Color32(b, b, b, b);
			b = 7;
			array[num2++] = new Color32(b, b, b, b);
			b = 196;
			array[num2++] = new Color32(b, b, b, b);
			b = 54;
			array[num2++] = new Color32(b, b, b, b);
			b = 243;
			array[num2++] = new Color32(b, b, b, b);
			b = 137;
			array[num2++] = new Color32(b, b, b, b);
			b = 74;
			array[num2++] = new Color32(b, b, b, b);
			b = 184;
			array[num2++] = new Color32(b, b, b, b);
			b = 121;
			array[num2++] = new Color32(b, b, b, b);
			b = 133;
			array[num2++] = new Color32(b, b, b, b);
			b = 70;
			array[num2++] = new Color32(b, b, b, b);
			b = 180;
			array[num2++] = new Color32(b, b, b, b);
			b = 117;
			array[num2++] = new Color32(b, b, b, b);
			b = 43;
			array[num2++] = new Color32(b, b, b, b);
			b = 231;
			array[num2++] = new Color32(b, b, b, b);
			b = 27;
			array[num2++] = new Color32(b, b, b, b);
			b = 215;
			array[num2++] = new Color32(b, b, b, b);
			b = 39;
			array[num2++] = new Color32(b, b, b, b);
			b = 227;
			array[num2++] = new Color32(b, b, b, b);
			b = 23;
			array[num2++] = new Color32(b, b, b, b);
			b = 211;
			array[num2++] = new Color32(b, b, b, b);
			b = 168;
			array[num2++] = new Color32(b, b, b, b);
			b = 105;
			array[num2++] = new Color32(b, b, b, b);
			b = 153;
			array[num2++] = new Color32(b, b, b, b);
			b = 90;
			array[num2++] = new Color32(b, b, b, b);
			b = 164;
			array[num2++] = new Color32(b, b, b, b);
			b = 102;
			array[num2++] = new Color32(b, b, b, b);
			b = 149;
			array[num2++] = new Color32(b, b, b, b);
			b = 86;
			array[num2++] = new Color32(b, b, b, b);
			_ditheringTexture.SetPixels32(array);
			_ditheringTexture.Apply();
		}
	}

	private Mesh CreateSpotLightMesh()
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[50];
		Color32[] array2 = new Color32[50];
		array[0] = new Vector3(0f, 0f, 0f);
		array[1] = new Vector3(0f, 0f, 1f);
		float num = 0f;
		float num2 = (float)Math.PI / 8f;
		float num3 = 0.9f;
		for (int i = 0; i < 16; i++)
		{
			array[i + 2] = new Vector3((0f - Mathf.Cos(num)) * num3, Mathf.Sin(num) * num3, num3);
			array2[i + 2] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			array[i + 2 + 16] = new Vector3(0f - Mathf.Cos(num), Mathf.Sin(num), 1f);
			array2[i + 2 + 16] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
			array[i + 2 + 32] = new Vector3((0f - Mathf.Cos(num)) * num3, Mathf.Sin(num) * num3, 1f);
			array2[i + 2 + 32] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			num += num2;
		}
		mesh.vertices = array;
		mesh.colors32 = array2;
		int[] array3 = new int[288];
		int num4 = 0;
		for (int j = 2; j < 17; j++)
		{
			array3[num4++] = 0;
			array3[num4++] = j;
			array3[num4++] = j + 1;
		}
		array3[num4++] = 0;
		array3[num4++] = 17;
		array3[num4++] = 2;
		for (int k = 2; k < 17; k++)
		{
			array3[num4++] = k;
			array3[num4++] = k + 16;
			array3[num4++] = k + 1;
			array3[num4++] = k + 1;
			array3[num4++] = k + 16;
			array3[num4++] = k + 16 + 1;
		}
		array3[num4++] = 2;
		array3[num4++] = 17;
		array3[num4++] = 18;
		array3[num4++] = 18;
		array3[num4++] = 17;
		array3[num4++] = 33;
		for (int l = 18; l < 33; l++)
		{
			array3[num4++] = l;
			array3[num4++] = l + 16;
			array3[num4++] = l + 1;
			array3[num4++] = l + 1;
			array3[num4++] = l + 16;
			array3[num4++] = l + 16 + 1;
		}
		array3[num4++] = 18;
		array3[num4++] = 33;
		array3[num4++] = 34;
		array3[num4++] = 34;
		array3[num4++] = 33;
		array3[num4++] = 49;
		for (int m = 34; m < 49; m++)
		{
			array3[num4++] = 1;
			array3[num4++] = m + 1;
			array3[num4++] = m;
		}
		array3[num4++] = 1;
		array3[num4++] = 34;
		array3[num4++] = 49;
		mesh.triangles = array3;
		mesh.RecalculateBounds();
		return mesh;
	}

	private void SetCloudProperties()
	{
		cloudsMat.SetTexture("_Noise", noiseTextureHigh);
		cloudsMat.SetTexture("_NoiseLow", noiseTexture);
		if (usedCloudsQuality.detailQuality == EnviroVolumeCloudsQualitySettings.CloudDetailQuality.Low)
		{
			cloudsMat.SetTexture("_DetailNoise", detailNoiseTexture);
		}
		else
		{
			cloudsMat.SetTexture("_DetailNoise", detailNoiseTextureHigh);
		}
		switch (myCam.stereoActiveEye)
		{
		case Camera.MonoOrStereoscopicEye.Mono:
		{
			projection = myCam.projectionMatrix;
			Matrix4x4 inverse4 = projection.inverse;
			cloudsMat.SetMatrix("_InverseProjection", inverse4);
			inverseRotation = myCam.cameraToWorldMatrix;
			cloudsMat.SetMatrix("_InverseRotation", inverseRotation);
			break;
		}
		case Camera.MonoOrStereoscopicEye.Left:
		{
			projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 inverse2 = projection.inverse;
			cloudsMat.SetMatrix("_InverseProjection", inverse2);
			inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
			cloudsMat.SetMatrix("_InverseRotation", inverseRotation);
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				Matrix4x4 inverse3 = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
				cloudsMat.SetMatrix("_InverseProjection_SP", inverse3);
				inverseRotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
				cloudsMat.SetMatrix("_InverseRotation_SP", inverseRotationSPVR);
			}
			break;
		}
		case Camera.MonoOrStereoscopicEye.Right:
		{
			projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			Matrix4x4 inverse = projection.inverse;
			cloudsMat.SetMatrix("_InverseProjection", inverse);
			inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
			cloudsMat.SetMatrix("_InverseRotation", inverseRotation);
			break;
		}
		}
		if (EnviroSky.instance.cloudsSettings.customWeatherMap == null)
		{
			cloudsMat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
		}
		else
		{
			cloudsMat.SetTexture("_WeatherMap", EnviroSky.instance.cloudsSettings.customWeatherMap);
		}
		cloudsMat.SetTexture("_CurlNoise", curlMap);
		cloudsMat.SetVector("_Steps", new Vector4((float)usedCloudsQuality.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, (float)usedCloudsQuality.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, 0f, 0f));
		cloudsMat.SetFloat("_BaseNoiseUV", usedCloudsQuality.baseNoiseUV);
		cloudsMat.SetFloat("_DetailNoiseUV", usedCloudsQuality.detailNoiseUV);
		cloudsMat.SetFloat("_AmbientSkyColorIntensity", EnviroSky.instance.cloudsSettings.ambientLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
		cloudsMat.SetVector("_CloudsLighting", new Vector4(EnviroSky.instance.cloudsConfig.scatteringCoef, EnviroSky.instance.cloudsSettings.hgPhase, EnviroSky.instance.cloudsSettings.silverLiningIntensity, EnviroSky.instance.cloudsSettings.silverLiningSpread));
		float z = (EnviroSky.instance.cloudsSettings.tonemapping ? 0f : 1f);
		if (!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor)
		{
			z = 0f;
		}
		cloudsMat.SetVector("_CloudsLightingExtended", new Vector4(EnviroSky.instance.cloudsConfig.edgeDarkness, EnviroSky.instance.cloudsConfig.ambientSkyColorIntensity, z, EnviroSky.instance.cloudsSettings.cloudsExposure));
		float num = usedCloudsQuality.bottomCloudHeight + EnviroSky.instance.cloudsSettings.cloudsHeightMod;
		float num2 = usedCloudsQuality.topCloudHeight + EnviroSky.instance.cloudsSettings.cloudsHeightMod;
		if (myCam.transform.position.y < num - 250f)
		{
			cloudsMat.SetVector("_CloudsParameter", new Vector4(num, num2, num2 - num, EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10f));
		}
		else
		{
			cloudsMat.SetVector("_CloudsParameter", new Vector4(myCam.transform.position.y + 250f, num2 + (myCam.transform.position.y + 250f - num), num2 + (myCam.transform.position.y + 250f - num) - (myCam.transform.position.y + 250f), EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10f));
		}
		cloudsMat.SetVector("_CloudDensityScale", new Vector4(EnviroSky.instance.cloudsConfig.density, EnviroSky.instance.cloudsConfig.densityLightning, 0f, 0f));
		cloudsMat.SetFloat("_CloudsType", EnviroSky.instance.cloudsConfig.cloudType);
		cloudsMat.SetVector("_CloudsCoverageSettings", new Vector4(EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage, EnviroSky.instance.cloudsConfig.coverageModBottom, EnviroSky.instance.cloudsConfig.coverageModTop, 0f));
		cloudsMat.SetVector("_CloudsAnimation", new Vector4(EnviroSky.instance.cloudAnim.x, EnviroSky.instance.cloudAnim.y, EnviroSky.instance.cloudsSettings.cloudsWindDirectionX, EnviroSky.instance.cloudsSettings.cloudsWindDirectionY));
		cloudsMat.SetColor("_LightColor", EnviroSky.instance.cloudsSettings.volumeCloudsColor.Evaluate(EnviroSky.instance.GameTime.solarTime));
		cloudsMat.SetColor("_MoonLightColor", EnviroSky.instance.cloudsSettings.volumeCloudsMoonColor.Evaluate(EnviroSky.instance.GameTime.lunarTime));
		cloudsMat.SetFloat("_stepsInDepth", usedCloudsQuality.stepsInDepthModificator);
		cloudsMat.SetFloat("_LODDistance", usedCloudsQuality.lodDistance);
		if (EnviroSky.instance.lightSettings.directionalLightMode == EnviroLightSettings.LightingMode.Dual)
		{
			if (EnviroSky.instance.GameTime.dayNightSwitch < EnviroSky.instance.GameTime.solarTime)
			{
				cloudsMat.SetVector("_LightDir", -EnviroSky.instance.Components.DirectLight.transform.forward);
			}
			else if (EnviroSky.instance.Components.AdditionalDirectLight != null)
			{
				cloudsMat.SetVector("_LightDir", -EnviroSky.instance.Components.AdditionalDirectLight.transform.forward);
			}
		}
		else
		{
			cloudsMat.SetVector("_LightDir", -EnviroSky.instance.Components.DirectLight.transform.forward);
		}
		cloudsMat.SetFloat("_LightIntensity", EnviroSky.instance.cloudsSettings.lightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
		cloudsMat.SetVector("_CloudsErosionIntensity", new Vector4(1f - EnviroSky.instance.cloudsConfig.baseErosionIntensity, EnviroSky.instance.cloudsConfig.detailErosionIntensity, 0f, EnviroSky.instance.cloudAnim.z));
		cloudsMat.SetTexture("_BlueNoise", blueNoise);
		cloudsMat.SetVector("_Randomness", new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
	}

	private void SetBlitmaterialProperties()
	{
		Matrix4x4 inverse = projection.inverse;
		blitMat.SetMatrix("_PreviousRotation", previousRotation);
		blitMat.SetMatrix("_Projection", projection);
		blitMat.SetMatrix("_InverseRotation", inverseRotation);
		blitMat.SetMatrix("_InverseProjection", inverse);
		if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
		{
			Matrix4x4 inverse2 = projectionSPVR.inverse;
			blitMat.SetMatrix("_PreviousRotationSPVR", previousRotationSPVR);
			blitMat.SetMatrix("_ProjectionSPVR", projectionSPVR);
			blitMat.SetMatrix("_InverseRotationSPVR", inverseRotationSPVR);
			blitMat.SetMatrix("_InverseProjectionSPVR", inverse2);
		}
		blitMat.SetFloat("_FrameNumber", subFrameNumber);
		blitMat.SetFloat("_ReprojectionPixelSize", reprojectionPixelSize);
		blitMat.SetVector("_SubFrameDimension", new Vector2(subFrameWidth, subFrameHeight));
		blitMat.SetVector("_FrameDimension", new Vector2(frameWidth, frameHeight));
	}

	private RenderTexture DownsampleDepth(int X, int Y, Texture src, Material mat, int downsampleFactor)
	{
		Vector2 vector = new Vector2(1f / (float)X, 1f / (float)X);
		X /= downsampleFactor;
		Y /= downsampleFactor;
		RenderTexture temporary = RenderTexture.GetTemporary(X, Y, 0);
		mat.SetVector("_PixelSize", vector);
		Graphics.Blit(src, temporary, mat);
		return temporary;
	}

	private void RenderClouds(RenderTexture source, RenderTexture tex)
	{
		if (cloudsMat == null)
		{
			cloudsMat = new Material(Shader.Find("Enviro/Standard/RaymarchClouds"));
		}
		cloudsMat.SetTexture("_MainTex", source);
		SetCloudProperties();
		Graphics.Blit(source, tex, cloudsMat);
	}

	private void CreateCloudsRenderTextures(RenderTexture source)
	{
		if (subFrameTex != null)
		{
			UnityEngine.Object.DestroyImmediate(subFrameTex);
			subFrameTex = null;
		}
		if (prevFrameTex != null)
		{
			UnityEngine.Object.DestroyImmediate(prevFrameTex);
			prevFrameTex = null;
		}
		RenderTextureFormat colorFormat = (myCam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		if (subFrameTex == null)
		{
			RenderTextureDescriptor desc = new RenderTextureDescriptor(subFrameWidth, subFrameHeight, colorFormat, 0);
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				desc.vrUsage = VRTextureUsage.TwoEyes;
			}
			subFrameTex = new RenderTexture(desc);
			subFrameTex.filterMode = FilterMode.Bilinear;
			subFrameTex.hideFlags = HideFlags.HideAndDontSave;
			isFirstFrame = true;
		}
		if (prevFrameTex == null)
		{
			RenderTextureDescriptor desc2 = new RenderTextureDescriptor(frameWidth, frameHeight, colorFormat, 0);
			if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
			{
				desc2.vrUsage = VRTextureUsage.TwoEyes;
			}
			prevFrameTex = new RenderTexture(desc2);
			prevFrameTex.filterMode = FilterMode.Bilinear;
			prevFrameTex.hideFlags = HideFlags.HideAndDontSave;
			isFirstFrame = true;
		}
	}

	private void SetReprojectionPixelSize(EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize pSize)
	{
		switch (pSize)
		{
		case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Off:
			reprojectionPixelSize = 1;
			break;
		case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Low:
			reprojectionPixelSize = 2;
			break;
		case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Medium:
			reprojectionPixelSize = 4;
			break;
		case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.High:
			reprojectionPixelSize = 8;
			break;
		}
		frameList = CalculateFrames(reprojectionPixelSize);
	}

	private void StartFrame()
	{
		textureDimensionChanged = UpdateFrameDimensions();
		switch (myCam.stereoActiveEye)
		{
		case Camera.MonoOrStereoscopicEye.Mono:
			projection = myCam.projectionMatrix;
			rotation = myCam.worldToCameraMatrix;
			inverseRotation = myCam.cameraToWorldMatrix;
			break;
		case Camera.MonoOrStereoscopicEye.Left:
			projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			inverseRotation = rotation.inverse;
			if (EnviroSky.instance.singlePassVR)
			{
				projectionSPVR = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
				rotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
				inverseRotationSPVR = rotationSPVR.inverse;
			}
			break;
		case Camera.MonoOrStereoscopicEye.Right:
			projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			inverseRotation = rotation.inverse;
			break;
		}
	}

	private void FinalizeFrame()
	{
		renderingCounter++;
		previousRotation = rotation;
		if (EnviroSky.instance.singlePassVR)
		{
			previousRotationSPVR = rotationSPVR;
		}
		int num = reprojectionPixelSize * reprojectionPixelSize;
		subFrameNumber = frameList[renderingCounter % num];
	}

	private bool UpdateFrameDimensions()
	{
		int i = myCam.pixelWidth / EnviroSky.instance.cloudsSettings.cloudsQualitySettings.cloudsRenderResolution;
		int j = myCam.pixelHeight / EnviroSky.instance.cloudsSettings.cloudsQualitySettings.cloudsRenderResolution;
		if (EnviroSky.instance != null && reprojectionPixelSize == 0)
		{
			SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.cloudsQualitySettings.reprojectionPixelSize);
		}
		for (; i % reprojectionPixelSize != 0; i++)
		{
		}
		for (; j % reprojectionPixelSize != 0; j++)
		{
		}
		int num = i / reprojectionPixelSize;
		int num2 = j / reprojectionPixelSize;
		if (i != frameWidth || num != subFrameWidth || j != frameHeight || num2 != subFrameHeight)
		{
			frameWidth = i;
			frameHeight = j;
			subFrameWidth = num;
			subFrameHeight = num2;
			return true;
		}
		frameWidth = i;
		frameHeight = j;
		subFrameWidth = num;
		subFrameHeight = num2;
		return false;
	}

	private int[] CalculateFrames(int reproSize)
	{
		subFrameNumber = 0;
		int num = 0;
		int num2 = reproSize * reproSize;
		int[] array = new int[num2];
		for (num = 0; num < num2; num++)
		{
			array[num] = num;
		}
		while (num-- > 0)
		{
			int num3 = array[num];
			int num4 = (int)((float)UnityEngine.Random.Range(0, 1) * 1000f) % num2;
			array[num] = array[num4];
			array[num4] = num3;
		}
		return array;
	}
}
