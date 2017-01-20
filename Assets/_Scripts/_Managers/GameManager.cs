using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
	// Events
	public event Action<bool> PauseEvent;
	public event Action<bool> PlayerDeathEvent;				// Listener: CustceneManager
	public event Action RestartEvent;						// Listener: CustceneManager
	public event Action CommonRoomEvent;					// Listener: CustceneManager
	public event Action<ICutsceneObject> KeyCollectedEvent;	// Listener: CustceneManager
	public event Action<bool> ClearEnemiesEvent;			// Listener: EnemyManager(s) <kill enemies?>

	// Boss events
	public event Action<ICutsceneObject> BossStartEvent;	// Listener: CustceneManager
	public event Action<ICutsceneObject> BossWinsEvent;		// Listener: CustceneManager
	public event Action<ICutsceneObject> BossLosesEvent;	// Listener: CustceneManager

	private static GameManager _instance;

	private bool _isInitialized;
	private bool _isPaused;
	private bool _isPlayerDead;
	private bool _isPlayerOkToRespawn;
	private bool _returnToStart;
	private bool _goToCommonRoom;
	private bool _isChangingFairy;
	private bool _isLeavingBossRoom;

	private float 	  _lastLocationStartTime = 0.0f;
	private eLocation _lastLocation = eLocation.NULL;
	private eLocation _targetLocation = eLocation.NULL;

	private Job _updateWildMagicRate;

	//=====================================================

	#region Public Interface

	public eLocation LastLocation { get { return _lastLocation; } }

	public eLocation CurrentLocation { get { return _targetLocation; } }

	public bool IsChangingFairy { get { return _isChangingFairy; } }

	public bool IsPaused { get { return _isPaused; }
}

	//=====================================================
	// Create an Instance of the game manager
	public static GameManager Instance
	{
		get
		{
			if( _instance != null ) return _instance;

			// Look for existing GameManager object in scene
			var gm = GameObject.FindGameObjectWithTag( UnityTags.GameManager );
			if( gm != null )
			{
				var script = gm.GetComponent<GameManager>();
				if( script != null )
				{
					_instance = script;
				}
			}
			// Otherwise, create new Instance
			else
			{
				// Because the GameManager is a component, we have to create a GameObject to attach it to.
				var managerObject = new GameObject( "GameManager" ) { tag = UnityTags.GameManager };

				// Add the DynamicObjectManager component, and set it as the defaultCenter
				_instance = (GameManager)managerObject.AddComponent( typeof( GameManager ) );
			}

			if( _instance != null )
				_instance.Init();

			return _instance;
		}
	}

	//=====================================================

	public void OnClearEnemies( bool killEnemies )
	{
		if( ClearEnemiesEvent != null )
			ClearEnemiesEvent( killEnemies );
	}

	//=====================================================

	public void OnPauseGame( bool pauseStatus )
	{
		PauseGame( pauseStatus );
	}

	//=====================================================

	public void OnPlayerDeath( bool isPlayerOkToRespawn )
	{
		_isPlayerDead = true;
		_isPlayerOkToRespawn = isPlayerOkToRespawn;

		switch( CurrentLocation )
		{
			default:
				StartCoroutine( GameDataManager.Instance.PlayerLives > 0.0f ? StartPlayerDeadCutscene() : ShowPlayerDeathPopup() );
				break;

			case eLocation.BOSS_ROOM:
				if( BossManager.Instance == null || BossManager.Instance.CutsceneBossWins == null ) return;

				_isLeavingBossRoom = true;

				SetNextLocation( eLocation.MAIN_HALL );

				// Play cutscene
				if( BossWinsEvent != null )
					BossWinsEvent( BossManager.Instance.CutsceneBossWins );

				// Apply wild magic penalty
				GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_BOSS_FIGHT_FAIL" ) );
				break;
		}
	}

	//=====================================================

	public void OnBossStartEvent()
	{
		if( BossManager.Instance == null ) return;

		// Play cutscene
		if( BossStartEvent != null )
			BossStartEvent( BossManager.Instance.CutsceneStart );
	}

	//=====================================================

	public void OnBossDeadEvent()
	{
		_isPlayerDead = false;

		if( BossManager.Instance == null || BossManager.Instance.CutsceneBossLoses == null ) return;

		_isLeavingBossRoom = true;

		SetNextLocation( eLocation.MAIN_HALL );

		// Play cutscene
		if( BossLosesEvent != null )
			BossLosesEvent( BossManager.Instance.CutsceneBossLoses );

		// Apply wild magic bonus
		if( PlayerPrefsWrapper.HasKey( "FirstBossKill" ) == false || PlayerPrefsWrapper.GetInt( "FirstBossKill" ) != 1 )
		{
			PlayerPrefsWrapper.SetInt( "FirstBossKill", 1 );

			GameDataManager.Instance.AddWildMagicAndPopulation(
				WildMagicItemsManager.GetWildMagicItem( "WM_RATE_BOSS_FIGHT_WIN_FIRST" ) );
		}
		else
		{
			GameDataManager.Instance.AddWildMagicAndPopulation(
				WildMagicItemsManager.GetWildMagicItem( "WM_RATE_BOSS_FIGHT_WIN_DEFAULT" ) );
		}

		// Increment boss level
		if( GameDataManager.Instance.PlayerBossLevel < GameDataManager.Instance.PlayerMaxFairyLevel )
			GameDataManager.Instance.PlayerBossLevel += 1;

		// If boss is at level 3 then start timer intervals between boss appearances
		// Note: time-interval-start checked by boss door on entering MainHall scene
		if( GameDataManager.Instance.PlayerBossLevel >= GameDataManager.Instance.PlayerMaxFairyLevel )
		{
			PlayerPrefsWrapper.SetDouble( "BossRoomTimedIntervalStarted", PreHelpers.UnixUtcNow() );
		}
	}

	//=====================================================

	public void OnReturnToStart()
	{
		_returnToStart = true;

		// Wait for cutscene completion before respawning player (fade-out triggered by a door or CutsceneManager)
		SetNextLocation( eLocation.MAIN_HALL );

		// Signal Cutscene to start fade-out. GameManager waits for fade-out completed event
		if( RestartEvent != null )
			RestartEvent();
	}

	//=====================================================

	public void OnGoToCommonRoom()
	{
		Debug.Log( "OnGoToCommonRoom" );

		_goToCommonRoom = true;

		// Wait for cutscene completion before respawning player (fade-out triggered by a door or CutsceneManager)
		SetNextLocation( eLocation.COMMON_ROOM );

		// Signal Cutscene to start fade-out. GameManager waits for fade-out completed event. PlayerManager plays fairy animation.
		if( CommonRoomEvent != null )
			CommonRoomEvent();
	}

	//=====================================================

	public void OnGoToMainHall()
	{
		OnExitPuzzleRoomFromPopup();
	}

	//=====================================================
	// 
	public void OnExitPuzzleRoomFromPopup()
	{
		// Wait for cutscene completion before respawning player (fade-out triggered by a door or CutsceneManager)
		SetNextLocation( eLocation.MAIN_HALL );

		// Signal Cutscene to start fade-out. GameManager waits for fade-out completed event
		if( RestartEvent != null )
			RestartEvent();
	}

	//=====================================================

	public void OnKeyCollected( ICutsceneObject obj, ePuzzleKeyType keyType )
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		// Eject player from puzzle-room - ensure player isn't flagged as dead
		_isPlayerDead = false;

		PlayerManager.OnCelebrate();

		StartCoroutine( ShowKeyCollectedPopup( obj, keyType ) );
	}

	//=====================================================

	public void InitFairy()
	{
		// Set correct fairy according to player-save
		OnChangeFairy( (eFairy)GameDataManager.Instance.PlayerCurrentFairy, true, false );
	}

	//=====================================================

	public bool OnChangeFairy( eFairy fairy, bool forceChange = false, bool useCurrentPosition = true )
	{
		// Check were not trying to change to the same fairy
		if( GameDataManager.Instance.PlayerCurrentFairy == (int)fairy && forceChange == false ) return false;

		// Get fairy prefab
		var pfb = GameDataManager.Instance.ChangeFairy( fairy, true );

		if( pfb == null ) return false;

		// Get world data for current fairy
		var position = PlayerManager.Position;
		var rotation = PlayerManager.Rotation;
		_isChangingFairy = true;

		// Destroy current fairy and instantiate the new one
		Destroy( PlayerManager.GameObject );
		Instantiate( pfb, position, rotation );

		// Spawn fairy - reset camera
		PlayerManager.SpawnPlayer( null, useCurrentPosition );

		return true;
	}

	//=====================================================

	public void DebugRespawnFairyNextLocation()
	{
		var location = SpawnManager.Instance.DebugGetNextRespawnPoint();

		// Spawn fairy - reset camera
		PlayerManager.SpawnPlayer( location );

		// Award all keys in scene
		GameDataManager.Instance.DebugAwardPlayerAllKeys( CurrentLocation, true );
	}

	//=====================================================

	public void SetNextLocation( eLocation target, bool registerForCutscene = true )
	{
		// Store current location as previous for use with alternate spawn-points in target room e.g. Main Hall
		_lastLocation = _targetLocation;
		_targetLocation = target;

		// If we have a valid start time for the last location then upload an analytics event
		// showing how long we've been in that location for
		if( _lastLocationStartTime > 0.0f )
		{
			var timeElapsed = Mathf.Clamp( Time.time - _lastLocationStartTime, 0.0f, 3600.0f );

			var eventDictionary = new Dictionary<string, object>();
			eventDictionary["locationID"] = _lastLocation.ToString();
			eventDictionary["timeSpent"] = timeElapsed;
			Analytics.CustomEvent( "VisitLocation", eventDictionary );
		}
		_lastLocationStartTime = Time.time;

		// Wait for cutscene completion before loading next scene (fade-out triggered by a door or CutsceneManager)
		if( registerForCutscene )
			CutsceneManager.CutsceneCompleteEvent += OnCutsceneCompleteEvent;
	}

	#endregion

	//=====================================================

	#region Private Methods

	// Constructor
	private GameManager() { }

	//=====================================================
	// Called after constructor
	private void Init()
	{
		if( _isInitialized == true ) return;

		DontDestroyOnLoad( this.gameObject );

		_isPaused = false;
		_isPlayerDead = false;
		_isPlayerOkToRespawn = true;
		_returnToStart = false;
		_goToCommonRoom = false;
		_isChangingFairy = false;
		_isLeavingBossRoom = false;
		_lastLocation = eLocation.MAIN_HALL;
		_targetLocation = eLocation.MAIN_HALL;

		// Look for GameLocation Instance
		var gameLocation = GameObject.Find( "GameLocation" );
		if( gameLocation != null )
		{
			var locationScript = gameLocation.GetComponent<GameLocation>();
			if( locationScript != null && locationScript.Location != eLocation.NULL )
			{
				_lastLocation = _targetLocation = locationScript.Location;
			}
		}

		// Update GameDataManager's wild magic at short intervals
		_updateWildMagicRate = new Job( GameDataManager.Instance.UpdateWildMagic( () =>
																				{
																					_updateWildMagicRate.Kill();
																					_updateWildMagicRate = null;
																				} ) );
		// Note: SceneManager now calls InitFairy()

		// Set up coroutine to check for new days - clear NPCs visited Player-data
		StartCoroutine( CheckForNewDay() );

		_isInitialized = true;

		//Debug.Log( "GameManager: location: " + _targetLocation );
	}

	//=====================================================

	//void Update()
	//{
	//	// DEBUG - REMOVE THIS
	//	if( Input.GetKeyDown( KeyCode.P ) )
	//	{
	//		PauseGame( !_isPaused );
	//	}
	//}

	//=====================================================

	void OnApplicationPause( bool pauseStatus )
	{
#if UNITY_IPHONE || UNITY_ANDROID
		PauseGame( pauseStatus );
#endif
		//if( pauseStatus == true ) return;
		
		// Unblock input after soft / hard lock on device
		//if( PopupPause.Instance != null && PopupPause.Instance.IsActive == false )
		//{
		//	if( InputManager.Instance != null )
		//		InputManager.Instance.OnBlockInput( false );
		//}
	}

	//=====================================================

	void OnApplicationQuit()
	{
		if( _updateWildMagicRate == null ) return;

		// Kill any active jobs
		_updateWildMagicRate.Kill();
		_updateWildMagicRate = null;
	}

	//=====================================================

	//void OnApplicationFocus( bool focusStatus )
	//	{
	//#if UNITY_ANDROID
	//		// Only apply on pause event when app loses focus e.g. device home-button press 
	//		if( focusStatus == true )
	//			PauseGame( focusStatus );
	//#endif
	//	}

	//=====================================================

	private IEnumerator CheckForNewDay()
	{
		while( true )
		{
			Debug.Log( "Check for new day" );

			if( PreHelpers.CheckForNewDay() == true )
				GameDataManager.Instance.ResetForNewDay( true );

			// Repeat check every hour
			yield return new WaitForSeconds( 3600.0f );
		}
	}

	//=====================================================

	private IEnumerator ShowKeyCollectedPopup( ICutsceneObject obj, ePuzzleKeyType keyType )
	{
		var isFinalKey = GameDataManager.Instance.GetNumKeysCollected( CurrentLocation ) >=
						 Convert.ToInt32( SettingsManager.GetSettingsItem( "NUM_PUZZLE_ROOM_KEYS", (int)CurrentLocation ) );

		yield return new WaitForSeconds( 1.5f );

		PopupKeyCollected.PopupKeyExitScene += OnPopupKeyExitScene;

		PopupKeyCollected.Instance.Show( obj, keyType, isFinalKey );
	}

	//=====================================================

	private void OnPopupKeyExitScene( ICutsceneObject obj, bool exitScene = true )
	{
		PopupKeyCollected.PopupKeyExitScene -= OnPopupKeyExitScene;

		// Remain in current scene? e.g. collecting 100 gems exposing 100_gem_key
		if( exitScene == false )
		{
			if( InputManager.Instance != null )
				InputManager.Instance.OnBlockInput( false );

			return;
		}

		SetNextLocation( eLocation.MAIN_HALL );

		// Signal Cutscene to start fade-out. GameManager waits for fade-out completed event
		if( KeyCollectedEvent != null )
			KeyCollectedEvent( obj );
	}

	//=====================================================

	private IEnumerator ShowPlayerDeathPopup()
	{
		yield return new WaitForSeconds( 0.75f );

		PopupPlayerDeath.PopupPlayerRevived += OnPopupPlayerRevived;
		PopupPlayerDeath.PopupPlayerDead += OnPopupPlayerDead;

		PopupPlayerDeath.Instance.Show();
	}

	//=====================================================

	private void OnPopupPlayerRevived()
	{
		PopupPlayerDeath.PopupPlayerRevived -= OnPopupPlayerRevived;
		PopupPlayerDeath.PopupPlayerDead -= OnPopupPlayerDead;

		_isPlayerOkToRespawn = true;

		StartCoroutine( StartPlayerDeadCutscene() );
	}

	//=====================================================

	private void OnPopupPlayerDead()
	{
		PopupPlayerDeath.PopupPlayerRevived -= OnPopupPlayerRevived;
		PopupPlayerDeath.PopupPlayerDead -= OnPopupPlayerDead;

		_isPlayerOkToRespawn = false;

		StartCoroutine( StartPlayerDeadCutscene() );
	}

	//=====================================================

	private IEnumerator StartPlayerDeadCutscene()
	{
		yield return new WaitForSeconds( 0.5f );

		// Wait for cutscene completion before respawning player (fade-out triggered by a door or CutsceneManager)
		if( GameDataManager.Instance.PlayerLives > 0 )
			SetNextLocation( CurrentLocation );
		else
			SetNextLocation( eLocation.MAIN_HALL );

		// Signal Cutscene to start fade-out. GameManager waits for fade-out completed event
		if( PlayerDeathEvent != null )
			PlayerDeathEvent( _isPlayerOkToRespawn );
	}

	//=====================================================
	// Once cutscene fade-out has completed, move player to next location
	private void OnCutsceneCompleteEvent()
	{
		CutsceneManager.CutsceneCompleteEvent -= OnCutsceneCompleteEvent;

		//=============================================
		// Return player to start of game. DEMO FEATURE
		if( _returnToStart == true )
		{
			_returnToStart = false;

			GameDataManager.Instance.ResetToPlayerDefaults( true );

			StartCoroutine( GoToNewScene( GetLocationName( _targetLocation ) ) );
		}
		// Take player to common room - don't save player data if in a puzzle room
		else if( _goToCommonRoom == true )
		{
			_goToCommonRoom = false;

			if( CurrentLocation == eLocation.MAIN_HALL )
			{
				// Update GameDataManager with current SceneManager data (player's gems)
				SceneManager.UpdateGameDataManager();
				GameDataManager.Instance.SaveGameData();
			}

			StartCoroutine( GoToNewScene( GetLocationName( _targetLocation ) ) );
		}
		// Player is leaving current scene
		else if( _isPlayerDead == false )
		{
			if( _isLeavingBossRoom == true )
			{
				_isLeavingBossRoom = false;
				// ToDo: something?
			}

			// Update GameDataManager with current SceneManager data (player's gems)
			SceneManager.UpdateGameDataManager();
			GameDataManager.Instance.SaveGameData();

			StartCoroutine( GoToNewScene( GetLocationName( _targetLocation ) ) );
		}
		// Player has died - respawn in scene or return player to main hall (no lives left)
		else
		{
			if( _isLeavingBossRoom == true )
			{
				_isLeavingBossRoom = false;

				// Reset player's health stats
				GameDataManager.Instance.ResetPlayerHealth();

				// Has player lost all lives
				if( _isPlayerOkToRespawn == false )
					GameDataManager.Instance.ResetPlayerLives( true );

				// Return player to Main Hall
				StartCoroutine( GoToNewScene( GetLocationName( eLocation.MAIN_HALL ) ) );
			}
			else if( _isPlayerOkToRespawn == true )
			{
				// Reset player health and respawn
				GameDataManager.Instance.ResetPlayerHealth( true );
				PlayerManager.RespawnPlayer( SpawnManager.Instance.GetRespawnPoint() );

				if( InputManager.Instance != null )
					InputManager.Instance.OnBlockInput( false );

				OnClearEnemies( false );
			}
			else
			{
				// Reset player's health stats
				GameDataManager.Instance.ResetPlayerLives();
				GameDataManager.Instance.ResetPlayerHealth( true );

				OnClearEnemies( true );

				// Return player to Main Hall
				StartCoroutine( GoToNewScene( GetLocationName( eLocation.MAIN_HALL ) ) );
			}
		}

		// Reset flag
		_isPlayerDead = false;
	}

	//=====================================================
	
	public static void UploadPlayerDataToServer()
	{
		// Save player data to the server
		var encoder = GameDataManager.Instance.GetJSON();
		if( (encoder != null) && (ServerManager.instance != null) )
		{
			ServerManager.instance.SavePlayerData( "playerData", encoder.serialized );
		}
	}

	//=====================================================

	private static IEnumerator GoToNewScene( string target )
	{
		// Save player data to the server
		UploadPlayerDataToServer();

		// Update high population score
		if( AchievementsManager.m_Instance != null )
		{
			AchievementsManager.m_Instance.ReportScore( GameDataManager.Instance.PlayerData.HighestEverPopulation );
		}

		// Update achievements
		GameDataManager.Instance.UpdateAchievements();

		yield return new WaitForSeconds( 0.25f );

		if( string.IsNullOrEmpty( target ) == false )
		{
			PlayerPrefsWrapper.SetString( "LoadingScreenScene", target );
			Application.LoadLevel( "LoadingScreen" );
		}
		else
			Debug.LogError( "GameManager->OnCutsceneCompleteEvent: " + target + " location not recognised" );
	}

	//=====================================================

	private void PauseGame( bool pauseStatus )
	{
		if( _isPaused == pauseStatus )
		{
			// Ensure GuiManager and InputManager are on the same page
			GuiManager.Instance.IsPaused = pauseStatus;
			InputManager.Instance.IsPaused = pauseStatus;
			return;
		}
		
		_isPaused = pauseStatus;

		if( PauseEvent != null )
			PauseEvent( _isPaused );

		if( AudioManager.Instance != null )
			AudioManager.Instance.OnPauseEvent( _isPaused );

		Debug.Log( ((_isPaused == true) ? "PAUSE GAME ... " : "RESUME GAME") );
	}

	//=====================================================

	private string GetLocationName( eLocation targetLocation )
	{
		switch( targetLocation )
		{
			case eLocation.NULL:
			case eLocation.NUM_PUZZLE_LOCATIONS:
			case eLocation.MAIN_HALL:
				return "MainHall";
			case eLocation.COMMON_ROOM:
				return "CommonRoom";
			case eLocation.BOSS_ROOM:
				return "BossRoom01";
			case eLocation.REFECTORY:
				return "RefectoryRoom";
			case eLocation.TRADING_CARD_ROOM:
				return "TradingCardsRoom";
			case eLocation.HIGHSCORES_ROOM:
				return "HighScoresRoom";
			case eLocation.CLOTHING_ROOM:
				return "ClothingRoom";
			default:	// Puzzle Rooms
				var loc = targetLocation.ToString();
				var index = loc.Substring( loc.Length - 2 );
				//Debug.Log( "Room index: " + index );
				return "PuzzleRoom" + index;
		}
	}

	#endregion

	//=====================================================
}
