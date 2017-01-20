using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent( typeof( Rigidbody ) )]
[RequireComponent( typeof( CapsuleCollider ) )]
[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( AudioSource ) )]
[RequireComponent( typeof( NavMeshAgent ) )]
public class NPC : MonoBehaviourEMS, ITargetWithinRange, IPauseListener
{
	public enum eNPCState
	{
		INIT,
		IDLE,		// white
		WALK,		// green
		ATTRACT,	// red
		TALK,		// blue
		DEAD,
		NULL
	}

	[SerializeField] private eNPC _type;
	[SerializeField] private AudioClip _clipFootstep;
	//[SerializeField] private AudioClip _clipOpenGuiBubble;

	private List<PathNode> _wayPoints;
	private List<Job> _currentJobs;
	private Job _listening;
	private Job _looking;
	private Job _walking;
	private Job _attracting;
	private Job _talking;

	private Transform _thisTransform;
	private NavMeshAgent _thisAgent;
	private NavMeshObstacle _thisNavMeshObstacle;
	private Rigidbody _thisRigidbody;
	private Animator _thisAnimator;
	private AudioSource _thisAudioSource;
	//private Renderer _debugStateRenderer;

	private GuiBubbleSimple _guiBubbleSimple;
	private GuiBubbleEmoticon _guiBubbleEmoticon;

	private eNPCState _currentState;
	private eNPCState _previousState;
	private const float _walkSpeed = 2.0f;
	//private const float _runSpeed = 4.0f;
	private float _animatorSpeed;

	// Trigger
	[SerializeField]
	private Transform _triggerFindTarget;
	//[Range( 3.0f, 8.0f )]
	[SerializeField]
	private float _triggerTargetRadius = 4.0f;

	private Transform _player;
	private int _maskPlayer;
	private ChestReward _reward;
	private bool _isEmoticonActivated;
	private bool _isRewardAwarded;
	private bool _isPaused;

	//=====================================================

	#region Public Interface

	public eNPCState CurrentState
	{
		get { return _currentState; }
		set
		{
			ExitState( _currentState );
			_currentState = value;
			EnterState( _currentState );
		}
	}

	public eNPC Type { get { return _type; } }

	public float TriggerTargetRadius
	{
		get { return _triggerTargetRadius; }
		set { _triggerTargetRadius = value; }
	}

	//=====================================================
	// Speech Bubble Interaction
	public void OnShowReward()
	{
		StartCoroutine( ShowReward() );
	}

	//=====================================================
	// Speech Bubble Interaction
	public void OnCollectReward()
	{
		_isRewardAwarded = true;

		if( _reward == null )
		{
			Debug.LogWarning( "NPC: OnCollectReward: reward not available." );
			return;
		}

		// Update GameDataManager's playerData for visited NPCs - blocks repeat awards on the same day 
		GameDataManager.Instance.VisitPlayerNPC( _type, true );

		// Reward: Gems or Card
		SceneManager.AwardPlayer( _reward );
	}

	//=====================================================

	public void OnPlayFootstep()
	{
		if( _thisAudioSource.isPlaying == false )
			_thisAudioSource.Play();
	}

	//=====================================================

	public void OnSpawn( PathNode[] waypoints )
	{
		SetWayPoints( waypoints );

		// Set default state to INIT -> GATHER
		if( _wayPoints.Count > 1 && _wayPoints[0] != null )
			CurrentState = eNPCState.INIT;
		else
			Debug.LogWarning( "OnSpawn: NPC appears to have none or no more than one waypoint for gathering gems" );
	}

	//=====================================================

	public void OnSleep( bool isSleeping )
	{
		OnPauseEvent( isSleeping );
	}

	//=====================================================

	public void OnDestroy()
	{
		if( _currentState != eNPCState.DEAD )
			CurrentState = eNPCState.DEAD;
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

		foreach( var job in _currentJobs )
		{
			if( job == null ) continue;

			if( isPaused == true )
				job.Pause();
			else
				job.Unpause();
		}

		// Pause / unpause agent & animations
		if( _isPaused == true )
		{
			_thisAgent.Stop();
			_animatorSpeed = _thisAnimator.speed;
			_thisAnimator.speed = 0;
		}
		else
		{
			_thisAgent.Resume();
			_thisAnimator.speed = _animatorSpeed;
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

			CurrentState = eNPCState.ATTRACT;

			// ToDo: pass in sprite from NPC via NPCManager
			//if( _guiBubbleSimple.gameObject.activeSelf == true ) return;

			if( _isEmoticonActivated == true )
				_guiBubbleEmoticon.ShowBubble( NPCManager.Instance.GetEmoticon() );
		}
		// NPC found
		//else
		//{
		//	// Check that gem hasn't already been found
		//	var isNewGem = true;

		//	foreach( var gem in _gems )
		//	{
		//		if( gem == target )
		//			isNewGem = false;
		//	}

		//	if( isNewGem == false ) return;

		//	// Check that gem is at same level as this enemy
		//	if( Mathf.Abs( _thisTransform.position.y - target.position.y ) > 2.0f ) return;

		//	// Add new gem to queue
		//	_gems.Enqueue( target );
		//}
	}

