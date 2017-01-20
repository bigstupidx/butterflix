using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviourEMS, ITargetWithinRange, IPauseListener
{
	public event Action EnemyDestroyedEvent;

	public enum eEnemyState
	{
		INIT,
		IDLE,		// white
		GATHER,		// green
		EAT,		// blue
		ESCAPE,		// yellow
		POST_ESCAPE,
		HUNT,		// magenta
		EXPLODE,	// red
		DAMAGED,	// black
		DEAD,
		FALL,
		NULL
	}

	[SerializeField] private GameObject _pfbGem;
	[SerializeField] private AudioClip _clipHit;
	[SerializeField] private AudioClip _clipChatter;
	[SerializeField] private AudioClip _clipAngry;

	private List<PathNode> _wayPoints;
	private List<Job> _currentJobs;
	private Job _listening;
	private Job _looking;
	private Job _gathering;
	private Job _eating;
	private Job _escaping;
	private Job _hunting;
	private Job _exploding;
	private Job _takingDamage;

	private Transform _thisTransform;
	private NavMeshAgent _thisAgent;
	private Rigidbody _thisRigidbody;
	private AudioSource _audioSource;
	private Transform _triggerFindTarget;
	private EnemyAnimation _animation;
	//private Renderer _debugStateRenderer;
	private GameObject _psDeathFx;
	private GameObject _triggerTargetInRange;

	private eEnemyState _currentState;
	private eEnemyState _previousState;
	private int _health;
	private const float _walkSpeed = 1.4f;
	private const float _runSpeed = 4.0f;
	[Range( 5.0f, 10.0f )]
	[SerializeField] private float _triggerTargetRadius = 8.0f;
	[Range( 3.0f, 6.0f )]
	[SerializeField] private float _attackRadius = 4.0f;
	//[SerializeField] private eDamageType _damageType = eDamageType.MEDIUM;

	private Queue<Transform> _gems;
	private int _numGemsEaten;
	private Transform _player;
	private int _maskPlayer;
	private int _maskShield;
	//private Vector3 _lastPlayerPosition;
	private float _playerSpeedSqrMag;

	private bool _isPaused;

	//=====================================================

	#region Public Interface

	public eEnemyState CurrentState
	{
		get { return _currentState; }
		set
		{
			ExitState( _currentState );
			_currentState = value;
			EnterState( _currentState );
		}
	}

	public float TriggerTargetRadius
	{
		get { return _triggerTargetRadius; }
		set { _triggerTargetRadius = value; }
	}

	//=====================================================

	public void OnHitEvent( int damage )
	{
		_health -= damage;
		//Debug.Log( "Enemy damaged: health: " + _health );

		if( _currentState == eEnemyState.EXPLODE || _currentState == eEnemyState.DEAD ) return;

		if( _health > 0.0f )
		{
			CurrentState = eEnemyState.DAMAGED;
		}
		else
		{
			CurrentState = eEnemyState.EXPLODE;

			// Add penalties to wild magic rate and population
			GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_KILL_ENEMY" ) );
		}

		// Play audio fx
		_audioSource.PlayOneShot( _clipHit );
	}

	//=====================================================

	public void OnFall()
	{
		if( _currentState != eEnemyState.FALL )
			CurrentState = eEnemyState.FALL;
	}

	//=====================================================

	public void OnSpawn( PathNode[] waypoints )
	{
		SetWayPoints( waypoints );

		if( _wayPoints.Count > 1 && _wayPoints[0] != null )
		{
			// Set default state to INIT -> GATHER
			CurrentState = eEnemyState.INIT;
		}
		else
		{
			Debug.LogWarning( "OnSpawn: Enemy appears to have none or no more than one waypoint for gathering gems" );
		}
	}

	//=====================================================

	public void OnSleep( bool isSleeping )
	{
		OnPauseEvent( isSleeping );
	}

	//=====================================================

	public void OnDestroy()
	{
		if( _currentState != eEnemyState.DEAD )
			CurrentState = eEnemyState.DEAD;
	}

	//=====================================================

	public void Refresh()
	{
		_triggerFindTarget.GetComponent<SphereCollider>().radius = _triggerTargetRadius;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		if( _isPaused == isPaused ) return;

		_isPaused = isPaused;

		// Pause / unpause jobs
		foreach( var job in _currentJobs )
		{
			if( job == null ) continue;

			if( _isPaused == true )
				job.Pause();
			else
				job.Unpause();
		}

		// Pause / unpause agent & animations
		if( _isPaused == true )
		{
			_thisAgent.Stop();
			_animation.Pause();
		}
		else
		{
			_thisAgent.Resume();
			_animation.Resume();
		}
	}

	#endregion

	//=====================================================

	#region ITargetWithinRange

	// Trigger update - player within range
	public void OnTargetWithinRange( Transform target, bool isPayer = false )
	{
		// Player found
		if( isPayer == true )
		{
			_player = target;
		}
		// Gem found
		else
		{
			// Check that gem hasn't already been found
			var isNewGem = true;

			foreach( var gem in _gems )
			{
				if( gem == target )
					isNewGem = false;
			}

			if( isNewGem == false ) return;

			// Check that gem is at same level as this enemy
			if( Mathf.Abs( _thisTransform.position.y - target.position.y ) > 2.0f ) return;

			// Add new gem to queue
			_gems.Enqueue( target );
		}
	}

	//=====================================================
	// Trigger update - player not within range
	public void OnTargetLost()
	{
		_player = null;
	}

	#endregion

	//=====================================================

	//=====================================================

	#region Private Methods

	#region Unity Calls

	private void Awake()
	{
		_wayPoints = new List<PathNode>();
		_currentJobs = new List<Job>();
		_gems = new Queue<Transform>();

		_maskPlayer = 1 << LayerMask.NameToLayer( "Player" ) | 1 << LayerMask.NameToLayer( "Collidable" );
		_maskShield = 1 << LayerMask.NameToLayer( "Collidable" );

		_thisTransform = this.transform;
		_thisAgent = _thisTransform.GetComponent<NavMeshAgent>();
		_thisRigidbody = _thisTransform.GetComponent<Rigidbody>();
		_audioSource = _thisRigidbody.GetComponent<AudioSource>();
		//guard_motor = GetComponent<BotFreeMovementMotor>();
		_triggerFindTarget = _thisTransform.Find( "TriggerFindTarget" );
		_triggerFindTarget.GetComponent<SphereCollider>().radius = _triggerTargetRadius;
		_animation = _thisTransform.GetComponentInChildren<EnemyAnimation>();
		_psDeathFx = _thisTransform.FindChild( "psDeathFx" ).gameObject;
		//_debugStateRenderer = _thisTransform.FindChild( "DebugState" ).GetComponent<Renderer>();
		_triggerTargetInRange = _thisTransform.FindChild( "TriggerFindTarget" ).gameObject;

		// Set other defaults
		_currentState = eEnemyState.INIT;
		_health = Convert.ToInt32( SettingsManager.GetSettingsItem( "ENEMY_HEALTH", GameDataManager.Instance.PlayerBossLevel ) );
		_isPaused = false;
	}

	//=====================================================

	private void OnEnable()
	{
		_numGemsEaten = 0;

		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
	}

	//=====================================================

	private void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;

		// Clear jobs
		ExitState( eEnemyState.NULL );
	}

	//=====================================================

	private void Start()
	{
		// Ensure daeth particle effect object is disabled
		if( _psDeathFx != null )
			_psDeathFx.SetActive( false );

		// Inject this object into trigger
		_triggerFindTarget.GetComponent<TriggerTargetInRange>().Init( this );

		// Clear jobs list
		_currentJobs.Clear();
	}

	//=====================================================

	private void LateUpdate()
	{
		if( _player == null ) return;

		switch( _currentState )
		{
			case eEnemyState.GATHER:
				// Monitor player speed - enemy listens for player movement
				_playerSpeedSqrMag = PlayerManager.SpeedSqrMag;
				break;
		}
	}

	#endregion

	//=====================================================

	#region State Controllers

	private void EnterState( eEnemyState enemyStateEntered )
	{
		_triggerTargetInRange.SetActive( true );

		switch( enemyStateEntered )
		{
			case eEnemyState.INIT:
				_thisAgent.enabled = true;
				_thisRigidbody.isKinematic = true;

				// Delay then GATHER - delay ensures jobs are added to JobManager correctly
				StartCoroutine( Initialising() );
				break;

			case eEnemyState.FALL:
				_triggerTargetInRange.SetActive( false );
				_thisAgent.enabled = false;
				_thisRigidbody.isKinematic = false;
				// Fall, delay then DEAD
				StartCoroutine( Falling() );
				break;

			case eEnemyState.GATHER:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.green;

				// Assign jobs
				_gathering = new Job( Gathering( () =>
									 {
										 //Debug.Log( "Gathered gem. Yes!" );
										 CurrentState = eEnemyState.EAT;
									 } ),
									 true );

				_looking = new Job( Looking( 150.0f, _triggerTargetRadius, 0.33f, false,
											 () =>
											 {
												 // GameDataManager.Instance.IsPlayerHealthFull() == true
												 CurrentState = IsPlayerAttackOk() == true ? eEnemyState.HUNT : eEnemyState.ESCAPE;
											 } ),
											 true );

				_listening = new Job( Listening( 0.36f,
												 () => 
												 {
													 CurrentState = IsPlayerAttackOk() == true ? eEnemyState.HUNT : eEnemyState.ESCAPE;
												 } ),
												 true );
				// Update jobs list
				_currentJobs.Add( _gathering );
				_currentJobs.Add( _looking );
				_currentJobs.Add( _listening );
				break;

			case eEnemyState.EAT:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.blue;

				// Assign jobs
				_eating = new Job( Eating( () =>
				{
					//Debug.Log( "Ate gem. Yum!" );

					// Add penalty to wild magic rate and population
					GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_ENEMY_EATS_GEM" ) );

					CurrentState = eEnemyState.GATHER;
				} ),
								   true );
				// Update jobs list
				_currentJobs.Add( _eating );
				break;

			case eEnemyState.ESCAPE:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.yellow;

				// Assign jobs
				_escaping = new Job( Escaping( () =>
												{
													//Debug.Log( "Escaped player. Phew!" );
													CurrentState = eEnemyState.POST_ESCAPE;
												} ),
												true );
				// Update jobs list
				_currentJobs.Add( _escaping );
				break;

			case eEnemyState.POST_ESCAPE:
				// Is player still in range
				if( _player != null && (_thisTransform.position - _player.position).sqrMagnitude < (_triggerTargetRadius * _triggerTargetRadius) )
					CurrentState = eEnemyState.ESCAPE;
				else
					CurrentState = eEnemyState.GATHER;
				break;

			case eEnemyState.HUNT:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.magenta;

				// Assign jobs
				_hunting = new Job( Hunting( () =>
				{
					//Debug.Log( "Found player. Explode!" );
					CurrentState = eEnemyState.EXPLODE;
				} ),
									true );

				_looking = new Job( Looking( 178.0f, _triggerTargetRadius, 0.25f, true,
											 () =>
											 {
												 //Debug.Log( "Lost the player. Find gems!" );
												 CurrentState = eEnemyState.GATHER;
											 } ),
											 true );

				// Update jobs list
				_currentJobs.Add( _hunting );
				_currentJobs.Add( _looking );
				break;

			case eEnemyState.EXPLODE:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.red;

				// Assign jobs
				_exploding = new Job( Exploding( () =>
												{
													//Debug.Log( "Explode! Explode!" );
													CurrentState = eEnemyState.DEAD;
												} ),
												true );
				// Update jobs list
				_currentJobs.Add( _exploding );
				break;

			case eEnemyState.DAMAGED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.black;

				// Assign jobs
				_takingDamage = new Job( TakingDamage( () =>
														{
															//Debug.Log( "Ouch! Find Gems!" );
															CurrentState = eEnemyState.GATHER;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( _takingDamage );
				break;

			case eEnemyState.DEAD:
				if( EnemyDestroyedEvent != null )
					EnemyDestroyedEvent();

				// Destroy enemy
				Destroy( this.gameObject );
				break;
		}
	}

	//=====================================================

	private void ExitState( eEnemyState enemyStateExited )
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

	private IEnumerator Initialising()
	{
		_animation.Spawn();

		yield return new WaitForSeconds( 1.0f );

		if( _thisAgent == null )
			_thisAgent = _thisTransform.GetComponent<NavMeshAgent>();

		CurrentState = eEnemyState.GATHER;
	}

	//=====================================================

	private IEnumerator Falling()
	{
		// ToDo: change to falling animation
		_animation.Idle();

		yield return new WaitForSeconds( 3.0f );

		CurrentState = eEnemyState.DEAD;
	}

	//=====================================================

	private IEnumerator Gathering( Action onComplete )
	{
		var wayPointIndex = Random.Range( 0, _wayPoints.Count );

		while( true )
		{
			// Select new target for gem-gathering
			var lastIndex = wayPointIndex;
			do
				wayPointIndex = Random.Range( 0, _wayPoints.Count );
			while( wayPointIndex == lastIndex );

			// Target gem if available
			var isTargetingGem = IsGemAvailable();

			// Else target next waypoint for hunting / gathering gems
			if( isTargetingGem == false )
				_thisAgent.SetDestination( _wayPoints[wayPointIndex].transform.position );

			// Set walk speed
			_thisAgent.speed = _walkSpeed;
			_animation.Walk();

			// Move toward target
			while( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > 0.5f )
			{
				// Check for any new gems found
				if( isTargetingGem == false )
					isTargetingGem = IsGemAvailable();

				//guard_motor.facingDirection = patrolPoints[i].position - _transform.position;
				yield return null;
			}

			// Arrived at target ( gem or gathering point )
			if( isTargetingGem == true )
			{
				// *** Gem gathered -> EAT gem ***
				if( onComplete != null )
					onComplete();
			}

			//guard_motor.facingDirection = patrolPoints[i].position - _transform.position;

			// Stop movement
			_thisAgent.speed = 0.0f;
			_animation.Stop();

			// Play audio fx
			_audioSource.PlayOneShot( _clipChatter );

			yield return new WaitForSeconds( 1.0f );
		}
	}

	//=====================================================

	private IEnumerator Eating( Action onComplete )
	{
		_thisAgent.speed = 0.0f;
		_animation.Stop();

		// Remove gem from queue and eat it
		if( _gems != null && _gems.Count > 0 )
		{
			if( _gems.Peek() != null )
			{
				var gem = _gems.Dequeue();

				gem.GetComponent<Collectable>().OnConsumed();
				++_numGemsEaten;
			}
		}

		yield return new WaitForSeconds( 1.0f );

		// *** Gem consumed -> GATHER gems ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator Looking( float fov, float distance, float delayBetweenLooks, bool isWatchingPlayer, Action onComplete )
	{
		while( true )
		{
			// Watching player
			if( isWatchingPlayer == true )
			{
				// En

				if( CheckPlayerInFOV( fov ) == false || CheckPlayerInRange( distance ) < 0.0f )
				{
					// *** Player lost -> GATHER ***
					if( onComplete != null )
						onComplete();
				}
			}
			// Looking for player
			else
			{
				if( CheckPlayerInFOV( fov ) == true && CheckPlayerInRange( distance ) > 0.0f )
				{
					// *** Player spotted -> HUNT or ESCAPE ***
					if( onComplete != null )
						onComplete();
				}
			}

			yield return new WaitForSeconds( delayBetweenLooks );
		}
	}

	//=====================================================

	private IEnumerator Listening( float delayBetweenListens, Action onComplete )
	{
		while( true )
		{
			if( _player != null )
			{
				var hearingRange = _triggerTargetRadius;
				var heardNoise = (_thisTransform.position - _player.position).sqrMagnitude < (hearingRange * hearingRange) &&
								 _playerSpeedSqrMag > 16.0f;

				// *** Player heard -> ESCAPE player ***
				if( heardNoise && onComplete != null ) onComplete();
			}

			yield return new WaitForSeconds( delayBetweenListens );
		}
	}

	//=====================================================

	private IEnumerator Escaping( Action onComplete )
	{
		// If player is out of range return to normal business
		if( _player == null )
		{
			// *** Escaped player -> GATHER gem ***
			if( onComplete != null )
				onComplete();
		}

		if( _player != null )
		{
			// Set new escape-destination away from player
			var escapePoint = (_thisTransform.position - _player.position).normalized * _triggerTargetRadius;
			escapePoint.y = 0.0f;
			_thisAgent.SetDestination( _thisTransform.position + escapePoint );

			// Set run speed
			_thisAgent.speed = _runSpeed;
			_animation.Run();

			// Run away from player
			while( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > 2.0f )
				yield return null;
		}

		// Stop movement
		_thisAgent.speed = 0.0f;
		_animation.Stop();

		yield return new WaitForSeconds( 0.25f );

		// *** Thinks it's escaped player -> POST_ESCAPE ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator Hunting( Action onComplete )
	{
		while( true )
		{
			// Set run speed
			_thisAgent.speed = _runSpeed;
			_animation.Run();

			if( _player != null )
			{
				// Play audio fx
				_audioSource.PlayOneShot( _clipAngry );

				_thisAgent.SetDestination( _player.position );
				var proximityMin = (_attackRadius * 0.5f) * (_attackRadius * 0.5f);

				// Run within attack radius of last recorded target position
				while( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > proximityMin )
				{
					//guard_motor.facingDirection = patrolPoints[i].position - _transform.position;
					yield return null;
				}

				// Check if target is in attack-range
				if( CheckPlayerInRange( _attackRadius ) > 0.0f )
				{
					// *** At player -> EXPLODE ***
					if( onComplete != null )
						onComplete();
				}
			}

			//guard_motor.facingDirection = patrolPoints[i].position - _transform.position;

			// Stop movement
			_thisAgent.speed = 0.0f;
			_animation.Stop();

			yield return new WaitForSeconds( 1.0f );
		}
	}

	//=====================================================

	private IEnumerator Exploding( Action onComplete )
	{
		// Halt movement
		_thisAgent.speed = 0.0f;
		_animation.Explode();

		yield return new WaitForSeconds( 1.1f );

		// Play audio fx
		_audioSource.PlayOneShot( _clipAngry );

		// Play particle effects
		if( _psDeathFx != null )
			_psDeathFx.SetActive( true );

		// Spill health-gem if number of gems eaten > 0		// && health > 0 i.e. attacking player rather than dying from damage
		if( _numGemsEaten > 0 )								// _health > 0.0f
		{
			if( _pfbGem != null )
			{
				var gem = Instantiate( _pfbGem, _thisTransform.position + Vector3.up, Quaternion.identity ) as GameObject;

				if( gem != null )
				{
					gem.name = "HealthGem";

					// Parent gem with others in hierarchy
					var container = GameObject.Find( "Collectables" ).transform;
					if( container != null )
					{
						container = container.Find( "Gems" );

						if( container != null )
							gem.transform.parent = GameObject.Find( "Collectables" ).transform.Find( "Gems" ).transform;
					}

					// Initialise gem
					var script = gem.GetComponent<CollectableHealth>();
					if( script != null )
					{
						script.Type = eCollectable.HEALTH_GEM;
						script.InitGemPrefab();
					}
				}
			}
		}

		// Damage player if in range
		var distance = CheckPlayerInRange( _attackRadius );
		if( distance > 0.0f )
			PlayerManager.OnObstacleHit( distance < (_attackRadius * 0.5f) ? eDamageType.MEDIUM : eDamageType.LOW, Vector3.zero );

		// Damage shield if in range
		//if( GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
		//{
		GameObject shield;
		distance = CheckShieldInRange( _attackRadius * 1.5f, out shield );
		if( distance > 0.0f )
		{
			var bossShield = shield.GetComponent<BossShield>();

			if( bossShield != null )
				bossShield.OnHit( distance < _attackRadius ? eDamageType.MEDIUM : eDamageType.LOW );
		}
		//}

		// Waiting for animation to finish
		yield return new WaitForSeconds( 1.2f );

		// *** Exploded -> DEAD ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator TakingDamage( Action onComplete )
	{
		//Debug.Log( "Ouch!" );

		// Halt movement
		_thisAgent.speed = 0.0f;
		_animation.Hit( true );

		yield return new WaitForSeconds( 0.25f );

		_animation.IdleFidget();

		yield return new WaitForSeconds( 4.0f );

		// *** Damaged -> GATHERING ***
		if( onComplete != null )
			onComplete();
	}

	#endregion

	//=====================================================

	#region State Helpers

	private void SetWayPoints( PathNode[] waypoints )
	{
		_wayPoints.Clear();

		if( waypoints == null ) return;

		foreach( var point in waypoints )
			_wayPoints.Add( point );
	}

	//=====================================================

	private bool IsGemAvailable()
	{
		//if( _thisAgent == null ) return false;

		// If gem has been spotted, select it as a target else select new gathering position
		if( _gems.Count <= 0 || _gems.Peek() == null ) return false;

		// Ignore health gems
		if( _gems.Peek().GetComponent<Collectable>().Type == eCollectable.HEALTH_GEM ) return false;

		_thisAgent.SetDestination( _gems.Peek().transform.position );

		return true;
	}

	//=====================================================

	private bool CheckPlayerInFOV( float fov )
	{
		if( _player == null ) return false;

		// Create a vector from the enemy to the player and store the angle between it and forward.
		var direction = _player.position - _thisTransform.position;
		var angle = Vector3.Angle( direction, _thisTransform.forward );

		// If the angle between forward and where the player is, is less than half the angle of view, player is in view
		return (angle < (fov * 0.5f));
	}

	//=====================================================
	// Returns distance to player if in range or -1
	private float CheckPlayerInRange( float distance )
	{
		if( _player == null ) return -1.0f;

		var direction = _player.position - _thisTransform.position;

		Vector3[] rayDirections =
		{
			direction,
			direction + new Vector3( 0.5f, 0, 0 ),
			direction + new Vector3( -0.5f, 0, 0 )
		};

		// Spread raycasts to detect player (unadjusted direction tends to a position in front of the player???)
		for( var i = 0; i < 3; i++ )
		{
			Debug.DrawRay( _thisTransform.position + (_thisTransform.up * 0.5f), rayDirections[i], Color.red, 0.75f, false );
			RaycastHit hit;

			// ... and if a raycast towards the player hits something...
			if( Physics.Raycast( _thisTransform.position + (_thisTransform.up * 0.5f),
								 rayDirections[i].normalized, out hit, distance, _maskPlayer ) == false ) continue;

			// ... and if the raycast hits the player...
			if( hit.collider.tag == UnityTags.Player )
			{
				// *** Player in range ***
				return hit.distance;
			}
		}

		return -1.0f;
	}

	//=====================================================
	// Boss Game: Returns distance to shield if in range or -1 - Used when damaging boss-shield
	private float CheckShieldInRange( float distance, out GameObject shield )
	{
		// ToDo: Change rays to a range of forward facing directions 
		Vector3[] rayDirections =
		{
			Vector3.forward,
			(Vector3.forward + Vector3.right).normalized,
			(Vector3.forward + Vector3.left).normalized,
			Vector3.back 
		};

		// Spread raycasts to detect shield
		var numRays = rayDirections.Length;
		for( var i = 0; i < numRays; i++ )
		{
			Debug.DrawRay( _thisTransform.position + (_thisTransform.up * 0.5f), rayDirections[i], Color.green, 0.75f, false );
			RaycastHit hit;

			// ... and if a raycast towards the shield hits something...
			if( Physics.Raycast( _thisTransform.position + (_thisTransform.up * 0.5f),
								 rayDirections[i], out hit, distance, _maskShield ) == false ) continue;

			// ... and if the raycast hits the shield...
			if( hit.collider.tag != UnityTags.Shield ) continue;
			
			// *** Shield in range ***
			shield = hit.collider.gameObject;
			return hit.distance;
		}

		shield = null;
		return -1.0f;
	}

	//=====================================================

	private static bool IsPlayerAttackOk()
	{
		var playerHealth = ((float)GameDataManager.Instance.PlayerHealth / GameDataManager.Instance.PlayerMaxHealth) * 100;

		var rand = Random.Range( 0, 99 );

		if( playerHealth >= 95 )
			return (rand <= Convert.ToInt32( SettingsManager.GetSettingsItem( "CHANCE_ATTACK_PLAYER_AT_95_HEALTH", -1 ) ) );

		if( playerHealth >= 80 )
			return (rand <= Convert.ToInt32( SettingsManager.GetSettingsItem( "CHANCE_ATTACK_PLAYER_AT_80_HEALTH", -1 ) ) );

		if( playerHealth >= 60 )
			return (rand <= Convert.ToInt32( SettingsManager.GetSettingsItem( "CHANCE_ATTACK_PLAYER_AT_60_HEALTH", -1 ) ) );

		if( playerHealth >= 40 )
			return (rand <= Convert.ToInt32( SettingsManager.GetSettingsItem( "CHANCE_ATTACK_PLAYER_AT_40_HEALTH", -1 ) ));

		return true;
	}

	#endregion

	#endregion

	//=====================================================
}
