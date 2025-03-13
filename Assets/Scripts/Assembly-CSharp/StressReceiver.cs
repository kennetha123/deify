using UnityEngine;

public class StressReceiver : MonoBehaviour
{
	private float _trauma;

	private Vector3 _lastPosition;

	private Vector3 _lastRotation;

	[Tooltip("Exponent for calculating the shake factor. Useful for creating different effect fade outs")]
	public float TraumaExponent = 1f;

	[Tooltip("Maximum angle that the gameobject can shake. In euler angles.")]
	public Vector3 MaximumAngularShake = Vector3.one * 5f;

	[Tooltip("Maximum translation that the gameobject can receive when applying the shake effect.")]
	public Vector3 MaximumTranslationShake = Vector3.one * 0.75f;

	private void Update()
	{
		float num = Mathf.Pow(_trauma, TraumaExponent);
		if (num > 0f)
		{
			Vector3 lastRotation = _lastRotation;
			Vector3 lastPosition = _lastPosition;
			_lastPosition = new Vector3(MaximumTranslationShake.x * (Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f), MaximumTranslationShake.y * (Mathf.PerlinNoise(1f, Time.time * 25f) * 2f - 1f), MaximumTranslationShake.z * (Mathf.PerlinNoise(2f, Time.time * 25f) * 2f - 1f)) * num;
			_lastRotation = new Vector3(MaximumAngularShake.x * (Mathf.PerlinNoise(3f, Time.time * 25f) * 2f - 1f), MaximumAngularShake.y * (Mathf.PerlinNoise(4f, Time.time * 25f) * 2f - 1f), MaximumAngularShake.z * (Mathf.PerlinNoise(5f, Time.time * 25f) * 2f - 1f)) * num;
			base.transform.localPosition += _lastPosition - lastPosition;
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles + _lastRotation - lastRotation);
			_trauma = Mathf.Clamp01(_trauma - Time.deltaTime);
		}
		else if (!(_lastPosition == Vector3.zero) || !(_lastRotation == Vector3.zero))
		{
			base.transform.localPosition -= _lastPosition;
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles - _lastRotation);
			_lastPosition = Vector3.zero;
			_lastRotation = Vector3.zero;
		}
	}

	public void InduceStress(float Stress)
	{
		_trauma = Mathf.Clamp01(_trauma + Stress);
	}
}
