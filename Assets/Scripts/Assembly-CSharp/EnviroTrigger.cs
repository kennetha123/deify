using UnityEngine;

public class EnviroTrigger : MonoBehaviour
{
	public EnviroInterior myZone;

	public string Name;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider col)
	{
		if (EnviroSkyMgr.instance.GetUseWeatherTag())
		{
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag())
			{
				EnterExit();
			}
		}
		else if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject))
		{
			EnterExit();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (myZone.zoneTriggerType != EnviroInterior.ZoneTriggerType.Zone)
		{
			return;
		}
		if (EnviroSkyMgr.instance.GetUseWeatherTag())
		{
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag())
			{
				EnterExit();
			}
		}
		else if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject))
		{
			EnterExit();
		}
	}

	private void EnterExit()
	{
		if (EnviroSkyMgr.instance.lastInteriorZone != myZone)
		{
			if (EnviroSkyMgr.instance.lastInteriorZone != null)
			{
				EnviroSkyMgr.instance.lastInteriorZone.StopAllFading();
			}
			myZone.Enter();
		}
		else if (!EnviroSkyMgr.instance.IsInterior())
		{
			myZone.Enter();
		}
		else
		{
			myZone.Exit();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.5f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}
}
