using System.Collections;
using UnityEngine;

public class TraumaInducer : MonoBehaviour
{
	[Tooltip("Seconds to wait before trigerring the explosion particles and the trauma effect")]
	public float Delay = 1f;

	[Tooltip("Maximum stress the effect can inflict upon objects Range([0,1])")]
	public float MaximumStress = 0.6f;

	[Tooltip("Maximum distance in which objects are affected by this TraumaInducer")]
	public float Range = 45f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(Delay);
		PlayParticles();
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			StressReceiver component = array[i].GetComponent<StressReceiver>();
			if (!(component == null))
			{
				float num = Vector3.Distance(base.transform.position, array[i].transform.position);
				if (!(num > Range))
				{
					float f = Mathf.Clamp01(num / Range);
					float stress = (1f - Mathf.Pow(f, 2f)) * MaximumStress;
					component.InduceStress(stress);
				}
			}
		}
	}

	private void PlayParticles()
	{
		ParticleSystem[] componentsInChildren = base.transform.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component != null)
		{
			component.Play();
		}
	}
}
