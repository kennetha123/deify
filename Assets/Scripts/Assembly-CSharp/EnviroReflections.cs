using UnityEngine;

public class EnviroReflections : MonoBehaviour
{
	public ReflectionProbe probe;

	public float ReflectionUpdateInGameHours = 1f;

	private double lastUpdate;

	private void Start()
	{
		if (probe == null)
		{
			probe = GetComponent<ReflectionProbe>();
		}
	}

	private void UpdateProbe()
	{
		probe.RenderProbe();
		lastUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours();
	}

	private void Update()
	{
		if ((!(EnviroSkyMgr.instance != null) || EnviroSkyMgr.instance.IsAvailable()) && (EnviroSkyMgr.instance.GetCurrentTimeInHours() > lastUpdate + (double)ReflectionUpdateInGameHours || EnviroSkyMgr.instance.GetCurrentTimeInHours() < lastUpdate - (double)ReflectionUpdateInGameHours))
		{
			UpdateProbe();
		}
	}
}
