using UnityEngine;
using System;
using System.Collections;

[RequireComponent( typeof( PlayerMovement ) )]
[RequireComponent( typeof( Rigidbody ) )]
[RequireComponent( typeof( CapsuleCollider ) )]
[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( AudioSource ) )]
public class PlayerManager : MonoBehaviourEMS
{
	[SerializeField] private GameObject _pfbRedGemText;
	[SerializeField] private AudioClip _clipFootstep;
	[SerializeField] private AudioClip _clipCastSpell;

	private static Transform _thisTransform;
	private static Rigidbody _rigidbody;
	private static PlayerMovement _playerMovement;
	private static ePlayerAction _currentPlayerAction = ePlayerAction.NULL;
	private static IPlayerInteraction _currentInteractiveObject;
	private static eDamageType _currentDamageType = eDamageType.NULL;
	private static Transform _currentSpawnPoint;
	private eCutsceneType _currentCutsceneType = eCutsceneType.NULL;

	private GameObject _blobShadow;
	private static AudioSource _audioSource;
	private static AudioClip _clipRespawn;
	private static AudioClip _clipCelebrate;
	private static AudioClip _clipDeath;
	private static AudioClip _clipInjured;
	private static AudioClip _clipShoutout;
	private static AudioClip _clipSpell;
	private static GameObject _pfbCurrentSpell;
	private static GameObject _pfbChangeFairyFx;
	private Vector3 _spellTarget;
	private static bool _playDelayedSpawnPlayer;
	private static bool _resetCameraPositionOnSpawn;

	//=====================================================

	#region Public Interface

	public static GameObject GameObject { get { return (_thisTransform != null) ? _thisTransform.gameObject : null; } }

	public static Transform Transform { get { return _thisTransform; } }

	public static Vector3 Position { get { return (_thisTransform != null) ? _thisTransform.position : Vector3.zero; } set { _thisTransform.position = value; } }

	public static Vector3 Direction { get { return (_thisTransform != null) ? _thisTransform.forward : Vector3.zero; } set { _thisTransform.forward = value; } }

	public static Quaternion Rotation { get { return (_thisTransform != null) ? _thisTransform.rotation : Quaternion.identity; } set { _thisTransform.rotation = value; } }

	public static float SpeedSqrMag { get { return (_rigidbody != null) ? _rigidbody.velocity.sqrMagnitude : 0.0f; } }

	public IPlayerInteraction InteractiveObject
	{
		set
		{
			if( value.GetType() == typeof( IPlayerInteraction ) )
				_currentInteractiveObject = value;
		}
	}

	public static bool IsPushing { get { return _playerMovement.IsPushingStateActive(); } }

	//=====================================================

	public void OnPlayFootstep()
	{
		if(_audioSource == null)
		{
			_audioSource = _thisTransform.GetComponent<AudioSource>();
		}
		if( _audioSource.isPlaying == false )
			_audioSource.Play();
	}

	//=====================================================
	// Store available playerAction and gameObject
	// - action checked and activated when player uses Action button (see OnPerformActionEvent) e.g. open door
	public static void OnActionOk( ePlayerAction playerAction, IPlayerInteraction interactiveObject = null, Transform spawnPoint = null )
	{
		if( playerAction != ePlayerAction.NULL )
		{
			_currentPlayerAction = playerAction;
			_currentInteractiveObject = interactiveObject;
			_currentSpawnPoint = spawnPoint;

			if( GuiManager.Instance != null )
				GuiManager.Instance.OnPlayerActionAvailable( _currentPlayerAction );
		}
		else
		{
			ClearCurrentAction();
		}
	}

	//=====================================================

