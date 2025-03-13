using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[AddComponentMenu("Enviro/Volume Light")]
[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class EnviroVolumeLight : MonoBehaviour
{
	private Light _light;

	public Material _material;

	public Shader volumeLightShader;

	public Shader volumeLightBlurShader;

	private CommandBuffer _commandBuffer;

	private CommandBuffer _cascadeShadowCommandBuffer;

	public RenderTexture temp;

	[Range(1f, 64f)]
	public int SampleCount = 8;

	public bool scaleWithTime = true;

	[Range(0f, 1f)]
	public float ScatteringCoef = 0.5f;

	[Range(0f, 0.1f)]
	public float ExtinctionCoef = 0.01f;

	[Range(0f, 0.999f)]
	public float Anistropy = 0.1f;

	[Header("3D Noise")]
	public bool Noise;

	[HideInInspector]
	public float NoiseScale = 0.015f;

	[HideInInspector]
	public float NoiseIntensity = 1f;

	[HideInInspector]
	public float NoiseIntensityOffset = 0.3f;

	[HideInInspector]
	public Vector2 NoiseVelocity = new Vector2(3f, 3f);

	private bool _reversedZ;

	public Light Light
	{
		get
		{
			return _light;
		}
	}

	public Material VolumetricMaterial
	{
		get
		{
			return _material;
		}
	}

	public event Action<EnviroSkyRendering, EnviroVolumeLight, CommandBuffer, Matrix4x4> CustomRenderEvent;

	private void Start()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStation4 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan || SystemInfo.graphicsDeviceType == GraphicsDeviceType.XboxOne)
		{
			_reversedZ = true;
		}
	}

	private void OnEnable()
	{
		if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.LW)
		{
			base.enabled = false;
			return;
		}
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "Light Command Buffer";
		_cascadeShadowCommandBuffer = new CommandBuffer();
		_cascadeShadowCommandBuffer.name = "Dir Light Command Buffer";
		_cascadeShadowCommandBuffer.SetGlobalTexture("_CascadeShadowMapTexture", new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
		_light = GetComponent<Light>();
		if (_light.type == LightType.Directional)
		{
			_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
			_light.AddCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
		}
		else
		{
			_light.AddCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
		}
		if (volumeLightShader == null)
		{
			volumeLightShader = Shader.Find("Enviro/Standard/VolumeLight");
		}
		if (volumeLightShader == null)
		{
			throw new Exception("Critical Error: \"Enviro/VolumeLight\" shader is missing.");
		}
		if (_light.type != LightType.Directional)
		{
			_material = new Material(volumeLightShader);
		}
		EnviroSkyRendering.PreRenderEvent += VolumetricLightRenderer_PreRenderEvent;
	}

	private void OnDisable()
	{
		if (_light != null && _commandBuffer != null)
		{
			if (_light.type == LightType.Directional)
			{
				_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
				_light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
			}
			else
			{
				_light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
			}
		}
		EnviroSkyRendering.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
	}

	public void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(_material);
	}

	private void VolumetricLightRenderer_PreRenderEvent(EnviroSkyRendering renderer, Matrix4x4 viewProj, Matrix4x4 viewProjSP)
	{
		if (EnviroSky.instance == null)
		{
			return;
		}
		if (_light == null || _light.gameObject == null)
		{
			EnviroSkyRendering.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
		}
		else if (_light.gameObject.activeInHierarchy && _light.enabled)
		{
			if (_material == null)
			{
				_material = new Material(volumeLightShader);
			}
			_material.SetVector("_CameraForward", Camera.current.transform.forward);
			_material.SetInt("_SampleCount", SampleCount);
			_material.SetVector("_NoiseVelocity", new Vector4(EnviroSky.instance.volumeLightSettings.noiseVelocity.x, EnviroSky.instance.volumeLightSettings.noiseVelocity.y) * EnviroSky.instance.volumeLightSettings.noiseScale);
			_material.SetVector("_NoiseData", new Vector4(EnviroSky.instance.volumeLightSettings.noiseScale, EnviroSky.instance.volumeLightSettings.noiseIntensity, EnviroSky.instance.volumeLightSettings.noiseIntensityOffset));
			_material.SetVector("_MieG", new Vector4(1f - Anistropy * Anistropy, 1f + Anistropy * Anistropy, 2f * Anistropy, 1f / (4f * (float)Math.PI)));
			float x = ScatteringCoef;
			if (scaleWithTime)
			{
				x = ScatteringCoef * (1f - EnviroSky.instance.GameTime.solarTime);
			}
			_material.SetVector("_VolumetricLight", new Vector4(x, ExtinctionCoef, _light.range, 1f));
			_material.SetTexture("_CameraDepthTexture", renderer.GetVolumeLightDepthBuffer());
			_material.SetFloat("_ZTest", 8f);
			if (_light.type == LightType.Point)
			{
				SetupPointLight(renderer, viewProj, viewProjSP);
			}
			else if (_light.type == LightType.Spot)
			{
				SetupSpotLight(renderer, viewProj, viewProjSP);
			}
		}
	}

	private void SetupPointLight(EnviroSkyRendering renderer, Matrix4x4 viewProj, Matrix4x4 viewProjSP)
	{
		_commandBuffer.Clear();
		int num = 0;
		if (!IsCameraInPointLightBounds())
		{
			num = 2;
		}
		_material.SetPass(num);
		Mesh pointLightMesh = EnviroSkyRendering.GetPointLightMesh();
		float num2 = _light.range * 2f;
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, _light.transform.rotation, new Vector3(num2, num2, num2));
		_material.SetMatrix("_WorldViewProj", viewProj * matrix4x);
		_material.SetMatrix("_WorldViewProj_SP", viewProjSP * matrix4x);
		if (Noise)
		{
			_material.EnableKeyword("NOISE");
		}
		else
		{
			_material.DisableKeyword("NOISE");
		}
		_material.SetVector("_LightPos", new Vector4(_light.transform.position.x, _light.transform.position.y, _light.transform.position.z, 1f / (_light.range * _light.range)));
		_material.SetColor("_LightColor", _light.color * _light.intensity);
		if (_light.cookie == null)
		{
			_material.EnableKeyword("POINT");
			_material.DisableKeyword("POINT_COOKIE");
		}
		else
		{
			Matrix4x4 inverse = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;
			_material.SetMatrix("_MyLightMatrix0", inverse);
			_material.EnableKeyword("POINT_COOKIE");
			_material.DisableKeyword("POINT");
			_material.SetTexture("_LightTexture0", _light.cookie);
		}
		bool flag = false;
		if ((_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).magnitude >= QualitySettings.shadowDistance)
		{
			flag = true;
		}
		if (_light.shadows != 0 && !flag)
		{
			if (XRSettings.enabled)
			{
				if (EnviroSky.instance.singlePassVR)
				{
					_material.EnableKeyword("SHADOWS_CUBE");
					_commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
					_commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
					_commandBuffer.DrawMesh(pointLightMesh, matrix4x, _material, 0, num);
					if (this.CustomRenderEvent != null)
					{
						this.CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
					}
				}
				else
				{
					_material.DisableKeyword("SHADOWS_CUBE");
					renderer.GlobalCommandBuffer.DrawMesh(pointLightMesh, matrix4x, _material, 0, num);
					if (this.CustomRenderEvent != null)
					{
						this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
					}
				}
			}
			else
			{
				_material.EnableKeyword("SHADOWS_CUBE");
				_commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
				_commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
				_commandBuffer.DrawMesh(pointLightMesh, matrix4x, _material, 0, num);
				if (this.CustomRenderEvent != null)
				{
					this.CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
				}
			}
			return;
		}
		_material.DisableKeyword("SHADOWS_DEPTH");
		if (EnviroSky.instance.PlayerCamera.actualRenderingPath == RenderingPath.Forward)
		{
			renderer.GlobalCommandBufferForward.SetRenderTarget(renderer.GetVolumeLightBuffer());
			renderer.GlobalCommandBufferForward.DrawMesh(pointLightMesh, matrix4x, _material, 0, num);
			if (this.CustomRenderEvent != null)
			{
				this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBufferForward, viewProj);
			}
		}
		else
		{
			renderer.GlobalCommandBuffer.DrawMesh(pointLightMesh, matrix4x, _material, 0, num);
			if (this.CustomRenderEvent != null)
			{
				this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
			}
		}
	}

	private void SetupSpotLight(EnviroSkyRendering renderer, Matrix4x4 viewProj, Matrix4x4 viewProjSP)
	{
		_commandBuffer.Clear();
		int shaderPass = 1;
		if (!IsCameraInSpotLightBounds())
		{
			shaderPass = 3;
		}
		Mesh spotLightMesh = EnviroSkyRendering.GetSpotLightMesh();
		float range = _light.range;
		float num = Mathf.Tan((_light.spotAngle + 1f) * 0.5f * ((float)Math.PI / 180f)) * _light.range;
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(num, num, range));
		Matrix4x4 inverse = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(-0.5f, -0.5f, 1f));
		Matrix4x4 matrix4x3 = Matrix4x4.Perspective(_light.spotAngle, 1f, 0f, 1f);
		_material.SetMatrix("_MyLightMatrix0", matrix4x2 * matrix4x3 * inverse);
		_material.SetMatrix("_WorldViewProj", viewProj * matrix4x);
		_material.SetMatrix("_WorldViewProj_SP", viewProjSP * matrix4x);
		_material.SetVector("_LightPos", new Vector4(_light.transform.position.x, _light.transform.position.y, _light.transform.position.z, 1f / (_light.range * _light.range)));
		_material.SetVector("_LightColor", _light.color * _light.intensity);
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		float value = 0f - Vector3.Dot(position + forward * _light.range, forward);
		_material.SetFloat("_PlaneD", value);
		_material.SetFloat("_CosAngle", Mathf.Cos((_light.spotAngle + 1f) * 0.5f * ((float)Math.PI / 180f)));
		_material.SetVector("_ConeApex", new Vector4(position.x, position.y, position.z));
		_material.SetVector("_ConeAxis", new Vector4(forward.x, forward.y, forward.z));
		_material.EnableKeyword("SPOT");
		if (Noise)
		{
			_material.EnableKeyword("NOISE");
		}
		else
		{
			_material.DisableKeyword("NOISE");
		}
		if (_light.cookie == null)
		{
			_material.SetTexture("_LightTexture0", EnviroSkyRendering.GetDefaultSpotCookie());
		}
		else
		{
			_material.SetTexture("_LightTexture0", _light.cookie);
		}
		bool flag = false;
		if ((_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).magnitude >= QualitySettings.shadowDistance)
		{
			flag = true;
		}
		if (_light.shadows != 0 && !flag)
		{
			if (XRSettings.enabled)
			{
				if (EnviroSky.instance.singlePassVR)
				{
					matrix4x2 = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
					matrix4x3 = ((!_reversedZ) ? Matrix4x4.Perspective(_light.spotAngle, 1f, _light.shadowNearPlane, _light.range) : Matrix4x4.Perspective(_light.spotAngle, 1f, _light.range, _light.shadowNearPlane));
					Matrix4x4 matrix4x4 = matrix4x2 * matrix4x3;
					matrix4x4[0, 2] *= -1f;
					matrix4x4[1, 2] *= -1f;
					matrix4x4[2, 2] *= -1f;
					matrix4x4[3, 2] *= -1f;
					_material.SetMatrix("_MyWorld2Shadow", matrix4x4 * inverse);
					_material.SetMatrix("_WorldView", matrix4x4 * inverse);
					_material.EnableKeyword("SHADOWS_DEPTH");
					_commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
					_commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
					_commandBuffer.DrawMesh(spotLightMesh, matrix4x, _material, 0, shaderPass);
					if (this.CustomRenderEvent != null)
					{
						this.CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
					}
				}
				else
				{
					_material.DisableKeyword("SHADOWS_DEPTH");
					renderer.GlobalCommandBuffer.DrawMesh(spotLightMesh, matrix4x, _material, 0, shaderPass);
					if (this.CustomRenderEvent != null)
					{
						this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
					}
				}
			}
			else
			{
				matrix4x2 = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
				matrix4x3 = ((!_reversedZ) ? Matrix4x4.Perspective(_light.spotAngle, 1f, _light.shadowNearPlane, _light.range) : Matrix4x4.Perspective(_light.spotAngle, 1f, _light.range, _light.shadowNearPlane));
				Matrix4x4 matrix4x5 = matrix4x2 * matrix4x3;
				matrix4x5[0, 2] *= -1f;
				matrix4x5[1, 2] *= -1f;
				matrix4x5[2, 2] *= -1f;
				matrix4x5[3, 2] *= -1f;
				_material.SetMatrix("_MyWorld2Shadow", matrix4x5 * inverse);
				_material.SetMatrix("_WorldView", matrix4x5 * inverse);
				_material.EnableKeyword("SHADOWS_DEPTH");
				_commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
				_commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
				_commandBuffer.DrawMesh(spotLightMesh, matrix4x, _material, 0, shaderPass);
				if (this.CustomRenderEvent != null)
				{
					this.CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
				}
			}
			return;
		}
		_material.DisableKeyword("SHADOWS_DEPTH");
		if (EnviroSky.instance.PlayerCamera.actualRenderingPath == RenderingPath.Forward)
		{
			renderer.GlobalCommandBufferForward.SetRenderTarget(renderer.GetVolumeLightBuffer());
			renderer.GlobalCommandBufferForward.DrawMesh(spotLightMesh, matrix4x, _material, 0, shaderPass);
			if (this.CustomRenderEvent != null)
			{
				this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBufferForward, viewProj);
			}
		}
		else
		{
			renderer.GlobalCommandBuffer.DrawMesh(spotLightMesh, matrix4x, _material, 0, shaderPass);
			if (this.CustomRenderEvent != null)
			{
				this.CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
			}
		}
	}

	private bool IsCameraInPointLightBounds()
	{
		float sqrMagnitude = (_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).sqrMagnitude;
		float num = _light.range + 1f;
		if (sqrMagnitude < num * num)
		{
			return true;
		}
		return false;
	}

	private bool IsCameraInSpotLightBounds()
	{
		float num = Vector3.Dot(_light.transform.forward, Camera.current.transform.position - _light.transform.position);
		float num2 = _light.range + 1f;
		if (num > num2)
		{
			return false;
		}
		if (Mathf.Acos(Vector3.Dot(base.transform.forward, (Camera.current.transform.position - _light.transform.position).normalized)) * 57.29578f > (_light.spotAngle + 3f) * 0.5f)
		{
			return false;
		}
		return true;
	}
}
