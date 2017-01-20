using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
public class CutsceneManager : MonoBehaviourEMS, IPauseListener
{
	public static event Action CutsceneCompleteEvent;

	public static CutsceneManager Instance;

	private Camera _camera;
	private Camera _mainCamera;
	private eCutsceneType _cutsceneType;
	private eCutsceneType _currentCutscene;
	private ICutsceneObject _cutsceneObject;
	private CameraPathAnimator _cameraPathAnimator;
	private float _cutsceneTimer;
	private float _fadeSpeed;
	private bool _isFadeCompelete;
	private bool _isPaused;

	private LTSpline _cameraPath;
	private Vector3[] _path;
	private int _tweenId;
	private float _durationStartToEnd;
	private bool _isCameraFollowingPath;

	private bool _checkForCancelCutscene;

	//=====================================================

	#region Public Interface

	public void OnPlayerDeathEvent( bool isRespawningInScene )
	{
		OnStartCutscene( (isRespawningInScene) ? eCutsceneType.DEATH_RESPAWN : eCutsceneType.DEATH );
	}

	//=====================================================

	public void OnRestartEvent()
	{
		OnStartCutscene( eCutsceneType.RETURN_TO_START );
	}

	//=====================================================

	public void OnCommonRoomEvent()
	{
		OnStartCutscene( eCutsceneType.GO_TO_COMMON_ROOM );
	}

	//=====================================================

	public void OnKeyCollectedEvent( ICutsceneObject obj )
	{
		OnStartCutscene( eCutsceneType.COLLECT_KEY, obj );
	}

	//=====================================================

	public void OnBossRoomStartEvent( ICutsceneObject cutsceneObject )
	{
		OnStartCutscene( eCutsceneType.BOSS_ROOM_START, cutsceneObject );
	}

	//=====================================================

	public void OnBossRoomBossLosesEvent( ICutsceneObject cutsceneObject )
	{
		OnStartCutscene( eCutsceneType.BOSS_ROOM_BOSS_LOSES, cutsceneObject );
	}

	//=====================================================

	public void OnBossRoomBossWinsEvent( ICutsceneObject cutsceneObject )
	{
		OnStartCutscene( eCutsceneType.BOSS_ROOM_BOSS_WINS, cutsceneObject );
	}