	public static void OnObstacleHit( eDamageType type, Vector3 hitPoint )
	{
		if( GameDataManager.Instance.PlayerHealth <= 0 )
			return;

		_currentDamageType = type;

		switch( type )
		{
			case eDamageType.LOW:
			case eDamageType.MEDIUM:
			case eDamageType.HIGH:
			case eDamageType.EXTREME:
				Debug.Log( "Player takes " + type + " damage" );
				break;
			case eDamageType.CONSTANT:
				// ToDo: triggers causing constant damage should call OnObstacleHit at set intervals while player is within bounds
				Debug.Log( "Player takes CONSTANT damage" );
				break;
		}

		// Exit if NULL damage type
		if( _currentDamageType == eDamageType.NULL ) return;

		// Adjust current health
		GameDataManager.Instance.AddPlayerHealth( -(Convert.ToInt32( SettingsManager.GetSettingsItem( "DAMAGE", (int)_currentDamageType ) )) );

		// Play between damaged and death animations
		if( GameDataManager.Instance.PlayerHealth > 0.0f )
		{
			_playerMovement.OnDamaged( hitPoint );

			// Play sfx
			if( _audioSource != null )
				_audioSource.PlayOneShot( _clipInjured );
		}
		else
		{
			_playerMovement.OnDeath( hitPoint );

			// Play sfx
			if( _audioSource != null )
				_audioSource.PlayOneShot( _clipDeath );

			var isPlayerOkToRespawn = false;

			if( GameDataManager.Instance.PlayerLives > 0 )
			{
				isPlayerOkToRespawn = true;

				// Add penalty to wild magic rate and population
				GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_LOSE_LIFE" ) );
			}
			else
			{
				// Add penalty to wild magic rate and population
				GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_DIE" ) );
			}

			GameManager.Instance.OnPlayerDeath( isPlayerOkToRespawn );
		}
	}

	//=====================================================

	public static void OnTrapEntered( Vector3 trapPosition )
	{
		_playerMovement.IsPlayerActionRequired = true;
		_playerMovement.IsTrapped = true;

		Position = trapPosition;
	}

	//=====================================================

	public static void OnTrapEscaped()
	{
		_playerMovement.IsEscapingTrap = true;
	}

	//=====================================================

	public static void OnCancelAction()
	{
		_playerMovement.ResetActionFlags();

		ClearCurrentAction();
	}

	//=====================================================

	public static void OnCelebrate()
	{
		_playerMovement.IsPlayerActionRequired = true;
		_playerMovement.IsCutsceneCelebrate = true;

		// Play sfx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipCelebrate );

		ClearCurrentAction();
	}

	//=====================================================

	public static void OnPlayCutsceneAnimation( eCutsceneType cutsceneType )
	{
		switch( cutsceneType )
		{
			case eCutsceneType.ENTER_BOSS_ROOM:
			case eCutsceneType.ENTER_PUZZLE_ROOM:
			case eCutsceneType.ENTER_PLAYER_HUB:
			case eCutsceneType.LEAVE_BOSS_ROOM:
			case eCutsceneType.LEAVE_PLAYER_HUB:
			case eCutsceneType.LEAVE_PUZZLE_ROOM:
				_playerMovement.IsPlayerActionRequired = true;
				_playerMovement.IsCutsceneWalk = true;
				break;

			case eCutsceneType.OBLIVION_PORTAL:
				_playerMovement.IsPlayerActionRequired = true;
				_playerMovement.IsCutscenePortal = true;
				break;

			case eCutsceneType.CRAWL_DOOR:
				_playerMovement.IsPlayerActionRequired = true;
				_playerMovement.IsCutsceneCrawl = true;
				break;

			case eCutsceneType.COLLECT_KEY:
			case eCutsceneType.BOSS_ROOM_BOSS_LOSES:
				_playerMovement.IsPlayerActionRequired = true;
				_playerMovement.IsCutsceneCelebrate = true;
				break;
		}
	}

	//=====================================================

	public static void RespawnPlayer( Transform respawnPoint = null )
	{
		SpawnPlayer( respawnPoint );
	}

	//=====================================================

