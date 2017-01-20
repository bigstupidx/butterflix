using UnityEngine;

public class CameraMovement : MonoBehaviourEMS, IPauseListener
{
	private enum eState { NORMAL = 0, BEHIND, TARGET, FREE_LOOK, BOSS, NUM_STATES, NULL }

	[SerializeField] private float _relDistanceBack = 5.0f;			// Distance behind player
	[SerializeField] private float _relDistanceUp = 1.2f;			// Distance above player
	[SerializeField] private Vector3 _lookAtOffset = new Vector3( 0.0f, 2.0f, 0.0f );
	[SerializeField] private float _smoothLookAtDefault = 3.0f;
	private float _smoothLookAt;									// The relative speed at which the camera-lookAt will catch up.
	//private float _smoothMovement = 2.5f;							// The relative speed at which the camera will catch up.
	[SerializeField] private Vector3 _smoothMovementVec = Vector3.zero;	// The relative speed at which the camera will catch up.
	[SerializeField] private float _smoothDampTimeDefault = 0.5f;
	private float _smoothDampTime;									// Smooth position
	
	private float _timeAccumulator = 0.0f;

	private Transform _thisTransform;

	private Transform _player;										// Reference to the player's transform.
	private PlayerMovement _playerController;
	private Vector3 _targetPostition;
	//private Vector3 _relCameraPos;								// The relative position of the camera from the player.
	//private float _relCameraPosMag;								// The distance of the camera from the player.
	private Vector3 _currentLookDir;
	private Vector3 _targetLookDir;
	private Vector3 _velocityLookDir;
	[SerializeField] private float _lookDirDampTime = 5.0f;
	private int _maskWalls;

	// Player Input
	private float _currentHorizontalMove;
	private float _currentVerticalMove;
	private float _currentHorizontalLook;
	private float _currentVerticalLook;

	// State
	private eState _currentState;									// Current camera state
	private const float _behindStateDelay = 1.0f;					// Delay before switching from normal to behind states
	private float _timer;											// Timer for switching states
	//private Vector3 _playerPos;									// Checking for player movement

	//private Vector3 _newPos;										// The position the camera is trying to reach.
	//private float _newPosMag;
	//private int _curCheckPoint;									// Index of latest camera checkPoint
	//private Vector3 _lastPlayerPos;

	// Free-look
	private float _distanceUpFree;
	//[SerializeField] private float _distanceAwayFree;
	[SerializeField] private float _distanceUpMultiplier = 3.5f;
	[SerializeField] private float _freeRotationDegPersec = 135.0f;

	// Boss Game
	private Vector3 _currentBossPosition;

	private bool _isPaused;

	//=====================================================

	#region Public Interface

	//=====================================================

	public void InitWithPlayer()
	{
		InitWithPlayer(true);
	}

	public void InitWithPlayer(bool resetPostion)
	{
		var player = GameObject.FindGameObjectWithTag( UnityTags.Player );

		if( player != null )
		{
			_player = player.transform;

			_playerController = _player.GetComponent<PlayerMovement>();

			if( _playerController == null )
			{
				Debug.Log( "CameraMovement: Player Controller not found" );
				return;
			}

			// Setting the relative position as the initial relative position of the camera in the scene.
			//_relCameraPos = _thisTransform.position - _player.position;
			//_relCameraPosMag = _relCameraPos.magnitude;		// - 0.5f;

			if( resetPostion )
				Reset();
		}
		else
		{
			Debug.Log( "CameraMovement: Player not found. Check player prefab exists in scene." );
		}
	}

	//=====================================================

	public void Reset()
	{
		if( _player == null ) return;

		// Update target postion behind player
		var characterOffset = _player.position + _lookAtOffset;
		_targetPostition = characterOffset - (_player.forward * _relDistanceBack) + (_player.up * _relDistanceUp);

		// Apply to camera
		_thisTransform.position = _targetPostition;
		_thisTransform.LookAt( characterOffset );

		// Set camera defaults (used to set state to BEHIND at this point)
		_currentLookDir = _player.forward;
		_currentState = eState.NORMAL;
		_smoothDampTime = _smoothDampTimeDefault;
		_smoothLookAt = _smoothLookAtDefault;
		_timer = 0.0f;
		_timeAccumulator = 0.0f;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;

		_maskWalls = 1 << LayerMask.NameToLayer( "Collidable" );

		_isPaused = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		InputManager.MoveLeftRightEvent += OnMoveLeftRightEvent;
		InputManager.MoveForwardBackEvent += OnMoveForwardBackEvent;
		InputManager.LookLeftRightEvent += OnLookLeftRightEvent;
		InputManager.LookUpDownEvent += OnLookUpDownEvent;
		//InputManager.PerformActionEvent += OnPerformActionEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		InputManager.MoveLeftRightEvent -= OnMoveLeftRightEvent;
		InputManager.MoveForwardBackEvent -= OnMoveForwardBackEvent;
		InputManager.LookLeftRightEvent -= OnLookLeftRightEvent;
		InputManager.LookUpDownEvent -= OnLookUpDownEvent;
		//InputManager.PerformActionEvent -= OnPerformActionEvent;

		if( GameManager.Instance.CurrentLocation != eLocation.BOSS_ROOM ) return;
		
		if( BossManager.Instance != null ) 
			BossManager.Instance.TeleportEvent -= OnTeleportEvent;
	}

