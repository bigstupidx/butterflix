using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( AudioSource ) )]
public class Door : MonoBehaviourEMS, IPauseListener, IPlayerInteraction
{
	public enum eHingePosition { LEFT = 0, RIGHT }

	// Editable in inspector
	[SerializeField] private eDoorType _type = eDoorType.BASIC;
	[SerializeField] private int _model = 0;
	[SerializeField] private eHingePosition _hingePosition = eHingePosition.LEFT;
	[SerializeField] private float _rotateOpenBy = 90.0f;
	[SerializeField] private float _rotateDuration = 1.0f;
	[SerializeField] private bool _hasDoubleDoor = false;
	[SerializeField] private int _keyLevel = 0;
	[SerializeField] private eFairy _fairyRequired = eFairy.NULL;
	[SerializeField] private Animator[] _locks;

	// Audio
	[SerializeField] private AudioClip _clipOpen;
	[SerializeField] private AudioClip _clipPuzzleUnlocked;

	// Editable - Basic / Basic-Double
	[SerializeField] private bool _autoClose = false;
	[SerializeField] private float _autoCloseDelay = 5.0f;
	[SerializeField] private bool _openInAndOut = false;

	// Editable - Puzzle Room Entrance / Exit & Boss Room
	[SerializeField] private eLocation _targetScene = eLocation.NULL;

	// Editable - Puzzle Door
	//[SerializeField] private int _numSwitches;
	[SerializeField] private Switch[] _switches;

	// Editable - Crawl Door / Oblivion Portal
	[SerializeField] private SpawnPoint _spawnPoint;

	// Editable - Boss Room
	[SerializeField] private BossTorch[] _bossTorches;

	// Other vars
	[SerializeField] private CutsceneContainer _cutsceneContainer;
	[SerializeField] private GuiBubbleDoorInteraction _guiDoorInteraction;
	[SerializeField] private GameObject _thisGameObject;
	[SerializeField] private Transform _thisTransform;
	[SerializeField] private string _currentModelName;
	[SerializeField] private Transform _door;
	[SerializeField] private Transform _trigger;
	[SerializeField] private Collider _collider;
	[SerializeField] private bool _isDoubleDoor = false;
	[SerializeField] private GameObject _doubleDoorsContainer;
	[SerializeField] private GameObject _doubleDoor;
	[SerializeField] private Door _doubleDoorScript;

	// Other vars - non-serialized
	private AudioSource _audioSource;
	private Vector3 _triggerSize = new Vector3( 1.0f, 0.3f, 2.5f );
	private bool _isOpen = false;
	private bool _isRotatingDoor = false;
	private bool _queueOpenDoor = false;
	private bool _queueCloseDoor = false;
	private int _tweenId = -1;
	private bool _isStartInteractionChecked;

	//=====================================================

	#region Public Inteface

	public eDoorType Type { get { return _type; } set { _type = value; } }

	public int Model { get { return _model; } set { _model = value; } }

	public eHingePosition HingePosition { get { return _hingePosition; } set { _hingePosition = value; } }

	public bool HasDoubleDoor { get { return _hasDoubleDoor; } set { _hasDoubleDoor = value; } }

	public bool IsDoubleDoor { get { return _isDoubleDoor; } set { _isDoubleDoor = value; } }

	public float RotateOpenBy { get { return _rotateOpenBy; } set { _rotateOpenBy = Mathf.Clamp( value, 45.0f, 180.0f ); } }

	public float RotateDuration { get { return _rotateDuration; } set { _rotateDuration = Mathf.Clamp( value, 1.0f, 5.0f ); } }

	public bool AutoClose { get { return _autoClose; } set { _autoClose = value; } }

	public float AutoCloseDelay { get { return _autoCloseDelay; } set { _autoCloseDelay = Mathf.Clamp( value, 1.0f, 10.0f ); } }

	public bool OpenInAndOut { get { return _openInAndOut; } set { _openInAndOut = value; } }

	public Transform SpawnPoint { get { return _spawnPoint.transform; } }

	public eLocation TargetScene { get { return _targetScene; } set { _targetScene = value; } }

	public int KeyLevel { get { return _keyLevel; } set { _keyLevel = value; } }

	public eFairy FairyRequired { get { return _fairyRequired; } set { _fairyRequired = value; } }

