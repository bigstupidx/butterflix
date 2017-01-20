using UnityEngine;

public class ParticleKill : MonoBehaviour
{
	private ParticleSystem _particleSystem;
	private float _lifetime = 3.0f;
	private float _startTime;

	//=====================================================

	void OnEnable()
	{
		_particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
		if( _particleSystem == null ) return;

		_lifetime = _particleSystem.duration * 2.0f;
		_startTime = Time.time;
	}

	//=====================================================

	void Update()
	{
		if( Time.time - _startTime > _lifetime )
		{
			// Destroy this particle
			Destroy( gameObject );
		}
	}

	//=====================================================
}
