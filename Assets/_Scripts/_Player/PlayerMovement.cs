using UnityEngine;

[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( CapsuleCollider ) )]
[RequireComponent( typeof( Rigidbody ) )]
public class PlayerMovement : MonoBehaviourEMS
{
	[SerializeField] private float _maxSpeed = 5.0f;
	//[SerializeField]
	//private float _turnSmoothing = 5.0f;
	[SerializeField] private float _directionDampTime = 0.5f;
	[SerializeField] private float _speedDampTime = 0.5f;
	[SerializeField] private float _maxTimeJumpAnim = 0.8f;
	[SerializeField] private float _directionSpeed = 3.0f;
	[SerializeField] private float _rotationDegPerSec = 200.0f;	// 120.0f

	private Camera _camera;
	private Transform _thisTransform;
	private Transform _parentTransform;
	private Rigidbody _rigidBody;
	private Animator _animator;
	private CapsuleCollider _collider;
	private ParticleSystem _fxPush;
	private bool _isPushing;

	private LayerMask _mask;
	private float _timer;
	private float _timerIdle;

	private float _currentHorizontal;
	private float _currentVertical;
	private bool _isMoveRequired;
	private float _direction;
	private float _speed;
	private float _playerAngle;

	//[SerializeField]
	//private GameObject _target;
	//private float _rotateSpeed = 5.0f;
	//public float CurrentMouseX;

	//=====================================================

	public float Speed { get { return _speed; } }

	public float LocomotionThreshold { get { return 0.2f; } }

	public bool IsPlayerActionRequired { private get; set; }

	public bool IsJumpingOk
	{
		get
		{
			return _animator.GetCurrentAnimatorStateInfo( 0 ).fullPathHash != HashIDs.StateJumping &&
				   _animator.GetCurrentAnimatorStateInfo( 0 ).fullPathHash != HashIDs.StateLanding;
		}
	}

	public bool IsClimbingUpOk { private get; set; }

	public bool IsClimbingDownOk { private get; set; }

	public bool IsUseFloorLeverOk { private get; set; }

	public bool IsUseWallLeverOk { private get; set; }

	public bool IsUsePressureSwitchOk { private get; set; }

	public bool IsOpenDoorOk { private get; set; }

	public bool IsOnPlatform { get { return _thisTransform.parent != null; } }

	public bool IsPushingOk { private get; set; }

	public bool IsPushingNotOk { private get; set; }

	public bool IsDamaged { get; private set; }

	public bool IsDead { get; private set; }

	public bool IsRespawning { get; private set; }

	public bool IsCastAttackSpellOk { private get; set; }

	public bool IsCastMeltSpellOk { private get; set; }

	public bool IsOpenChestOk { private get; set; }

	public bool IsInteractFail { private get; set; }

	public bool IsTrapped { private get; set; }

	public bool IsEscapingTrap { private get; set; }

	public bool IsCutsceneCrawl { private get; set; }

	public bool IsCutscenePortal { private get; set; }

	public bool IsCutsceneWalk { private get; set; }

	public bool IsCutsceneCelebrate { private get; set; }

	//=====================================================

	public void OnStartIdling()
	{
		// Reset timer for selecting random idle animations
		_timerIdle = Random.Range( 2.5f, 5.0f );
	}

	//=====================================================

	public bool IsPushingStateActive()
	{
		return (_animator.GetCurrentAnimatorStateInfo( 0 ).fullPathHash == HashIDs.StatePushingObject ||
				_animator.GetCurrentAnimatorStateInfo( 0 ).fullPathHash == HashIDs.StatePushingObjectBuffer);
	}

	//=====================================================

	public void OnDamaged( Vector3 hitPoint )
	{
		if( IsDamaged == true ) return;
		
		IsDamaged = true;
		_animator.SetTrigger( HashIDs.IsDamaged );

		//hitPoint = new Vector3( hitPoint.x, 0.0f, hitPoint.z );
		//var fairyPos = new Vector3( _thisTransform.position.x, 0.5f, _thisTransform.position.z );

		// ToDo: Lateral forces don't appear to work well with the mecanim setup
		// Apply external force
		//_rigidBody.AddForce( (fairyPos - hitPoint).normalized * 20000.0f, ForceMode.Force );
		_rigidBody.AddForce( Vector3.up * 15000.0f, ForceMode.Force );
	}

