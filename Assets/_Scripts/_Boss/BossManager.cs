using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( Collider ) )]
[RequireComponent( typeof( GuiBubbleBossHealthBar ) )]
public class BossManager : MonoBehaviourEMS, IPauseListener
{
	public enum eBossState
	{
		INIT,
		IDLE,
		TELEPORT,
		SUMMON_GEMS,
		SUMMON_ENEMIES,
		ATTACK,
		DISABLED,
		DISABLED_DAMAGED,
		RECOVER,
		DEAD,
		CUTSCENE,
		NULL
	}

	public event Action<Vector3> TeleportEvent;						// <next shield position>

	private static BossManager _instance;

	// Cutscene objects
	[SerializeField] private CutsceneTrigger _cutsceneStart;
	[SerializeField] private CutsceneTrigger _cutsceneBossLoses;
	[SerializeField] private CutsceneTrigger _cutsceneBossWins;

	// Boss positions
	[SerializeField] private Transform _pointLeft;
	[SerializeField] private Transform _pointCentre;
	[SerializeField] private Transform _pointRight;

	// Shield
	[SerializeField] private BossShield _shield;
	private int[] _shieldsUsed;

	// Health Bar
	[SerializeField] private GuiBubbleBossHealthBar _healthBar;

	// Enemies
	[SerializeField] private EnemyManager _enemyManager;

	// Gems
	[SerializeField] private GemManager _gemManager;

	// Grid
	[SerializeField] private GridManager _gridManager;

	private Transform _thisTransform;
	private Animator _animator;
	//private Renderer _debugStateRenderer;

	private eBossState _currentState;
	private eBossState _previousState;
	private float _health;
	private float _maxHealth;
	//private static int _currentShieldIndex;

	private List<Job> _currentJobs;
	private int _bossLevel;
	private float _attackInterval;
	private float _disabledInterval;
	private float _timer;
	private bool _isPaused;
	private bool _isPlayerDefeated = false;

	//=====================================================

	#region Public Interface

	public eBossState CurrentState
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

	public static BossManager Instance { get { return _instance; } }

	//public Vector3 CurrentShieldPosition { get { return (_currentShieldIndex == 0) ? _pointLeft.position : (_currentShieldIndex == 1) ? _pointCentre.position : _pointRight.position; } }

	public ICutsceneObject CutsceneStart { get { return _cutsceneStart; } }

	public ICutsceneObject CutsceneBossLoses { get { return _cutsceneBossLoses; } }

	public ICutsceneObject CutsceneBossWins { get { return _cutsceneBossWins; } }

	//=====================================================

	public void OnStartGame()
	{
		// Teleport out -> Teleport in -> Summon content -> Attack player -> Idle
		CurrentState = eBossState.TELEPORT;

		if( TeleportEvent != null )
			TeleportEvent( _pointCentre.position );
	}

	//=====================================================

