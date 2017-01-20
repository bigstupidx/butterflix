using UnityEngine;
using System.Collections;

public class CrumblingPlatform : MonoBehaviourEMS
{
	[Range( 0.0f, 5.0f )]
	[SerializeField] private float _delay = 0.0f;

	private Transform _thisTransform;
	private GameObject _solidPlatform;
	private GameObject _animatedPlatform;
	private bool _isActivated;

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_solidPlatform = _thisTransform.FindChild( "SolidPlatform" ).gameObject;
		_animatedPlatform = _thisTransform.FindChild( "AnimatedPlatform" ).gameObject;
		_isActivated = false;
	}

	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.PlayerDeathEvent += OnPlayerDeathEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		GameManager.Instance.PlayerDeathEvent -= OnPlayerDeathEvent;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( _isActivated == true || other.tag != UnityTags.Player ) return;

		_isActivated = true;

		StartCoroutine( CrumblePlatform( _delay ) );
	}

	//=====================================================

	private void OnPlayerDeathEvent( bool isRespawningInScene )
	{
		if( isRespawningInScene == true )
			Reset();
	}

	//=====================================================

	private void Reset()
	{
		_solidPlatform.SetActive( true );
		_animatedPlatform.SetActive( false );
		_isActivated = false;
	}

	//=====================================================

	private IEnumerator CrumblePlatform( float delay )
	{
		yield return new WaitForSeconds( delay );

		_animatedPlatform.SetActive( true );
		_solidPlatform.SetActive( false );
	}

	//=====================================================
}