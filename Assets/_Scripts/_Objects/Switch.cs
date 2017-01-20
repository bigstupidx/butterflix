using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class Switch : MonoBehaviourEMS, IPauseListener, IPlayerInteraction
{
	public event Action	<int> SwitchActivated;
	public event Action	<int> SwitchDeactivated;

	// Editable in inspector
	[SerializeField] private eSwitchType _type = eSwitchType.FLOOR_LEVER;
	[SerializeField] private int _model = 0;

	// Other vars
	[SerializeField] private Transform _thisTransform;
	[SerializeField] private Transform _switch;
	[SerializeField] private Transform _trigger;
	[SerializeField] private Transform _interactPosition;
	[SerializeField] private Collider _collider;
	[SerializeField] private Animator _animator;
	[SerializeField] private int _index = 0;
	[SerializeField] private int _interactiveLevel = 0;

	// Non-serialized
	private AudioSource _audioSource;
	private bool _isPaused;
	private bool _isActivated;
	private bool _checkAnimIsFinished;

	//=====================================================

	#region Public Interface

	public eSwitchType Type { get { return _type; } set { _type = value; } }
	public int Model { get { return _model; } set { _model = value; } }
	public int Index { get { return _index; } set { _index = value; ResetName(); } }
	public bool IsActivated { get { return _isActivated; } private set { _isActivated = value; } }
	public int InteractiveLevel { get { return _interactiveLevel; } set { _interactiveLevel = value; } }

	//=====================================================

	public void Init( GameObject switchModel )
	{
		// Remove previous switch instance
		if( _switch != null )
			DestroyImmediate( _switch.gameObject );

		_switch = switchModel.transform;
		_switch.parent = _thisTransform;
		_switch.name = "Switch";

		CheckReferences();

		// Store this switch-prefab's rotation then zero it out before updating switch
		var rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position switch
		_switch.localPosition = Vector3.zero;
		_switch.localRotation = Quaternion.Euler( Vector3.zero );

		// Initialize switch-trigger
		InitTrigger();

		// Set interaction position
		switch( _type )
		{
			case eSwitchType.FLOOR_LEVER:
				_interactPosition.localPosition = new Vector3( 0.0f, 0.0f, -0.5f );
				break;
			case eSwitchType.WALL_SWITCH:
				_interactPosition.localPosition = new Vector3( 0.0f, 0.0f, 1.5f );
				break;
		}

		// Reset the door prefab rotation
		_thisTransform.rotation = rot;
	}

	//=====================================================

	public void Refresh()
	{
		CheckReferences();

		// Update switch name with index e.g. Switch-Floor-0, Switch-Wall-1
		ResetName();

		// Update switch-model
		//Debug.Log( "Model type: " + _type + " : " + _model );
		var mdlSwitch = ResourcesSwitches.GetModel( _type, _model );
		var switchModel = Instantiate( mdlSwitch ) as GameObject;
		Init( switchModel );
	}

	//=====================================================

	public void OnDeactivation()
	{
		OnDeactivateSwitch();

		// Update related door / object about switch-deactivation
		if( SwitchDeactivated != null )
			SwitchDeactivated( _index );
	}

	#endregion

	//=====================================================

	#region IPlayerInteraction

	public bool IsInteractionOk()
	{
		// Block player-object interaction if current fairy level is too low
		if( _interactiveLevel > GameDataManager.Instance.PlayerCurrentFairyLevel )
			return false;

		// Normal switches are activated once - pressure switches can be deactivated
		if( _type == eSwitchType.PRESSURE )
			return true;

		return !_isActivated;
	}

	//=====================================================

	public Transform OnPlayerInteraction()
	{
		if( _type != eSwitchType.PRESSURE )
		{
			PlayerManager.Position = _interactPosition.position;
			PlayerManager.Direction = _switch.transform.position - _interactPosition.position;
		}

		OnActivateSwitch();

		// Managing player position relative to switch - transform not required
		return null;
	}

	//=====================================================

	public void OnPlayCutsceneAnimation( int animationIndex = 1 )
	{
		// No cutscene for this object
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		// No cutscene for this object
		return null;
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

	public void OnPauseEvent( bool isPaused )
	{
		CheckReferences();

		// Only unpause animator if it was active prior to pause-event
		if( _animator == null ) return;
		
		if( isPaused )
			_animator.speed = 0.0f;
		else if( _checkAnimIsFinished == true )
			_animator.speed = 1.0f;
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		_interactPosition = _thisTransform.FindChild( "InteractPosition" );
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying )
		{
			GameManager.Instance.PauseEvent += OnPauseEvent;

			_isPaused = false;
			_isActivated = false;
			_checkAnimIsFinished = false;
			
			// Pressure switches don't require an animator
			if( _animator != null )
				_animator.speed = 0.0f;
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying )
		{
			if( _isAppQuiting == false )
				GameManager.Instance.PauseEvent -= OnPauseEvent;
		}
	}

	//=====================================================

	void Update()
	{
		if( _isPaused == true ) return;

#if UNITY_EDITOR
		// DEBUG - REMOVE THIS
		if( Input.GetKeyDown( KeyCode.L ) )
			OnActivateSwitch();
#endif
		// When switch anim completes send message to attached door
		if( _checkAnimIsFinished == true )
		{
			CheckSwitchAnimCompleted();
		}
	}

	//=====================================================

	//void OnDrawGizmos()
	//{
	//		// Allow for rotations
	//		Gizmos.matrix = transform.localToWorldMatrix;
	//		Gizmos.color = GetGizmoColor();
	//		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	//}

	//=====================================================

	private void OnActivateSwitch()
	{
		CheckReferences();

		// Pressure switches don't require an animator
		if( _animator != null )
		{
			_animator.speed = 1.0f;
			_animator.SetBool( HashIDs.IsSwitchOn, true );
		}

		// Play audio fx
		if( _audioSource != null )
			_audioSource.Play();

		_isActivated = true;
		_checkAnimIsFinished = true;
	}

	//=====================================================

	private void OnDeactivateSwitch()
	{
		CheckReferences();

		// Pressure switches don't require an animator
		if( _animator != null )
		{
			_animator.speed = 1.0f;
			_animator.SetBool( HashIDs.IsSwitchOn, false );
		}

		_isActivated = false;
		_checkAnimIsFinished = true;
	}

	//=====================================================

	private void CheckSwitchAnimCompleted()
	{
		if( _type != eSwitchType.PRESSURE )
		{
			if( _animator == null ) _animator = _switch.GetComponent<Animator>();

			// Get current state's info (0 : on Base Layer)
			var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

			if( currentStateInfo.normalizedTime < 1 || _animator.IsInTransition( 0 ) == true ) return;

			if( currentStateInfo.nameHash != HashIDs.StateSwitchOnHash ) return;
		}

		// Update related door about switch-activation
		if( _isActivated == true && SwitchActivated != null )
			SwitchActivated( _index );

		_checkAnimIsFinished = false;

		if( _animator != null )
			_animator.speed = 0.0f;
	}

	//=====================================================

	//IEnumerator OnSwitchAnimComplete()
	//{
	//	if( _animator == null ) _animator = _switch.GetComponent<Animator>();

	//	// Get current state's info (0 : on Base Layer)
	//	var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

	//	while( currentStateInfo.nameHash != Animator.StringToHash( "Base Layer.Switch_On" ) )
	//	{
	//		Debug.Log( "yielding: " + Animator.StringToHash( "Base Layer.Switch_On" ) + " : " + currentStateInfo.nameHash );
	//		yield return new WaitForSeconds( 0.1f );
	//	}

	//	Debug.Log( "Switch anim completed" );

	//	// Update related door about switch-activation
	//	if( SwitchActivated != null )
	//		SwitchActivated( _index );
	//}

	//=====================================================

	private void InitTrigger()
	{
		var triggerSize = new Vector3( 1.0f, 0.25f, 1.5f );

		_trigger = _thisTransform.FindChild( "TriggerActivateSwitch" );

		if( _collider != null )
		{
			var switchWidth = _collider.bounds.size.x;
			triggerSize = new Vector3( switchWidth * 3.0f, triggerSize.y, triggerSize.z );
		}

		// Set trigger position
		switch( _type )
		{
			case eSwitchType.FLOOR_LEVER:
				_trigger.localScale = triggerSize;
				_trigger.localPosition = new Vector3( 0.0f, _trigger.localScale.y * 0.5f, -_trigger.localScale.z * 0.5f );
				break;
			case eSwitchType.WALL_SWITCH:
				_trigger.localScale = triggerSize;
				_trigger.localPosition = new Vector3( 0.0f, _trigger.localScale.y * 0.5f, _trigger.localScale.z * 0.75f );
				break;
			case eSwitchType.PRESSURE:
				//_trigger.GetComponent<TriggerPressureSwitch>().Radius = _radius;
				break;
		}
	}

	//=====================================================

	private void ResetName()
	{
		// Update switch name with index e.g. Switch-Floor-0, Switch-Wall-1
		var name = _thisTransform.name;
		name = name.Substring( 0, name.LastIndexOf( "-", StringComparison.Ordinal ) + 1 ) + _index;
		_thisTransform.name = name;
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	private void CheckReferences()
	{
		// Try to find switch if reference has been lost
		if( _switch == null )
			_switch = _thisTransform.FindChild( "Switch" );

		if( _switch != null )
		{
			// Get animator to control switch animations
			if( _animator == null )
			{
				_animator = _switch.GetComponent<Animator>();
				//if( _animator == null )
				//	Debug.Log( "CheckReferences: Switch Animator not found" );
			}

			// Get collider to determine trigger size
			if( _collider == null && _type != eSwitchType.PRESSURE )
			{
				_collider = _switch.GetComponent<Collider>();
				if( _collider == null )
					Debug.Log( "CheckReferences: Switch Collider not found" );
			}
		}
		else
		{
			Debug.Log( "CheckReferences: Switch not found" );
		}
	}

	//=====================================================

	//private Color GetGizmoColor()
	//{
	//	switch( _switchType )
	//	{
	//		case eSwitchType.FLOOR_LEVER:
	//		case eSwitchType.WALL_SWITCH:
	//			return Color.blue;
	//		default:
	//			return Color.white;
	//	}
	//}

	#endregion

	//=====================================================
}