	//=====================================================

	public void OnDeath( Vector3 hitPoint )
	{
		if( IsDead == true ) return;

		IsDead = true;
		_animator.SetTrigger( HashIDs.IsDead );
		//_rigidBody.AddForce( Vector3.up * 20000.0f, ForceMode.Force );
	}

	//=====================================================

	public void OnRespawn()
	{
		if( IsRespawning == true ) return;

		IsDead = false;
		IsRespawning = true;
		_animator.SetTrigger( HashIDs.IsRespawning );
	}

	//=====================================================

	public bool IsInLocomotionState()
	{
		var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

		return (currentStateInfo.fullPathHash == HashIDs.StateLocomotion);
			// || currentStateInfo.nameHash == HashIDs.StateRunning);
	}

	//=====================================================
	// Player leaves moving platform - un-parent from platform
	public void ResetParentTransform()
	{
		transform.parent = _parentTransform;
	}

	//=====================================================

	public void ResetActionFlags()
	{
		IsPlayerActionRequired = false;
		IsClimbingUpOk = false;
		IsClimbingDownOk = false;
		IsUseFloorLeverOk = false;
		IsUseWallLeverOk = false;
		IsUsePressureSwitchOk = false;
		IsOpenDoorOk = false;
		IsPushingOk = false;
		IsPushingNotOk = false;
		IsDamaged = false;
		IsDead = false;
		IsRespawning = false;
		IsCastAttackSpellOk = false;
		IsCastMeltSpellOk = false;
		IsOpenChestOk = false;
		IsInteractFail = false;
		IsTrapped = false;
		IsEscapingTrap = false;
		IsCutsceneCrawl = false;
		IsCutscenePortal = false;
		IsCutsceneWalk = false;
		IsCutsceneCelebrate = false;

		_isMoveRequired = false;
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_parentTransform = _thisTransform.parent ?? null;
		_rigidBody = GetComponent<Rigidbody>();
		_collider = GetComponent<CapsuleCollider>();
		_animator = GetComponent<Animator>();
		_fxPush = GetComponentInChildren<ParticleSystem>();

		var cam = GameObject.FindGameObjectWithTag( UnityTags.MainCamera );
		if( cam != null )
			_camera = cam.GetComponent<Camera>();
		else
			Debug.LogError( "PlayerMovement: Main camera not found. Check main camera prefab exists in scene." );

		_mask = 1 << LayerMask.NameToLayer( "CollidableRaycast" );
		_timer = _timerIdle = 0.0f;
		_isPushing = false;

		ResetActionFlags();
	}

	//=====================================================

	void OnEnable()
	{
		InputManager.MoveLeftRightEvent += OnMoveLeftRightEvent;
		InputManager.MoveForwardBackEvent += OnMoveForwardBackEvent;
		//InputManager.PerformActionEvent += OnPerformActionEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		InputManager.MoveLeftRightEvent -= OnMoveLeftRightEvent;
		InputManager.MoveForwardBackEvent -= OnMoveForwardBackEvent;
		//InputManager.PerformActionEvent -= OnPerformActionEvent;
	}

	//=====================================================

	void Update()
	{
		if( _camera == null ) return;

		// Update player position
		MovePlayer();
	}