	//=====================================================

	void Start()
	{
		// Delay then attach to player - allows for GameManager switching to player's last selected fairy
		Invoke( "InitWithPlayer", 0.1f );

		if( GameManager.Instance.CurrentLocation != eLocation.BOSS_ROOM ) return;
		
		if( BossManager.Instance != null )
			BossManager.Instance.TeleportEvent += OnTeleportEvent;
	}

	void LateUpdate()
	{
		if( _isPaused == true || _player == null || _playerController == null ) return;

		var characterOffset = _player.position + _lookAtOffset;

		switch( _currentState )
		{
			case eState.BOSS:
				// Update lookAt direction
				_currentLookDir = _currentBossPosition - _thisTransform.position;
				_currentLookDir.Normalize();

				// Update camera target position
				_targetPostition = characterOffset - ( _currentLookDir * _relDistanceBack * 2.0f ) + (_player.up * _relDistanceUp);

				CompensateForWalls( characterOffset, ref _targetPostition );
				SmoothPosition( _thisTransform.position, _targetPostition );
				SmoothLookAt( _currentBossPosition );
				return;

			case eState.NORMAL:
				// ToDo: DEBUG - REMOVE THIS
				//GuiManager.Instance.OnShowGuiLookInput( true );

				// FREE LOOK
				var isFreeLookActive = false;
				if( Mathf.Abs( _currentHorizontalLook ) > 0.05f || Mathf.Abs( _currentVerticalLook ) > 0.05f )
				{
					isFreeLookActive = true;

					// ToDo: DEBUG - REMOVE THIS
					// Update camera rotation around player
					_thisTransform.RotateAround( characterOffset, _player.up,
												 _freeRotationDegPersec * _currentHorizontalLook * Time.deltaTime );

					// Update lookAt direction
					_currentLookDir = characterOffset - _thisTransform.position;
					_currentLookDir.y = 0.0f;
					_currentLookDir.Normalize();

					// Determine any change in vertical axis
					if( _currentVerticalLook >= 0 )
					{
						// Reducing multiplier slightly here to limit max camera height - balances better visually with camera at lowest viewpoint
						_distanceUpFree = Mathf.Lerp( _relDistanceUp, _relDistanceUp * _distanceUpMultiplier * 0.75f,
													  Mathf.Abs( _currentVerticalLook ) );
					}
					else
					{
						_distanceUpFree = Mathf.Lerp( _relDistanceUp,
													  _relDistanceUp - ((_relDistanceUp * _distanceUpMultiplier) - _relDistanceUp),
													  Mathf.Abs( _currentVerticalLook ) );
					}

					// Update camera target position
					//_targetPostition = characterOffset - ( _currentLookDir * _relDistanceBack ) + ( _player.up * _distanceUpFree );
				}

				// FOLLOW PLAYER
				if( _playerController.IsInLocomotionState() && (_playerController.Speed > _playerController.LocomotionThreshold) )
				{
					_targetLookDir = Vector3.Lerp( _player.right * (_currentHorizontalMove < 0.0f ? -1.0f : 1.0f),
													_player.forward * (_currentVerticalMove < 0.0f ? -1.0f : 1.0f),
													Mathf.Abs( Vector3.Dot( _thisTransform.forward, _player.forward ) ) );
					//Debug.DrawRay( _thisTransform.position, _targetLookDir, Color.white );

					_currentLookDir = characterOffset - _thisTransform.position;
					_currentLookDir.y = 0.0f;
					_currentLookDir.Normalize();
					//Debug.DrawRay( _thisTransform.position, _currentLookDir, Color.yellow );

					_currentLookDir = Vector3.SmoothDamp( _currentLookDir, _targetLookDir, ref _velocityLookDir, _lookDirDampTime * Time.deltaTime );
					_currentLookDir.Normalize();

					// Tend camera towards normal height offset from player
					if( isFreeLookActive == false )
					{
						_distanceUpFree = Mathf.Lerp( _distanceUpFree, _relDistanceUp, 1.0f * Time.deltaTime );
					}
				}
				break;
		}

		// Update camera target position
		//_targetPostition = characterOffset - ( _currentLookDir * _relDistanceBack ) + ( _player.up * _relDistanceUp );
		_targetPostition = characterOffset - (_currentLookDir * _relDistanceBack) + (_player.up * _distanceUpFree);
		//Debug.DrawLine( _player.position, _targetPostition, Color.magenta );

		CompensateForWalls( characterOffset, ref _targetPostition );
		//_thisTransform.position = Vector3.Lerp( _thisTransform.position, _targetPostition, _smoothMovement * Time.deltaTime );
		SmoothPosition( _thisTransform.position, _targetPostition );
		SmoothLookAt( characterOffset );

		//Debug.Log( "Dist:" + (_thisTransform.position - _player.position).magnitude );
	}

