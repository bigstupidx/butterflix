using System.Collections;
using FxProNS;
using UnityEngine;

[RequireComponent( typeof( Camera ) )]
[RequireComponent( typeof( FxPro ) )]
public class CameraPostFx : MonoBehaviourEMS
{
	private enum eWildMagicState { BAD = -1, NEUTRAL = 0, GOOD = 1, NULL }

	public EffectsQuality DefaultQuality = EffectsQuality.Fast;

	private FxPro _fxPro;
	private eWildMagicState _currentState;
	private eWildMagicState _previousState;
	private float _transitionSpeed;

	//=====================================================

	void Awake()
	{
		_fxPro = transform.GetComponent<FxPro>();
		_previousState = _currentState = eWildMagicState.NULL;
	}

	//=====================================================

	void OnEnable()
	{
		GameDataManager.Instance.PlayerWildMagicEvent += OnPlayerWildMagicEvent;

		_fxPro.Quality = DefaultQuality;
		_transitionSpeed = 1.5f;

		ChangeState( _currentState );
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == false )
			GameDataManager.Instance.PlayerWildMagicEvent -= OnPlayerWildMagicEvent;
	}

	//=====================================================

	void Update()
	{
		// ToDo: DEBUG - REMOVE THIS
		//if( Input.GetKeyDown( KeyCode.K ) )
		//	GameDataManager.Instance.AddWildMagicRate( 10 );
		//else if( Input.GetKeyDown( KeyCode.J ) )
		//	GameDataManager.Instance.AddWildMagicRate( -10 );
	}

	//=====================================================

	private void OnPlayerWildMagicEvent( float maxWildMagic, float currentWildMagic )
	{
		// Wild Magic rage [-100 ... 100]
		if( currentWildMagic > 33.3f )
			ChangeState( eWildMagicState.GOOD );
		else if( currentWildMagic < -33.3f )
			ChangeState( eWildMagicState.BAD );
		else
			ChangeState( eWildMagicState.NEUTRAL );
	}

	//=====================================================

	private void ChangeState( eWildMagicState state )
	{
		if( _currentState == state ) return;

		//Debug.Log( state );
		_fxPro.FogStrength = 0.0f;
		_fxPro.FilmGrainIntensity = 0.0f;

		switch( state )
		{
			case eWildMagicState.BAD:
				_fxPro.BloomEnabled = false;
				_fxPro.ColorEffectsEnabled = true;

				StartCoroutine( TansistionTo( false, 0.0f, true, 1.0f, true, 0.5f, true, 0.5f ) );
				break;

			case eWildMagicState.NEUTRAL:
				_fxPro.BloomEnabled = true;
				_fxPro.ColorEffectsEnabled = true;

				StartCoroutine( TansistionTo( true, 0.99f, true, 0.5f, true, 0.25f, true, 0.25f ) );
				break;

			case eWildMagicState.GOOD:
				_fxPro.BloomEnabled = true;
				_fxPro.ColorEffectsEnabled = true;

				StartCoroutine( TansistionTo( true, 0.9f, true, 0.35f, true, 0.0f, true, 0.0f ) );
				break;
		}

		_previousState = _currentState;
		_currentState = state;

		_fxPro.Init( false );
	}

	//=====================================================

	private IEnumerator TansistionTo( bool transBloom, float bloomTarget, bool transVignette, float vignetteTarget,
									  bool transCloseTint, float closeTintTarget, bool transFarTint, float farTintTarget)
	{
		var bloomStartAt = _fxPro.BloomParams.BloomThreshold;
		var vignetteStartAt = _fxPro.VignettingIntensity;
		var closeTintStartAt = _fxPro.CloseTintStrength;
		var farTintStartAt = _fxPro.FarTintStrength;

		var counter = 0;
		var lerpBy = 0.0f;

		do
		{
			if( transBloom == true )
				_fxPro.BloomParams.BloomThreshold = Mathf.Lerp( bloomStartAt, bloomTarget, lerpBy );
			if( transVignette == true )
				_fxPro.VignettingIntensity = Mathf.Lerp( vignetteStartAt, vignetteTarget, lerpBy );
			if( transCloseTint == true )
				_fxPro.CloseTintStrength = Mathf.Lerp( closeTintStartAt, closeTintTarget, lerpBy );
			if( transFarTint == true )
				_fxPro.FarTintStrength = Mathf.Lerp( farTintStartAt, farTintTarget, lerpBy );

			lerpBy += Time.deltaTime * _transitionSpeed;

			yield return null;

			// Stagger render updates?
			if( counter++ < 2 ) continue;

			_fxPro.Init( false );
			counter = 0;
		} while( lerpBy < 1.0f );

		if( transBloom == true )
			_fxPro.BloomParams.BloomThreshold = bloomTarget;
		if( transVignette == true )
			_fxPro.VignettingIntensity = vignetteTarget;
		if( transCloseTint == true )
			_fxPro.CloseTintStrength = closeTintTarget;
		if( transFarTint == true )
			_fxPro.FarTintStrength = farTintTarget;

		_fxPro.Init( false );
	}

	//=====================================================
}