	public static void SpawnPlayer( Transform respawnPoint = null, bool useCurrentPosition = false )
	{
		// Fairy is respawning after a death? Set respawn location
		if( respawnPoint != null )
			_currentSpawnPoint = respawnPoint;

		// Move fairy to current spawn point and match forward vector to spawn point's
		if( _currentSpawnPoint != null )
		{
			_thisTransform.position = _currentSpawnPoint.position + new Vector3( 0.0f, 0.05f, 0.0f );
			_thisTransform.forward = _currentSpawnPoint.forward;
		}
		else
		{
			// Set fairy at default spawn point or zero in scene
			if( useCurrentPosition == false && SpawnManager.Instance != null )
			{
				_currentSpawnPoint = SpawnManager.Instance.GetSpawnPoint();
				
				if( _currentSpawnPoint != null )
				{
					_thisTransform.position = _currentSpawnPoint.position + new Vector3( 0.0f, 0.05f, 0.0f );
					_thisTransform.forward = _currentSpawnPoint.forward;
				}
				else
				{
					_thisTransform.position = Vector3.zero + new Vector3( 0.0f, 0.05f, 0.0f );
					_thisTransform.forward = Vector3.forward;
				}
			}
		}

		// Pull spell and other fx from Resources for current fairy type and level
		SetSpell();
		SetChangeFairyFx();

		// Play sfx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipRespawn );

		// Reset camera?
		_resetCameraPositionOnSpawn = !useCurrentPosition;

		// Hack work around to introduce short delays while spawning player
		// (static function means we can't use StartCoroutine or Invoke)
		// See this.Update() for spawning fairy and camera
		_playDelayedSpawnPlayer = true;

		// Clear spawn point
		_currentSpawnPoint = null;
		
		// If we're using 'high quality' mode then switch on 'FastGPU' object layer
		GameDataManager.Instance.SetCameraGPUFlags();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = transform;
		_rigidbody = _thisTransform.GetComponent<Rigidbody>();
		_playerMovement = _thisTransform.GetComponent<PlayerMovement>();
		
		Projector thisProjector = _thisTransform.GetComponentInChildren<Projector>();
		if(thisProjector != null )
			_blobShadow = thisProjector.gameObject;
		else
			_blobShadow = null;
		
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		_audioSource.clip = _clipFootstep;
		_clipRespawn = ResourcesSpellsAndFX.GetAudioClip( eFairy.NULL, "Respawn" );

		var name = GameDataManager.Instance.PlayerCurrentFairyName;
		_clipCelebrate = ResourcesSpellsAndFX.GetAudioClip( name, "Celebrate" );
		_clipDeath = ResourcesSpellsAndFX.GetAudioClip( name, "Death" );
		_clipInjured = ResourcesSpellsAndFX.GetAudioClip( name, "Injured" );
		//_clipSpell = ResourcesSpellsAndFX.GetAudioClip( name, "Spell" );

		if( _blobShadow != null )
			_blobShadow.SetActive( false );

		ClearCurrentAction();

		_playDelayedSpawnPlayer = false;
		_resetCameraPositionOnSpawn = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		InputManager.PerformActionEvent += OnPerformActionEvent;
		InputManager.CastSpell += OnCastSpell;
		GameManager.Instance.CommonRoomEvent += OnCommonRoomEvent;
		SceneManager.RedGemEvent += OnRedGemEvent;
		GameDataManager.Instance.QualityChangeEvent += OnQualityChangeEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		InputManager.PerformActionEvent -= OnPerformActionEvent;
		InputManager.CastSpell -= OnCastSpell;
		CutsceneManager.CutsceneCompleteEvent -= OnCutsceneCompleteEvent;
		GameManager.Instance.CommonRoomEvent -= OnCommonRoomEvent;
		SceneManager.RedGemEvent -= OnRedGemEvent;
		GameDataManager.Instance.QualityChangeEvent -= OnQualityChangeEvent;
	}

	//=====================================================

	//void Start()
	//{
	//	// See GameManager->Init->InitFairy->OnChangeFairy->PlayerManager.SpawnPlayer()
	//}

	//=====================================================

