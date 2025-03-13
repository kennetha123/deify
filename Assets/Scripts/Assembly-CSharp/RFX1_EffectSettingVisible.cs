using System.Collections.Generic;
using UnityEngine;

public class RFX1_EffectSettingVisible : MonoBehaviour
{
	public bool IsActive = true;

	public float FadeOutTime = 3f;

	private bool previousActiveStatus;

	private bool needUpdate;

	private bool needLastUpdate;

	private Dictionary<string, float> startAlphaColors;

	private string[] colorProperties = new string[9] { "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor", "_MainColor", "_CoreColor", "_FresnelColor" };

	private float alpha;

	private float prevAlpha;

	private void OnEnable()
	{
		alpha = 1f;
		prevAlpha = 1f;
		IsActive = true;
	}

	private void Update()
	{
		if (!IsActive && startAlphaColors == null)
		{
			InitStartAlphaColors();
		}
		if (IsActive && alpha < 1f)
		{
			alpha += Time.deltaTime / FadeOutTime;
		}
		if (!IsActive && alpha > 0f)
		{
			alpha -= Time.deltaTime / FadeOutTime;
		}
		if (alpha > 0f && alpha < 1f)
		{
			needUpdate = true;
		}
		else
		{
			needUpdate = false;
			alpha = Mathf.Clamp01(alpha);
			if (Mathf.Abs(prevAlpha - alpha) >= Mathf.Epsilon)
			{
				UpdateVisibleStatus();
			}
		}
		prevAlpha = alpha;
		if (needUpdate)
		{
			UpdateVisibleStatus();
		}
	}

	private void InitStartAlphaColors()
	{
		startAlphaColors = new Dictionary<string, float>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				GetStartAlphaByProperties(renderer.GetHashCode().ToString(), j, materials[j]);
			}
		}
		Light[] componentsInChildren2 = GetComponentsInChildren<Light>(true);
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			startAlphaColors.Add(componentsInChildren2[k].GetHashCode().ToString() + k, componentsInChildren2[k].intensity);
		}
		Projector[] componentsInChildren3 = GetComponentsInChildren<Projector>();
		foreach (Projector projector in componentsInChildren3)
		{
			Material material = projector.material;
			GetStartAlphaByProperties(projector.GetHashCode().ToString(), 0, material);
		}
		AudioSource[] componentsInChildren4 = GetComponentsInChildren<AudioSource>(true);
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			startAlphaColors.Add(componentsInChildren4[l].GetHashCode().ToString() + l, componentsInChildren4[l].volume);
		}
	}

	private void UpdateVisibleStatus()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(true);
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				UpdateAlphaByProperties(renderer.GetHashCode().ToString(), j, materials[j], alpha);
			}
		}
		RFX1_LightCurves[] componentsInChildren2 = GetComponentsInChildren<RFX1_LightCurves>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = IsActive;
		}
		Light[] componentsInChildren3 = GetComponentsInChildren<Light>(true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			float num = startAlphaColors[componentsInChildren3[k].GetHashCode().ToString() + k];
			componentsInChildren3[k].intensity = alpha * num;
		}
		ParticleSystem[] componentsInChildren4 = GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem particleSystem in componentsInChildren4)
		{
			if (!IsActive && !particleSystem.isStopped)
			{
				particleSystem.Stop();
			}
			if (IsActive && particleSystem.isStopped)
			{
				particleSystem.Play();
			}
		}
		Projector[] componentsInChildren5 = GetComponentsInChildren<Projector>();
		foreach (Projector projector in componentsInChildren5)
		{
			Material material = projector.material;
			UpdateAlphaByProperties(projector.GetHashCode().ToString(), 0, material, alpha);
		}
		AudioSource[] componentsInChildren6 = GetComponentsInChildren<AudioSource>(true);
		for (int l = 0; l < componentsInChildren6.Length; l++)
		{
			float num2 = startAlphaColors[componentsInChildren6[l].GetHashCode().ToString() + l];
			componentsInChildren6[l].volume = alpha * num2;
		}
	}

	private void UpdateAlphaByProperties(string rendName, int materialNumber, Material mat, float alpha)
	{
		string[] array = colorProperties;
		foreach (string text in array)
		{
			if (mat.HasProperty(text))
			{
				float num = startAlphaColors[rendName + materialNumber + text.ToString()];
				Color color = mat.GetColor(text);
				color.a = alpha * num;
				mat.SetColor(text, color);
			}
		}
	}

	private void GetStartAlphaByProperties(string rendName, int materialNumber, Material mat)
	{
		string[] array = colorProperties;
		foreach (string text in array)
		{
			if (mat.HasProperty(text))
			{
				startAlphaColors.Add(rendName + materialNumber + text.ToString(), mat.GetColor(text).a);
			}
		}
	}
}
