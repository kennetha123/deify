using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class EnviroPostProcessing : MonoBehaviour
{
	public enum SunShaftsResolution
	{
		Low = 0,
		Normal = 1,
		High = 2
	}

	public enum ShaftsScreenBlendMode
	{
		Screen = 0,
		Add = 1
	}

	private Camera cam;

	[HideInInspector]
	public int radialBlurIterations = 2;

	private Material sunShaftsMaterial;

	private Material moonShaftsMaterial;

	private Material simpleSunClearMaterial;

	private Material simpleMoonClearMaterial;

	private void OnEnable()
	{
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		CreateMaterialsAndTextures();
	}

	private void OnDisable()
	{
		CleanupMaterials();
	}

	private void CreateMaterialsAndTextures()
	{
		sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
		moonShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
		simpleSunClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
		simpleMoonClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
	}

	private void CleanupMaterials()
	{
		if (sunShaftsMaterial != null)
		{
			Object.DestroyImmediate(sunShaftsMaterial);
		}
		if (moonShaftsMaterial != null)
		{
			Object.DestroyImmediate(moonShaftsMaterial);
		}
		if (simpleSunClearMaterial != null)
		{
			Object.DestroyImmediate(simpleSunClearMaterial);
		}
		if (simpleMoonClearMaterial != null)
		{
			Object.DestroyImmediate(simpleMoonClearMaterial);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (EnviroSkyMgr.instance == null || !EnviroSkyMgr.instance.IsAvailable())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (cam.actualRenderingPath == RenderingPath.Forward)
		{
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
		RenderTextureDescriptor desc = new RenderTextureDescriptor(source.width, source.height, source.format, source.depth);
		desc.msaaSamples = source.antiAliasing;
		RenderTexture temporary = RenderTexture.GetTemporary(desc);
		if (sunShaftsMaterial == null)
		{
			CleanupMaterials();
			CreateMaterialsAndTextures();
		}
		if (EnviroSkyMgr.instance.useSunShafts && EnviroSkyMgr.instance.useMoonShafts)
		{
			RenderLightShaft(source, temporary, sunShaftsMaterial, simpleSunClearMaterial, EnviroSkyMgr.instance.Components.Sun.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
			RenderLightShaft(temporary, destination, moonShaftsMaterial, simpleMoonClearMaterial, EnviroSkyMgr.instance.Components.Moon.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
		}
		else if (EnviroSkyMgr.instance.useSunShafts)
		{
			RenderLightShaft(source, destination, sunShaftsMaterial, simpleSunClearMaterial, EnviroSkyMgr.instance.Components.Sun.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
		}
		else if (EnviroSkyMgr.instance.useMoonShafts)
		{
			RenderLightShaft(source, destination, moonShaftsMaterial, simpleMoonClearMaterial, EnviroSkyMgr.instance.Components.Moon.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
		}
		else
		{
			Graphics.Blit(source, destination);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void RenderLightShaft(RenderTexture source, RenderTexture destination, Material mat, Material clearMat, Transform lightSource, Color treshold, Color clr)
	{
		int num = 4;
		if (EnviroSkyMgr.instance.LightShaftsSettings.resolution == SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if (EnviroSkyMgr.instance.LightShaftsSettings.resolution == SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = Vector3.one * 0.5f;
		vector = ((!lightSource) ? new Vector3(0.5f, 0.5f, 0f) : cam.WorldToViewportPoint(lightSource.position));
		int width = source.width / num;
		int height = source.height / num;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
		mat.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * EnviroSkyMgr.instance.LightShaftsSettings.blurRadius);
		mat.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, EnviroSkyMgr.instance.LightShaftsSettings.maxRadius));
		mat.SetVector("_SunThreshold", treshold);
		if (!EnviroSkyMgr.instance.LightShaftsSettings.useDepthTexture)
		{
			RenderTextureFormat format = (EnviroSkyMgr.instance.Camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
			RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(source.width, source.height, 0, format));
			GL.ClearWithSkybox(false, cam);
			mat.SetTexture("_Skybox", renderTexture);
			Graphics.Blit(source, temporary, mat, 3);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		else
		{
			Graphics.Blit(source, temporary, mat, 2);
		}
		if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
		{
			DrawBorder(temporary, clearMat);
		}
		radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);
		float num2 = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * 0.0013020834f;
		mat.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
		mat.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, EnviroSkyMgr.instance.LightShaftsSettings.maxRadius));
		for (int i = 0; i < radialBlurIterations; i++)
		{
			RenderTexture temporary3 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
			Graphics.Blit(temporary, temporary3, mat, 1);
			RenderTexture.ReleaseTemporary(temporary);
			num2 = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			mat.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
			temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
			Graphics.Blit(temporary3, temporary, mat, 1);
			RenderTexture.ReleaseTemporary(temporary3);
			num2 = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			mat.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			mat.SetVector("_SunColor", new Vector4(clr.r, clr.g, clr.b, clr.a) * EnviroSkyMgr.instance.LightShaftsSettings.intensity);
		}
		else
		{
			mat.SetVector("_SunColor", Vector4.zero);
		}
		mat.SetTexture("_ColorBuffer", temporary);
		Graphics.Blit(source, destination, mat, (EnviroSkyMgr.instance.LightShaftsSettings.screenBlendMode != 0) ? 4 : 0);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void DrawBorder(RenderTexture dest, Material material)
	{
		RenderTexture.active = dest;
		bool flag = true;
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < material.passCount; i++)
		{
			material.SetPass(i);
			float y;
			float y2;
			if (flag)
			{
				y = 1f;
				y2 = 0f;
			}
			else
			{
				y = 0f;
				y2 = 1f;
			}
			float x = 0f + 1f / ((float)dest.width * 1f);
			float y3 = 0f;
			float y4 = 1f;
			GL.Begin(7);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			float x2 = 1f - 1f / ((float)dest.width * 1f);
			x = 1f;
			y3 = 0f;
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			x = 1f;
			y3 = 0f;
			y4 = 0f + 1f / ((float)dest.height * 1f);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			x = 1f;
			y3 = 1f - 1f / ((float)dest.height * 1f);
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			GL.End();
		}
		GL.PopMatrix();
	}
}
