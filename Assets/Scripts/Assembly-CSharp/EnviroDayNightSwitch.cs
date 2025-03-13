using UnityEngine;

public class EnviroDayNightSwitch : MonoBehaviour
{
	private Light[] lightsArray;

	private void Start()
	{
		lightsArray = GetComponentsInChildren<Light>();
		EnviroSkyMgr.instance.OnDayTime += delegate
		{
			Deactivate();
		};
		EnviroSkyMgr.instance.OnNightTime += delegate
		{
			Activate();
		};
		if (EnviroSkyMgr.instance.IsNight())
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	private void Activate()
	{
		for (int i = 0; i < lightsArray.Length; i++)
		{
			lightsArray[i].enabled = true;
		}
	}

	private void Deactivate()
	{
		for (int i = 0; i < lightsArray.Length; i++)
		{
			lightsArray[i].enabled = false;
		}
	}
}
