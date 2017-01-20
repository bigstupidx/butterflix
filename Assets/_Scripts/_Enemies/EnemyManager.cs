using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class EnemyManager : MonoBehaviourEMS, ITargetWithinRange, IPauseListener
{
	// Editable in inspector
	[SerializeField] private EnemyDoorway[] _doorwayTriggers;
	[SerializeField] private PathNode[] _wayPoints;					// Used as both spawnPoints and wayPoints for gathering gems

	// Private vars - serialized
	[SerializeField] private Transform _doorwayContainer;
	[SerializeField] private Transform _wayPointContainer;
	[SerializeField] private Transform _enemyContainer;

	// Private vars
	private Transform _thisTransform;
	private Object _pfbEnemy;
	private List<EnemyAI> _enemies;
	private int _maxEnemies;
	private int _currentNumEnemies;
	private Job _checkForEnemyRespawn;

	//=====================================================

	#region Public Interface

	public void OnClearEnemiesEvent( bool killEnemies )
	{
		// Enemies sleep or die
		if( _enemies != null && _enemies.Count > 0 )
		{
			foreach( var enemy in _enemies )
			{
				if( enemy == null ) continue;

				if( killEnemies == false )
					enemy.OnSleep( true );
				else
					enemy.OnDestroy();
			}
		}

		// Kill respawn checks
		if( _checkForEnemyRespawn == null ) return;

		_checkForEnemyRespawn.Kill();
		_checkForEnemyRespawn = null;
	}

	//=====================================================

	public void BossSpawnsEnemies()
	{
		if( _enemies == null ) _enemies = new List<EnemyAI>();

		_maxEnemies = SetMaxEnemies();

		if( _currentNumEnemies >= _maxEnemies ) return;

		var enemiesRemaining = new List<EnemyAI>();
		_currentNumEnemies = 0;

		// Copy remaining enemies into temp list
		foreach( var enemy in _enemies )
		{
			if(enemy != null)
			{
				enemiesRemaining.Add( enemy );
				++_currentNumEnemies;
			}
		}
		
		// Clear enemy list and reset with temp list
		_enemies.Clear();
		_enemies = enemiesRemaining;
		enemiesRemaining = null;

		// Spawn missing enemies
		while( _currentNumEnemies < _maxEnemies )
			SpawnNewEnemy();
	}

	//=====================================================

	public void Refresh()
	{
		_thisTransform.localRotation = Quaternion.identity;

		// Set doorway container
		if( _doorwayContainer == null )
		{
			_doorwayContainer = new GameObject( "DoorwayContainer" ).transform;

			_doorwayContainer.parent = _thisTransform;
			_doorwayContainer.rotation = Quaternion.identity;
			_doorwayContainer.position = Vector3.zero;
		}

		// Set spawnPoint / wayPoint container
		if( _wayPointContainer == null )
		{
			_wayPointContainer = new GameObject( "SpawnPointContainer" ).transform;

			_wayPointContainer.parent = _thisTransform;
			_wayPointContainer.rotation = Quaternion.identity;
			_wayPointContainer.position = Vector3.zero;
		}

		// Set enemy container
		if( _enemyContainer == null )
		{
			_enemyContainer = new GameObject( "EnemyContainer" ).transform;

			_enemyContainer.parent = _thisTransform;
			_enemyContainer.rotation = Quaternion.identity;
			_enemyContainer.position = Vector3.zero;
		}

		// Add doorways to container
		if( _doorwayTriggers.Length > 0 )
		{
			for( var i = 0; i < _doorwayTriggers.Length; i++ )
			{
				if( _doorwayTriggers[i] == null ) continue;

				_doorwayTriggers[i].transform.parent = _doorwayContainer;
				_doorwayTriggers[i].name = "Doorway" + i.ToString( "00" );
			}
		}

		// Add wayPoints to container
		if( _wayPoints.Length <= 0 ) return;

		for( var i = 0; i < _wayPoints.Length; i++ )
		{
			if( _wayPoints[i] == null ) continue;

			_wayPoints[i].transform.parent = _wayPointContainer;
			_wayPoints[i].name = "WayPoint" + i.ToString( "00" );
		}
	}

	#endregion

	//=====================================================

	#region ITargetWithinRange

	public void OnTargetWithinRange( Transform target, bool isPlayer = false )
	{
		if( isPlayer == false ) return;

		if( _enemies == null ) _enemies = new List<EnemyAI>();

		_currentNumEnemies = 0;
		_maxEnemies = SetMaxEnemies();

		// Enemies awaken
		if( _enemies.Count > 0 )
		{
			foreach( var enemy in _enemies )
			{
				if( enemy == null ) continue;

				_currentNumEnemies++;
				enemy.OnSleep( false );
			}
		}
		else
		{
			// Instantiate and spawn enemies - parent enemies to container and set their waypoints
			for( var i = 0; i < _maxEnemies; i++ )
				SpawnNewEnemy();
		}

		// Block auto-respawn for enemies - shouldn't be necessary as Boss should summon enemies rather than being triggered by EnemyDoorway
		if( GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM ) return;

		// Set check for respawning enemies while room is active
		if( _checkForEnemyRespawn == null )
			_checkForEnemyRespawn = new Job( AutoReSpawnEnemies() );
	}

	//=====================================================

	public void OnTargetLost()
	{
		if( GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM ) return;

		OnClearEnemiesEvent( false );
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		if( _checkForEnemyRespawn == null ) return;

		if( isPaused == true )
			_checkForEnemyRespawn.Pause();
		else
			_checkForEnemyRespawn.Unpause();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_enemies = new List<EnemyAI>();
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameManager.Instance.ClearEnemiesEvent += OnClearEnemiesEvent;

		// Determine how many enemies should be spawned - according to current wild magic rate
		_currentNumEnemies = 0;
		_maxEnemies = SetMaxEnemies();
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		OnClearEnemiesEvent( true );

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameManager.Instance.ClearEnemiesEvent -= OnClearEnemiesEvent;
	}

	//=====================================================

	void Start()
	{
		if( Application.isPlaying == false ) return;

		// Load enemy prefab from resources
		_pfbEnemy = ResourcesEnemies.GetPrefab();

		if( _doorwayTriggers == null || _doorwayTriggers.Length == 0 ) return;

		foreach( var trigger in _doorwayTriggers )
		{
			if( trigger == null ) continue;

			// Inject this manager into all doorway triggers
			trigger.Init( this );
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		// Draw lines between manager and referenced dorrway triggers
		if( _doorwayTriggers.Length == 0 ) return;
		
		foreach( var trigger in _doorwayTriggers )
		{
			if( trigger != null )
				Gizmos.DrawLine( _thisTransform.position, trigger.transform.position );
		}
	}

	//=====================================================

	private void OnEnemyDestroyedEvent()
	{
		--_currentNumEnemies;

		if( _currentNumEnemies < 0 ) _currentNumEnemies = 0;
	}

	//=====================================================

	private int SetMaxEnemies()
	{
		// Determine how many enemies should be spawned - Boss Room
		if( GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
			return Convert.ToInt32( SettingsManager.GetSettingsItem( "BOSS_NUM_ENEMIES", -1 ) );

		// Determine how many enemies should be spawned - according to current wild magic rate
		var max = WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MAX" );
		var min = WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MIN" );

		if( GameDataManager.Instance.PlayerWildMagicRate < min * 0.5f )
			_maxEnemies = Convert.ToInt32( WildMagicItemsManager.GetWildMagicItemValue( "ENEMIES_WM_STRONG" ) );
		else if( GameDataManager.Instance.PlayerWildMagicRate < 0.0f )
			_maxEnemies = Convert.ToInt32( WildMagicItemsManager.GetWildMagicItemValue( "ENEMIES_WM_MEDIUM" ) );
		else if( GameDataManager.Instance.PlayerWildMagicRate < max * 0.5f )
			_maxEnemies = Convert.ToInt32( WildMagicItemsManager.GetWildMagicItemValue( "ENEMIES_WM_MILD" ) );
		else
			_maxEnemies = Convert.ToInt32( WildMagicItemsManager.GetWildMagicItemValue( "ENEMIES_WM_WEAK" ) );

		return _maxEnemies;
	}

	//=====================================================

	private void SpawnNewEnemy()
	{
		// Determine random starting point for enemy
		var index = Random.Range( 0, _wayPoints.Length );
		var position = _wayPoints[index].transform.position;

		// Create enemy
		var enemy = AddNewEnemy( position );
		if( enemy == null ) return;

		_currentNumEnemies++;

		// Parent enemies to container
		enemy.transform.parent = _enemyContainer;
		enemy.name = "Enemy";

		// Spawn enemy and set waypoints
		if( _wayPoints.Length > 0 )
			enemy.OnSpawn( _wayPoints );

		// Register for its destroy event
		enemy.EnemyDestroyedEvent += OnEnemyDestroyedEvent;
	}

	//=====================================================

	private EnemyAI AddNewEnemy( Vector3 atPosition )
	{
		if( _pfbEnemy == null ) return null;

		var prefab = Instantiate( _pfbEnemy, atPosition, Quaternion.identity ) as GameObject;
		if( prefab == null ) return null;

		// Add enemy to list
		var scriptAI = prefab.GetComponent<EnemyAI>();

		if( scriptAI == null )
		{
			Destroy( prefab );
			return null;
		}

		_enemies.Add( scriptAI );

		return scriptAI;
	}

	//=====================================================

	private IEnumerator AutoReSpawnEnemies()
	{
		while( true )
		{
			yield return new WaitForSeconds( 20.0f );
			//Debug.Log( "Checking for enemy respawn" );

			if( _currentNumEnemies >= SetMaxEnemies() ) continue;

			var enemiesRemaining = new List<EnemyAI>();

			// Copy remaining enemies into temp list
			foreach( var enemy in _enemies )
			{
				if( enemy != null )
					enemiesRemaining.Add( enemy );
			}

			// Clear enemy list and reset with temp list
			_enemies.Clear();
			_enemies = enemiesRemaining;
			enemiesRemaining = null;

			// Spawn missing enemies
			while( _currentNumEnemies < _maxEnemies )
				SpawnNewEnemy();
		}
	}

	//=====================================================

	//private void SpawnEnemies()
	//{
	//	if( _enemies.Count > 0 )
	//	{
	//		for( var i = 0; i < _enemies.Count; i++ )
	//		{
	//			if( _enemies[i] != null )
	//			{
	//				// Position enemy at random starting point
	//				var index = Random.Range( 0, _wayPoints.Length );
	//				_enemies[i].transform.position = _wayPoints[index].transform.position;

	//				// Spawn enemy and set waypoints
	//				if( _wayPoints.Length > 0 )
	//					_enemies[i].OnSpawn( _wayPoints );
	//			}
	//		}
	//	}
	//}

	#endregion

	//=====================================================
}
