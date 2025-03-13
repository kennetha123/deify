using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Invector
{
	public class vAudioSurface : ScriptableObject
	{
		public AudioSource audioSource;

		public AudioMixerGroup audioMixerGroup;

		public List<string> TextureOrMaterialNames;

		public List<AudioClip> audioClips;

		public GameObject particleObject;

		private vFisherYatesRandom randomSource = new vFisherYatesRandom();

		public bool useStepMark;

		[vHideInInspector("useStepMark", false)]
		public GameObject stepMark;

		[vHideInInspector("useStepMark", false)]
		public LayerMask stepLayer;

		[vHideInInspector("useStepMark", false)]
		public float timeToDestroy = 5f;

		public vAudioSurface()
		{
			audioClips = new List<AudioClip>();
			TextureOrMaterialNames = new List<string>();
		}

		public void PlayRandomClip(FootStepObject footStepObject, bool spawnParticle = true, bool spawnStepMark = true, float volume = 1f)
		{
			if (audioClips != null && audioClips.Count != 0)
			{
				if (randomSource == null)
				{
					randomSource = new vFisherYatesRandom();
				}
				GameObject gameObject = null;
				if (audioSource != null)
				{
					gameObject = Object.Instantiate(audioSource.gameObject, footStepObject.sender.position, Quaternion.identity);
				}
				else
				{
					gameObject = new GameObject("audioObject");
					gameObject.transform.position = footStepObject.sender.position;
				}
				vAudioSurfaceControl vAudioSurfaceControl2 = gameObject.AddComponent<vAudioSurfaceControl>();
				if (audioMixerGroup != null)
				{
					vAudioSurfaceControl2.outputAudioMixerGroup = audioMixerGroup;
				}
				int index = randomSource.Next(audioClips.Count);
				if (spawnParticle && (bool)particleObject && (bool)footStepObject.ground && stepLayer.ContainsLayer(footStepObject.ground.gameObject.layer))
				{
					Object.Instantiate(particleObject, footStepObject.sender.position, footStepObject.sender.rotation).transform.SetParent(vObjectContainer.root, true);
				}
				if (spawnStepMark && useStepMark)
				{
					StepMark(footStepObject);
				}
				vAudioSurfaceControl2.PlayOneShot(audioClips[index], volume);
			}
		}

		private void StepMark(FootStepObject footStep)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(footStep.sender.transform.position + new Vector3(0f, 0.1f, 0f), -footStep.sender.up, out hitInfo, 1f, stepLayer) && (bool)stepMark)
			{
				Quaternion quaternion = Quaternion.FromToRotation(footStep.sender.up, hitInfo.normal);
				GameObject gameObject = Object.Instantiate(stepMark, hitInfo.point, quaternion * footStep.sender.rotation);
				gameObject.transform.SetParent(vObjectContainer.root, true);
				Object.Destroy(gameObject, timeToDestroy);
			}
		}
	}
}
