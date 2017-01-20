using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( Animator ) )]
public class Enemy : MonoBehaviourEMS, IPauseListener, ITargetWithinRange, ICanAttackTarget
{
	[SerializeField] private Transform _thisTransform;
	[SerializeField] private Animator _thisAnimator;
	[SerializeField] private Transform _triggerFindTarget;
	//[SerializeField] private Transform _triggerInteractive;
	[SerializeField] private float _radiusTriggerForTarget = 4.0f;
	//[SerializeField] private float _radiusTriggerInteractive = 2.0f;
	[SerializeField] private float _preAttackDelay = 2.0f;

	private eEnemyStateOld _currentStateOld = eEnemyStateOld.IDLE;
	//private eEnemyStateOld _previousState = eEnemyStateOld.IDLE;

	private bool _isActive;
	private bool _isPaused;
	private bool _isTargetPlayer;
	private Transform _target;
	private float _timer;
	private bool _ignoreFirstCheckAfterAttack;
	private int _health;

	//=====================================================

	#region Public Interface

	public float RadiusTriggerForTarget { get { return _radiusTriggerForTarget; } set { _radiusTriggerForTarget = value; } }
	//public float RadiusTriggerInteractive { get { return _radiusTriggerInteractive; } set { _radiusTriggerInteractive = value; } }
	public float PreAttackDelay { get { return _preAttackDelay; } set { _preAttackDelay = value; } }

	//=====================================================

	public void OnTargetWithinRange( Transform target, bool isPlayer )
	{
		if( isPlayer == true )
			_isTargetPlayer = true;

		_target = target;

		ChangeState( eEnemyStateOld.ATTACKING );
	}

	//=====================================================

	public void OnTargetLost()
	{
		ChangeState( eEnemyStateOld.IDLE );
	}

	//=====================================================

	public bool IsAttackingTargetOk()
	{
		if( _currentStateOld == eEnemyStateOld.ATTACKING )
			return true;

		return false;
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	public void OnHitEvent( int damage )
	{
		_health -= damage;
		Debug.Log( "Enemy damaged: health: " + _health );

		// Change to state-damaged
		//ChangeState( eEnemyStateOld.DAMAGED );

		if( _health <= 0.0f )
			Destroy( this.gameObject );
	}

	//=====================================================

	public void Refresh()
	{
		_triggerFindTarget.GetComponent<SphereCollider>().radius = _radiusTriggerForTarget;
		//_triggerInteractive.GetComponent<SphereCollider>().radius = _radiusTriggerInteractive;
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_thisAnimator = _thisTransform.GetComponent<Animator>();

		_triggerFindTarget = _thisTransform.Find( "TriggerFindTarget" );
		//_triggerInteractive = _thisTransform.Find( "TriggerInteractive" );

		_triggerFindTarget.GetComponent<SphereCollider>().radius = _radiusTriggerForTarget;
		//_triggerInteractive.GetComponent<SphereCollider>().radius = _radiusTriggerInteractive;

		RegisterWithColliders();

		_isActive = false;
		_isPaused = false;
		_isTargetPlayer = false;
		_target = null;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying )
		{
			GameManager.Instance.PauseEvent += OnPauseEvent;

			// Set starting health
			_health = Convert.ToInt32( SettingsManager.GetSettingsItem( "ENEMY_HEALTH", -1 ) );
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying )
		{
			if( _isAppQuiting == false )
				GameManager.Instance.PauseEvent -= OnPauseEvent;
		}
	}

	//=====================================================

	void Start()
	{
		// Inject this object into trigger
		_triggerFindTarget.GetComponent<TriggerTargetInRange>().Init( this );

		ChangeState( eEnemyStateOld.IDLE );

		// DEBUG - REMOVE THIS
		_isActive = true;
	}

	//=====================================================

	void Update()
	{
		if( _isActive == false || _isPaused == true )
			return;

		// Monitor current state
		switch( _currentStateOld )
		{
			case eEnemyStateOld.IDLE:
				// Reset target parameters
				_target = null;
				_timer = 0.0f;
				_isTargetPlayer = false;
				break;
			case eEnemyStateOld.ATTACKING:
				CheckAttackCompleted();
				break;
			case eEnemyStateOld.PRE_ATTACK:
				DelayBeforeAttack();
				break;
			case eEnemyStateOld.DAMAGED:
				// Do nothing for now
				break;
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere( _thisTransform.position, _radiusTriggerForTarget );
		//Gizmos.DrawWireSphere( _thisTransform.position, _radiusTriggerInteractive );
	}

	//=====================================================

	private void RegisterWithColliders()
	{
		var children = PreHelpers.GetAllChildren( _thisTransform );

		foreach( var child in children )
		{
			if( child.tag == UnityTags.ColliderCanDamage )
			{
				var script = child.GetComponent<ColliderObstacle>();
				if( script != null )
					script.Init( this );
			}
		}
	}

	//=====================================================

	private void ChangeState( eEnemyStateOld newStateOld )
	{
		// Store previous and new states
		//_previousState = _currentStateOld;
		_currentStateOld = newStateOld;

		switch( newStateOld )
		{
			case eEnemyStateOld.IDLE:
				// Do nothing
				break;
			case eEnemyStateOld.ATTACKING:
				AttackTarget();
				break;
			case eEnemyStateOld.PRE_ATTACK:
				_timer = _preAttackDelay;
				break;
			case eEnemyStateOld.DAMAGED:
				// Do nothing for now
				break;
		}
	}

	//=====================================================

	private void AttackTarget()
	{
		if( _target != null && _isTargetPlayer == true )
		{
			// For now, point this enemy at target (player)
			var dir = _target.position - _thisTransform.position;
			dir = new Vector3( dir.x, 0.0f, dir.z );
			_thisTransform.forward = dir;

			// Play attack animation
			_thisAnimator.SetTrigger( HashIDs.IsAttacking );

			// Introduce delay to allow mecanim to catch up before checking against current state is EXPLODE
			_ignoreFirstCheckAfterAttack = true;
		}
	}

	//=====================================================

	private void CheckAttackCompleted()
	{
		// Allow mecanim to catch up before checking against current state is EXPLODE
		if( _ignoreFirstCheckAfterAttack == true )
		{
			_ignoreFirstCheckAfterAttack = false;
			return;
		}

		// Get current state's info (0 : on Base Layer)
		var currentStateInfo = _thisAnimator.GetCurrentAnimatorStateInfo( 0 );

		// Allow for transition to EXPLODE
		if( _thisAnimator.IsInTransition( 0 ) == false )	// currentStateInfo.normalizedTime >= 1 && 
		{
			// Has attack animation returned to IDLE
			if( currentStateInfo.nameHash != HashIDs.StateAttacking )
			{
				// Change to Recover state
				ChangeState( eEnemyStateOld.PRE_ATTACK );
			}
		}
	}

	//=====================================================

	private void DelayBeforeAttack()
	{
		_timer -= Time.deltaTime;

		// Check there's no delay required before next attack
		if( _timer <= 0.0f )
			ChangeState( eEnemyStateOld.ATTACKING );
	}

	#endregion

	//=====================================================
}