	//=====================================================
	// Trigger update - player not within range
	public void OnTargetLost()
	{
		_player = null;

		if( _isEmoticonActivated == true )
			_guiBubbleEmoticon.HideBubble();
	}

	#endregion

	//=====================================================

	#region Private Methods

	#region Unity Calls

	public void Awake()
	{
		_wayPoints = new List<PathNode>();
		_currentJobs = new List<Job>();

		_maskPlayer = 1 << LayerMask.NameToLayer( "Player" ) | 1 << LayerMask.NameToLayer( "Collidable" );
		//_maskShield = 1 << LayerMask.NameToLayer( "Collidable" );

		_thisTransform = this.transform;
		_thisAgent = _thisTransform.GetComponent<NavMeshAgent>();
		_thisNavMeshObstacle = _thisTransform.GetComponent<NavMeshObstacle>();
		_thisRigidbody = _thisTransform.GetComponent<Rigidbody>();
		_thisAnimator = _thisTransform.GetComponent<Animator>();
		_triggerFindTarget = _thisTransform.FindChild( "TriggerFindTarget" );
		_triggerFindTarget.GetComponent<SphereCollider>().radius = _triggerTargetRadius;
		_guiBubbleSimple = _thisTransform.GetComponentInChildren<GuiBubbleSimple>();
		_guiBubbleEmoticon = _thisTransform.GetComponentInChildren<GuiBubbleEmoticon>();
		//_debugStateRenderer = _thisTransform.FindChild( "DebugState" ).GetComponent<Renderer>();

		_thisAudioSource = _thisTransform.GetComponent<AudioSource>();
		_thisAudioSource.clip = _clipFootstep;
		_isPaused = false;
		_isEmoticonActivated = false;
	}

	//=====================================================

	private void OnEnable()
	{
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
		ExitState( eNPCState.NULL );
	}

	//=====================================================

	private void Start()
	{
		// Inject this object into trigger
		_triggerFindTarget.GetComponent<TriggerTargetInRange>().Init( this );

		// Clear jobs list
		if( _currentJobs != null )
			_currentJobs.Clear();

		// ToDo: Update this so that a reward is only given on a new day (GameDataManager should clear NPCsVisited array each new-day) 
		// Set up simple speech bubble
		var isRewardAvailable = true;
		//_isRewardAwarded = false;

		if( GameDataManager.Instance.IsPlayerNPCVisited( _type ) )
		{
			_guiBubbleSimple.ShowNewMeetingIcon();
			isRewardAvailable = false;
			//_isRewardAwarded = true;
			//Debug.Log( "Visited" );
		}
		else
		{
			_guiBubbleSimple.ShowAttentionIcon();
			//Debug.Log( "NOT Visited" );
		}

		// Set up reward speech bubble
		if( _reward == null )
			_reward = new ChestReward();

		switch( _type )
		{
			case eNPC.NPC_STUDENT:
				// Gems
				_reward.Gems = Convert.ToInt32( NPCItemsManager.GetNPCItemGemReward( _type ) );
				break;

			default:
				// Reward common card or gems
				if( Random.Range( 0, 99 ) < 50 )
					_reward.Card = SceneManager.GetNPCReward();
				else
					_reward.Gems = Convert.ToInt32( NPCItemsManager.GetNPCItemGemReward( _type ) );
				break;
		}

		// Set reward available / unavailable
		if( _guiBubbleEmoticon != null )
			_guiBubbleEmoticon.SetReward( isRewardAvailable, _reward.Gems > 0 );
		else
			Debug.Log( "Warning: Emoticon bubble is probably disabled in NPC prefab." );
		
	}

	#endregion

	//=====================================================

