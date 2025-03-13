using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnviroAudioSettings
{
	[Tooltip("A list of all possible thunder audio effects.")]
	public List<AudioClip> ThunderSFX = new List<AudioClip>();
}
