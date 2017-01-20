using System;
using UnityEngine;
using System.Collections;

public class GridBox : MonoBehaviour, ITargetWithinRange
{
	private enum eState { DISABLED = 0, INCOMING, PROJECTILE, ATTACK }

	private Transform _thisTransform;
	private GameObject _trigger;
	[SerializeField]
	private GameObject _incomingFx;
	[SerializeField]
	private GameObject[] _projectileFxArray;
	[SerializeField]
	private GameObject[] _attackFxArray;

	private eState _currentState;
	private eBossAttack _currentBossAttack;
	private int _numAttacks;
	private bool _isPlayerInRange;

	//=====================================================

	public void OnAttack( eBossAttack attack, int numAttacks = 1 )
	{
		if( _currentState != eState.DISABLED ) return;

		// Set alternate attacks
		_currentBossAttack = attack;

		_numAttacks = numAttacks;

		ChangeState( eState.INCOMING );
	}

	//=====================================================

	public void OnTargetWithinRange( Transform target, bool isPlayer = false )
	{
		_isPlayerInRange = true;
	}

	//=====================================================

	public void OnTargetLost()
	{
		_isPlayerInRange = false;
	}

	//=====================================================

	public void OnPause()
	{
		OnDeath();
	}

	//=====================================================

	public void OnDeath()
	{
		// Stop attack
		_numAttacks = 0;
		_isPlayerInRange = false;

		_currentState = eState.DISABLED;
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_trigger = _thisTransform.FindChild( "Trigger" ).gameObject;

		// Inject this gridBox as parent
		_trigger.GetComponent<TriggerGridBox>().Init( this );

		_currentState = eState.DISABLED;
		_incomingFx.SetActive( false );
		_currentBossAttack = eBossAttack.NULL;
		_numAttacks = 0;
		_isPlayerInRange = false;
	}

	//=====================================================

	//private void Update()
	//{
	//	if( Input.GetKeyDown( KeyCode.Q ) )
	//		OnAttack( eBossAttack.ATTACK_01, 3 );
	//}

	//=====================================================

	private void ChangeState( eState state )
	{
		float stateDuration;

		switch( state )
		{
			case eState.DISABLED:
				//if( _trigger == null ) break;

				// Deactivate trigger and particle fx
				_trigger.SetActive( false );
				_incomingFx.SetActive( false );

				foreach( var particle in _projectileFxArray )
					particle.SetActive( false );

				foreach( var particle in _attackFxArray )
					particle.SetActive( false );

				// Reset variables
				_currentBossAttack = eBossAttack.NULL;
				_numAttacks = 0;
				_isPlayerInRange = false;
				break;

			case eState.INCOMING:
				//if( _trigger == null ) ChangeState( eState.DISABLED ); break;

				// Activate trigger
				_trigger.SetActive( true );

				// Consume number of attacks
				_numAttacks = Mathf.Clamp( _numAttacks - 1, 0, 10 );

				// Introduce short random delay
				var startDelay = UnityEngine.Random.Range( 0.0f, 0.3f );

				// Get particle duration with small reduction to trigger next state early
				stateDuration = Mathf.Clamp( _incomingFx.GetComponent<ParticleSystem>().duration - 0.5f, 0.0f, 10.0f );

				// Start incoming particle fx
				_incomingFx.SetActive( true );

				// Set state duration
				StartCoroutine( StartInterval( startDelay, stateDuration, () => { ChangeState( eState.PROJECTILE ); } ) );
				break;

			case eState.PROJECTILE:
				// Get particle duration and start projectile fx
				stateDuration = ActivateIncomingFx( _currentBossAttack );

				// Set state duration
				StartCoroutine( StartInterval( 0.0f, stateDuration, () => { ChangeState( eState.ATTACK ); } ) );
				break;

			case eState.ATTACK:
				// Start projectile fx
				stateDuration = ActivateAttackFx( _currentBossAttack );

				// Set state duration
				StartCoroutine( StartInterval( 0.0f, stateDuration, () =>
																{
																	if( _numAttacks > 0 )
																	{
																		// Disable particles before next attack
																		_incomingFx.SetActive( false );

																		foreach( var particle in _projectileFxArray )
																			particle.SetActive( false );

																		foreach( var particle in _attackFxArray )
																			particle.SetActive( false );

																		ChangeState( eState.INCOMING );
																	}
																	else
																	{
																		ChangeState( eState.DISABLED );
																	}
																} ) );
				// Attack player
				if( _isPlayerInRange )
					PlayerManager.OnObstacleHit( eDamageType.LOW, Vector3.zero );
				break;
		}

		_currentState = state;
	}

	//=====================================================

	private float ActivateIncomingFx( eBossAttack attack )
	{
		if( _projectileFxArray.Length < ((int)attack + 1) ) return 0.0f;

		var particle = _projectileFxArray[(int)attack];

		if( particle == null ) return 0.0f;

		particle.SetActive( true );

		// Return particle duration with small reduction to trigger next state early
		return Mathf.Clamp( particle.GetComponent<ParticleSystem>().duration - 0.1f, 0.0f, 10.0f );
	}

	//=====================================================

	private float ActivateAttackFx( eBossAttack attack )
	{
		if( _attackFxArray.Length < ((int)attack + 1) ) return 0.0f;

		var particle = _attackFxArray[(int)attack];

		if( particle == null ) return 0.0f;

		particle.SetActive( true );

		// Return particle duration
		return particle.GetComponent<ParticleSystem>().duration;
	}

	//=====================================================

	private static IEnumerator StartInterval( float startDelay, float interval, Action onComplete )
	{
		yield return new WaitForSeconds( startDelay + interval );

		if( onComplete != null )
			onComplete();
	}

	//=====================================================
}