	private IEnumerator ShowReward()
	{
		yield return new WaitForSeconds( 0.4f );

		//_guiBubbleEmoticon.gameObject.SetActive( true );
		_guiBubbleEmoticon.ShowBubble( NPCManager.Instance.GetEmoticon() );

		_isEmoticonActivated = true;
	}

	//=====================================================

	#region State Controllers

	private void EnterState( eNPCState npcStateEntered )
	{
		//_triggerTargetInRange.SetActive( true );

		switch( npcStateEntered )
		{
			case eNPCState.INIT:
				_thisNavMeshObstacle.enabled = false;
				_thisAgent.enabled = true;
				_thisRigidbody.isKinematic = true;

				// Delay then GATHER - delay ensures jobs are added to JobManager correctly
				StartCoroutine( Initialising() );
				break;

			case eNPCState.IDLE:
				// ToDo: do something
				Debug.Log( "NPC Idle!" );

				_thisAgent.enabled = false;
				//_thisNavMeshObstacle.enabled = true;
				break;

			case eNPCState.WALK:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.green;
				//Debug.Log( "NPC Walk!" );

				// Assign jobs
				_walking = new Job( Walking( () =>
				{
					Debug.Log( "Met NPC. Talking!" );
					CurrentState = eNPCState.TALK;
				} ),
									 true );

				//_looking = new Job( Looking( 150.0f, _triggerTargetRadius, 0.33f, false,
				//							 () =>
				//							 {
				//								 if( GameDataManager.Instance.IsPlayerHealthFull() == true || IsPlayerAttackOk() == false )
				//								 {
				//									 //Debug.Log( "Saw player. Run!" );
				//									 CurrentState = eEnemyState.ESCAPE;
				//								 }
				//								 else
				//								 {
				//									 //Debug.Log( "Saw player. Attack!" );
				//									 CurrentState = eEnemyState.HUNT;
				//								 }
				//							 } ),
				//							 true );


				// Update jobs list
				_currentJobs.Add( _walking );
				//_currentJobs.Add( _looking );
				break;

			case eNPCState.ATTRACT:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.blue;
				//Debug.Log( "NPC Attract!" );

				// Assign jobs
				_attracting = new Job( Attracting( () =>
													{
														//Debug.Log( "Attracting attention!" );

														CurrentState = eNPCState.WALK;
													} ),
													true );
				// Update jobs list
				_currentJobs.Add( _attracting );
				break;

			case eNPCState.TALK:
				// ToDo: DEBUG - REMOVE THIS
				//_debugStateRenderer.material.color = Color.yellow;
				//Debug.Log( "NPC Talk!" );

				_thisAgent.enabled = false;
				_thisNavMeshObstacle.enabled = true;

				// Assign jobs
				//_escaping = new Job( Escaping( () =>
				//{
				//	//Debug.Log( "Escaped player. Phew!" );
				//	CurrentState = eEnemyState.POST_ESCAPE;
				//} ),
				//								true );
				//// Update jobs list
				//_currentJobs.Add( _escaping );
				break;

			case eNPCState.DEAD:
				// Do nothing
				break;
		}
	}

	//=====================================================

	private void ExitState( eNPCState npcStateExited )
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
		yield return new WaitForSeconds( 1.0f );

		if( _thisAgent == null )
			_thisAgent = _thisTransform.GetComponent<NavMeshAgent>();

		_thisAgent.avoidancePriority = Random.Range( 10, 30 );

		_guiBubbleSimple.ShowNewMeetingIcon();