	void Update()
	{
		if( _playDelayedSpawnPlayer == false ) return;

		// Delayed calls to animate player when spawning and fire particle fx
		StartCoroutine( DelayedSpawnPlayer() );
		_playDelayedSpawnPlayer = false;
	}

	//=====================================================

	private void OnQualityChangeEvent( int quality )
	{
		// Switch blob shadow on / off according to current render quality setting
		if( _blobShadow != null )
			_blobShadow.SetActive( quality != 0 );
	}

	//=====================================================

	private static IEnumerator DelayedSpawnPlayer()
	{
		yield return new WaitForSeconds( 0.05f );

		// Link camera with fairy (reset position is spawning rather than just changing fairy)
		Camera.main.GetComponent<CameraMovement>().InitWithPlayer( _resetCameraPositionOnSpawn );
		
		yield return new WaitForSeconds( 0.1f );

		// Player is respawning after a death - trigger respawn animation
		_playerMovement.OnRespawn();

		yield return new WaitForSeconds( 0.3f );

		// Instantiate particle fx
		var fx = Instantiate( _pfbChangeFairyFx, _thisTransform.position, Quaternion.identity );
		fx.name = "ChangeFairyFx";
	}

	//=====================================================

	private void OnPerformActionEvent()
	{
		var startCutscene = false;
		_currentCutsceneType = eCutsceneType.NULL;

		switch( _currentPlayerAction )
		{
			case ePlayerAction.CLIMB_UP:
				_playerMovement.IsClimbingUpOk = true;
				break;

			case ePlayerAction.CLIMB_DOWN:
				_playerMovement.IsClimbingDownOk = true;
				break;

			case ePlayerAction.USE_FLOOR_LEVER:
				if( _currentInteractiveObject != null && _currentInteractiveObject.IsInteractionOk() == true )
				{
					_playerMovement.IsUseFloorLeverOk = true;
					_playerMovement.IsPlayerActionRequired = true;
					_currentInteractiveObject.OnPlayerInteraction();

					// Note: Switch activation triggers door / object to start cutscene
				}
				break;

			case ePlayerAction.USE_WALL_LEVER:
				if( _currentInteractiveObject != null && _currentInteractiveObject.IsInteractionOk() == true )
				{
					_playerMovement.IsUseWallLeverOk = true;
					_playerMovement.IsPlayerActionRequired = true;
					_currentInteractiveObject.OnPlayerInteraction();

					// Note: Switch activation triggers door / object to start cutscene
				}
				break;

			case ePlayerAction.OPEN_DOOR:
				if( _currentInteractiveObject != null )
				{
					if( _currentInteractiveObject.IsInteractionOk() == true )
					{
						_playerMovement.IsOpenDoorOk = true;
						_currentInteractiveObject.OnPlayerInteraction();
					}
					else
						_playerMovement.IsInteractFail = true;
				}
				break;

			case ePlayerAction.CRAWL_THROUGH_DOOR:
				// Start cutscene - crawl through door - then move player to new spawn point				
				_currentCutsceneType = eCutsceneType.CRAWL_DOOR;
				startCutscene = true;
				break;

			case ePlayerAction.TELEPORT_OBLIVION_PORTAL:
				// Start cutscene - teleport from portal - then move player to new spawn point
				_currentCutsceneType = eCutsceneType.OBLIVION_PORTAL;
				startCutscene = true;
				break;

			case ePlayerAction.ENTER_PUZZLE_ROOM:
				// Start cutscene - move to next scene
				_playerMovement.IsOpenDoorOk = true;
				_playerMovement.IsPlayerActionRequired = true;
				// Update GameManager with target-scene and open door during cutscene fade-out
				_currentInteractiveObject.OnPlayerInteraction();
				_currentCutsceneType = eCutsceneType.ENTER_PUZZLE_ROOM;
				startCutscene = true;
				break;

			case ePlayerAction.LEAVE_PUZZLE_ROOM:
				// Start cutscene - move to next scene
				_playerMovement.IsOpenDoorOk = true;
				_playerMovement.IsPlayerActionRequired = true;
				// Update GameManager with target-scene and open door during cutscene fade-out
				_currentInteractiveObject.OnPlayerInteraction();
				_currentCutsceneType = eCutsceneType.LEAVE_PUZZLE_ROOM;
				startCutscene = true;
				break;

			case ePlayerAction.ENTER_PLAYER_HUB:
				// Start cutscene - move to next scene
				_playerMovement.IsOpenDoorOk = true;
				_playerMovement.IsPlayerActionRequired = true;
				// Update GameManager with target-scene and open door during cutscene fade-out
				_currentInteractiveObject.OnPlayerInteraction();
				_currentCutsceneType = eCutsceneType.ENTER_PLAYER_HUB;
				startCutscene = true;
				break;

			case ePlayerAction.LEAVE_PLAYER_HUB:
				// Start cutscene - move to next scene
				_playerMovement.IsOpenDoorOk = true;
				_playerMovement.IsPlayerActionRequired = true;
				// Update GameManager with target-scene and open door during cutscene fade-out
				_currentInteractiveObject.OnPlayerInteraction();
				_currentCutsceneType = eCutsceneType.LEAVE_PLAYER_HUB;
				startCutscene = true;
				break;

			case ePlayerAction.ENTER_BOSS_ROOM:
				if( _currentInteractiveObject != null && _currentInteractiveObject.IsInteractionOk() == true )
				{
					// Start cutscene - move to next scene
					_playerMovement.IsOpenDoorOk = true;
					_playerMovement.IsPlayerActionRequired = true;
					// Update GameManager with target-scene and open door during cutscene fade-out
					_currentInteractiveObject.OnPlayerInteraction();
					_currentCutsceneType = eCutsceneType.ENTER_BOSS_ROOM;
					startCutscene = true;
				}
				else
					_playerMovement.IsInteractFail = true;
				break;

			case ePlayerAction.LEAVE_BOSS_ROOM:
				// Start cutscene - move to next scene
				_playerMovement.IsOpenDoorOk = true;
				_playerMovement.IsPlayerActionRequired = true;
				// Update GameManager with target-scene and open door during cutscene fade-out
				_currentInteractiveObject.OnPlayerInteraction();
				_currentCutsceneType = eCutsceneType.LEAVE_BOSS_ROOM;
				startCutscene = true;
				break;

			case ePlayerAction.PUSH_OBJECT:
				if( _currentInteractiveObject != null )
				{
					if( _currentInteractiveObject.IsInteractionOk() == true )
					{
						_playerMovement.IsPushingOk = true;
						_currentInteractiveObject.OnPlayerInteraction();
					}
					else
					{
						_playerMovement.IsPushingNotOk = true;
					}

					// Trigger action but don't clear it - pushing can be looped until exiting obstacle collider
					_playerMovement.IsPlayerActionRequired = true;
				}
				return;

			case ePlayerAction.CAST_SPELL_ATTACK:
			case ePlayerAction.CAST_SPELL_MELT:
			case ePlayerAction.CAST_SPELL_DISABLE_TRAP:
				// Don't allow casting spells in the tutorial scene
				if( GameManager.Instance.CurrentLocation == eLocation.TUTORIAL )
				{
					ClearCurrentAction();
					return;
				}

				// ToDo: alter CastSpell to suit playerAction
				// Face player towards target / enemy
				var dir = _spellTarget - _thisTransform.position;
				Direction = new Vector3( dir.x, 0.0f, dir.z );

				// Set player ready to cast spell (play animation)
				_playerMovement.IsCastAttackSpellOk = true;

				// Trigger cast-spell action (plays animation) but don't clear it - player may cast more than one spell while near enemy
				//_playerMovement.IsPlayerActionRequired = true;

				// Cast spell
				StartCoroutine( CastSpell( _spellTarget ) );
				break;

			case ePlayerAction.OPEN_CHEST:
				if( _currentInteractiveObject != null && _currentInteractiveObject.IsInteractionOk() == true )
				{
					_playerMovement.IsOpenChestOk = true;
					_playerMovement.IsPlayerActionRequired = true;
					_currentInteractiveObject.OnPlayerInteraction();
				}
				else
					_playerMovement.IsInteractFail = true;
				break;
		}

		if( startCutscene )
		{
			CutsceneManager.CutsceneCompleteEvent += OnCutsceneCompleteEvent;
			CutsceneManager.Instance.OnStartCutscene( _currentCutsceneType, _currentInteractiveObject );
			ClearCurrentAction();

			return;
		}

		// Defaults to jump if no player-action has been recorded
		if( _playerMovement.IsJumpingOk == true )
			_playerMovement.IsPlayerActionRequired = true;

		ClearCurrentAction();
	}