	//=====================================================

	private void OnTeleportEvent( Vector3 shieldPosition )
	{
		_currentBossPosition = shieldPosition + Vector3.up;
		_smoothLookAt = _smoothLookAtDefault * 0.5f;

		_currentState = eState.BOSS;
	}

	//=====================================================

	//private void ResetCamera()
	//{
	//	//lookWeight = Mathf.Lerp( lookWeight, 0.0f, firstPersonLookSpeed * Time.deltaTime );
	//	_thisTransform.localRotation = Quaternion.Lerp( _thisTransform.localRotation, Quaternion.identity, Time.deltaTime );
	//}

	//=====================================================

	//private bool CheckForCamStateNormal()
	//{
	//	// Keep camera in behind-mode if player is on platform (checking against palyer's rigidbody while on platorm was causing camera jitter)
	//	if( _playerController.IsOnPlatform == true ) return false;	// && _playerPos != _player.position )

	//	if( PlayerManager.SpeedSqrMag < 0.01f ) return false;

	//	_currentState = eState.NORMAL;
	//	_smoothDampTime = _smoothDampTimeDefault;
	//	_smoothLookAt = _smoothLookAtDefault;

	//	return true;
	//}

	//=====================================================

	private void SmoothPosition( Vector3 fromPos, Vector3 toPos )
	{
		_thisTransform.position = Vector3.SmoothDamp( fromPos, toPos, ref _smoothMovementVec, _smoothDampTime );
	}

	//=====================================================

	private void SmoothLookAt( Vector3 targetLookAt )
	{
		//_thisTransform.LookAt( targetLookAt );

		// Create a vector from the camera towards the player.
		var relPlayerPosition = targetLookAt - _thisTransform.position;

		// Create a rotation based on the relative position of the player being the forward vector.
		var lookAtRotation = Quaternion.LookRotation( relPlayerPosition, Vector3.up );

		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		_thisTransform.rotation = Quaternion.Lerp( _thisTransform.rotation, lookAtRotation, _smoothLookAt * Time.deltaTime );
	}

	//=====================================================

	private void CompensateForWalls( Vector3 fromObject, ref Vector3 toTarget )
	{
		//Debug.DrawLine( fromObject, toTarget, Color.cyan );

		var offsetFromWall = fromObject - toTarget;
		offsetFromWall.Normalize();
		offsetFromWall *= 0.35f;

		var hit = new RaycastHit();
		
		if( Physics.Linecast( fromObject, toTarget, out hit, _maskWalls ) == false ) return;
		
		Debug.DrawRay( hit.point, Vector3.left, Color.red );
		toTarget = new Vector3( hit.point.x, toTarget.y, hit.point.z );
		toTarget += offsetFromWall;
	}

	//=====================================================

	private void OnMoveLeftRightEvent( float value )
	{
		_currentHorizontalMove = value;
	}

	//=====================================================

	private void OnMoveForwardBackEvent( float value )
	{
		_currentVerticalMove = value;
	}

	//=====================================================

	private void OnLookLeftRightEvent( float value )
	{
		_currentHorizontalLook = value;
	}

	//=====================================================

	private void OnLookUpDownEvent( float value )
	{
		_currentVerticalLook = value;
	}

	#endregion

	//=====================================================
}