	//=====================================================
	// Code that moves the character needs to be checked against physics
	void FixedUpdate()
	{
		if( _camera == null ) return;

		// Update player position
		//MovePlayer();

		// Play step loops
		//AudioManagement();

		var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

		//Debug.Log( "HashId: " + HashIDs.StateJumping + " : nameHash: " + currentStateInfo.nameHash + " : fullPath " + currentStateInfo.fullPathHash );

		// Start / stop push particle emmissions
		if( IsPushingStateActive() == true )
		{
			if( _isPushing == false )
			{
				_isPushing = true;
				_fxPush.enableEmission = true;
			}
		}
		else if( _isPushing == true )
		{
			_isPushing = false;
			_fxPush.enableEmission = false;
		}

		// CurrentState Idling
		if( currentStateInfo.fullPathHash == HashIDs.StateIdling )
		{
			// Clear damaged flag
			if( IsDamaged == true )
				IsDamaged = false;

			// Clear respawn flag
			if( IsRespawning == true )
				IsRespawning = false;

			// Clear trap flags
			if( IsTrapped == true || IsEscapingTrap == true )
			{
				IsTrapped = false;
				IsEscapingTrap = false;
			}

			// Check for player interactions with scene objects
			if( IsPlayerActionRequired == true )
			{
				// Clear triggers
				//_animator.ResetTrigger( HashIDs.IsIdling02 );
				//_animator.ResetTrigger( HashIDs.IsIdling03 );
				_animator.ResetTrigger( HashIDs.IsJumping );
				_animator.ResetTrigger( HashIDs.IsLanding );

				if( IsClimbingUpOk )
					ChangeState( HashIDs.IsClimbingUp );
				else if( IsClimbingDownOk )
					ChangeState( HashIDs.IsClimbingDown );
				else if( IsOpenDoorOk )
					ChangeState( HashIDs.IsOpeningDoor );
				else if( IsUseFloorLeverOk )
					ChangeState( HashIDs.IsUsingFloorLever );
				else if( IsUseWallLeverOk )
					ChangeState( HashIDs.IsUsingWallLever );
				//else if( IsUsePressureSwitchOk )
				//	doNothing = true;
				else if(IsPushingOk)
					ChangeState( HashIDs.IsPushing );
				else if( IsPushingNotOk )
					ChangeState( HashIDs.IsPushFail );
				else if( IsCastAttackSpellOk )
					ChangeState( HashIDs.IsCastingSpell );
				else if( IsCastMeltSpellOk )
					ChangeState( HashIDs.IsCastingSpell );
				else if( IsOpenChestOk )
					ChangeState( HashIDs.IsOpeningChest );
				else if( IsInteractFail )
					ChangeState( HashIDs.IsInteractFail );
				else if( IsTrapped )
					ChangeState( HashIDs.IsTrapped );
				else if( IsCutsceneCrawl )
					ChangeState( HashIDs.IsCutsceneCrawl );
				else if( IsCutscenePortal )
					ChangeState( HashIDs.IsCutscenePortal );
				else if( IsCutsceneWalk )
					ChangeState( HashIDs.IsCutsceneWalk );
				else if( IsCutsceneCelebrate )
					ChangeState( HashIDs.IsCutsceneCelebrate );
				else
					ChangeState( HashIDs.IsJumping, _maxTimeJumpAnim, false );

				IsPlayerActionRequired = false;
			}
			//else
			//{
			//	// Player is idling / switch between alternate idle animations
			//	_timerIdle -= Time.fixedDeltaTime;

			//	if( _timerIdle <= 0.0f )
			//	{
			//		_timerIdle = 0.0f;
			//		_animator.SetBool( ( (Random.Range( 0, 99 ) < 50) ? HashIDs.IsIdling02 : HashIDs.IsIdling03 ), true );
			//	}
			//}
		}
		// CurrentState Walking / Running
		else if( currentStateInfo.fullPathHash == HashIDs.StateLocomotion || currentStateInfo.fullPathHash == HashIDs.StateRunning )
		{
			// Check for player interactions with scene objects
			if( IsPlayerActionRequired == true )
			{
				// Clear triggers
				_animator.ResetTrigger( HashIDs.IsJumping );
				_animator.ResetTrigger( HashIDs.IsLanding );

				if( IsClimbingUpOk )
					ChangeState( HashIDs.IsClimbingUp );
				else if( IsOpenDoorOk )
					ChangeState( HashIDs.IsOpeningDoor );
				else if( IsUseFloorLeverOk )
					ChangeState( HashIDs.IsUsingFloorLever );
				else if( IsUseWallLeverOk )
					ChangeState( HashIDs.IsUsingWallLever );
				else if( IsPushingOk )
					ChangeState( HashIDs.IsPushing );
				else if( IsPushingNotOk )
					ChangeState( HashIDs.IsPushFail );
				else if( IsOpenChestOk )
					ChangeState( HashIDs.IsOpeningChest );
				else if( IsInteractFail )
					ChangeState( HashIDs.IsInteractFail );
				else if( IsCastAttackSpellOk )
					ChangeState( HashIDs.IsCastingSpell );
				else if( IsCastMeltSpellOk )
					ChangeState( HashIDs.IsCastingSpell );
				else if( IsTrapped )
					ChangeState( HashIDs.IsTrapped );
				else if( IsCutsceneCelebrate )
					ChangeState( HashIDs.IsCutsceneCelebrate );
				else
					ChangeState( HashIDs.IsJumping, _maxTimeJumpAnim, false );

				IsPlayerActionRequired = false;
			}
		}
		//else if( IsInPivotState() )
		//{
		//	// Clamp speed while pivoting
		//	_speed = Mathf.Clamp( _speed, 0.0f, LocomotionThreshold );
		//}
		else if( currentStateInfo.fullPathHash == HashIDs.StateJumping )
		{
			// Check for player interactions with scene objects
			if( IsPlayerActionRequired == true )
			{
				if( IsTrapped )
				{
					_timer = 0.0f;
					ChangeState( HashIDs.IsTrapped );
					IsPlayerActionRequired = false;
					return;
				}
			}

			var isJumping = true;
			_timer -= Time.fixedDeltaTime;

			// Allow jump while within max jump-anim duration, else enter landing state
			if( _timer > 0.0f )
			{
				// If falling during jump, raycast to detect ground-hit. On hit, enter landing state/
				// Addition block for early test at start of jump state - possible mecanim or anim-blend issue causing early raycast.
				if( (GetComponent<Rigidbody>().velocity.y < 0.0f) && (_maxTimeJumpAnim - _timer > 0.2f) )
				{
					var direction = _thisTransform.TransformDirection( Vector3.down );
					RaycastHit hit;
					if( Physics.Raycast( _thisTransform.position, direction, out hit, 200.0f, _mask ) )
					{
						//Debug.Log("Jump - raycast: " + hit.distance);
						//Debug.DrawLine( _thisTransform.position, _thisTransform.position + direction * 10.0f, Color.cyan );
						if( hit.distance < 0.26f )
							isJumping = false;
					}
				}
			}
			else
			{
				isJumping = false;
				_timer = 0.0f;
			}

			if( isJumping == false )
			{
				ChangeState( HashIDs.IsLanding, 0.0f, false );
			}
		}
		// CurrentState Landing
		else if( currentStateInfo.fullPathHash == HashIDs.StateLanding )
		{
			// Check for player interactions with scene objects
			if( IsPlayerActionRequired == true )
			{
				if( IsTrapped )
				{
					ChangeState( HashIDs.IsTrapped );
					IsPlayerActionRequired = false;
					return;
				}

				ChangeState( HashIDs.IsJumping, _maxTimeJumpAnim, false );

				IsPlayerActionRequired = false;
			}
		}
		// CurrentState ClimbingUp
		else if( currentStateInfo.fullPathHash == HashIDs.StateClimbingUp )
		{
			if( !_animator.IsInTransition( 0 ) )
			{
				_collider.height = _animator.GetFloat( HashIDs.ColliderHeight );
				_collider.center = new Vector3( 0, _animator.GetFloat( HashIDs.ColliderY ), 0 );
			}

			return;
		}
		// CurrentState Trapped
		else if( currentStateInfo.nameHash == HashIDs.StateIsTrapped )
		{
			if( IsEscapingTrap == true )
			{
				IsTrapped = false;
				ChangeState( HashIDs.IsEscapingTrap );
			}
		}
		// CurrentState Damaged, DEAD, Respawning
		//else if( currentStateInfo.fullPathHash == HashIDs.StateDamaged ||
		//		currentStateInfo.nameHash == HashIDs.StateDead ||
		//		currentStateInfo.nameHash == HashIDs.StateRespawning )
		//{
		//	// Do nothing
		//}

		// Rotation
		if( currentStateInfo.fullPathHash == HashIDs.StateLocomotion ||
			currentStateInfo.fullPathHash == HashIDs.StateRunning ||
			currentStateInfo.fullPathHash == HashIDs.StateJumping )
		{
			// Rotate character model if stick is tilted left/right, but only if character is moving in that direction
			RotatePlayer();
		}
	}