	//=====================================================

	private void OnRedGemEvent( int numGems )
	{
		var gemText = Instantiate( _pfbRedGemText, _thisTransform.position + (Vector3.up * 2.0f), Quaternion.identity ) as GameObject;

		if( gemText == null ) return;

		// Update text with number of red gems
		gemText.transform.GetComponent<GuiBouncingText>().Text = numGems.ToString();

		// Apply force away from player
		gemText.GetComponent<Rigidbody>().AddForce( (_thisTransform.forward + Vector3.up).normalized * 5.0f, ForceMode.Impulse );
	}

	//=====================================================

	private static void SetSpell()
	{
		_pfbCurrentSpell = ResourcesSpellsAndFX.GetPrefabSpell( GameDataManager.Instance.PlayerCurrentFairyName,
																GameDataManager.Instance.PlayerCurrentFairyLevel ) as GameObject;
	}

	//=====================================================

	private static void SetChangeFairyFx()
	{
		_pfbChangeFairyFx = ResourcesSpellsAndFX.GetPrefabFx( GameDataManager.Instance.PlayerCurrentFairyName ) as GameObject;
	}

	//=====================================================

	private void OnCastSpell( Vector3 target, ePlayerAction playerAction )
	{
		_spellTarget = target;
		_currentPlayerAction = playerAction;

		OnPerformActionEvent();
	}

