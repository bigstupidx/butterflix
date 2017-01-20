using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class Platform : MonoBehaviourEMS, IPauseListener, ICutsceneObject
{
	public enum ePathParameter
	{
		ONE_SHOT = 0,
		PING_PONG,
		LOOP
	}

	//public event Action PlatformActivated;

	// Editable in inspector
	[SerializeField] private ePlatformType _type = ePlatformType.ON_PATH;
	[SerializeField] private int _model = 0;
	[SerializeField] protected Transform[] _pathNodes;
	[SerializeField] private float _durationNodeToNode = 2.0f;
	[SerializeField] private float _durationStartToEnd = 7.0f;
	[SerializeField] protected Switch[] _switches;

	// Other vars
	[SerializeField] private Transform _thisTransform;
	[SerializeField] private Transform _platform;
	[SerializeField] private Transform _trigger;
	[SerializeField] private LTSpline _spline;
	[SerializeField] protected ePathParameter _pathParameter = ePathParameter.PING_PONG;
	[SerializeField] private Vector3[] _path;
	[SerializeField] private GameObject _nodesContainer;
	[SerializeField] private CutsceneContainer _cutsceneContainer;

	// Non-serialized
	private int _pathIndex;
	private int _tweenId = -1;
	private bool _isPlatformActivated;
	private bool _isPlatformFollowingPath;

	//=====================================================

	#region Public Interface

	public ePlatformType Type { get { return _type; } set { _type = value; } }

	public int Model { get { return _model; } set { _model = value; } }

	public float DurationNodeToNode { get { return _durationNodeToNode; } set { _durationNodeToNode = value; } }

	public float DurationStartToEnd { get { return _durationStartToEnd; } set { _durationStartToEnd = value; } }

	//=====================================================

	public void Init( GameObject platformModel )
	{
		// Remove previous model instance
		if( _platform != null )
			DestroyImmediate( _platform.gameObject );

		_platform = platformModel.transform;
		_platform.parent = _thisTransform;
		_platform.name = "Platform";

		CheckReferences();

		// Store this platform-prefab's rotation then zero it out before updating model
		var rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position model
		_platform.localPosition = Vector3.zero;
		_platform.localRotation = Quaternion.Euler( Vector3.zero );

		// Reset the prefab rotation
		_thisTransform.rotation = rot;
	}

	//=====================================================

	public void Refresh()
	{
		CheckReferences();

		// Update platform-model
		var model = ResourcesPlatforms.GetModel( _model );
		var instance = Instantiate( model ) as GameObject;
		Init( instance );

		if( _pathNodes.Length <= 0 || _pathNodes[0] == null )
			return;

		if( _nodesContainer == null )
		{
			_nodesContainer = _type == ePlatformType.ON_PATH ? new GameObject( "PlatformPathContainer" ) : new GameObject( "PlatformSplineContainer" );

			// Move this platform into container
			_thisTransform.parent = _nodesContainer.transform;
			_nodesContainer.transform.position = Vector3.zero;
		}

		// Update path node names to math their path index
		for( var i = 0; i < _pathNodes.Length; i++ )
		{
			if( _pathNodes[i] == null ) continue;
			
			_pathNodes[i].name = "PathNode" + i.ToString( "00" );
			// Move node into container
			_pathNodes[i].parent = _nodesContainer.transform;
		}

		if( _type == ePlatformType.ON_PATH )
			InitPlatformPath();
		else
			InitPlatformSpline();

		// Update switch indexes
		for( var i = 0; i < _switches.Length; i++ )
		{
			if( _switches[i] == null ) continue;
			
			_switches[i].Index = i;
			Debug.Log( "Switch " + _switches[i].Index + " found: activated: " + _switches[i].IsActivated );
		}
	}

	#endregion

	//=====================================================

	#region ICutsceneObject

	public virtual void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		SwitchActivatesPlatform();
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
		if( _isPlatformFollowingPath == false ) return;

		if( _isPlatformActivated == false ) return;

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

	private void Awake()
	{
		_thisTransform = this.transform;
		_isPlatformActivated = false;
		_isPlatformFollowingPath = false;
	}

	//=====================================================

	private void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		// Ensure path nodes have been assigned for moving platform
		if( _pathNodes.Length <= 0 || _pathNodes[0] == null )
		{
			Debug.LogWarning( "Platform has no path nodes assigned!" );
			return;
		}

		GameManager.Instance.PauseEvent += OnPauseEvent;

		// If no switches are attached then activate platform along path
		if( _switches == null || _switches.Length <= 0 )
		{
			switch( _type )
			{
				case ePlatformType.ON_PATH:
					FollowPath();
					break;

				case ePlatformType.ON_SPLINE:
					FollowSpline();
					break;
			}

			_isPlatformActivated = true;
		}
		else
		{
			// Set switch index and register with switch-activation events
			for( var i = 0; i < _switches.Length; i++ )
			{
				if( _switches[i] == null ) continue;

				_switches[i].Index = i;
				_switches[i].SwitchActivated += OnSwitchActivated;

				if( _switches[i].Type == eSwitchType.PRESSURE )
					_switches[i].SwitchDeactivated += OnSwitchDeactivated;
			}
		}

		// Platforms should have a parent container that may include a cutscene container
		_cutsceneContainer = _thisTransform.parent != null ? _thisTransform.parent.GetComponentInChildren<CutsceneContainer>() : null;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;

		// Unregister with switches (puzzle-door only) for switch activations
		foreach( var s in _switches )
		{
			if( s == null ) continue;

			s.SwitchActivated -= OnSwitchActivated;

			if( s.Type == eSwitchType.PRESSURE )
				s.SwitchDeactivated -= OnSwitchDeactivated;
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;

		if( _path == null || _path.Length <= 0 ) return;

		// Visualize the path / spline
		switch( _type )
		{
			case ePlatformType.ON_PATH:
				for( var i = 0; i < _path.Length - 1; i++ )
					Gizmos.DrawLine( _path[i], _path[i + 1] );
				break;

			case ePlatformType.ON_SPLINE:
				if( _spline != null )
					_spline.gizmoDraw();
				break;
		}

		// Draw lines between door and referenced switches
		foreach( var s in _switches )
		{
			if( s != null )
				Gizmos.DrawLine( _thisTransform.position, s.transform.position );
		}
	}

	//=====================================================

	private void OnSwitchActivated( int index )
	{
		// Start cutscene
		if( _cutsceneContainer != null )
			CutsceneManager.Instance.OnStartCutscene( eCutsceneType.SWITCH_ACTIVATES_PLATFORM, this );
		else
			SwitchActivatesPlatform();

		// Only unregister from one-shot switches i.e. that can't be deactivated
		if( _switches[index].Type != eSwitchType.PRESSURE )
			_switches[index].SwitchActivated -= OnSwitchActivated;

		//Debug.Log( "Switch activated: " + _switches[index].Index );
	}

	//=====================================================

	private void OnSwitchDeactivated( int index )
	{
		// Deactivating a switch should result in the door(s) closing
		if( _isPlatformFollowingPath == true && _isPlatformActivated == true )
		{
			switch( _type )
			{
				case ePlatformType.ON_PATH:
					// Return platform to start
					if( _pathParameter == ePathParameter.ONE_SHOT )
						FollowPathReversed();
					else
						OnPauseEvent( true );
					break;

				case ePlatformType.ON_SPLINE:
					OnPauseEvent( true );
					break;
			}
		}

		_isPlatformActivated = false;

		//Debug.Log( "Switch deactivated: " + _switches[index].Type + " : " + _switches[index].Index );
	}

	//=====================================================

	protected void SwitchActivatesPlatform()
	{
		// Activate platform only if all switches are active
		if( IsInteractionOk() == false ) return;

		_isPlatformActivated = true;

		if( _isPlatformFollowingPath == true )
		{
			// Unpause platform on path
			OnPauseEvent( false );
		}
		else
		{
			switch( _type )
			{
				case ePlatformType.ON_PATH:
					FollowPath();
					break;

				case ePlatformType.ON_SPLINE:
					FollowSpline();
					break;
			}
		}
	}

	//=====================================================

	private bool IsInteractionOk()
	{
		var isInteractionOk = true;

		// Game logic to determine whether or not the door is currently available goes here
		foreach( var s in _switches )
		{
			if( s != null && s.IsActivated == false )
			{
				isInteractionOk = false;
				break;
			}
		}

		return isInteractionOk;
	}

	//=====================================================
	// Move directly between path nodes
	private void InitPlatformPath()
	{
		_pathIndex = 0;

		if( _pathNodes.Length <= 0 || _pathNodes[0] == null )
			return;

		_path = new Vector3[_pathNodes.Length];

		for( var i = 0; i < _pathNodes.Length; i++ )
		{
			if( _pathNodes[i] != null )
				_path[i] = _pathNodes[i].position;
		}

		// Snap platform to start of path
		_thisTransform.position = _path[_pathIndex];
	}

	//=====================================================
	// Move along spline following path nodes
	private void InitPlatformSpline()
	{
		_pathIndex = 0;

		if( _pathNodes.Length <= 0 || _pathNodes[0] == null )
			return;

		// Build spline nodes depending on path parameter
		switch( _pathParameter )
		{
			case ePathParameter.ONE_SHOT:
			case ePathParameter.PING_PONG:
				_path = new Vector3[_pathNodes.Length + 2];
				_path[0] = _pathNodes[0].position;

				for( var i = 0; i < _pathNodes.Length; i++ )
				{
					if( _pathNodes[i] != null )
						_path[i + 1] = _pathNodes[i].position;
				}

				if( _pathNodes.Length > 0 && _pathNodes[_pathNodes.Length - 1] != null )
					_path[_path.Length - 1] = _pathNodes[_pathNodes.Length - 1].position;
				break;

			case ePathParameter.LOOP:
				_path = new Vector3[_pathNodes.Length + 3];
				_path[0] = _pathNodes[0].position;

				for( var i = 0; i < _pathNodes.Length; i++ )
				{
					if( _pathNodes[i] != null )
						_path[i + 1] = _pathNodes[i].position;
				}

				if( _pathNodes.Length > 1 )
					_path[_path.Length - 2] = _pathNodes[0].position;
				if( _pathNodes.Length > 1 )
					_path[_path.Length - 1] = _pathNodes[0].position;
				break;
		}

		// Create spline
		_spline = new LTSpline( _path );

		// Snap platform to start of path
		_thisTransform.position = _path[_pathIndex];
	}

	//=====================================================

	private void FollowPath()
	{
		if( _path == null || _path.Length <= 1 )
			return;

		++_pathIndex;

		// Check path index
		switch( _pathParameter )
		{
			case ePathParameter.ONE_SHOT:
				if( _pathIndex >= _path.Length )
				{
					_pathIndex = _path.Length - 1;
					return;
				}
				break;
			case ePathParameter.LOOP:
				if( _pathIndex >= _path.Length )
					_pathIndex = 0;
				break;

			case ePathParameter.PING_PONG:
				if( _pathIndex >= _path.Length )
					_pathIndex = (_path.Length * -1) + 2;
				break;
		}

		_isPlatformFollowingPath = true;

		if( _tweenId != -1 )
			LeanTween.cancel( _thisTransform.gameObject, _tweenId );

		// Start platform-tween along path
		_tweenId = LeanTween.move( _thisTransform.gameObject, _path[Mathf.Abs( _pathIndex )], _durationNodeToNode )
				 .setOnComplete( () =>
				 {
					 _tweenId = -1;
					 FollowPath();
				 } ).id;
	}

	//=====================================================
	// Only used with 'switched' one-shot paths i.e. platform returns to starting point if pressure switch is deactivated
	private void FollowPathReversed()
	{
		if( _pathParameter != ePathParameter.ONE_SHOT ) return;

		if( _path == null || _path.Length <= 1 )
			return;

		--_pathIndex;

		if( _pathIndex < 0 )
		{
			_pathIndex = 0;
			_isPlatformFollowingPath = false;
			return;
		}

		_isPlatformFollowingPath = true;

		if( _tweenId != -1 )
			LeanTween.cancel( _thisTransform.gameObject, _tweenId );

		// Start platform-tween along path
		_tweenId = LeanTween.move( _thisTransform.gameObject, _path[Mathf.Abs( _pathIndex )], _durationNodeToNode * 0.5f )
				 .setOnComplete( () =>
				 {
					 _tweenId = -1;
					 FollowPathReversed();
				 } ).id;
	}

	//=====================================================

	private void FollowSpline()
	{
		if( _tweenId != -1 )
			LeanTween.cancel( _thisTransform.gameObject, _tweenId );

		_isPlatformFollowingPath = true;

		// Start platform-tween along spline
		switch( _pathParameter )
		{
			case ePathParameter.ONE_SHOT:
				_tweenId = LeanTween.moveSpline( _thisTransform.gameObject, _spline.pts, _durationStartToEnd ).setRepeat( 1 ).setOnComplete( () => { _tweenId = -1; } ).id;
				break;

			case ePathParameter.PING_PONG:
				_tweenId = LeanTween.moveSpline( _thisTransform.gameObject, _spline.pts, _durationStartToEnd ).setRepeat( -1 ).setLoopPingPong().id;	// .setOrientToPath( false )
				break;

			case ePathParameter.LOOP:
				_tweenId = LeanTween.moveSpline( _thisTransform.gameObject, _spline.pts, _durationStartToEnd ).setRepeat( -1 ).setLoopClamp().id;
				break;
		}
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	private void CheckReferences()
	{
		// Try to find switch if reference has been lost
		if( _platform == null )
			_platform = _thisTransform.FindChild( "Platform" );

		if( _platform == null )
			Debug.Log( "CheckReferences: Platform not found" );
	}

	#endregion

	//=====================================================
}
