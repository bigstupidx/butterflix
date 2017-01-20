using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent( typeof( Animator ) )]
public class PopupPause : MonoBehaviourEMS
{

	public static event Action PopupUnPauseGame;

	public static PopupPause Instance;

	[SerializeField] private Animator m_animatorBg;
	[SerializeField] private Animator m_animator;

	public	GameObject[]		m_pages;

	public RectTransform		m_panelTransform;
	public	GameObject			m_sprPanelStats;
	public	GameObject			m_sprPanelOptions;
	public	GameObject			m_btnExitRoom;
	public	GameObject			m_listCredits;
	public	GameObject			m_grpCouponCode;
	public  GameObject 			panelOptions;//Ainur's code
	public  GameObject 			panelSettings;//Ainur's code
	public	Text				m_txtNumGems;
	public	Text				m_txtNumRedGems;
	public	Text				m_txtNumKeys;
	public	InputField			m_txtCouponCodeInputField;
	public	Text				m_txtCouponCode;

	public	Button				m_btnSettings;
	public	Button				m_btnSettingsAlt;
	public	Button				m_btnShare;

	public	Toggle				m_toggleMusic;
	public	Toggle				m_toggleFX;

	public	Text				m_txtQuality;

	private	GameObject 			_camera;
	private	float				m_CurScrollPos;
	private	bool				m_bIsActive;
	//private	Image _image;

	//=====================================================

	public bool IsActive { get { return m_bIsActive; } }

	//=====================================================

	public void Show()
	{
		if (_camera == null)
			_camera = transform.FindChild ("GuiCamera").gameObject;

		if (_camera == null)
			return;

		_camera.SetActive (true);

		var currentLocation 	= (GameManager.Instance != null) ? GameManager.Instance.CurrentLocation : eLocation.NULL;
		m_CurScrollPos = 0.0f;
		SetPage( 0 );
		
		// Enable/Disable coupon codes
		if( PlayerPrefs.GetInt( "couponCodesActive" , 0 ) == 0 )
			m_grpCouponCode.SetActive( false );
		else
			m_grpCouponCode.SetActive( true );

		// Fill in current stats
		if( (currentLocation == eLocation.MAIN_HALL) ||
			(currentLocation == eLocation.CLOTHING_ROOM) ||
			(currentLocation == eLocation.TRADING_CARD_ROOM) ||
			(currentLocation == eLocation.COMMON_ROOM) )
		{
			/*m_txtNumGems.text = string.Format( "{0}", GameDataManager.Instance.PlayerGems );
			m_txtNumRedGems.text = string.Format( "{0}", GameDataManager.Instance.PlayerRedGems );
			m_btnExitRoom.SetActive( false );

			m_btnSettings.gameObject.SetActive( false );
			m_btnSettingsAlt.gameObject.SetActive( true );
			m_btnShare.gameObject.transform.localPosition = new Vector3( 220.0f, 0.0f, 0.0f );
			
			// Hide stats panel when not in a puzzle room
			m_sprPanelStats.SetActive( false );
			m_sprPanelOptions.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );*/

			//Ainur's code
			panelOptions.SetActive (false);
			panelSettings.SetActive (true);
		}
		else
		{
			m_txtNumGems.text = string.Format( "{0} / {1}", SceneManager.NumGemsCollectedInScene, Convert.ToInt32( SettingsManager.GetSettingsItem( "PUZZLE_ROOM_MAX_GEMS", -1 ) ) );
			m_txtNumRedGems.text = string.Format( "{0} / {1}", SceneManager.NumRedGemsCollectedInScene, Convert.ToInt32( SettingsManager.GetSettingsItem( "PUZZLE_ROOM_MAX_REDGEMS", -1 ) ) );
			m_btnExitRoom.SetActive( true );

			m_btnSettings.gameObject.SetActive( true );
			m_btnSettingsAlt.gameObject.SetActive( false );
			m_btnShare.gameObject.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );

			// Show stats panel when in a puzzle room
			m_sprPanelStats.SetActive( true );
			m_sprPanelOptions.transform.localPosition = new Vector3( 0.0f, 195.0f, 0.0f );
		}