		CurrentState = eNPCState.WALK;
	}

	//=====================================================

	private IEnumerator Walking( Action onComplete )
	{
		if( _wayPoints == null || _wayPoints.Count == 0 )
		{
			if( _currentState != eNPCState.IDLE )
			{
				Debug.LogWarning( "Waypoint no found! Add waypoints to NPC prefab." );
				CurrentState = eNPCState.IDLE;
			}
			yield return null;
		}

		var wayPointIndex = Random.Range( 0, _wayPoints.Count );

		while( true )
		{
			// Select new waypoint in scene
			var lastIndex = wayPointIndex;
			var isNewIndexOk = true;
			var skip = 0;

			_thisNavMeshObstacle.enabled = false;
			yield return new WaitForSeconds( 0.1f );
			_thisAgent.enabled = true;

			do
			{
				wayPointIndex = Random.Range( 0, _wayPoints.Count );

				if( wayPointIndex == lastIndex )
					isNewIndexOk = false;
				else
				{
					// Limit selection to a max distance between next waypoint
					isNewIndexOk = (_wayPoints[wayPointIndex].transform.position - _wayPoints[lastIndex].transform.position).sqrMagnitude < 225;

					if( isNewIndexOk == false )
					{
						// Allow for edge case where NPC is somehow stuck at a point out of range of other waypoints
						if( ++skip > 5 )
							isNewIndexOk = true;
					}
				}

			} while( isNewIndexOk == false );

			// Set walk speed
			_thisAnimator.SetBool( HashIDs.Stop, false );
			_thisAgent.speed = _walkSpeed;
			_thisAnimator.SetTrigger( HashIDs.Walk );

			// Attract player if available
			//var isTargetingGem = IsGemAvailable();

			// Else target next waypoint for hunting / gathering gems
			//if( isTargetingGem == false )
			if( _thisAgent.SetDestination( _wayPoints[wayPointIndex].transform.position ) == false )
				Debug.Log( "Problem with destination" );

			// Move toward target
			while( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > 1.0f )
			{
				// Check for any new NPCs found
				//guard_motor.facingDirection = patrolPoints[i].position - _transform.position;
				yield return null;
			}

			// Arrived at target ( gem or gathering point )
			//if( isTargetingGem == true )
			//{
			//	// *** Gem gathered -> EAT gem ***
			//	if( onComplete != null )
			//		onComplete();
			//}

			// Stop movement
			_thisAgent.speed = 0.0f;
			_thisAnimator.SetBool( HashIDs.Stop, true );

			_thisAgent.enabled = false;
			yield return new WaitForSeconds( 0.1f );
			_thisNavMeshObstacle.enabled = true;

			yield return new WaitForSeconds( 2.0f );
		}
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
					// *** Player spotted -> EXPLODE or ESCAPE ***
					if( onComplete != null )
						onComplete();
				}
			}

			yield return new WaitForSeconds( delayBetweenLooks );
		}
	}

	//=====================================================

	private IEnumerator Attracting( Action onComplete )
	{
		// Stop movement
		if( _thisAgent.speed > 0.0f )
		{
			_thisAgent.speed = 0.0f;
			_thisAnimator.SetTrigger( HashIDs.Stop );
		}

		_thisNavMeshObstacle.enabled = false;
		yield return new WaitForSeconds( 0.1f );
		_thisAgent.enabled = true;

		if( _player == null )
			_player = PlayerManager.Transform;

		var destination = (_player.position - _thisTransform.position).normalized;

		if( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > 1.0f )
		{
			_thisAgent.SetDestination( _thisTransform.position + destination * 1.5f );

			// Set walk speed
			_thisAgent.speed = _walkSpeed;
			_thisAnimator.SetTrigger( HashIDs.Walk );

			// Move toward target
			while( (_thisTransform.position - _thisAgent.destination).sqrMagnitude > 1.0f )
			{
				yield return null;
			}

			// Stop movement
			_thisAgent.speed = 0.0f;
		}

		_thisAgent.enabled = false;
		yield return new WaitForSeconds( 0.1f );
		_thisNavMeshObstacle.enabled = true;

		// Turn to player
		while( _player != null && SmoothLookAt( _player.position ) == false )
		{
			yield return null;
		}

		_thisAnimator.SetTrigger( HashIDs.Attract );

		// Stall for 'talking' to player
		yield return new WaitForSeconds( 2.0f );
		_thisAnimator.SetTrigger( HashIDs.Interact );
		yield return new WaitForSeconds( 3.0f );

		// *** Attracting attention -> WALK ***
		if( onComplete != null )
			onComplete();
	}

	//=====================================================
	// Returns true if close to target lookAt
	private bool SmoothLookAt( Vector3 targetLookAt )
	{
		const float smoothLookAt = 7.5f;

		// Create a vector from the camera towards the player.
		var relPlayerPosition = targetLookAt - _thisTransform.position;
		relPlayerPosition.y = 0.0f;

		// Create a rotation based on the relative position of the player being the forward vector.
		var lookAtRotation = Quaternion.LookRotation( relPlayerPosition, Vector3.up );

		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		_thisTransform.rotation = Quaternion.Lerp( _thisTransform.rotation, lookAtRotation, smoothLookAt * Time.deltaTime );

		// Stop when rougly 1 degree from target lookAt
		return (Mathf.Abs( _thisTransform.rotation.eulerAngles.y - lookAtRotation.eulerAngles.y )) < 1.0f;
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

	#endregion

	#endregion

	//=====================================================
}
