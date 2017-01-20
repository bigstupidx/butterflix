using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class GuiManager : MonoBehaviourEMS, IPauseListener
{
	[SerializeField] private Camera _guiCamera;
	[SerializeField] private RectTransform _panelDebug;
	[SerializeField] private RectTransform _panelTopLeft;
	[SerializeField] private RectTransform _panelTopRight;
	[SerializeField] private RectTransform _panelWildMagicMeter;
	[SerializeField] private RectTransform _panelHealthMeter;
	[SerializeField] private RectTransform _panelBoundaryActionBtn;
	[SerializeField] private RectTransform _panelBoundaryJoystickMove;
	[SerializeField] private RectTransform _panelBoundaryJoystickLook;
	[SerializeField] private GuiJoystick _joystickMove;
	[SerializeField] private GuiJoystick _joystickLook;
	[SerializeField] private Image _imgActionBtn;
	[SerializeField] private Text _txtGems;
	[SerializeField] private Text _txtDiamonds;
	[SerializeField] private Text _txtPopulation;
	[SerializeField] private Text _txtKeys;
	//[SerializeField] private Text _txtWildMagicRate;
	[SerializeField] private GameObject _pfbShopPopup;
	[SerializeField] private GameObject _btnCommonRoom;
	[SerializeField] private GameObject _btnMainHall;

	[SerializeField] private Sprite _sprActionDefault;
	[SerializeField] private Sprite _sprActionInteract;
	[SerializeField] private Sprite _sprActionClimbUp;

	[SerializeField] private ParticleSystem _particleGems;
	[SerializeField] private ParticleSystem _particleDiamonds;
	[SerializeField] private ParticleSystem _particleKeys;
	[SerializeField] private ParticleSystem _particlePopulation;

	[SerializeField] private AudioClip _clipBtnClick;

	//[SerializeField] private Text _txtDebug01;

	private AudioSource _audioSource ;
	private bool _isPaused;
	private int _currentGems;
	private int _targetGems;
	private int _incGems;
	private int _currentDiamonds;
	private int _targetDiamonds;
	private int _incDiamonds;
	private int _currentPopulation;
	private int _targetPopulation;
	private int _incPopulation;
	private int _num1000Population;

	//=====================================================

	#region Public Interface

	public static GuiManager Instance { get; private set; }

	public GuiJoystick JoystickMove { get { return _joystickMove; } }

	public GuiJoystick JoystickLook { get { return _joystickLook; } }

	public string TxtGems { set { _targetGems = Convert.ToInt32( value ); if( _txtGems.text != value ) StartCoroutine( FireParticle( _particleGems ) ); } }

	public string TxtDiamonds { set { _targetDiamonds = Convert.ToInt32( value ); if( _txtDiamonds.text != value ) StartCoroutine( FireParticle( _particleDiamonds ) ); } }

	public string TxtKeys { set { _txtKeys.text = value; if( _txtKeys.text != value ) StartCoroutine( FireParticle( _particleKeys ) ); } }

	//public string TxtWildMagicRate { set { _txtWildMagicRate.text = value; } }

	//public string TxtDebug01 { set { _txtDebug01.text = value; } }

	public bool IsPaused { get { return _isPaused; } set { _isPaused = value; } }

	//=====================================================

	public void OnPlayerPopulationEvent( int maxPopulation, int currentPopulation )
	{
		_targetPopulation = currentPopulation;

		var num1000Population = _targetPopulation / 1000;

		// Play particle effect?
		if( num1000Population == _num1000Population ) return;

		// Play particle effect?
		if( num1000Population < _num1000Population )
		{
			_num1000Population = num1000Population;
			return;
		}
		
		// Store how many 1000's are in current population
		_num1000Population = num1000Population;

		// Play particle effect
		StartCoroutine( FireParticle( _particlePopulation ) );
	}

	//=====================================================

	public void OnPlayerKeysEvent( int currentKeys )
	{
		TxtKeys = currentKeys.ToString();
	}

	//=====================================================

	public void OnPlayerGemsEvent( int currentGems )
	{
		var location = GameManager.Instance.CurrentLocation;

		// Only update running gems total outwith puzzle room areas - otherwise, updates are via SceneManager (local data)
		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( location != eLocation.NULL && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			// Do nothing
		}
		else
		{
			switch( location )
			{
				case eLocation.MAIN_HALL:
					// Work around to force SceneManager to update gui-gem count with GameDataManager + SceneManager gems
					SceneManager.AddPlayerGems( 0 );
					break;

				default:
					TxtGems = currentGems.ToString();
					break;
			}
		}
	}

	//=====================================================

	public void OnPlayerDiamondsEvent( int currentDiamonds )
	{
		TxtDiamonds = currentDiamonds.ToString();
	}

	//=====================================================

	public void OnPlayerActionAvailable( ePlayerAction playerAction )
	{
		switch( playerAction )
		{
			default:
				_imgActionBtn.sprite = _sprActionDefault;
				break;
			case ePlayerAction.CLIMB_UP:
			case ePlayerAction.OPEN_CHEST:
				_imgActionBtn.sprite = _sprActionClimbUp;
				break;
			case ePlayerAction.CLIMB_DOWN:
			case ePlayerAction.CRAWL_THROUGH_DOOR:
			case ePlayerAction.ENTER_BOSS_ROOM:
			case ePlayerAction.ENTER_PLAYER_HUB:
			case ePlayerAction.ENTER_PUZZLE_ROOM:
			case ePlayerAction.LEAVE_BOSS_ROOM:
			case ePlayerAction.LEAVE_PLAYER_HUB:
			case ePlayerAction.LEAVE_PUZZLE_ROOM:
			case ePlayerAction.OPEN_DOOR:
			case ePlayerAction.PUSH_OBJECT:
			case ePlayerAction.TELEPORT_OBLIVION_PORTAL:
			case ePlayerAction.USE_FLOOR_LEVER:
			case ePlayerAction.USE_PRESSURE_SWITCH:
			case ePlayerAction.USE_WALL_LEVER:
				_imgActionBtn.sprite = _sprActionInteract;
				break;
		}
	}

	//=====================================================

	public void OnPauseButtonClick()
	{
		_isPaused = !_isPaused;

		GameManager.Instance.OnPauseGame( _isPaused );

		_audioSource.PlayOneShot( _clipBtnClick );

		if( _isPaused == false ) return;

		if( PopupPause.Instance != null )
			PopupPause.Instance.Show();
	}

	//=====================================================

	public void OnInputBlocked()
	{
		if( _joystickMove != null )
			_joystickMove.OnEndDrag( null );

		if( _joystickLook != null )
			_joystickLook.OnEndDrag( null );
	}

	//=====================================================

	public void OnShopButtonClick()
	{
		// Create shop popup
		Instantiate( _pfbShopPopup );

		_audioSource.PlayOneShot( _clipBtnClick );
	}

	//=====================================================

	public void OnCommonRoomButtonClick()
	{
		// Exit to Common Room scene
		if( GameManager.Instance != null )
			GameManager.Instance.OnGoToCommonRoom();

		_audioSource.PlayOneShot( _clipBtnClick );
	}

	//=====================================================

	public void OnMainHallButtonClick()
	{
		// Exit to Common Room scene
		if( GameManager.Instance != null )
			GameManager.Instance.OnGoToCommonRoom();

		_audioSource.PlayOneShot( _clipBtnClick );
	}

	//=====================================================

	public void OnShowDebugGuiButtonClick()
	{
		// DEBUG - REMOVE THIS
		//_panelDebug.gameObject.SetActive( !_panelDebug.gameObject.activeSelf );
	}

	//=====================================================
	// Not in use. Used with older player input controls with separate free-look joystick
	public void OnShowGuiLookInput( bool isVisible )
	{
		// Block free-look gui if player is currently in action-zone e.g. open door
		if( _imgActionBtn.sprite != _sprActionDefault ) return;

		_panelBoundaryActionBtn.gameObject.SetActive( !isVisible );
		_panelBoundaryJoystickLook.gameObject.SetActive( isVisible );
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		if( _isPaused == isPaused ) return;

		_isPaused = isPaused;

		// Ignore displaying the pause popup while we're in the tutorial scene
		if( GameManager.Instance != null )
			if( GameManager.Instance.CurrentLocation == eLocation.TUTORIAL ) return;
		
		if( PopupPause.Instance == null || _isPaused == false ) return;
		
		// Show pause popup
		PopupPause.Instance.Show();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		Instance = this;
		_audioSource = transform.GetComponent<AudioSource>();
		_isPaused = false;

		_currentGems = _targetGems = 0;
		_currentDiamonds = _targetDiamonds = 0;
		_currentPopulation = _targetPopulation = _num1000Population = 0;
	}

	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameDataManager.Instance.PlayerPopulationEvent += OnPlayerPopulationEvent;
		GameDataManager.Instance.PlayerKeysEvent += OnPlayerKeysEvent;
		GameDataManager.Instance.PlayerGemsEvent += OnPlayerGemsEvent;
		GameDataManager.Instance.PlayerDiamondsEvent += OnPlayerDiamondsEvent;

		// Show top left and right gui panels
		_panelTopLeft.gameObject.SetActive( true );
		_panelTopRight.gameObject.SetActive( true );

		// Check location
		var currentLocation = GameManager.Instance.CurrentLocation;

		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( currentLocation != eLocation.NULL && (int)currentLocation < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			OnShowGuiGameInput( true );

			// Show wild meter
			_panelWildMagicMeter.gameObject.SetActive( true );

			// Hide Common Room Button
			_btnCommonRoom.SetActive( false );

			// Hide Main Hall Button
			_btnMainHall.SetActive( false );
		}
		else
		{
			switch( currentLocation )
			{
				default:
					OnShowGuiGameInput( false );

					// Hide wild meter
					_panelWildMagicMeter.gameObject.SetActive( false );

					// Hide Common Room Button
					_btnCommonRoom.SetActive( false );

					// Show Main Hall Button
					_btnMainHall.SetActive( true );
					break;

				case eLocation.TRADING_CARD_ROOM:
					OnShowGuiGameInput( false );

					// Hide wild meter and health
					_panelWildMagicMeter.gameObject.SetActive( false );
					_panelHealthMeter.gameObject.SetActive( false );

					// Hide Common Room Button
					_btnCommonRoom.SetActive( false );

					// Show Main Hall Button
					_btnMainHall.SetActive( true );
					break;

				case eLocation.MAIN_HALL:
					OnShowGuiGameInput( true );

					// Show wild meter
					_panelWildMagicMeter.gameObject.SetActive( true );

					// Show Common Room Button
					_btnCommonRoom.SetActive( true );

					// Hide Main Hall Button
					_btnMainHall.SetActive( false );
					break;

				case eLocation.BOSS_ROOM:
					OnShowGuiGameInput( true );

					// Show wild meter
					_panelWildMagicMeter.gameObject.SetActive( true );

					// Hide Common Room Button
					_btnCommonRoom.SetActive( false );

					// Hide Main Hall Button
					_btnMainHall.SetActive( false );
					break;

				case eLocation.TUTORIAL:
					OnShowGuiGameInput( true );

					// Hide top left and right gui panels
					_panelTopLeft.gameObject.SetActive( false );
					_panelTopRight.gameObject.SetActive( false );

					// Hide wild meter and health
					_panelWildMagicMeter.gameObject.SetActive( false );
					_panelHealthMeter.gameObject.SetActive( false );

					// Hide Common Room Button
					_btnCommonRoom.SetActive( false );

					// Hide Main Hall Button
					_btnMainHall.SetActive( false );
					break;
			}
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameDataManager.Instance.PlayerPopulationEvent -= OnPlayerPopulationEvent;
		GameDataManager.Instance.PlayerGemsEvent -= OnPlayerGemsEvent;
		GameDataManager.Instance.PlayerKeysEvent -= OnPlayerKeysEvent;
		GameDataManager.Instance.PlayerDiamondsEvent -= OnPlayerDiamondsEvent;
	}

	//=====================================================

	void Start()
	{
		if( GameManager.Instance.CurrentLocation == eLocation.TUTORIAL ) return;

		// Update gui elements
		GameDataManager.Instance.BroadcastGuiData();
	}

	//=====================================================

	void Update()
	{
		// DEBUG - REMOVE THIS
		//if( Input.GetKeyDown( KeyCode.P ) )
		//{
		//	OnPauseButtonClick();
		//	return;
		//}

		// Update gems
		if( _currentGems != _targetGems )
		{
			_incGems = SetIncrement( _targetGems - _currentGems );

			_currentGems += _incGems;

			if( Mathf.Abs( _targetGems - _currentGems ) < Mathf.Abs( _incGems ) )
				_currentGems = _targetGems;

			_txtGems.text = _currentGems.ToString();
		}

		// Update diamonds (currency)
		if( _currentDiamonds != _targetDiamonds )
		{
			_incDiamonds = SetIncrement( _targetDiamonds - _currentDiamonds );

			_currentDiamonds += _incDiamonds;

			if( Mathf.Abs( _targetDiamonds - _currentDiamonds ) < Mathf.Abs( _incDiamonds ) )
				_currentDiamonds = _targetDiamonds;

			_txtDiamonds.text = _currentDiamonds.ToString();
		}

		// Update population
		if( _currentPopulation != _targetPopulation )
		{
			_incPopulation = SetIncrement( _targetPopulation - _currentPopulation );

			_currentPopulation += _incPopulation;

			if( Mathf.Abs( _targetPopulation - _currentPopulation ) < Mathf.Abs( _incPopulation ) )
				_currentPopulation = _targetPopulation;

			_txtPopulation.text = _currentPopulation.ToString();
		}
	}

	//=====================================================

	private void OnShowGuiGameInput( bool isVisible )
	{
		_panelBoundaryActionBtn.gameObject.SetActive( isVisible );
		_panelBoundaryJoystickMove.gameObject.SetActive( isVisible );
		_panelBoundaryJoystickLook.gameObject.SetActive( isVisible );
	}

	//=====================================================

	private IEnumerator FireParticle( ParticleSystem particle )
	{
		if( particle.enableEmission == true ) yield return null;

		particle.Play();

		yield return new WaitForSeconds( 1.0f );

		particle.Stop();
	}

	//=====================================================

	private int SetIncrement( int diff )
	{
		var sign = (int)Mathf.Sign( diff );

		var incBy = Mathf.Abs( diff ) / 50;
		if( incBy >= 10 ) return incBy * sign;
		
		incBy = Mathf.Abs( diff ) / 25;
		if( incBy >= 10 ) return incBy * sign;
		
		incBy = Mathf.Abs( diff ) / 10;
		if( incBy >= 10 ) return incBy * sign;
		
		incBy = Mathf.Abs( diff ) / 5;
		if( incBy < 10 ) incBy = 1;

		return incBy * sign;
	}

	#endregion

	//=====================================================
}
