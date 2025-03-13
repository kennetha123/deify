using System.Collections;
using UnityEngine;

public class EnviroLightning : MonoBehaviour
{
	public void Lightning()
	{
		StartCoroutine(LightningBolt());
	}

	public void StopLightning()
	{
		StopAllCoroutines();
		GetComponent<Light>().enabled = false;
		EnviroSkyMgr.instance.SetLightningFlashTrigger(0f);
	}

	public IEnumerator LightningBolt()
	{
		GetComponent<Light>().enabled = true;
		float defaultIntensity = GetComponent<Light>().intensity;
		int flashCount = Random.Range(2, 5);
		for (int thisFlash = 0; thisFlash < flashCount; thisFlash++)
		{
			GetComponent<Light>().intensity = defaultIntensity * Random.Range(1f, 1.5f);
			EnviroSkyMgr.instance.SetLightningFlashTrigger(Random.Range(5f, 10f));
			yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
			GetComponent<Light>().intensity = defaultIntensity;
			EnviroSkyMgr.instance.SetLightningFlashTrigger(1f);
		}
		GetComponent<Light>().enabled = false;
	}
}