	//=====================================================
	// Start cutscene. Optional param could be used for door-bolt-index
	public void OnStartCutscene( eCutsceneType cutsceneType, ICutsceneObject cutsceneObject = null, int param = 0 )
	{
		Debug.Log( "OnStartCutscene" );

		// Clear for now - set after fade-in to cutscene
		_currentCutscene = eCutsceneType.NULL;

		// Set up cutscene stuff then fade-out -> fade-in to cutscene -> fade-out (optional fade-in to same scene)
		_cutsceneType = cutsceneType;
		_cutsceneObject = cutsceneObject;
		_cutsceneTimer = (_cutsceneObject != null) ? _cutsceneObject.CutsceneDuration() : 2.5f;

		// Is the cutscene using a camera fly-thru object
		if( cutsceneObject != null && cutsceneObject.IsFlyThruAvailable() == true )
		{
			_cameraPathAnimator = cutsceneObject.GetFlyThruAnimator();
		}

		_cameraPath = (_cameraPathAnimator == null && _cutsceneObject != null) ? _cutsceneObject.CameraPath() : null;

		_fadeSpeed = 1.0f;
		_isFadeCompelete = false;

		switch( _cutsceneType )
		{
			case eCutsceneType.RETURN_TO_START:
			case eCutsceneType.GO_TO_COMMON_ROOM:
			case eCutsceneType.CRAWL_DOOR:
			case eCutsceneType.OBLIVION_PORTAL:
			case eCutsceneType.ENTER_PUZZLE_ROOM:
			case eCutsceneType.LEAVE_PUZZLE_ROOM:
			case eCutsceneType.ENTER_PLAYER_HUB:
			case eCutsceneType.LEAVE_PLAYER_HUB:
			case eCutsceneType.ENTER_BOSS_ROOM:
			case eCutsceneType.LEAVE_BOSS_ROOM:
			case eCutsceneType.COLLECT_KEY:
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_01:
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_02:
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_03:
			case eCutsceneType.SWITCH_OPENS_DOOR:
			case eCutsceneType.SWITCH_ACTIVATES_PLATFORM:
			case eCutsceneType.OPEN_CHEST:
			case eCutsceneType.DEATH_RESPAWN:
			case eCutsceneType.DEATH:
			case eCutsceneType.FLY_THRU:
			case eCutsceneType.BOSS_ROOM_START:
			case eCutsceneType.BOSS_ROOM_BOSS_LOSES:
			case eCutsceneType.BOSS_ROOM_BOSS_WINS:
				_fadeSpeed = 1.0f;
				break;
		}

		_durationStartToEnd = _cutsceneTimer;

		// Fade out from game -> fade in to cutscene
		if( cutsceneType != eCutsceneType.NULL &&
			cutsceneType != eCutsceneType.GO_TO_COMMON_ROOM &&
			cutsceneType != eCutsceneType.RETURN_TO_START &&
			cutsceneType != eCutsceneType.DEATH_RESPAWN &&
			cutsceneType != eCutsceneType.DEATH &&
			_cutsceneType != eCutsceneType.BOSS_ROOM_START )
		{
			ScreenManager.FadeOutCompleteEvent += OnPreCutsceneFadeOutCompleteEvent;
			ScreenManager.FadeOut( _fadeSpeed );
		}
		// Fade in to Boss game immediately
		else if( _cutsceneType == eCutsceneType.BOSS_ROOM_START )
		{
			Invoke( "OnPreCutsceneFadeOutCompleteEvent", 0.5f);
		}
		// Fade out from game -> no cutscene
		else
		{
			ScreenManager.FadeOutCompleteEvent += OnPostCutsceneFadeOutCompleteEvent;
			ScreenManager.FadeOut( _fadeSpeed );

			_currentCutscene = cutsceneType;
		}

		// Block player input
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;

		if( _cameraPathAnimator != null )
		{
			// Manage active fly-thru
			switch( _isPaused )
			{
				case true:
					_cameraPathAnimator.Pause();
					break;
				case false:
					_cameraPathAnimator.Play();
					break;
			}
			return;
		}

		if( _isCameraFollowingPath == false ) return;

		// Manage active tween
		if( _isPaused == true && _tweenId != -1 )
		{
			LeanTween.pause( _tweenId );
		}
		else if( _isPaused == false && _tweenId != -1 )
		{
			LeanTween.resume( _tweenId );
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		Instance = this;
		_camera = this.transform.GetComponentInChildren<Camera>();

		_currentCutscene = eCutsceneType.NULL;
		_cutsceneObject = null;
		_cutsceneTimer = 0.0f;
		_cameraPath = null;
		_isCameraFollowingPath = false;
		_cameraPathAnimator = null;
		_isFadeCompelete = false;
		_isPaused = false;
		_durationStartToEnd = 0.0f;
		_tweenId = -1;
		_checkForCancelCutscene = false;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		_camera.GetComponent<AudioListener>().enabled = false;
		_camera.enabled = false;
		_mainCamera = Camera.main;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameManager.Instance.PlayerDeathEvent += OnPlayerDeathEvent;
		GameManager.Instance.RestartEvent += OnRestartEvent;
		GameManager.Instance.CommonRoomEvent += OnCommonRoomEvent;
		GameManager.Instance.KeyCollectedEvent += OnKeyCollectedEvent;
		GameManager.Instance.BossStartEvent += OnBossRoomStartEvent;
		GameManager.Instance.BossLosesEvent += OnBossRoomBossLosesEvent;
		GameManager.Instance.BossWinsEvent += OnBossRoomBossWinsEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameManager.Instance.PlayerDeathEvent -= OnPlayerDeathEvent;
		GameManager.Instance.RestartEvent -= OnRestartEvent;
		GameManager.Instance.CommonRoomEvent -= OnCommonRoomEvent;
		GameManager.Instance.KeyCollectedEvent -= OnKeyCollectedEvent;
		GameManager.Instance.BossStartEvent -= OnBossRoomStartEvent;
		GameManager.Instance.BossLosesEvent -= OnBossRoomBossLosesEvent;
		GameManager.Instance.BossWinsEvent -= OnBossRoomBossWinsEvent;
	}

	//=====================================================

	void Update()
	{
		if( _isPaused == true || _isFadeCompelete == false ) return;

		if( _currentCutscene == eCutsceneType.NULL) return;

		// Cancel cutscene?
		if( _checkForCancelCutscene == true )
		{
#if UNITY_EDITOR
			if( Input.GetKeyDown( KeyCode.Space ) )
			{
				if( _cameraPathAnimator != null )
					OnAnimationFinishedEvent();
				else
					_cutsceneTimer = 0.0f;
				
				_checkForCancelCutscene = false;
			}
#elif UNITY_ANDROID || UNITY_IOS
			if( Input.touchCount > 0 )
			{
				var touch = Input.GetTouch( 0 );

				if( touch.phase == TouchPhase.Began )
				{
					if( _cameraPathAnimator != null )
						OnAnimationFinishedEvent();
					else
						_cutsceneTimer = 0.0f;

					_checkForCancelCutscene = false;
				}
			}
#endif
		}

		if( _cameraPathAnimator != null ) return;

		_cutsceneTimer = Mathf.Clamp( _cutsceneTimer - Time.deltaTime, 0.0f, 100.0f );

		if( _cutsceneTimer > 0.0f ) return;

		// Cutscene completed - start fade-out
		ScreenManager.FadeOutCompleteEvent += OnPostCutsceneFadeOutCompleteEvent;
		ScreenManager.FadeOut( _fadeSpeed );
		_isFadeCompelete = false;

		//Debug.Log( "End cutscene ..." );
	}

	//=====================================================

	private void OnAnimationFinishedEvent()
	{
		_cameraPathAnimator.AnimationFinishedEvent -= OnAnimationFinishedEvent;
		// Note: stop and clear cameraPathAnimator ref in OnPostCutsceneFadeOutCompleteEvent

		// Cutscene completed - start fade-out
		ScreenManager.FadeOutCompleteEvent += OnPostCutsceneFadeOutCompleteEvent;
		ScreenManager.FadeOut( _fadeSpeed );
		_isFadeCompelete = false;

		Debug.Log( "End cutscene ..." );
	}

	//=====================================================
	// Fade out from game -> fade in to cutscene
	private void OnPreCutsceneFadeOutCompleteEvent()
	{
		ScreenManager.FadeOutCompleteEvent -= OnPreCutsceneFadeOutCompleteEvent;

		_isCameraFollowingPath = false;

		// Are we using a fly-thru camera
		if( _cameraPathAnimator != null )
		{
			//_cameraPathAnimator.animationObject = _camera.transform;
			_cameraPathAnimator.AnimationFinishedEvent += OnAnimationFinishedEvent;
		}
		else
		{
			// Camera will track lookAt position
			if( _cutsceneObject != null && _cutsceneObject.CutsceneCameraLookAt() != null )
			{
				_camera.GetComponent<CutsceneCamera>().LookAt = _cutsceneObject.CutsceneCameraLookAt().position;

				var camPoints = _cutsceneObject.CutsceneCameraPoints();

				// Set camera start point and spline
				if( camPoints.Length > 0 && camPoints[0] != null )
				{
					_camera.transform.position = camPoints[0].position;

					if( camPoints.Length > 1 )
						_isCameraFollowingPath = true;
				}
				else
				{
					Debug.LogError( "CutsceneManager: cutscene object has no camera start position." );
				}
			}
			else
			{
				Debug.LogError( "CutsceneManager: cutscene object is missing." );
				_camera.transform.position = _mainCamera.transform.position;
				_camera.transform.rotation = _mainCamera.transform.rotation;
			}
		}

		// Position player if necessary
		if( _cutsceneObject != null )
		{
			switch( _cutsceneType )
			{
				case eCutsceneType.ENTER_BOSS_ROOM:
				case eCutsceneType.ENTER_PUZZLE_ROOM:
				case eCutsceneType.ENTER_PLAYER_HUB:
				case eCutsceneType.LEAVE_BOSS_ROOM:
				case eCutsceneType.LEAVE_PLAYER_HUB:
				case eCutsceneType.LEAVE_PUZZLE_ROOM:
				case eCutsceneType.CRAWL_DOOR:
				case eCutsceneType.OBLIVION_PORTAL:
					PlayerManager.Position = _cutsceneObject.CutscenePlayerPoint().position;
					PlayerManager.Rotation = _cutsceneObject.CutscenePlayerPoint().rotation;
					break;
			}
		}

		// Switch cameras
		_mainCamera.GetComponent<AudioListener>().enabled = false;
		_mainCamera.enabled = false;
		_mainCamera.transform.FindChild( "CameraInGameGui" ).GetComponent<Camera>().enabled = false;
		_camera.enabled = true;
		_camera.GetComponent<AudioListener>().enabled = true;

		// Start camera tracking
		if( _isCameraFollowingPath == true )
			CameraFollowSpline();
		else if( _cameraPathAnimator != null )
			_cameraPathAnimator.Play();

		ScreenManager.FadeInCompleteEvent += OnPreCutsceneFadeInCompleteEvent;
		ScreenManager.FadeIn( _fadeSpeed );

		_isFadeCompelete = false;
	}

	//=====================================================

	private void OnPreCutsceneFadeInCompleteEvent()
	{
		Debug.Log( "OnPreCutsceneFadeInCompleteEvent" );

		ScreenManager.FadeInCompleteEvent -= OnPreCutsceneFadeInCompleteEvent;

		_isFadeCompelete = true;

		// *** Start Cutscene - See Update() ***
		_currentCutscene = _cutsceneType;

		PlayCutscene();

		_checkForCancelCutscene = true;
	}

	//=====================================================

	private void PlayCutscene()
	{
		if( _cutsceneObject == null ) return;

		Debug.Log( "Start cutscene anims ..." );

		// Animate cutscene object(s)
		switch( _currentCutscene )
		{
			default:
				_cutsceneObject.OnPlayCutsceneAnimation();
				break;
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_01:
				_cutsceneObject.OnPlayCutsceneAnimation();
				break;
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_02:
				_cutsceneObject.OnPlayCutsceneAnimation( 1 );
				break;
			case eCutsceneType.SWITCH_OPENS_DOOR_BOLT_03:
				_cutsceneObject.OnPlayCutsceneAnimation( 2 );
				break;
			case eCutsceneType.ENTER_BOSS_ROOM:
			case eCutsceneType.ENTER_PUZZLE_ROOM:
			case eCutsceneType.ENTER_PLAYER_HUB:
			case eCutsceneType.LEAVE_BOSS_ROOM:
			case eCutsceneType.LEAVE_PLAYER_HUB:
			case eCutsceneType.LEAVE_PUZZLE_ROOM:
			case eCutsceneType.CRAWL_DOOR:
			case eCutsceneType.COLLECT_KEY:
			case eCutsceneType.BOSS_ROOM_BOSS_LOSES:
				PlayerManager.OnPlayCutsceneAnimation( _currentCutscene );
				break;

			case eCutsceneType.OBLIVION_PORTAL:
				_cutsceneObject.OnPlayCutsceneAnimation();					// Play sfx
				PlayerManager.OnPlayCutsceneAnimation( _currentCutscene );
				break;
		}
	}

	//=====================================================

	private void OnPostCutsceneFadeOutCompleteEvent()
	{
		ScreenManager.FadeOutCompleteEvent -= OnPostCutsceneFadeOutCompleteEvent;

		_isFadeCompelete = true;

		// Stop and clear cameraPathAnimator reference
		if( _cameraPathAnimator )
		{
			_cameraPathAnimator.Stop();
			_cameraPathAnimator = null;
		}

		// Returning to same scene - fade-in
		if( _currentCutscene == eCutsceneType.CRAWL_DOOR ||
		   _currentCutscene == eCutsceneType.OBLIVION_PORTAL ||
		   _currentCutscene == eCutsceneType.SWITCH_OPENS_DOOR_BOLT_01 ||
		   _currentCutscene == eCutsceneType.SWITCH_OPENS_DOOR_BOLT_02 ||
		   _currentCutscene == eCutsceneType.SWITCH_OPENS_DOOR_BOLT_03 ||
		   _currentCutscene == eCutsceneType.SWITCH_OPENS_DOOR ||
		   _currentCutscene == eCutsceneType.SWITCH_ACTIVATES_PLATFORM ||
		   _currentCutscene == eCutsceneType.OPEN_CHEST ||
		   _currentCutscene == eCutsceneType.DEATH_RESPAWN ||
		   _currentCutscene == eCutsceneType.FLY_THRU ||
		   _currentCutscene == eCutsceneType.BOSS_ROOM_START )
		{
			// Switch cameras
			_camera.GetComponent<AudioListener>().enabled = false;
			_camera.enabled = false;
			_mainCamera.GetComponent<AudioListener>().enabled = true;
			_mainCamera.enabled = true;
			_mainCamera.transform.FindChild( "CameraInGameGui" ).GetComponent<Camera>().enabled = true;
			_mainCamera.GetComponent<CameraMovement>().InitWithPlayer( false );

			// Fade in - include short pause if necessary for fairy aniamtions
			var delay = ( _currentCutscene == eCutsceneType.CRAWL_DOOR ||
			              _currentCutscene == eCutsceneType.OBLIVION_PORTAL ) ? 0.3f : 0.0f;
			StartCoroutine( DelayPostCutsceneFadeIn( delay ) );
		}

		// Reset for next cutscene
		_currentCutscene = eCutsceneType.NULL;
		_cutsceneObject = null;

		if( CutsceneCompleteEvent != null )
			CutsceneCompleteEvent();

		// Un-block player input
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );
	}

	//=====================================================

	private IEnumerator DelayPostCutsceneFadeIn( float delay )
	{
		yield return new WaitForSeconds( delay );

		ScreenManager.FadeIn( _fadeSpeed );
	}

	//=====================================================

	private void CameraFollowSpline()
	{
		if( _tweenId != -1 )
			LeanTween.cancel( _camera.gameObject, _tweenId ); ;

		// Disable camera script (lookAt tracks target) if orientated to path
		_camera.GetComponent<CutsceneCamera>().enabled = !_cutsceneObject.OrientCameraToPath();

		// Start camera-tween along spline
		_tweenId = LeanTween.moveSpline( _camera.gameObject, _cameraPath.pts, _durationStartToEnd ).setOrientToPath( _cutsceneObject.OrientCameraToPath() ).setRepeat( 1 ).setOnComplete( () => { _tweenId = -1; } ).id;
	}

	#endregion

	//=====================================================
}