	//=====================================================

	private bool IsInPivotState()
	{
		var stateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

		return (stateInfo.fullPathHash == HashIDs.StateIdlePivotLeft ||
				stateInfo.fullPathHash == HashIDs.StateIdlePivotRight ||
				stateInfo.fullPathHash == HashIDs.StateRunningPivotLeft ||
				stateInfo.fullPathHash == HashIDs.StateRunningPivotRight);
	}

	//=====================================================

	private void MovePlayer()
	{
		//float inputMagSqr;

		if( _isMoveRequired && !IsDamaged && !IsDead && !IsRespawning )
		{
			// Direction
			//inputMagSqr = new Vector2( _currentHorizontal, _currentVertical ).sqrMagnitude;

			// Convert to rough range [0.0 to 1.0] - tweaked slightly (5 + 0.5) to increase lower values slightly
			//inputMagSqr = Mathf.Clamp( inputMagSqr * 5.5f, 0.0f, 1.0f );

			_playerAngle = 0.0f;
			_direction = 0.0f;
			_speed = 0.0f;

			StickToWorldSpace( _thisTransform, _camera.transform, out _direction, out _speed, out _playerAngle, IsInPivotState() );

			// DEBUG
			// GuiManager.Instance.TxtDebug01 = "Dir: " + _direction.ToString("0.00") + " : Speed: " + _speed.ToString("0.00") + " : Angle: " + _playerAngle;

			// Direction (stick left  / right) : direction range [-1 - 1] * _directionSpeed
			_animator.SetFloat( HashIDs.Direction, _direction, _directionDampTime, Time.deltaTime );
			// Speed (stick forward / back) : speed range [0 - 5]
			_animator.SetFloat( HashIDs.Speed, _speed * _maxSpeed, _speedDampTime, Time.deltaTime );	// stickDirection.magnitude			

			// Allows for input deadzone
			if( _speed > LocomotionThreshold )
			{
				if( IsInPivotState() == false )
					_animator.SetFloat( HashIDs.Angle, _playerAngle );
			}
			else if( Mathf.Abs( _currentHorizontal ) < 0.05f && Mathf.Abs( _currentVertical ) < 0.05f )
			{
				_animator.SetFloat( HashIDs.Direction, 0.0f );
				_animator.SetFloat( HashIDs.Angle, 0.0f );
			}
		}
		else
		{
			if( Mathf.Abs( _animator.GetFloat( HashIDs.Speed ) ) > 0.005f )
			{
				// Lerp speed back to zero
				_animator.SetFloat( HashIDs.Speed, 0.0f, _speedDampTime * 0.05f, Time.deltaTime );
			}
			else
			{
				_animator.SetFloat( HashIDs.Speed, 0.0f );
				_animator.SetFloat( HashIDs.Direction, 0.0f );
			}
		}

		//Debug.Log( "speed: " + _speed + " dir: " + _direction + " angle: " + _playerAngle );
		//Debug.Log( _animator.GetFloat( HashIDs.Speed ) + " : " + _animator.GetFloat( HashIDs.Direction ) );

		_isMoveRequired = false;
	}

