using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	[DisallowMultipleComponent]
	[vClassHeader("AI Noise Listener", true, "icon_v2", false, "")]
	public class vAINoiseListener : vMonoBehaviour, vIAIComponent
	{
		[vHelpBox("The noise has a radius effect and the noise volume decreases depending on the distance, 'Listener Power'  will applify the distance of the noise to listener", vHelpBoxAttribute.MessageType.None)]
		[Range(0f, 10f)]
		public float listenerPower = 1f;

		public bool debugMode;

		public List<string> ignoreNoiseType;

		public Type ComponentType
		{
			get
			{
				return typeof(vAINoiseListener);
			}
		}

		protected virtual List<vNoise> noises
		{
			get
			{
				return vAINoiseManager.Instance.noises;
			}
		}

		public vNoise lastListenedNoise { get; protected set; }

		protected virtual bool IsInListenerPower(vNoise noise)
		{
			return NoiseVolume(noise) > 0f;
		}

		protected virtual List<vNoise> NoisesCanBeListened()
		{
			return noises.FindAll((vNoise n) => !ignoreNoiseType.Contains(n.noiseType) && IsInListenerPower(n));
		}

		protected virtual List<vNoise> SortByDistance()
		{
			List<vNoise> list = NoisesCanBeListened();
			if (list.Count > 1)
			{
				list.Sort((vNoise noiseA, vNoise noiseB) => Vector3.Distance(base.transform.position, noiseA.position).CompareTo(Vector3.Distance(base.transform.position, noiseB.position)));
			}
			if (list.Count > 0)
			{
				lastListenedNoise = list[0];
			}
			return list;
		}

		protected virtual List<vNoise> SortNoisesTypeByDistance(string noiseType)
		{
			List<vNoise> list = NoisesCanBeListened().FindAll((vNoise n) => n.noiseType.Equals(noiseType));
			if (list.Count > 1)
			{
				list.Sort((vNoise noiseA, vNoise noiseB) => Vector3.Distance(base.transform.position, noiseA.position).CompareTo(Vector3.Distance(base.transform.position, noiseB.position)));
			}
			if (list.Count > 0)
			{
				lastListenedNoise = list[0];
			}
			return list;
		}

		protected virtual List<vNoise> SortNoisesTypesByDistance(List<string> noiseTypes)
		{
			List<vNoise> list = NoisesCanBeListened().FindAll((vNoise n) => noiseTypes.Contains(n.noiseType));
			if (list.Count > 1)
			{
				list.Sort((vNoise noiseA, vNoise noiseB) => Vector3.Distance(base.transform.position, noiseA.position).CompareTo(Vector3.Distance(base.transform.position, noiseB.position)));
			}
			if (list.Count > 0)
			{
				lastListenedNoise = list[0];
			}
			return list;
		}

		public float NoiseVolume(vNoise noise)
		{
			float num = 0f;
			if (listenerPower > 0f)
			{
				float num2 = noise.minDistance * listenerPower;
				float num3 = noise.maxDistance * listenerPower;
				float num4 = Vector3.Distance(noise.position, base.transform.position) - num2;
				num = 1f - num4 / ((num2 == num3) ? num3 : ((num2 > num3) ? (num2 - num3) : (num3 - num2)));
			}
			return noise.volume * num;
		}

		public virtual bool IsListeningNoise()
		{
			return SortByDistance().Count > 0;
		}

		public virtual bool IsListeningSpecificNoises(List<string> noiseTypes)
		{
			return SortNoisesTypesByDistance(noiseTypes).Count > 0;
		}

		public virtual vNoise GetNearNoise()
		{
			if (SortByDistance().Count > 0)
			{
				return noises[0];
			}
			return null;
		}

		public virtual vNoise GetNearNoiseByType(string noiseType)
		{
			if (SortNoisesTypeByDistance(noiseType).Count > 0)
			{
				return noises[0];
			}
			return null;
		}

		public virtual vNoise GetNearNoiseByTypes(List<string> noiseTypes)
		{
			if (SortNoisesTypesByDistance(noiseTypes).Count > 0)
			{
				return noises[0];
			}
			return null;
		}

		public virtual List<vNoise> GetNoiseByTypes(List<string> noiseTypes)
		{
			if (SortNoisesTypesByDistance(noiseTypes).Count > 0)
			{
				return noises;
			}
			return null;
		}
	}
}