		// Key totals
		var totalKeys = "0";
		var numKeys = 0;

		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( currentLocation != eLocation.NULL && (int)currentLocation < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			if( GameDataManager.Instance != null )
				numKeys = GameDataManager.Instance.GetNumKeysCollected( currentLocation );

			totalKeys = SettingsManager.GetSettingsItem( "NUM_PUZZLE_ROOM_KEYS", (int)currentLocation );
		}

		m_txtNumKeys.text = string.Format( "{0} / {1}", numKeys, totalKeys );	// (int)ePuzzleKeyType.NUM_KEYS );

		// Setup music/fx toggles
		if( GameDataManager.Instance != null )
		{
			m_toggleMusic.isOn = GameDataManager.Instance.MusicOn;
			m_toggleFX.isOn = GameDataManager.Instance.FXOn;
		}

		UpdateQualityText();

		m_bIsActive = true;

		// Animate popup open
		StartCoroutine( AnimatePanel( true ) );
	}

	//=====================================================

	public void OnButtonPressedOk()
	{
		if( m_bIsActive == false ) return;

		if( PopupUnPauseGame != null )
			PopupUnPauseGame();

		//if(GameManager.Instance.CurrentLocation == eLocation.MAIN_HALL ||
		//   GameManager.Instance.CurrentLocation == eLocation.PUZZLE_ROOM_01 ||
		//   GameManager.Instance.CurrentLocation == eLocation.PUZZLE_ROOM_02 ||
		//   GameManager.Instance.CurrentLocation == eLocation.PUZZLE_ROOM_03 ||
		//   GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
		//{
		// Ensure we only tell GuiManager (and GameManager etc.) to unpause if the game paused
		//  Work-around for device soft / hard resets pausing / unpausing the game independently of the game's pause feature
		if( GameManager.Instance != null )
			if( GameManager.Instance.IsPaused == true )
				GuiManager.Instance.OnPauseButtonClick();
		//}

		// Animate popup closed
		StartCoroutine( AnimatePanel( false ) );
	}

	//=====================================================

	public void OnButtonPressedSettings()
	{
		if( m_bIsActive == false ) return;

		SetPage( 1 );
	}

	//=====================================================

	public void OnButtonPressedShare()
	{
		if( m_bIsActive == false ) return;
		EveryplayWrapper.ShareRecording( GameDataManager.Instance.PlayerCurrentFairyName.ToString() );
	}

	//=====================================================

	public void OnButtonPressedExitRoom()
	{
		if( m_bIsActive == false ) return;

		SetPage( 3 );
	}

	//=====================================================

	public void OnButtonPressedQuality()
	{
		if( m_bIsActive == false ) return;

		GameDataManager.Instance.QualityLevel = GameDataManager.Instance.QualityLevel == 0 ? 1 : 0;

		UpdateQualityText();

		// If we're using 'high quality' mode then switch on 'FastGPU' object layer
		GameDataManager.Instance.SetCameraGPUFlags();
	}

	//=====================================================

	public void UpdateQualityText()
	{
		if( GameDataManager.Instance.QualityLevel == 0 )
			m_txtQuality.text = TextManager.GetText( "POPUP_PAUSE_QUALITY_BEST" );
		else
			m_txtQuality.text = TextManager.GetText( "POPUP_PAUSE_QUALITY_FASTEST" );
	}

	//=====================================================

	public void OnButtonPressedCredits()
	{
		if( m_bIsActive == false ) return;

		m_CurScrollPos = 0.0f;
		SetPage( 2 );
	}

	//=====================================================

	public void OnButtonPressedSendPromoCode()
	{
		if( m_bIsActive == false ) return;

		if( m_txtCouponCode.text.Length > 1 )
		{
			// Remove first letter (this identifies a butterflix coupon)
			string Code = m_txtCouponCode.text.Substring( 1 );
			
			if( ServerManager.instance != null )
				ServerManager.instance.RedeemCouponCode( Code );
		}
	}

	//=====================================================

	public void OnButtonPressedMusicToggle()
	{
		if( m_bIsActive == false ) return;

		GameDataManager.Instance.MusicOn = m_toggleMusic.isOn;
	}

	//=====================================================

	public void OnButtonPressedFXToggle()
	{
		if( m_bIsActive == false ) return;

		GameDataManager.Instance.FXOn = m_toggleFX.isOn;
	}

	//=====================================================

	public void OnButtonPressedSettingsBack()
	{
		if( m_bIsActive == false ) return;

		SetPage( 0 );
	}

	//=====================================================

	public void OnButtonPressedCreditsBack()
	{
		if( m_bIsActive == false ) return;

		SetPage( 1 );
	}

	//=====================================================

	public void OnButtonPressedExitConfirmBack()
	{
		if( m_bIsActive == false ) return;

		SetPage( 0 );
	}

	//=====================================================

	public void OnButtonPressedExitConfirm()
	{
		if( m_bIsActive == false ) return;

		// Exit the room
		OnButtonPressedOk();
		GameManager.UploadPlayerDataToServer();
		GameManager.Instance.OnExitPuzzleRoomFromPopup();
	}

	//=====================================================

	public void SetPage( int Page )
	{
		for( int Idx = 0; Idx < m_pages.Length; Idx++ )
		{
			m_pages[Idx].SetActive( Idx == Page ? true : false );
		}
	}

	//=====================================================

	void Awake()
	{
		Instance = this;
		m_bIsActive = false;
	}

	//=====================================================

	void OnEnable()
	{
		ServerManager.RedeemCouponSuccessEvent += OnRedeemCouponSuccessEvent;
		ServerManager.RedeemCouponFailEvent += OnRedeemCouponFailEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		ServerManager.RedeemCouponSuccessEvent -= OnRedeemCouponSuccessEvent;
		ServerManager.RedeemCouponFailEvent -= OnRedeemCouponFailEvent;
	}

	//=====================================================

	void Update()
	{
		m_CurScrollPos += Time.deltaTime;
		if( m_CurScrollPos > 60.0f )
			m_CurScrollPos = 0.0f;

		m_listCredits.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, -4200.0f + (m_CurScrollPos * 150.0f) );
	}

	//=====================================================

	private IEnumerator AnimatePanel( bool isOpening )
	{
		var startScale = m_panelTransform.localScale;
		var endScale = startScale;

		startScale.y = (isOpening == true) ? 0.0f : 0.5f;
		endScale.y = (isOpening == true) ? 0.5f : 0.0f;

		var currentLerpTime = 0.0f;

		// Animate panel scale (y-axis)
		while( currentLerpTime < 1.0f )
		{
			currentLerpTime += Time.deltaTime * 4.0f;
			if( currentLerpTime > 1 )
				currentLerpTime = 1;

			// Apply 'smoothstep' formula
			var t = currentLerpTime / 1.0f;
			t = t * t * (3f - 2f * t);

			m_panelTransform.localScale = Vector3.Lerp( startScale, endScale, t );

			yield return null;
		}

		if( isOpening == true ) yield break;

		// If closing panel, deactivate popup and camera
		m_bIsActive = false;

		yield return new WaitForSeconds( 0.5f );

		_camera.SetActive( false );
	}

	//=====================================================

	private void OnRedeemCouponSuccessEvent( long couponGems, long couponHearts )
	{
		Debug.Log( "OnRedeemCouponSuccessEvent: gems: " + couponGems + " hearts: " + couponHearts );
		m_txtCouponCodeInputField.text = TextManager.GetText( "POPUP_PAUSE_PROMOCODE_SUCCESS" );
		
		// Add gems/diamonds
		if( GameDataManager.Instance == null )
			return;
		
		if( couponGems > 0 )
			GameDataManager.Instance.AddPlayerGems( (int)couponGems );
		
		if( couponHearts > 0 )
			GameDataManager.Instance.AddPlayerDiamonds( (int)couponHearts );
		
		GameDataManager.Instance.BroadcastGuiData();
	}

	//=====================================================

	private void OnRedeemCouponFailEvent()
	{
		Debug.Log( "OnRedeemCouponFailEvent" );
		m_txtCouponCodeInputField.text = TextManager.GetText( "POPUP_PAUSE_PROMOCODE_FAILURE" );
	}

	//=====================================================
}