	//=====================================================
	//public void Init( GameObject doorModel, bool isDoubleDoor = false )
	public void Init( bool isDoubleDoor = false )
	{
		_isDoubleDoor = isDoubleDoor;
		var resetDoor = false;
		var mdlDoor = ResourcesDoors.GetModel( _type, _model );

		if( _door == null )
		{
			resetDoor = true;
		}
		else if( _currentModelName != mdlDoor.name )
		{
			// Remove previous door instance
			DestroyImmediate( _door.gameObject );
			resetDoor = true;
		}
		// Hack to force oblivion / boss doors to update to new replacement door
		// (Late project updates - there is only one door model at present so can't switch between models to trigger refresh)
		else if( _type == eDoorType.OBLIVION_PORTAL || _type == eDoorType.BOSS )
		{
			// Remove previous door instance
			DestroyImmediate( _door.gameObject );
			resetDoor = true;
		}

		if( resetDoor == true )
		{
			_currentModelName = mdlDoor.name;

			var doorModel = Instantiate( mdlDoor ) as GameObject;
			if( doorModel == null ) return;

			_door = doorModel.transform;
			_door.parent = _thisTransform;
			_door.name = "Door";

			// Get collider to determine door size
			_collider = _door.GetComponent<Collider>();
		}

		// Store this door prefab's rotation then zero it out before updating door - doesn't handle double-doors yet
		Quaternion rot;
		if( _doubleDoorsContainer == null )
		{
			rot = _thisTransform.rotation;
			_thisTransform.rotation = Quaternion.identity;
		}
		else
		{
			rot = _doubleDoorsContainer.transform.rotation;
			_doubleDoorsContainer.transform.rotation = Quaternion.identity;

		}

		// Rotate and position door
		if( _hingePosition == eHingePosition.LEFT )
		{
			_door.localRotation = Quaternion.Euler( 0.0f, 180.0f, 0.0f );
			_door.localPosition = new Vector3( _collider.bounds.size.x * 0.5f, _collider.bounds.size.y * 0.5f, 0.0f );
		}
		else
		{
			_door.localRotation = Quaternion.Euler( Vector3.zero );
			_door.localPosition = new Vector3( _collider.bounds.size.x * -0.5f, _collider.bounds.size.y * 0.5f, 0.0f );
		}

		// Initialize door-trigger (only for single / master door)
		if( _isDoubleDoor == false )
			InitDoorTrigger();
		else
		{
			// Disable double door's trigger
			_trigger.gameObject.SetActive( false );

			// Remove any double door instance in this double door - this shouldn't be required
			if( _doubleDoor == true )
			{
				Destroy( _doubleDoor );
				_doubleDoor = null;
			}
		}

		// Reset the door prefab rotation
		if( _doubleDoorsContainer == null )
			_thisTransform.rotation = rot;
		else
			_doubleDoorsContainer.transform.rotation = rot;
	}

	//=====================================================

	public void Refresh()
	{
		CheckReferences();

		// Update switch indexes
		if( _type == eDoorType.PUZZLE_LOCKED )
		{
			for( var i = 0; i < _switches.Length; i++ )
			{
				if( _switches[i] != null )
				{
					_switches[i].Index = i;
					//Debug.Log( "Switch " + _switches[i].Index + " found: activated: " + _switches[i].IsActivated );
				}
			}
		}

		// Update door-model
		//var mdlDoor = ResourcesDoors.GetModel( _type, _model );
		//var doorModel = Instantiate( mdlDoor ) as GameObject;
		Init();

		// Check for double door
		if( _hasDoubleDoor == false ) return;

		// Create instance of double door - also create parent gameObject for both doors
		if( _doubleDoorsContainer == null )
		{
			switch( _type )
			{
				default:
					_doubleDoorsContainer = new GameObject( "DoubleDoor" );
					break;
				case eDoorType.PUZZLE_ENTRANCE:
					_doubleDoorsContainer = new GameObject( "PuzzleRoomDoor" );
					break;
				case eDoorType.PLAYER_HUB:
					_doubleDoorsContainer = new GameObject( "MainHallDoor" );
					break;
				case eDoorType.BOSS:
					_doubleDoorsContainer = new GameObject( "BossRoomDoor" );
					break;
			}

			_doubleDoorsContainer.transform.rotation = _thisTransform.rotation;
			_doubleDoorsContainer.transform.position = _thisTransform.position;
			_thisTransform.parent = _doubleDoorsContainer.transform;
			_thisTransform.localRotation = Quaternion.identity;
			_thisTransform.localPosition = Vector3.zero;
		}

		CreateDoubleDoor();

		// Parent double door to container and position relative to this door
		_doubleDoor.transform.parent = _doubleDoorsContainer.transform;
		_doubleDoor.transform.localRotation = Quaternion.identity;
		_doubleDoor.transform.localPosition = new Vector3( _collider.bounds.size.x * 1.97f, 0.0f, 0.0f );
	}

