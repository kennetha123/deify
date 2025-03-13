using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	public class vAINoiseManager : MonoBehaviour
	{
		private static vAINoiseManager _instance;

		public List<vNoise> noises;

		public static vAINoiseManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Object.FindObjectOfType<vAINoiseManager>();
				}
				if (_instance == null)
				{
					_instance = new GameObject("AI Noise Manager").AddComponent<vAINoiseManager>();
					_instance.noises = new List<vNoise>();
				}
				return _instance;
			}
		}

		public void AddNoise(vNoise noise)
		{
			if (noises == null)
			{
				noises = new List<vNoise>();
			}
			if (noises.Contains(noise))
			{
				noises[noises.IndexOf(noise)].AddDuration(noise.duration);
			}
			else
			{
				noise.onFinishNoise.AddListener(RemoveNoise);
				noises.Add(noise);
			}
			if (!noise.isPlaying)
			{
				StartCoroutine(noise.Play());
			}
		}

		public void RemoveNoise(vNoise noise)
		{
			if (noises == null)
			{
				noises = new List<vNoise>();
			}
			if (noises.Contains(noise))
			{
				noises.Remove(noise);
			}
		}
	}
}
