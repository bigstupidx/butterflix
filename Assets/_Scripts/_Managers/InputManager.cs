using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviourEMS, IPauseListener
{
	public static event Action<float>					MoveLeftRightEvent;
	public static event Action<float>					MoveForwardBackEvent;
	public static event Action<float>					LookLeftRightEvent;
	public static event Action<float>					LookUpDownEvent;
	public static event Action							PerformActionEvent;
	public static event Action<Vector3, ePlayerAction>	CastSpell;

	public static InputManager Instance;

	[SerializeField]
	private bool					_useGuiInputOnly = false;
	private float					_currentMoveInputH;
	private float					_currentMoveInputV;
	private float					_currentLookInputH;
	private float					_currentLookInputV;
	private static bool				_isBlocked = false;
	private bool					_isPaused = false;
	private int						_maskCollidable;
	private int						_maskMagicalTrap;
	private List<RaycastResult>		_hits;
	private bool					_isInputIntervalActive;

	//=====================================================

	public bool IsPaused { get { return _isPaused; } set { _isPaused = value; } }

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	public void OnBlockInput( bool isBlocked )
	{
		_isBlocked = isBlocked;

		if( _isBlocked == false ) return;
		
		if( GuiManager.Instance == null ) return;

		// Reset gui-joysticks
		GuiManager.Instance.OnInputBlocked();
	}

	//=====================================================

	public void OnFakePerformAction()
	{
		OnPerformActionEvent();
	}

	//=====================================================

	void Awake()
	{
		Instance = this;
		_hits = new List<RaycastResult>();
	}

	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.PauseEvent += OnPauseEvent;

		GuiButtonAction.PerformActionEvent += OnPerformActionEvent;

		_isBlocked = false;
		_isPaused = false;
		_isInputIntervalActive = false;
		_maskCollidable = 1 << LayerMask.NameToLayer( "Collidable" );
		_maskMagicalTrap = 1 << LayerMask.NameToLayer( "MagicalTrap" );
	}

	//=====================================================

	void Start()
	{
		GuiManager.Instance.JoystickMove.JoystickEvent += OnJoystickMoveEvent;
		GuiManager.Instance.JoystickLook.JoystickEvent += OnJoystickLookEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;
		
		GameManager.Instance.PauseEvent -= OnPauseEvent;

		GuiManager.Instance.JoystickMove.JoystickEvent -= OnJoystickMoveEvent;
		GuiManager.Instance.JoystickLook.JoystickEvent -= OnJoystickLookEvent;
		GuiButtonAction.PerformActionEvent -= OnPerformActionEvent;
	}

	//=====================================================

	void Update()
	{
		if( _isBlocked == true ) return;

		if( _isPaused == true ) return;

#if UNITY_EDITOR || UNITY_STANDALONE
		if( _useGuiInputOnly == false )
		{
			// Monitor Input (PC) - Keyboard / Joypad
			var input = Input.GetAxis( "Horizontal" );

			OnMoveLeftRight( input );

			input = Input.GetAxis( "Vertical" );

			OnMoveForwardBack( input );
		}

		// Check for 'touch' (mouse-down) events
		if( Input.GetMouseButtonDown( 0 ) )
		{
			var ray = Camera.main.ScreenPointToRay( Input.mousePosition );

			// Is player trying to attack an enemy or magical trap or collect a reward
			if( CheckForInteraction( ray ) == true )
				StartCoroutine( Interval( 0, 0.25f, () => { _isInputIntervalActive = false; } ) );
		}
#elif UNITY_ANDROID || UNITY_IPHONE
		// Check for 'touch' events
		if( Input.touchCount > 0 )
		{
			var ray = Camera.main.ScreenPointToRay( Input.GetTouch( 0 ).position );
			
			// Is player trying to attack an enemy or magical trap or collect a reward
			if( CheckForInteraction( ray ) )
				StartCoroutine( Interval( 0, 0.25f, () => { _isInputIntervalActive = false; } ) );
		}
#endif
		if( Input.GetButtonDown( "Jump" ) )
			OnPerformActionEvent();
	}

	//=====================================================

	private IEnumerator Interval( float startDelay, float interval, Action onComplete )
	{
		_isInputIntervalActive = true;

		yield return new WaitForSeconds( startDelay + interval );

		if( onComplete != null )
			onComplete();
	}

	//=====================================================
	// Return true if interacting with object e.g. casting spell at enemy or trap or collecting reward from chest
	private bool CheckForInteraction( Ray ray )
	{
		if( _isInputIntervalActive == true )
			return false;

		// Get pointer event data (gui-context)
		var peData = new PointerEventData( EventSystem.current ) { position = Input.mousePosition };

		// Block potential spell-cast if touching gui element
		if( _hits == null ) _hits = new List<RaycastResult>();

		_hits.Clear();
		EventSystem.current.RaycastAll( peData, _hits );

		// Break for positive hits
		if( _hits.Count > 0 )
		{
			// Releasing data
			peData = null;
			return false;
		}

		// Releasing data
		peData = null;

		RaycastHit hit;

		// Is player trying to attack an magical trap
		if( Physics.Raycast( ray, out hit, 100.0f, _maskMagicalTrap ) )
		{
			if( hit.collider.tag == UnityTags.MagicalTrap )
			{
				//Debug.Log( "TOUCHED MAGICAL TRAP" );
				if( CastSpell != null )
					CastSpell( hit.collider.transform.position, ePlayerAction.CAST_SPELL_DISABLE_TRAP );

				return true;
			}
		}

		// Is player trying to attack enemy or collect from treasure chest
		if( Physics.Raycast( ray, out hit, 100.0f, _maskCollidable ) )
		{
			if( hit.collider.tag == UnityTags.Enemy )
			{
				//Debug.Log( "TOUCHED ENEMY" );
				if( CastSpell != null )
					CastSpell( hit.collider.transform.position, ePlayerAction.CAST_SPELL_ATTACK );

				return true;
			}

			if( hit.collider.tag == UnityTags.ChestReward )
			{
				//Debug.Log( "COLLECT TREASURE" );
				var chest = hit.collider.transform.parent.GetComponent<ChestItem>();
				if( chest != null )
					chest.OnCollectReward();

				return true;
			}
		}

		return false;
	}

	//=====================================================

	private void OnJoystickMoveEvent( Vector2 input )
	{
		OnMoveLeftRight( input.x );
		OnMoveForwardBack( input.y );
	}

	//=====================================================

	private void OnJoystickLookEvent( Vector2 input )
	{
		OnLookLeftRight( input.x );
		OnLookUpDown( input.y );
	}

	//=====================================================

	private void OnPerformActionEvent()
	{
		if( PerformActionEvent == null || _isInputIntervalActive == true ) return;
		
		PerformActionEvent();

		StartCoroutine( Interval( 0, 0.25f, () => { _isInputIntervalActive = false; } ) );
	}

	//=====================================================

	private void OnMoveLeftRight( float input )
	{
		if( Math.Abs( input ) < 0.01f )
			input = 0.0f;

		if( input == 0.0f && Math.Abs( _currentMoveInputH - input ) < 0.01f )
		{
			_currentMoveInputH = input;
			return;
		}

		_currentMoveInputH = input;
	
		if( MoveLeftRightEvent != null )
			MoveLeftRightEvent( _currentMoveInputH );

		//Debug.Log( "H: " + _currentMoveInputH );
	}

	//=====================================================

	private void OnMoveForwardBack( float input )
	{
		if( Math.Abs( input ) < 0.01f )
			input = 0.0f;

		if( input == 0.0f && Math.Abs( _currentMoveInputV - input ) < 0.01f )
		{
			_currentMoveInputV = input;
			return;
		}

		_currentMoveInputV = input;
		
		if( MoveForwardBackEvent != null )
			MoveForwardBackEvent( _currentMoveInputV );

		//Debug.Log( "V: " + _currentMoveInputV );
	}

	//=====================================================

	private void OnLookLeftRight( float input )
	{
		if( Math.Abs( input ) < 0.01f )
			input = 0.0f;

		if( input == 0.0f && Math.Abs( _currentLookInputH - input ) < 0.01f )
		{
			_currentLookInputH = input;
			return;
		}

		_currentLookInputH = input;

		if( LookLeftRightEvent != null )
			LookLeftRightEvent( _currentLookInputH );

		//Debug.Log( "H: " + _currentLookInputH );
	}

	//=====================================================

	private void OnLookUpDown( float input )
	{
		if( Math.Abs( input ) < 0.01f )
			input = 0.0f;

		if( input == 0.0f && Math.Abs( _currentLookInputV - input ) < 0.01f )
		{
			_currentLookInputV = input;
			return;
		}

		_currentLookInputV = input;

		if( LookUpDownEvent != null )
			LookUpDownEvent( _currentLookInputV );

		//Debug.Log( "V: " + _currentLookInputV );
	}

	//=====================================================
}