	//=====================================================

	private void StickToWorldSpace( Transform root, Transform playerCamera, out float directionOut, out float speedOut, out float angleOut, bool isPivoting )
	{
		var rootDirection = root.forward;
		var stickDirection = new Vector3( _currentHorizontal, 0.0f, _currentVertical );

		speedOut = Mathf.Clamp( stickDirection.magnitude, 0.0f, 1.0f );

		// Get camera rotation
		var camDirection = playerCamera.forward;
		camDirection.y = 0.0f;
		var relRotation = Quaternion.FromToRotation( Vector3.forward, Vector3.Normalize( camDirection ) );

		// Convert joystick input (stickDirection) into world space coords (from camera's perspective)
		var moveDirection = relRotation * stickDirection;	// '*' combines rotations

		// Left (+ up) Right (- down)
		var axisSign = Vector3.Cross( moveDirection, rootDirection );

		// Debug rays
		//Debug.DrawRay( new Vector3( root.position.x, root.position.y + 2.0f, root.position.z ), rootDirection, Color.magenta );
		//Debug.DrawRay( new Vector3(root.position.x, root.position.y + 2.0f, root.position.z), stickDirection, Color.red );
		Debug.DrawRay( new Vector3( root.position.x, root.position.y + 2.0f, root.position.z ), moveDirection, Color.green );
		//Debug.DrawRay( new Vector3( root.position.x, root.position.y + 2.0f, root.position.z ), axisSign, Color.magenta );

		// Range [-180 : 0 : 180]
		var angleRootToMove = Vector3.Angle( rootDirection, moveDirection ) * (axisSign.y >= 0.0f ? -1.0f : 1.0f);

		angleOut = 0.0f;
		if( isPivoting == false )
			angleOut = angleRootToMove;

		angleRootToMove /= 180.0f;

		// Range [-1 : 0 : 1] * _directionSpeed
		directionOut = angleRootToMove * _directionSpeed;

		// DEBUG
		//GuiManager.Instance.TxtDebug01 = "angleOut: " + angleOut.ToString( "##.00" ) + " directionOut: " + directionOut.ToString( "##.00" );
	}

