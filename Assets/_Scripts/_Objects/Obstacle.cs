using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( Rigidbody ) )]
public class Obstacle : MonoBehaviourEMS, IPauseListener, IPlayerInteraction
{
	// Editable in inspector
	[SerializeField] protected eObstacleType _type = eObstacleType.SWINGING;
	[SerializeField] protected int _model = 0;

	// Other vars
	[SerializeField] protected Transform _thisTransform;
	[SerializeField] protected Transform _obstacle;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private Transform _trigger;
	[SerializeField] private Collider _collider;
	[SerializeField] private int _interactiveLevel = 0;

	// Non-serialized
	private AudioSource _audioSource;
	private bool _isMoving;

	//=====================================================

	#region Public Interface

	public eObstacleType Type { get { return _type; } set { _type = value; } }
	public int Model { get { return _model; } set { _model = value; } }
	public int InteractiveLevel { get { return _interactiveLevel; } set { _interactiveLevel = value; } }

	//=====================================================

	public void Init( GameObject model )
	{
		// Remove previous obstacle instances
		if( _obstacle != null )
			DestroyImmediate( _obstacle.gameObject );

		_obstacle = model.transform;
		_obstacle.parent = _thisTransform;
		_obstacle.name = "Obstacle";

		// Store this switch-prefab's rotation then zero it out before updating obstacle
		var rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position obstacle
		_obstacle.localPosition = Vector3.zero;
		_obstacle.localRotation = Quaternion.Euler( Vector3.zero );

		// Reset the door prefab rotation
		_thisTransform.rotation = rot;

		// Position above / on floor (world origin)
		_thisTransform.position = new Vector3( _thisTransform.position.x,
												_obstacle.GetComponent<Collider>().bounds.size.y * 0.5f,
												_thisTransform.position.z );

		// Pushable box - set trigger size according to obstacle's axis-restraint
		if( _type != eObstacleType.PUSHABLE_BOX ) return;
		
		CheckAxisRestraints();

		_collider = _obstacle.GetComponent<Collider>();
		var triggerSize = _collider.bounds.size;

		if( IsRestrainedOnXAxis() == false )
			triggerSize = new Vector3( triggerSize.x + 0.9f, triggerSize.y * 0.5f, triggerSize.z * 0.65f );
		else if( IsRestrainedOnZAxis() == false )
			triggerSize = new Vector3( triggerSize.x * 0.65f, triggerSize.y * 0.5f, triggerSize.z + 0.9f );

		_trigger.localScale = triggerSize;
		_trigger.localPosition = Vector3.zero;
	}

	//=====================================================

	public virtual void Refresh()
	{
		CheckReferences();

		CheckAxisRestraints();

		// Update obstacle-models
		var mdl = ResourcesObstacles.GetModel( _type, _model );
		var model = Instantiate( mdl ) as GameObject;

		Init( model );
	}

	//=====================================================

	public bool IsRestrainedOnXAxis()
	{
		return ((_rigidbody.constraints & RigidbodyConstraints.FreezePositionX) != RigidbodyConstraints.None);
	}

	//=====================================================

	public bool IsRestrainedOnZAxis()
	{
		return ((_rigidbody.constraints & RigidbodyConstraints.FreezePositionZ) != RigidbodyConstraints.None);
	}

	#endregion

	//=====================================================

	#region IPlayerInteraction

	public bool IsInteractionOk()
	{
		// Block player-object interaction if current fairy level is too low
		if( _type != eObstacleType.PUSHABLE_BOX ) return true;

		return _interactiveLevel <= GameDataManager.Instance.PlayerCurrentFairyLevel;
	}

	//=====================================================

	public Transform OnPlayerInteraction()
	{
		if( _type != eObstacleType.PUSHABLE_BOX ) return null;

		// Allow fairy to push box
		_rigidbody.isKinematic = false;

		Vector3 direction, position;

		// Face player along axis that obstacle can move and snap into position
		if( IsRestrainedOnXAxis() == true )
		{
			direction = (_thisTransform.position.z > PlayerManager.Position.z) ? Vector3.forward : -Vector3.forward;
			position = new Vector3( _thisTransform.position.x, PlayerManager.Position.y, PlayerManager.Position.z );
		}
		else
		{
			direction = (_thisTransform.position.x > PlayerManager.Position.x) ? Vector3.right : -Vector3.right;
			position = new Vector3( PlayerManager.Position.x, PlayerManager.Position.y, _thisTransform.position.z );
		}

		PlayerManager.Direction = direction;
		PlayerManager.Position = position;

		// We're managing player position relative to obstacle so transform not required
		return null;
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public void OnPlayCutsceneAnimation( int animationIndex = 1 )
	{
		// No cutscene for this object
	}

	//=====================================================

	public Transform[] CutsceneCameraPoints()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public Transform CutsceneCameraLookAt()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public Transform CutscenePlayerPoint()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public float CutsceneDuration()
	{
		// No cutscene for this object
		return 2.5f;
	}

	//=====================================================

	public bool OrientCameraToPath()
	{
		// No cutscene for this object
		return false;
	}

	//=====================================================

	public bool IsFlyThruAvailable()
	{
		// No cutscene for this object
		return false;
	}

	//=====================================================

	public CameraPathAnimator GetFlyThruAnimator()
	{
		// No cutscene for this object
		return null;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public virtual void OnPauseEvent( bool isPaused )
	{
		// Do nothing
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;

		if( _type != eObstacleType.PUSHABLE_BOX ) return;

		_rigidbody = _thisTransform.GetComponent<Rigidbody>();
		_trigger = _thisTransform.FindChild( "TriggerPushObject" );
		_audioSource = _thisTransform.GetComponent<AudioSource>();

		_isMoving = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying )
			GameManager.Instance.PauseEvent += OnPauseEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;
		
		if( _isAppQuiting == false )
			GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================

	void Update()
	{
		if( _type != eObstacleType.PUSHABLE_BOX ) return;

		if( _rigidbody == null ) return;

		// Play pushing box audio clip?
		if( _isMoving == false && _rigidbody.velocity.sqrMagnitude > 0.1f )
		{
			_isMoving = true;

			// Play audio fx
			if( _audioSource != null )
				_audioSource.Play();
		}
		else if( _isMoving == true && _rigidbody.velocity.sqrMagnitude < 0.1f )
		{
			_isMoving = false;

			// Stop audio fx
			if( _audioSource != null )
				_audioSource.Stop();
		}
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	protected virtual void CheckReferences()
	{
		// Try to find obstacle components if reference has been lost
		if( _obstacle == null )
		{
			_obstacle = _thisTransform.FindChild( "Obstacle" );

			if( _type == eObstacleType.PUSHABLE_BOX )
				_collider = _obstacle.GetComponent<Collider>();
		}

		if( _obstacle == null )
			Debug.Log( "CheckReferences: Obstacle not found" );
	}

	//=====================================================

	// If x-ais is unlocked, lock z-axis and vica-versa
	private void CheckAxisRestraints()
	{
		if( (_rigidbody.constraints & RigidbodyConstraints.FreezePositionX) == RigidbodyConstraints.None )
		{
			// Restrain on Y and Z
			_rigidbody.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
		}
		else
		{
			// Restrain on X and Y
			_rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
			_rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
		}

		// Setting these just in case
		_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
	}

	#endregion

	//=====================================================
}
