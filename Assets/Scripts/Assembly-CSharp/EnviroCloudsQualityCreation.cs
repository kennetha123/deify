using System.Collections.Generic;
using UnityEngine;

public static class EnviroCloudsQualityCreation
{
	public static GameObject GetAssetPrefab(string name)
	{
		return null;
	}

	public static AudioClip GetAudioClip(string name)
	{
		return null;
	}

	public static Cubemap GetAssetCubemap(string name)
	{
		return null;
	}

	public static EnviroProfile GetDefaultProfile(string name)
	{
		return null;
	}

	public static Texture GetAssetTexture(string name)
	{
		return null;
	}

	public static Gradient CreateGradient(Color clr1, float time1, Color clr2, float time2)
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[2];
		GradientAlphaKey[] array2 = new GradientAlphaKey[2];
		array[0].color = clr1;
		array[0].time = time1;
		array[1].color = clr2;
		array[1].time = time2;
		array2[0].alpha = 1f;
		array2[0].time = 0f;
		array2[1].alpha = 1f;
		array2[1].time = 1f;
		gradient.SetKeys(array, array2);
		return gradient;
	}

	public static Gradient CreateGradient(List<Color> clrs, List<float> times)
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[clrs.Count];
		GradientAlphaKey[] array2 = new GradientAlphaKey[2];
		for (int i = 0; i < clrs.Count; i++)
		{
			array[i].color = clrs[i];
			array[i].time = times[i];
		}
		array2[0].alpha = 1f;
		array2[0].time = 0f;
		array2[1].alpha = 1f;
		array2[1].time = 1f;
		gradient.SetKeys(array, array2);
		return gradient;
	}

	public static Gradient CreateGradient(List<Color> clrs, List<float> times, List<float> alpha, List<float> timesAlpha)
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[clrs.Count];
		GradientAlphaKey[] array2 = new GradientAlphaKey[alpha.Count];
		for (int i = 0; i < clrs.Count; i++)
		{
			array[i].color = clrs[i];
			array[i].time = times[i];
		}
		for (int j = 0; j < alpha.Count; j++)
		{
			array2[j].alpha = alpha[j];
			array2[j].time = timesAlpha[j];
		}
		gradient.SetKeys(array, array2);
		return gradient;
	}

	public static Color GetColor(string hex)
	{
		Color color = default(Color);
		ColorUtility.TryParseHtmlString(hex, out color);
		return color;
	}

	public static Keyframe CreateKey(float value, float time)
	{
		Keyframe result = default(Keyframe);
		result.value = value;
		result.time = time;
		return result;
	}

	public static Keyframe CreateKey(float value, float time, float inTangent, float outTangent)
	{
		Keyframe result = default(Keyframe);
		result.value = value;
		result.time = time;
		result.inTangent = inTangent;
		result.outTangent = outTangent;
		return result;
	}
}