	public void OnHitEvent( int damage )
	{
		// Special case: if boss has no shields allow attacks while IDLE
		if( IsShieldsAvailable() == false &&
		   _currentState == eBossState.IDLE ||
		   _currentState == eBossState.SUMMON_GEMS ||
		   _currentState == eBossState.SUMMON_ENEMIES ||
		   _currentState == eBossState.ATTACK )
		{
			// Stun boss
			_animator.SetTrigger( HashIDs.Stun );

			CurrentState = eBossState.DISABLED;

			return;
		}

		// Only allow attacks while DISABLED
		if( _currentState != eBossState.DISABLED ) return;

		_health -= damage;
		Debug.Log( "Boss damaged: health: " + _health );

		if( _health > 0.0f )
		{
			_animator.SetTrigger( HashIDs.StunHit );

			_healthBar.SetHealthBar( _health / _maxHealth );

			CurrentState = eBossState.DISABLED_DAMAGED;
		}
		else
		{
			// Hide health bar
			_healthBar.HideBubble();

			CurrentState = eBossState.DEAD;
		}
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;

		foreach( var job in _currentJobs )
		{
			if( job == null ) continue;

			if( isPaused == true )
				job.Pause();
			else
				job.Unpause();
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	#region Unity Calls

	void Awake()
	{
		_instance = this;
		_thisTransform = this.transform;
		_animator = _thisTransform.GetComponent<Animator>();
		_healthBar = _thisTransform.GetComponentInChildren<GuiBubbleBossHealthBar>();
		//_debugStateRenderer = _thisTransform.FindChild( "DebugState" ).GetComponent<Renderer>();

		_currentJobs = new List<Job>();
		_shieldsUsed = new[] { 0, 0, 0 };
		_isPaused = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameManager.Instance.BossWinsEvent += OnBossWinsEvent;

		_shield.ShieldDamagedEvent += OnShieldDamagedEvent;
		_shield.ShieldDestroyedEvent += OnShieldDestroyedEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameManager.Instance.BossWinsEvent -= OnBossWinsEvent;

		_shield.ShieldDamagedEvent -= OnShieldDamagedEvent;
		_shield.ShieldDestroyedEvent -= OnShieldDestroyedEvent;

		// Clear jobs
		ExitState( eBossState.NULL );
	}

	//=====================================================

	private void Start()
	{
		// Ensure daeth particle effect object is disabled
		//if( _psDeathFx != null )
		//	_psDeathFx.SetActive( false );

		// Clear jobs list
		_currentJobs.Clear();

		CurrentState = eBossState.INIT;
		_timer = -1.0f;

		_healthBar.ResetHealthBar();
		_healthBar.HideBubble();

		// Init Gem Manager with current grid locations
		_gemManager.Locations = _gridManager.GetGridLocations();
	}

	//=====================================================

	private void Update()
	{
		if( _isPaused == true || _isPlayerDefeated == true ) return;

		if( _timer < 0.0f ) return;

		if( _currentState != eBossState.IDLE &&
			_currentState != eBossState.DISABLED &&
			_currentState != eBossState.DISABLED_DAMAGED ) return;

		// Timed attack (or recover from damage)
		_timer -= Time.deltaTime;

		//Debug.Log( "" + _currentState + " t: " + _timer );

		if( _timer > 0.0f ) return;

		// Summon gems -> Summon enemies -> Attack  ELSE  Recover -> Teleport -> Summon ...
		CurrentState = (_currentState == eBossState.IDLE) ? eBossState.SUMMON_GEMS : eBossState.RECOVER;

		_timer = -1.0f;
	}

	#endregion

	//=====================================================

	private void OnBossWinsEvent( ICutsceneObject cutsceneObject )
	{
		Debug.Log( "OnBossWinsEvent !!!!!!" );

		_timer = -1.0f;

		// Hide health bar
		_healthBar.gameObject.SetActive( false );

		CurrentState = eBossState.IDLE;

		// Stop referenced managers
		_gridManager.OnBossDeadEvent();
		_enemyManager.OnClearEnemiesEvent( true );

		_isPlayerDefeated = true;
	}

	//=====================================================

	private void OnShieldDamagedEvent()
	{
		// Shock boss
		_animator.SetTrigger( HashIDs.Shock );
	}

	//=====================================================

	private void OnShieldDestroyedEvent()
	{
		Debug.Log( "BossManager: OnShieldDestroyedEvent" );

		// Stun boss
		_animator.SetTrigger( HashIDs.Stun );

		// Change to disabled state
		CurrentState = eBossState.DISABLED;
	}

	//=====================================================

	private bool IsShieldsAvailable()
	{
		foreach( var shield in _shieldsUsed )
		{
			if( shield == 0 ) return true;
		}

		return false;
	}

	//=====================================================

	private int NumShieldsUsed()
	{
		var count = 0;

		foreach( var shield in _shieldsUsed )
		{
			if( shield != 0 ) ++count;
		}

		return count;
	}

	//=====================================================
	// Returns shield transform if available or null
	private Transform GetShield()
	{
		if( IsShieldsAvailable() == false ) return null;

		var numShields = _shieldsUsed.Length;
		int i;
		do
		{
			i = Random.Range( 0, numShields );
		}
		while( _shieldsUsed[i] == 1 );

		// Set shield used and pass back position
		_shieldsUsed[i] = 1;
		//_currentShieldIndex = i;

		switch( i )
		{
			case 0:
				return _pointLeft;
			case 1:
				return _pointCentre;
			case 2:
				return _pointRight;
			default:
				return null;
		}
	}

	//=====================================================

	private void ResetShields()
	{
		for( var i = 0; i < _shieldsUsed.Length; i++ )
			_shieldsUsed[i] = 0;
	}

	//=====================================================

	#region State Controllers

	private void EnterState( eBossState bossStateEntered )
	{
		switch( bossStateEntered )
		{
			case eBossState.INIT:
				// Delay then IDLE
				StartCoroutine( Initialising() );
				break;

			case eBossState.IDLE:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.cyan;

				// Assign jobs and update jobs list
				_currentJobs.Add( new Job( Idling() ) );

				// Note: Exiting from state ( -> TELEPORT ) is managed in Update()
				break;

			case eBossState.TELEPORT:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.black;

				// Assign jobs
				var teleporting = new Job( Teleporting( () =>
														{
															Debug.Log( "Teleported into scene!" );
															CurrentState = eBossState.SUMMON_GEMS;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( teleporting );
				break;

			case eBossState.SUMMON_GEMS:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.magenta;

				// Assign jobs
				var summonGems = new Job( SummoningGems( () =>
														{
															Debug.Log( "Summoned Gems!" );
															CurrentState = eBossState.SUMMON_ENEMIES;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( summonGems );
				break;

			case eBossState.SUMMON_ENEMIES:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.magenta;

				// Assign jobs
				var summonEnemies = new Job( SummoningEnemies( () =>
														{
															Debug.Log( "Summoned Enemies!" );
															CurrentState = eBossState.ATTACK;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( summonEnemies );
				break;

			case eBossState.ATTACK:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.red;

				// Assign jobs
				var attacking = new Job( Attacking( () =>
													{
														Debug.Log( "Attacked complete!" );
														CurrentState = eBossState.IDLE;
														_timer = _attackInterval;
														Debug.Log( "_attackInterval: timer: " + _timer );
													} ),
													true );
				// Update jobs list
				_currentJobs.Add( attacking );
				break;

			case eBossState.DISABLED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.white;

				// Exiting from state (-> RECOVER ) is managed in Update()
				if( _previousState != eBossState.DISABLED_DAMAGED )
				{
					_timer = _disabledInterval;
					Debug.Log( "***_disabledInterval: timer: " + _timer );

					// Show health bar
					_healthBar.SetHealthBar( _health / _maxHealth );
					_healthBar.ShowBubble();
				}
				break;

			case eBossState.DISABLED_DAMAGED:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.red;

				Debug.Log( "DISABLED_DAMAGED" );

				// Damaged -> DISABLED
				StartCoroutine( Interval( 0, 0.5f, () => { if( _timer > 0.0f ) CurrentState = eBossState.DISABLED; } ) );
				break;

			case eBossState.RECOVER:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.white;

				// Assign jobs
				var recovering = new Job( Recovering( () =>
														{
															Debug.Log( "Recovered!" );

															// Hide health bar
															_healthBar.HideBubble();

															CurrentState = eBossState.TELEPORT;
														} ),
														true );
				// Update jobs list
				_currentJobs.Add( recovering );
				break;

			case eBossState.DEAD:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.black;

				_animator.SetTrigger( HashIDs.Dead );

				// Add penalties to wild magic rate and population (first time awards then default awards thereafter)
				if( PlayerPrefsWrapper.HasKey( "PlayerWinsFirstBossFight" ) && 
				    PlayerPrefsWrapper.GetInt( "PlayerWinsFirstBossFight" ) == 1 )
				{
					GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_BOSS_FIGHT_WIN_DEFAULT" ) );
				}
				else
				{
					GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_BOSS_FIGHT_WIN_FIRST" ) );
					PlayerPrefsWrapper.SetInt( "PlayerWinsFirstBossFight", 1 );
				}

				// Update managers
				_gridManager.OnBossDeadEvent();
				_enemyManager.OnClearEnemiesEvent( true );
				GameManager.Instance.OnBossDeadEvent();
				break;
		}
	}

	//=====================================================

	private void ExitState( eBossState shieldStateExited )
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
		// Set start point at centre position
		_thisTransform.position = _pointCentre.position;
		_thisTransform.rotation = _pointCentre.rotation;

		// Set variables from spreadsheet (allow for boss levels e.g. 1, 2, 3 ...)
		_bossLevel = GameDataManager.Instance.PlayerBossLevel;
		Debug.Log( "BOSS LEVEL: " + _bossLevel );
		_maxHealth = Convert.ToInt32( SettingsManager.GetSettingsItem( "BOSS_HEALTH", _bossLevel ) );
		_health = _maxHealth;
		_attackInterval = Convert.ToSingle( SettingsManager.GetSettingsItem( "BOSS_ATTACK_INTERVAL", _bossLevel ) );
		_disabledInterval = Convert.ToSingle( SettingsManager.GetSettingsItem( "BOSS_DISABLED_INTERVAL", _bossLevel ) );

		ResetShields();

		//yield return new WaitForSeconds( 3.0f );

		// Start intro cutscene
		//GameManager.Instance.OnBossStartEvent();

		yield return CurrentState = eBossState.IDLE;
	}

	//=====================================================

	private IEnumerator Idling()
	{
		Debug.Log( "Idling!" );

		while( true )
		{
			// Set delay until next idle animation
			var delay = Random.Range( 2.5f, 5.0f );

			if( _isPlayerDefeated == false )
				_animator.SetTrigger( Random.Range( 0, 99 ) < 50 ? HashIDs.Idle : HashIDs.IdleTaunt );
			else
				_animator.SetTrigger( Random.Range( 0, 99 ) < 50 ? HashIDs.Idle : HashIDs.Celebrate );

			yield return new WaitForSeconds( delay );
		}
	}

	//=====================================================

	private IEnumerator Teleporting( Action onComplete )
	{
		Debug.Log( "Teleporting from scene!" );

		// Select location to teleport to (an available shield)
		var nextLocation = GetShield();

		// THere's no shields left - don't teleport - just start spawning and attacking again
		if( nextLocation == null )
		{
			Debug.LogWarning( "No shields available" );

			yield return new WaitForSeconds( 1.0f );

			// *** ... -> SUMMON_GEMS ***
			if( onComplete != null )
				onComplete();

			yield return null;
		}

		_animator.SetTrigger( HashIDs.TeleportOut );

		yield return new WaitForSeconds( 4.0f );

		// Update player's camera
		if( TeleportEvent != null )
			TeleportEvent( nextLocation.position );

		// Set start point at centre position
		if( nextLocation != null )
		{
			_thisTransform.position = nextLocation.position;
			_thisTransform.rotation = nextLocation.rotation;
		}

		_animator.SetTrigger( HashIDs.TeleportIn );

		// Enable shield
		_shield.OnActivate( _thisTransform.position + Vector3.down, _thisTransform.rotation );

		yield return new WaitForSeconds( 2.0f );

		// *** Teleport -> SUMMON_GEMS ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator SummoningGems( Action onComplete )
	{
		Debug.Log( "Summoning Gems!" );

		_animator.SetTrigger( HashIDs.Summon );

		yield return new WaitForSeconds( 1.0f );

		// Summon gems ToDo: set number of gems according to ... something?
		_gemManager.BossSpawnsGems( 5 );

		yield return new WaitForSeconds( 2.0f );

		// *** Damaged -> ENABLED ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator SummoningEnemies( Action onComplete )
	{
		Debug.Log( "Summoning Enemies!" );

		_animator.SetTrigger( HashIDs.Summon );

		yield return new WaitForSeconds( 1.0f );

		// Summon enemies
		_enemyManager.BossSpawnsEnemies();

		yield return new WaitForSeconds( 2.0f );

		// *** Damaged -> ENABLED ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator Attacking( Action onComplete )
	{
		Debug.Log( "Attacking player!" );

		// Number of group-attacks e.g. 1 attack has 1, 2, or 3 sub-attacks
		var numAttacks = 3;

		// As each shield is used the frequency of sub-attacks per group-attack increases
		var numSubAttacks = Mathf.Clamp( NumShieldsUsed(), 1, 3 );

		// Fixed delay per sub-attack
		const int delayPerSubAttack = 3;

		while( numAttacks > 0 )
		{
			--numAttacks;

			// ToDo: set attack type by boss level?
			// Set attack type
			var attackType = Random.Range( 0, (int)eBossAttack.NUM_ATTACKS );

			// ToDo: select appropriate attack animation
			_animator.SetTrigger( Random.Range( 0, 99 ) < 50 ? HashIDs.Attack01 : HashIDs.Attack02 );

			yield return new WaitForSeconds( 0.5f );

			// ToDo: play FX

			yield return new WaitForSeconds( 0.5f );

			// Attack player
			_gridManager.OnAttack( (eBossAttack)attackType, numSubAttacks );

			if( numAttacks > 0 )
				yield return new WaitForSeconds( numSubAttacks * delayPerSubAttack );
			else
				yield return new WaitForSeconds( 1.0f );
		}

		// *** Attack -> IDLE ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator Recovering( Action onComplete )
	{
		Debug.Log( "Boss Recovering!" );

		// Trigger animation
		_animator.SetTrigger( HashIDs.Recover );

		yield return new WaitForSeconds( 1.5f );

		// *** Damaged -> TELEPORT ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private static IEnumerator Interval( float startDelay, float interval, Action onComplete )
	{
		yield return new WaitForSeconds( startDelay + interval );

		if( onComplete != null )
			onComplete();
	}

	#endregion

	#endregion

	//=====================================================
}