	#endregion

	//=====================================================

	#region IPlayerInteraction

	public bool IsInteractionOk()
	{
		// Is door already open?
		if( _isOpen == true ) return false;

		// Block player-object interaction for this location if key count is too low or fairy type is wrong
		if( _type == eDoorType.BASIC ||
			_type == eDoorType.BASIC_DOUBLE ||
			_type == eDoorType.CRAWL ||
			_type == eDoorType.OBLIVION_PORTAL )
		{
			var keysRequired = GameDataManager.Instance.GetNumKeysCollected( GameManager.Instance.CurrentLocation );
			if( _keyLevel > keysRequired )
			{
				// Show interact-fail message - allowing for interaction check at Start() that opens opened doors
				if( _isStartInteractionChecked == true )
					ShowInteractionPopup( _type, _keyLevel, eFairy.NULL );

				_isStartInteractionChecked = true;
				return false;
			}

			if( _fairyRequired != eFairy.NULL )
			{
				// Block for interaction check at Start() that opens opened doors
				if( _isStartInteractionChecked == false )
				{
					_isStartInteractionChecked = true;
					return false;
				}

				if( GameDataManager.Instance.PlayerCurrentFairy != (int)_fairyRequired )
				{
					// Show interact-fail message
					ShowInteractionPopup( _type, 0, _fairyRequired );

					_isStartInteractionChecked = true;
					return false;
				}
			}
		}

		// Determine whether or not the door is currently available - Boss Door
		// Ok to open if all torches are enabled
		if( _type == eDoorType.BOSS )
		{
			var count = 0;

			foreach( var torch in _bossTorches )
			{
				if( torch.IsEnabled() )
					++count;
			}

			if( count == (int)eFairy.NUM_FAIRIES )
				return true;
			
			// Show interact-fail message
			ShowInteractionPopup( _type, _keyLevel, eFairy.NULL );

			return false;
		}

		// Determine whether or not the door is currently available - Puzzle-Locked Door
		// Ok to open if all switches are enabled
		if( _type != eDoorType.PUZZLE_LOCKED ) return true;

		var isInteractionOk = true;

		foreach( var s in _switches )
		{
			if( s == null || s.IsActivated == true ) continue;

			isInteractionOk = false;

			// Show interact-fail message (switches need pulled)
			ShowInteractionPopup( _type, 0, eFairy.NULL );
			break;
		}

		return isInteractionOk;
	}

	//=====================================================

	public Transform OnPlayerInteraction()
	{
		// If necessary, register next target-scene to travel to (block the second door also registering)
		if( _isDoubleDoor == false )
		{
			switch( _type )
			{
				case eDoorType.PUZZLE_ENTRANCE:
				case eDoorType.PLAYER_HUB:
				case eDoorType.BOSS:
					GameManager.Instance.SetNextLocation( _targetScene );
					break;
			}
		}

		// Open this door
		OnOpenDoor();

		// Open second door?
		if( _hasDoubleDoor == false ) return null;

		CheckReferences();

		if( _doubleDoor != null )
			_doubleDoor.GetComponent<Door>().OnPlayerInteraction();

		// Door transform not required
		return null;
	}

	//=====================================================

	public void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		if( _type == eDoorType.OBLIVION_PORTAL )
		{
			StartCoroutine( DelayPlaySfx( 0.3f ) );
			return;
		}

		// Manage animating locks if available
		if( _type != eDoorType.PUZZLE_LOCKED ) return;

