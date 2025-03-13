using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class EnviroSkyRenderingLW : MonoBehaviour
{
	[HideInInspector]
	public bool isAddionalCamera;

	private Camera myCam;

	[HideInInspector]
	public Material material;

	[HideInInspector]
	public bool simpleFog;

	private bool currentSimpleFog;

	[HideInInspector]
	public bool volumeLighting = true;

	[HideInInspector]
	public bool dirVolumeLighting = true;

	[HideInInspector]
	public bool distanceFog = true;

	[HideInInspector]
	public bool useRadialDistance;

	[HideInInspector]
	public bool heightFog = true;

	[HideInInspector]
	public float height = 1f;

	[Range(0.001f, 10f)]
	[HideInInspector]
	public float heightDensity = 2f;

	[HideInInspector]
	public float startDistance;

	private void Start()
	{
		if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.LW)
		{
			base.enabled = false;
			return;
		}
		myCam = GetComponent<Camera>();
		if (myCam.actualRenderingPath == RenderingPath.Forward)
		{
			myCam.depthTextureMode = DepthTextureMode.Depth;
		}
	}

	private void OnEnable()
	{
		CreateFogMaterial();
	}

	private void OnDisable()
	{
		DestroyFogMaterial();
	}

	private void CreateFogMaterial()
	{
		if (material != null)
		{
			UnityEngine.Object.DestroyImmediate(material);
		}
		if (!simpleFog)
		{
			Shader shader = Shader.Find("Enviro/Lite/EnviroFogRendering");
			if (shader == null)
			{
				throw new Exception("Critical Error: \"Enviro/EnviroFogRendering\" shader is missing.");
			}
			material = new Material(shader);
		}
		else
		{
			Shader shader2 = Shader.Find("Enviro/Lite/EnviroFogRenderingSimple");
			if (shader2 == null)
			{
				throw new Exception("Critical Error: \"Enviro/EnviroFogRendering\" shader is missing.");
			}
			material = new Material(shader2);
		}
		if (EnviroSkyMgr.instance.FogSettings.useSimpleFog)
		{
			Shader.EnableKeyword("ENVIRO_SIMPLE_FOG");
		}
		else
		{
			Shader.DisableKeyword("ENVIRO_SIMPLE_FOG");
		}
	}

	private void DestroyFogMaterial()
	{
		if (material != null)
		{
			UnityEngine.Object.Destroy(material);
		}
	}

	private void Update()
	{
		if (currentSimpleFog != simpleFog)
		{
			CreateFogMaterial();
			currentSimpleFog = simpleFog;
		}
	}

	private void OnPreRender()
	{
		if (myCam.stereoEnabled)
		{
			Matrix4x4 inverse = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
			Matrix4x4 inverse2 = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
			Matrix4x4 stereoProjectionMatrix = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoProjectionMatrix2 = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			Matrix4x4 inverse3 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix, true).inverse;
			Matrix4x4 inverse4 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix2, true).inverse;
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
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
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
			{
				inverse5[1, 1] *= -1f;
			}
			Shader.SetGlobalMatrix("_LeftWorldFromView", cameraToWorldMatrix);
			Shader.SetGlobalMatrix("_LeftViewFromScreen", inverse5);
		}
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (EnviroSkyLite.instance == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (myCam.actualRenderingPath == RenderingPath.Forward)
		{
			myCam.depthTextureMode |= DepthTextureMode.Depth;
		}
		float num = myCam.transform.position.y - height;
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
		Shader.SetGlobalVector("_SceneFogParams", value);
		Shader.SetGlobalVector("_SceneFogMode", new Vector4((float)fogMode, useRadialDistance ? 1 : 0, 0f, 0f));
		Shader.SetGlobalVector("_HeightParams", new Vector4(height, num, z, heightDensity * 0.5f));
		Shader.SetGlobalVector("_DistanceParams", new Vector4(0f - Mathf.Max(startDistance, 0f), 0f, 0f, 0f));
		material.SetTexture("_MainTex", source);
		Graphics.Blit(source, destination, material);
	}
}
