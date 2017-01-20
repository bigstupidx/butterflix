using UnityEngine;

public class BossTorch : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	//=====================================================

	public void EnableTorch()
	{
		if( _particleSystem != null )
			_particleSystem.enableEmission = true;
	}

	//=====================================================

	public bool IsEnabled()
	{
		return _particleSystem != null && _particleSystem.enableEmission == true;	// _particleSystem.isPlaying;
	}

	//=====================================================

	void Awake()
	{
		_particleSystem = transform.GetComponentInChildren<ParticleSystem>();
		
		if( _particleSystem != null )
			_particleSystem.enableEmission = false;
	}

	//=====================================================
}