	//=====================================================

	private void RotatePlayer()
	{
		if( _speed <= LocomotionThreshold ) return;
		
		// Lerp between zero degrees and maxDegPerSec * stick left/right amount applied to y-axis
		var rotAmount = Vector3.Lerp( Vector3.zero,
										new Vector3( 0.0f, _rotationDegPerSec * (_direction < 0.0f ? -1.0f : 1.0f), 0.0f ),
										Mathf.Abs( _direction / _directionSpeed ) );

		// Ensuring degrees of rotation per second 
		var deltaRot = Quaternion.Euler( rotAmount * Time.deltaTime );

		// Add delta rotation to current rotation
		_thisTransform.rotation *= deltaRot;

		//========================================================================================================
		// Rotate character model if stick is tilted left/right, but only if character is moving in that direction
		//if( (_direction >= 0 && _currentHorizontal >= 0) || (_direction < 0 && _currentHorizontal < 0) )
		//{
		//	// Lerp between zero degrees and maxDegPerSec * stick left/right amount applied to y-axis
		//	var rotAmount = Vector3.Lerp( Vector3.zero,
		//								  new Vector3( 0.0f, _rotationDegPerSec * (_currentHorizontal < 0.0f ? -1.0f : 1.0f), 0.0f ),
		//								  Mathf.Abs( _currentHorizontal ) );

		//	// Ensuring degrees of rotation per second 
		//	var deltaRot = Quaternion.Euler( rotAmount * Time.deltaTime );

		//	// Add delta rotation to current rotation
		//	_thisTransform.rotation *= deltaRot;
		//}

		//Vector3 targetDirection = new Vector3( _currentHorizontal, 0.0f, _currentVertical );
		//Quaternion targetRotation = Quaternion.LookRotation( targetDirection, Vector3.up );
		//Quaternion newRotation = Quaternion.Lerp( rigidbody.rotation, targetRotation, _turnSmoothing * Time.deltaTime );
		//rigidbody.MoveRotation( newRotation );
	}

	//=====================================================

	private void OnMoveLeftRightEvent( float value )
	{
		_currentHorizontal = value;
		_isMoveRequired = true;
	}

	//=====================================================

	private void OnMoveForwardBackEvent( float value )
	{
		_currentVertical = value;
		_isMoveRequired = true;
	}

	//=====================================================

	private void ChangeState( int triggerId, float timer = 0.0f, bool resetActionFlags = true )
	{
		_animator.SetTrigger( triggerId );
		_timer = timer;

		if( resetActionFlags == true )
			ResetActionFlags();
	}

	//=====================================================

	//void AudioManagement()
	//{
	//	var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

	//	if( currentStateInfo.nameHash == HashIDs.StateLocomotion ||
	//		currentStateInfo.nameHash == HashIDs.StateLanding )
	//	{
	//		// Play footstep audioclip
	//		if( !audio.isPlaying )
	//			audio.Play();
	//	}
	//	else
	//	{
	//		audio.Stop();
	//	}
	//}

	//=====================================================
}