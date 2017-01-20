using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof( Animator ) )]
public class BossShield : MonoBehaviour
{
	public enum eShieldState { INIT, DISABLED, ENABLED, DAMAGED, DESTROYED }

	public event Action ShieldDamagedEvent;
	public event Action ShieldDestroyedEvent;

	private Transform _thisTransform;
	private Animator _animator;
	private Collider _collider;
	private GameObject _psDamagedFx;
	private GameObject _psExplodeFx;
	//private Renderer _debugStateRenderer;

	[SerializeField]
	private Material _shieldMaterial;

	private eShieldState _currentState;
	private eShieldState _previousState;
	private int _health;
	private int _healthMax;

	private List<Job> _currentJobs;
	private Job _disabling;
	private Job _takingDamage;

	//=====================================================

	#region Public Interface

	public eShieldState CurrentState
	{
		get { return _currentState; }
		set
		{
			ExitState( _currentState );
			_previousState = _currentState;
			_currentState = value;
			EnterState( _currentState );
		}
	}

	//=====================================================

	public void OnActivate()
	{
		if( _currentState != eShieldState.DISABLED ) return;

		CurrentState = eShieldState.ENABLED;
	}

	//=====================================================

	public void OnActivate( Vector3 postion, Quaternion rotation )
	{
		_thisTransform.position = postion;
		_thisTransform.rotation = rotation;

		OnActivate();
	}

	//=====================================================

	public void OnHit( eDamageType damage )
	{
		if( _currentState != eShieldState.ENABLED ) return;

		_health = Mathf.Clamp( _health - Convert.ToInt32( SettingsManager.GetSettingsItem( "DAMAGE", (int)damage ) ), 0, 999999 );

		//Debug.Log( "Shield health: " + _health );

		if( _health > 0.0f )
		{
			if( ShieldDamagedEvent != null )
				ShieldDamagedEvent();

			CurrentState = eShieldState.DAMAGED;
		}
		else
		{
			CurrentState = eShieldState.DESTROYED;
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	#region Unity Calls

	void Awake()
	{
		_thisTransform = this.transform;
		_animator = _thisTransform.GetComponent<Animator>();
		_collider = _thisTransform.GetComponent<Collider>();
		//_psDamagedFx = _thisTransform.FindChild( "psDeathFx" ).gameObject;
		//_psExplodeFx = _thisTransform.FindChild( "psExplodeFx" ).gameObject;
		//_debugStateRenderer = _thisTransform.FindChild( "DebugState" ).GetComponent<Renderer>();

		_currentJobs = new List<Job>();

		// Set defaults
		CurrentState = eShieldState.INIT;
	}

	//=====================================================

	void OnEnable()
	{
		var shield = _thisTransform.FindChild( "AnimControl" ).FindChild( "Shield" );
		_shieldMaterial = shield.GetComponent<Renderer>().material;
	}

	#endregion

	//=====================================================

	private void SetHealth()
	{
		// Allow for boss levels e.g. 1, 2, 3 ...
		var bossLevel = 1;
		if( GameManager.Instance != null &&
			GameDataManager.Instance != null &&
			GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
			bossLevel = GameDataManager.Instance.PlayerBossLevel;

		_health = Convert.ToInt32( SettingsManager.GetSettingsItem( "BOSS_SHIELD_STRENGTH", bossLevel ) );
		_healthMax = _health;
	}

	//=====================================================

	private void SetShieldColour()
	{
		// Determine current shield percentage
		float health;

		if( Double.IsNaN( _health ) || Double.IsNaN( _healthMax ) )
			health = 100;
		else
			health = ((float)_health / _healthMax) * 100;

		if( health > 66 )
		{
			_shieldMaterial.SetFloat( "_Bias", 0.75f );
			_shieldMaterial.SetFloat( "_Noise", 0.2f );
			_shieldMaterial.SetColor( "_Color", new Color( 20.0f / 255, 160.0f / 255, 130.0f / 255, 1.0f ) );
		}
		else if( health > 33 )
		{
			_shieldMaterial.SetFloat( "_Bias", 0.65f );
			_shieldMaterial.SetFloat( "_Noise", 0.3f );
			_shieldMaterial.SetColor( "_Color", new Color( 20.0f / 255, 80.0f / 255, 180.0f / 255, 1.0f ) );
		}
		else
		{
			_shieldMaterial.SetFloat( "_Bias", 0.5f );
			_shieldMaterial.SetFloat( "_Noise", 0.45f );
			_shieldMaterial.SetColor( "_Color", new Color( 70.0f / 255, 20.0f / 255, 170.0f / 255, 1.0f ) );
		}
	}

	//=====================================================

	#region State Controllers

	private void EnterState( eShieldState shieldStateEntered )
	{
		switch( shieldStateEntered )
		{
			case eShieldState.INIT:
				SetShieldColour();
				CurrentState = eShieldState.DISABLED;
				break;

			case eShieldState.DISABLED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.white;

				_collider.enabled = false;

				SetHealth();

				// Play animation
				if( _previousState == eShieldState.DESTROYED )
					_animator.SetTrigger( HashIDs.Disable );
				break;

			case eShieldState.ENABLED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.blue;

				_collider.enabled = true;

				SetShieldColour();

				// Play animation
				if( _previousState != eShieldState.DAMAGED )
					_animator.SetTrigger( HashIDs.Enable );
				break;

			case eShieldState.DAMAGED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.yellow;

				SetShieldColour();

				// Assign jobs
				_takingDamage = new Job( TakingDamage( () =>
														{
															//Debug.Log( "Damaged Shield Activating!" );
															CurrentState = eShieldState.ENABLED;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( _takingDamage );
				break;

			case eShieldState.DESTROYED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.red;

				SetShieldColour();

				// Assign jobs
				_disabling = new Job( Disabling( () =>
									{
										//Debug.Log( "Shield Destroyed - Disabling!" );
										CurrentState = eShieldState.DISABLED;
									} ),
									true );
				// Update jobs list
				_currentJobs.Add( _disabling );

				if( ShieldDestroyedEvent != null )
					ShieldDestroyedEvent();
				break;
		}
	}

	//=====================================================

	private void ExitState( eShieldState shieldStateExited )
	{
		if( _currentJobs == null || _currentJobs.Count <= 0 ) return;

		foreach( var job in _currentJobs )
		{
			if( job != null )
				job.Kill();
		}

		// Clear jobs list
		_currentJobs.Clear();
	}

	#endregion

	//=====================================================

	#region State Activities

	//=====================================================

	private IEnumerator TakingDamage( Action onComplete )
	{
		Debug.Log( "Shield Damaged!" );

		// Play  animation
		_animator.SetTrigger( HashIDs.IsDamaged );

		yield return new WaitForSeconds( 0.3f );

		// Play particle effects
		if( _psDamagedFx != null ) _psDamagedFx.SetActive( true );

		yield return new WaitForSeconds( 2.0f );

		// *** Damaged -> ENABLED ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator Disabling( Action onComplete )
	{
		// Trigger animation
		//_animator.SetTrigger( HashIDs.Disable );

		//yield return new WaitForSeconds( 0.3f );

		// Play particle effects
		if( _psExplodeFx != null ) _psExplodeFx.SetActive( true );

		// Waiting for animation to finish
		yield return new WaitForSeconds( 1.0f );

		// *** Destroyed -> DISABLED ***
		if( onComplete != null )
			onComplete();
	}

	#endregion

	#endregion

	//=====================================================
}