	//=====================================================

	private IEnumerator CastSpell( Vector3 target, int type = 0 )
	{
		// Set up temp gameObject rotated towards target
		var dummy = new GameObject();
		dummy.transform.position = _thisTransform.position + new Vector3( 0.0f, 1.2f, 0.0f );
		dummy.transform.LookAt( target + new Vector3( 0.0f, 0.6f, 0.0f ) );

		// Delay for fairy animation to reach 'cast spell' position
		yield return new WaitForSeconds( 0.5f );

		var spell = Instantiate( _pfbCurrentSpell, dummy.transform.position, dummy.transform.rotation );
		spell.name = "FairySpell";

		// Play sfx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipCastSpell );

		Destroy( dummy );
	}

	//=====================================================

	private void OnCutsceneCompleteEvent()
	{
		CutsceneManager.CutsceneCompleteEvent -= OnCutsceneCompleteEvent;

		switch( _currentCutsceneType )
		{
			case eCutsceneType.CRAWL_DOOR:
			case eCutsceneType.OBLIVION_PORTAL:
				SpawnPlayer();
				break;
		}
	}

	//=====================================================

	private void OnCommonRoomEvent()
	{
		GameManager.Instance.CommonRoomEvent -= OnCommonRoomEvent;

		// ToDo: Play leaving scene animation - play respawn animation for now
		_playerMovement.OnRespawn();
	}

	//=====================================================

	private static void ClearCurrentAction()
	{
		_currentPlayerAction = ePlayerAction.NULL;
		_currentInteractiveObject = null;

		if( GuiManager.Instance != null )
			GuiManager.Instance.OnPlayerActionAvailable( _currentPlayerAction );
	}

	#endregion

	//=====================================================
}
