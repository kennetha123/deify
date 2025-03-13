using UnityEngine;

namespace Aura2API
{
	public class LightFlicker : MonoBehaviour
	{
		public float maxFactor = 1.2f;

		public float minFactor = 1f;

		public float moveRange = 0.1f;

		public float speed = 0.1f;

		private float _currentFactor = 1f;

		private Vector3 _currentPos;

		private float _deltaTime;

		private Vector3 _initPos;

		private float _targetFactor;

		private Vector3 _targetPos;

		private float _initialFactor;

		private float _time;

		private float _timeLeft;

		private void Start()
		{
			Random.InitState((int)base.transform.position.x + (int)base.transform.position.y);
			_initialFactor = GetComponent<Light>().intensity;
		}

		private void OnEnable()
		{
			_initPos = base.transform.localPosition;
			_currentPos = _initPos;
		}

		private void OnDisable()
		{
			base.transform.localPosition = _initPos;
		}

		private void Update()
		{
			_deltaTime = Time.deltaTime;
			if (_timeLeft <= _deltaTime)
			{
				_targetFactor = Random.Range(minFactor, maxFactor);
				_targetPos = _initPos + Random.insideUnitSphere * moveRange;
				_timeLeft = speed;
				return;
			}
			float t = _deltaTime / _timeLeft;
			_currentFactor = Mathf.Lerp(_currentFactor, _targetFactor, t);
			GetComponent<Light>().intensity = _initialFactor * _currentFactor;
			_currentPos = Vector3.Lerp(_currentPos, _targetPos, t);
			base.transform.localPosition = _currentPos;
			_timeLeft -= _deltaTime;
		}
	}
}
