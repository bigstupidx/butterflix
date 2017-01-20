using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class NPCManager : MonoBehaviourEMS, IPauseListener {

	public static NPCManager Instance { get; private set; }

	//[SerializeField] private Object[] _pfbNPCs;
	[SerializeField] private PathNode[] _wayPoints;			// Used as both spawnPoints and wayPoints for moving through scene

	// Private vars - serialized
	[SerializeField] private Transform _wayPointContainer;
	[SerializeField] private Transform _npcContainer;

	// Private vars - emoticons
	[SerializeField] private Sprite[] _emoticons;

	// Private vars
	private Transform _thisTransform;
	private List<NPC> _npcs;
	private int _maxNPCs;
	private int _npcPopulationRatio;
	private int _currentNumNPCs;
	private int _targetNumNPCs;

	//=====================================================

	#region Public Interface

	public Sprite GetEmoticon()
	{
		if( _emoticons != null && _emoticons.Length > 0 )
			return _emoticons[Random.Range( 0, _emoticons.Length )];

		return null;
	}

	//=====================================================

	public int NumStudentsAvailable()
	{
		var numStudents = 0;

		for( var i = 0; i < _targetNumNPCs; i++ )
		{
			// Spawn special npc e.g. Faragonda or a student
			if( (i + 1) < (int)eNPC.NUM_NPCS &&
				NPCItemsManager.GetNPCItemPopulationUnlock( (eNPC)(i + 1) ) < GameDataManager.Instance.PlayerPopulation &&
				GameDataManager.Instance.IsPlayerNPCUnlocked( (eNPC)(i + 1) ) == true )
			{
				// Ignore student count
			}
			else
			{
				numStudents++;
			}
		}

		Debug.Log( numStudents );

		return numStudents;
	}

	//=====================================================

	public void OnClearNPCsEvent( bool killNPCs )
	{
		// NPCs sleep or die
		if( _npcs != null && _npcs.Count > 0 )
		{
			foreach( var npc in _npcs )
			{
				if( npc == null ) continue;

				if( killNPCs == false )
					npc.OnSleep( true );
				else
					npc.OnDestroy();
			}
		}

		// Kill respawn checks
		//if( _checkForEnemyRespawn == null ) return;

		//_checkForEnemyRespawn.Kill();
		//_checkForEnemyRespawn = null;
	}

	//=====================================================

	public void Refresh()
	{
		_thisTransform.localRotation = Quaternion.identity;

		// Set spawnPoint / wayPoint container
		if( _wayPointContainer == null )
		{
			_wayPointContainer = new GameObject( "SpawnPointContainer" ).transform;

			_wayPointContainer.parent = _thisTransform;
			_wayPointContainer.rotation = Quaternion.identity;
			_wayPointContainer.position = Vector3.zero;
		}

		// Set enemy container
		if( _npcContainer == null )
		{
			_npcContainer = new GameObject( "NPCContainer" ).transform;

			_npcContainer.parent = _thisTransform;
			_npcContainer.rotation = Quaternion.identity;
			_npcContainer.position = Vector3.zero;
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

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		//if( _checkForEnemyRespawn == null ) return;

		//if( isPaused == true )
		//	_checkForEnemyRespawn.Pause();
		//else
		//	_checkForEnemyRespawn.Unpause();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		Instance = this;
		_thisTransform = this.transform;
		_npcs = new List<NPC>();
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameManager.Instance.ClearEnemiesEvent += OnClearNPCsEvent;

		// Determine how many npcs should be spawned - according to current wild magic rate
		_targetNumNPCs = 0;
		_currentNumNPCs = 0;
		_maxNPCs = Convert.ToInt32( SettingsManager.GetSettingsItem( "NPC_MAX_IN_SCENE", -1 ) );
		_npcPopulationRatio = Convert.ToInt32( SettingsManager.GetSettingsItem( "NPC_POPULATION_RATIO", -1 ) );
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		OnClearNPCsEvent( true );

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameManager.Instance.ClearEnemiesEvent -= OnClearNPCsEvent;
	}

	//=====================================================

	void Start()
	{
		if( Application.isPlaying == false ) return;

		_npcs.Clear();

		// Load enemy prefab from resources
		//_pfbNPCs = ResourcesNPCs.GetPrefab();

		// NPCs awaken
		//if( _npcs.Count > 0 )
		//{
		//	foreach( var npc in _npcs )
		//	{
		//		if( npc == null ) continue;

		//		_currentNumNPCs++;
		//		npc.OnSleep( false );
		//	}
		//}
		//else
		{
			_targetNumNPCs = Mathf.Clamp( GameDataManager.Instance.PlayerPopulation / _npcPopulationRatio, 0, _maxNPCs );

			// Instantiate and spawn enemies - parent enemies to container and set their waypoints
			for( var i = 0; i < _targetNumNPCs; i++ )
				SpawnNewNPC();
		}
	}

	//=====================================================
	// ToDo: GAME UPDATE (v1.1) - Add missing special character models (Avalon, Palladium, Wizgiz) - see GameDataManager.IsPlayerNPCUnlocked
	private void SpawnNewNPC()
	{
		// Choose appropriate NPC depending on population level and which NPCs have been unlocked
		Object pfbNPC = null;

		_currentNumNPCs++;

		// Spawn special npc e.g. Faragonda or a student
		if( _currentNumNPCs < (int)eNPC.NUM_NPCS &&
			NPCItemsManager.GetNPCItemPopulationUnlock( (eNPC)_currentNumNPCs ) < GameDataManager.Instance.PlayerPopulation &&
			GameDataManager.Instance.IsPlayerNPCUnlocked( (eNPC)_currentNumNPCs ) == true )
		{
			//Debug.Log( "s.NPC" + _pfbNPCs[_currentNumNPCs].name );
			//pfbNPC = _pfbNPCs[_currentNumNPCs];
			pfbNPC = ResourcesNPCs.GetSpecialsPrefab( _currentNumNPCs );
		}
		else
		{
			//Debug.Log( "n.NPC" + _pfbNPCs[0].name );
			//pfbNPC = _pfbNPCs[0];
			pfbNPC = ResourcesNPCs.GetStudentPrefab();
		}

		if( pfbNPC == null ) return;

		// Determine random starting point for enemy
		var index = Random.Range( 0, _wayPoints.Length );
		var position = _wayPoints[index].transform.position;

		// Create npc
		var npc = AddNewNPC( pfbNPC, position );
		if( npc == null ) return;

		// Parent enemies to container
		npc.transform.parent = _npcContainer;
		npc.name = "NPC";

		// Spawn enemy and set waypoints
		if( _wayPoints.Length > 0 )
			npc.OnSpawn( _wayPoints );
	}

	//=====================================================

	private NPC AddNewNPC( Object npc, Vector3 atPosition )
	{
		if( npc == null ) return null;

		var prefab = Instantiate( npc, atPosition, Quaternion.identity ) as GameObject;
		if( prefab == null ) return null;

		// Add npc to list
		var scriptNPC = prefab.GetComponent<NPC>();

		if( scriptNPC == null )
		{
			Destroy( prefab );
			return null;
		}

		_npcs.Add( scriptNPC );

		return scriptNPC;
	}

	#endregion

	//=====================================================
}