		if( _locks.Length > animationIndex && _locks[animationIndex] != null )
		{
			_locks[animationIndex].SetBool( HashIDs.IsOpen, true );

			// If all locks are now unlocked play sfx
			if( IsInteractionOk() == false ) return;

			if( _audioSource != null )
				_audioSource.PlayOneShot( _clipPuzzleUnlocked );
		}
		else
		{
			// Open door
			if( IsInteractionOk() == true )
				OnPlayerInteraction();
		}
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPath : null;
	}

	//=====================================================

	public Transform[] CutsceneCameraPoints()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPoints : null;
	}

	//=====================================================

	public Transform CutsceneCameraLookAt()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraLookAt : null;
	}

	//=====================================================

	public Transform CutscenePlayerPoint()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.PlayerPoint : null;
	}

	//=====================================================

	public float CutsceneDuration()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.Duration : 2.5f;
	}

	//=====================================================

	public bool OrientCameraToPath()
	{
		return (_cutsceneContainer != null) && _cutsceneContainer.OrientCameraToPath;
	}

	//=====================================================

	public bool IsFlyThruAvailable()
	{
		return (_cutsceneContainer != null) && _cutsceneContainer.IsFlyThruAvailable;
	}

	//=====================================================

	public CameraPathAnimator GetFlyThruAnimator()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.FlyThruAnimator : null;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		// Manage active tween
		if( isPaused == true && _tweenId != -1 )
		{
			LeanTween.pause( _tweenId );
		}
		else if( isPaused == false && _tweenId != -1 )
		{
			LeanTween.resume( _tweenId );
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisGameObject = this.gameObject;
		_thisTransform = this.transform;
		_trigger = _thisTransform.FindChild( "TriggerOpenDoor" );
		_guiDoorInteraction = _thisTransform.GetComponentInChildren<GuiBubbleDoorInteraction>();
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		
		// Deactivate
		if( _isDoubleDoor == true )
		{
			if( _guiDoorInteraction != null )
			{
				_guiDoorInteraction.gameObject.SetActive( false );
				_guiDoorInteraction = null;
			}

			if( _audioSource != null )
			{
				_audioSource.enabled = false;
				_audioSource = null;
			}
		}

		// Deactivate
		switch( _type )
		{
			case eDoorType.CRAWL:
			case eDoorType.OBLIVION_PORTAL:
				if( _guiDoorInteraction != null )
				{
					_guiDoorInteraction.gameObject.SetActive( false );
					_guiDoorInteraction = null;
				}
				break;
			case eDoorType.BOSS:
				// Only show interactive bubble for boss door that leads to boss room (not out of boss room)
				if( _targetScene != eLocation.BOSS_ROOM )
				{
					if( _guiDoorInteraction != null )
					{
						_guiDoorInteraction.gameObject.SetActive( false );
						_guiDoorInteraction = null;
					}
				}
				break;
		}

		_isStartInteractionChecked = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;

		if( _audioSource != null )
			_audioSource.clip = _clipOpen;

		if( _guiDoorInteraction != null )
			_guiDoorInteraction.HideBubble();

		// Auto-open doors (that don't auto-close) if player has collected enough keys in the current location
		switch( _type )
		{
			case eDoorType.BASIC:
			case eDoorType.BASIC_DOUBLE:
				if( _autoClose == false && IsInteractionOk() )
					OnPlayerInteraction();
				return;

			case eDoorType.PUZZLE_LOCKED:
				// Force puzzle locked door to not auto-close
				_autoClose = false;

				for( var i = 0; i < _switches.Length; i++ )
				{
					if( _switches[i] == null ) continue;

					_switches[i].Index = i;
					_switches[i].SwitchActivated += OnSwitchActivated;

					if( _switches[i].Type == eSwitchType.PRESSURE )
						_switches[i].SwitchDeactivated += OnSwitchDeactivated;
				}
				break;
		}

		// Get cutscene container
		if( _type == eDoorType.PUZZLE_LOCKED ||
			_type == eDoorType.CRAWL ||
			_type == eDoorType.OBLIVION_PORTAL ||
			_type == eDoorType.PUZZLE_ENTRANCE ||
			_type == eDoorType.PLAYER_HUB ||
			_type == eDoorType.BOSS )
		{
			// For non-double doors find the cutscene container as a child of the 'door'
			_cutsceneContainer = _thisTransform.GetComponentInChildren<CutsceneContainer>();
			if( _cutsceneContainer != null ) return;

			// Otherwise, double doors should have a parent container that may include a cutscene container
			_cutsceneContainer = _thisTransform.parent != null ? _thisTransform.parent.GetComponentInChildren<CutsceneContainer>() : null;
		}
		else
		{
			_cutsceneContainer = null;
		}
	}

	//=====================================================

	void Start()
	{
		if( Application.isPlaying == false ) return;

		// Boss Doors - enable boss torches - all torches enabled unlocks boss door
		if( _type != eDoorType.BOSS || _targetScene != eLocation.BOSS_ROOM || _bossTorches == null ) return;

		// Max level for fairies and boss
		var maxLevel = GameDataManager.Instance.PlayerMaxFairyLevel;

		// Get current boss level
		var bossLevel = GameDataManager.Instance.PlayerBossLevel;

		// Get current min fairy level for boss fight
		var minFairyLevel = GameDataManager.Instance.PlayerFairyMinLevel();

		// Are we at the max-level boss? If so, check time interval else check current fairy levels
		// Note: time-interval-start set after killing level 3 boss: GameManager->OnBossDeadEvent()
		if( bossLevel >= maxLevel && minFairyLevel >= maxLevel )
		{
			// Get time in seconds since last boss appearance
			var timeNow = PreHelpers.UnixUtcNow();
			double timePassed = 0;
			if( PlayerPrefsWrapper.HasKey( "BossRoomTimedIntervalStarted" ) )
				timePassed = timeNow - PlayerPrefsWrapper.GetDouble( "BossRoomTimedIntervalStarted" );
			var intervalInDays = Convert.ToSingle( SettingsManager.GetSettingsItem( "BOSS_RESPAWN_TIME", -1 ) );
			var intervalInSeconds = intervalInDays * 24 * 60 * 60;

			// ToDo: check time since boss last appeared
			if( timePassed > intervalInSeconds )
			{
				// Enable torches
				foreach( var torch in _bossTorches )
					torch.EnableTorch();

				return;
			}
		}

		// Check current fairy levels
		var fairyLevels = GameDataManager.Instance.PlayerFairyLevels();

		if( fairyLevels.Length > _bossTorches.Length ) return;

		// Enable torches for fairies at high enough level
		for( var i = 0; i < fairyLevels.Length; i++ )
		{
			if( fairyLevels[i] >= bossLevel )
				_bossTorches[i].EnableTorch();
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;

		// Unregister with switches (puzzle-door only) for switch activations
		if( _type != eDoorType.PUZZLE_LOCKED ) return;

		foreach( var s in _switches )
		{
			if( s == null ) continue;

			s.SwitchActivated -= OnSwitchActivated;

			if( s.Type == eSwitchType.PRESSURE )
				s.SwitchDeactivated -= OnSwitchDeactivated;
		}
	}

	//=====================================================

	void Update()
	{
		//#if UNITY_EDITOR
		// DEBUG - REMOVE THIS
		//if( Input.GetKeyDown( KeyCode.O ) )
		//	OnOpenDoor();
		//if( Input.GetKeyDown( KeyCode.C ) )
		//	OnCloseDoor();
		//#endif
		// Manage fast switching of door open / close i.e. before door(s) has had time to fully open / close
		if( _isDoubleDoor == true ) return;

		if( _queueOpenDoor == true && _isOpen == false )
		{
			_queueOpenDoor = false;

			// Open door(s)
			OnPlayerInteraction();
		}
		else if( _queueCloseDoor == true && _isOpen == true )
		{
			_queueCloseDoor = false;

			// Close door(s)
			OnCloseDoor();
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = GetGizmoColor();
		DrawArrow.ForGizmo( _thisTransform.position + new Vector3( 0.0f, 0.05f, 0.0f ), _thisTransform.forward, 0.4f );

		if( _isDoubleDoor == true ) return;

		switch( _type )
		{
			case eDoorType.PUZZLE_LOCKED:
				// Draw lines between door and referenced switches
				for( var i = 0; i < _switches.Length; i++ )
				{
					if( _switches[i] != null )
					{
						Gizmos.DrawLine( _thisTransform.position, _switches[i].transform.position );
					}
				}

				// Draw hinge position - allow for rotations
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.DrawCube( new Vector3( 0.0f, 1.0f, 0.0f ), new Vector3( 0.05f, 2.0f, 0.05f ) );
				break;

			case eDoorType.CRAWL:
			case eDoorType.OBLIVION_PORTAL:
				// Draw lines between door and referenced spawn points
				if( _spawnPoint != null )
					Gizmos.DrawLine( _thisTransform.position, _spawnPoint.transform.position );
				break;

			default:
				// Draw hinge position - allow for rotations
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.DrawCube( new Vector3( 0.0f, 1.0f, 0.0f ), new Vector3( 0.05f, 2.0f, 0.05f ) );

				// Indicate door position (opposite to door rotation)
				//int direction = (_hingePosition == eHingePosition.LEFT) ? 1 : -1;
				//Gizmos.DrawCube( new Vector3( 0.5f * direction, 0.025f, 0.0f ), new Vector3( 1.0f, 0.05f, 0.05f ) );
				break;
		}
	}

	//=====================================================

	private void OnOpenDoor()
	{
		if( _type == eDoorType.CRAWL || _type == eDoorType.OBLIVION_PORTAL ) return;

		// Log that door is to open again (after closing)
		if( _isRotatingDoor == true )
		{
			// If door is closing
			if( _isOpen == true )
				_queueOpenDoor = true;

			return;
		}

		if( _isOpen == false )
		{
			_isRotatingDoor = true;
			float rotateBy;

			// Determine rotation
			if( _openInAndOut == true )
			{
				// Get target vector (from here to player)
				var target = PlayerManager.Position - _thisTransform.position;
				target = new Vector3( target.x, 0.0f, target.z );
				target.Normalize();

				// Determine which side of door the player is on (ignore y-axis)
				if( Vector3.Dot( target, _thisTransform.forward ) > 0 )	// In front of door
					rotateBy = (_hingePosition == eHingePosition.LEFT) ? _rotateOpenBy : -_rotateOpenBy;
				else                                                    // Behind door
					rotateBy = (_hingePosition == eHingePosition.LEFT) ? -_rotateOpenBy : _rotateOpenBy;
			}
			else
			{
				rotateBy = (_hingePosition == eHingePosition.LEFT) ? -_rotateOpenBy : _rotateOpenBy;
			}

			// Tween door open around hinge - on completion tween door to closed position if required
			_tweenId = LeanTween.rotateAround( _thisGameObject, Vector3.up, rotateBy, _rotateDuration )
				 .setEase( LeanTweenType.easeInQuad )
				 .setOnComplete( () =>
				 {
					 _isRotatingDoor = false;
					 _isOpen = true;

					 if( _autoClose == true )
					 {
						 _isRotatingDoor = true;
						 _tweenId = LeanTween.rotateAround( _thisGameObject, Vector3.up, -rotateBy, _rotateDuration )
								 .setDelay( _autoCloseDelay )
								 .setEase( LeanTweenType.easeInQuad )
								 .setOnComplete( () => { _isRotatingDoor = false; _isOpen = false; _tweenId = -1; } )
								 .id;

						 // Play audio fx
						 if( _audioSource != null )
							 StartCoroutine( PlayDelayedAudioClip( _autoCloseDelay ) );
					 }
					 else
					 {
						 _tweenId = -1;
					 }
				 } )
				 .id;

			// Play audio fx
			if( _audioSource != null )
				_audioSource.Play();
		}
	}

	//=====================================================

	private IEnumerator DelayPlaySfx( float delay )
	{
		yield return new WaitForSeconds( delay );

		// Play audio fx
		if( _audioSource != null )
			_audioSource.Play();
	}

	//=====================================================

	private void ShowInteractionPopup( eDoorType doorType, int numKeysRequired, eFairy fairyRequired )
	{
		if( _guiDoorInteraction != null )
			_guiDoorInteraction.ShowBubble( doorType, numKeysRequired, fairyRequired );
	}

	//=====================================================

	private IEnumerator PlayDelayedAudioClip( float delay )
	{
		yield return new WaitForSeconds( delay );

		if( _audioSource != null )
			_audioSource.Play();
	}

	//=====================================================

	public void OnCloseDoor( float delayBeforeClosing = 0.0f )
	{
		// Log that door is to close again (after opening)
		if( _isRotatingDoor == true )
		{
			// If door is opening
			if( _isOpen == false )
				_queueCloseDoor = true;

			return;
		}

		// Return if door is already closed
		if( _isOpen == false ) return;

		// Start closing door
		_isRotatingDoor = true;

		// Determine rotation - puzzle doors only open in one direction
		var rotateBy = (_hingePosition == eHingePosition.LEFT) ? -_rotateOpenBy : _rotateOpenBy;

		// Tween door to closed position
		_tweenId = LeanTween.rotateAround( _thisGameObject, Vector3.up, -rotateBy, _rotateDuration )
								.setDelay( delayBeforeClosing )
								.setEase( LeanTweenType.easeInQuad )
								.setOnComplete( () => { _isRotatingDoor = false; _isOpen = false; _tweenId = -1; } )
								.id;

		// Play audio fx
		if( _audioSource != null )
			_audioSource.Play();

		// Return if there's no second door or this is a second door
		if( _isDoubleDoor == true ) return;
		if( _hasDoubleDoor == false ) return;

		// Close second door
		CheckReferences();

		if( _doubleDoor != null )
			_doubleDoor.GetComponent<Door>().OnCloseDoor( delayBeforeClosing );
	}

	//=====================================================

	private void OnSwitchActivated( int index )
	{
		Debug.Log( "" + _thisTransform.parent.name + " : Switch activated: " + index + " : " + _switches[index].Index );

		// Start cutscene
		if( _cutsceneContainer != null )
		{
			switch( index )
			{
				case 0:
					CutsceneManager.Instance.OnStartCutscene( eCutsceneType.SWITCH_OPENS_DOOR_BOLT_01, this );
					break;
				case 1:
					CutsceneManager.Instance.OnStartCutscene( eCutsceneType.SWITCH_OPENS_DOOR_BOLT_02, this );
					break;
				default:
					CutsceneManager.Instance.OnStartCutscene( eCutsceneType.SWITCH_OPENS_DOOR_BOLT_03, this );
					break;
			}
		}
		else
		{
			// Open door if ok to do so
			OnPlayCutsceneAnimation();
		}

		// Only unregister from one-shot switches i.e. that can't be deactivated
		if( _switches[index].Type != eSwitchType.PRESSURE )
			_switches[index].SwitchActivated -= OnSwitchActivated;
	}

	//=====================================================

	private void OnSwitchDeactivated( int animationIndex )
	{
		// Deactivating a switch should result in the door(s) closing
		if( _isOpen ) OnCloseDoor();

		//Debug.Log( "Switch deactivated: " + _switches[animationIndex].Type + " : " + _switches[animationIndex].Index );

		if( _type != eDoorType.PUZZLE_LOCKED ) return;

		if( _locks.Length > animationIndex && _locks[animationIndex] != null )
		{
			_locks[animationIndex].SetBool( HashIDs.IsOpen, false );
		}
	}

	//=====================================================

	private void InitDoorTrigger()
	{
		// Set door trigger's action
		var triggerScript = _trigger.GetComponent<TriggerInteractive>();

		switch( _type )
		{
			default:
				triggerScript.TriggerAction = ePlayerAction.OPEN_DOOR;
				break;
			case eDoorType.CRAWL:
				triggerScript.TriggerAction = ePlayerAction.CRAWL_THROUGH_DOOR;
				break;
			case eDoorType.OBLIVION_PORTAL:
				triggerScript.TriggerAction = ePlayerAction.TELEPORT_OBLIVION_PORTAL;
				break;
			case eDoorType.PUZZLE_ENTRANCE:
				if( _targetScene == eLocation.MAIN_HALL )
					triggerScript.TriggerAction = ePlayerAction.LEAVE_PUZZLE_ROOM;
				else
					triggerScript.TriggerAction = ePlayerAction.ENTER_PUZZLE_ROOM;
				break;
			case eDoorType.PLAYER_HUB:
				if( _targetScene == eLocation.MAIN_HALL )
					triggerScript.TriggerAction = ePlayerAction.LEAVE_PLAYER_HUB;
				else
					triggerScript.TriggerAction = ePlayerAction.ENTER_PLAYER_HUB;
				break;
			case eDoorType.BOSS:
				if( _targetScene == eLocation.MAIN_HALL )
					triggerScript.TriggerAction = ePlayerAction.LEAVE_BOSS_ROOM;
				else
					triggerScript.TriggerAction = ePlayerAction.ENTER_BOSS_ROOM;
				break;
		}

		// Set trigger size according to object's collider
		var doorWidth = 1.0f;
		if( _collider != null )
		{
			doorWidth = (_hasDoubleDoor == false) ? _collider.bounds.size.x : _collider.bounds.size.x * 2.0f;
			_triggerSize = new Vector3( doorWidth, _triggerSize.y, _triggerSize.z );
		}

		// Special case for oblivion portal - reducing trigger size to allow player to be inside particle fx
		if( _type == eDoorType.OBLIVION_PORTAL )
		{
			_trigger.localScale = new Vector3( 1.0f, 0.2f, 1.0f );  // new Vector3( 0.3f, 0.2f, 0.3f );
			_trigger.localPosition = new Vector3( doorWidth * 0.5f, _trigger.localScale.y * 0.5f, 0.0f );
			return;
		}

		_trigger.localScale = _triggerSize;

		// Update door model position according to hinge position
		if( _hingePosition == eHingePosition.LEFT )
			_trigger.localPosition = new Vector3( _trigger.localScale.x * 0.5f, _trigger.localScale.y * 0.5f, 0.0f );
		else
			_trigger.localPosition = new Vector3( _trigger.localScale.x * -0.5f, _trigger.localScale.y * 0.5f, 0.0f );
	}

	//=====================================================

	private void CreateDoubleDoor()
	{
		// Get content from resources
		var mdlDoor = ResourcesDoors.GetModel( _type, _model );
		if( mdlDoor == null ) return;

		if( _doubleDoor != null && _currentModelName == mdlDoor.name )
		{
			// Do nothing
		}
		else
		{
			// Get content from resources
			var pfbDoor = ResourcesDoors.GetPrefab();
			if( pfbDoor == null ) return;

			if( _doubleDoor != null )
				DestroyImmediate( _doubleDoor );

			// Add second door
			_doubleDoor = Instantiate( pfbDoor ) as GameObject;
			if( _doubleDoor == null ) return;

			// Rename door
			_doubleDoor.name = "Door 2";

			// Get script
			_doubleDoorScript = _doubleDoor.GetComponent<Door>();
			if( _doubleDoorScript == null ) return;

			// Init door
			_doubleDoorScript.Type = _type;
			//var doorModel = Instantiate( mdlDoor ) as GameObject;
			_doubleDoorScript.HingePosition = eHingePosition.RIGHT;
		}

		// Init door
		_doubleDoorScript.Model = _model;
		_doubleDoorScript.Init( true );

		// Match other door parameters
		_doubleDoorScript.RotateOpenBy = _rotateOpenBy;
		_doubleDoorScript.RotateDuration = _rotateDuration;
		_doubleDoorScript.AutoClose = _autoClose;
		_doubleDoorScript.AutoCloseDelay = _autoCloseDelay;
		_doubleDoorScript.OpenInAndOut = _openInAndOut;
		_doubleDoorScript.KeyLevel = _keyLevel;
		_doubleDoorScript.FairyRequired = _fairyRequired;
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	private void CheckReferences()
	{
		// Try to find door if reference has been lost
		if( _door == null )
			_door = _thisTransform.FindChild( "Door" );

		if( _door != null )
		{
			// Get collider to determine trigger size
			if( _collider == null )
			{
				_collider = _door.GetComponent<Collider>();
				if( _collider == null )
					Debug.Log( "CheckReferences: Door Collider not found" );
			}
		}
		else
		{
			Debug.Log( "CheckReferences: Door not found" );
		}

		// Try to find container gameobject if reference has been lost (noticed this happening after refreshing door)
		if( _hasDoubleDoor == true )
		{
			if( _doubleDoorsContainer == null )
			{
				if( _thisTransform.parent != null )
					_doubleDoorsContainer = _thisTransform.parent.gameObject;
			}

			if( _doubleDoor == null && _doubleDoorsContainer != null )
			{
				var doors = _doubleDoorsContainer.transform.GetComponentsInChildren<Door>();
				foreach( var door in doors )
				{
					if( door.name.Contains( "2" ) )
					{
						_doubleDoor = door.gameObject;
						break;
					}
				}
			}
		}
	}

	//=====================================================

	private Color GetGizmoColor()
	{
		switch( _type )
		{
			case eDoorType.BASIC:
			case eDoorType.BASIC_DOUBLE:
			case eDoorType.CRAWL:
			case eDoorType.OBLIVION_PORTAL:
			case eDoorType.PUZZLE_LOCKED:
			case eDoorType.PUZZLE_ENTRANCE:
			case eDoorType.PLAYER_HUB:
			case eDoorType.BOSS:
				return Gizmos.color = new Color( Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f );
			default:
				return Gizmos.color = new Color( Color.white.r, Color.white.g, Color.white.b, 0.5f );
		}
	}

	#endregion

	//=====================================================
}
