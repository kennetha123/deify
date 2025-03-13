using System;
using UnityEngine;
using UnityEngine.Events;

public class EnviroEvents : MonoBehaviour
{
	[Serializable]
	public class EnviroActionEvent : UnityEvent
	{
	}

	public EnviroActionEvent onHourPassedActions = new EnviroActionEvent();

	public EnviroActionEvent onDayPassedActions = new EnviroActionEvent();

	public EnviroActionEvent onYearPassedActions = new EnviroActionEvent();

	public EnviroActionEvent onWeatherChangedActions = new EnviroActionEvent();

	public EnviroActionEvent onSeasonChangedActions = new EnviroActionEvent();

	public EnviroActionEvent onNightActions = new EnviroActionEvent();

	public EnviroActionEvent onDayActions = new EnviroActionEvent();

	public EnviroActionEvent onZoneChangedActions = new EnviroActionEvent();

	private void Start()
	{
		EnviroSkyMgr.instance.OnHourPassed += delegate
		{
			HourPassed();
		};
		EnviroSkyMgr.instance.OnDayPassed += delegate
		{
			DayPassed();
		};
		EnviroSkyMgr.instance.OnYearPassed += delegate
		{
			YearPassed();
		};
		EnviroSkyMgr.instance.OnWeatherChanged += delegate
		{
			WeatherChanged();
		};
		EnviroSkyMgr.instance.OnSeasonChanged += delegate
		{
			SeasonsChanged();
		};
		EnviroSkyMgr.instance.OnNightTime += delegate
		{
			NightTime();
		};
		EnviroSkyMgr.instance.OnDayTime += delegate
		{
			DayTime();
		};
		EnviroSkyMgr.instance.OnZoneChanged += delegate
		{
			ZoneChanged();
		};
	}

	private void HourPassed()
	{
		onHourPassedActions.Invoke();
	}

	private void DayPassed()
	{
		onDayPassedActions.Invoke();
	}

	private void YearPassed()
	{
		onYearPassedActions.Invoke();
	}

	private void WeatherChanged()
	{
		onWeatherChangedActions.Invoke();
	}

	private void SeasonsChanged()
	{
		onSeasonChangedActions.Invoke();
	}

	private void NightTime()
	{
		onNightActions.Invoke();
	}

	private void DayTime()
	{
		onDayActions.Invoke();
	}

	private void ZoneChanged()
	{
		onZoneChangedActions.Invoke();
	}
}
