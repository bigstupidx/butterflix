using UnityEngine;
using System;

public class CommonRoomManager : MonoBehaviourEMS
{
	private enum eCommonRoomMode
	{
		FadingIn,
		Idle,
		WalkingToDestination,
		FadingOut,
		Exiting
	}
	
	public	GameObject[]			m_RoomDestinationPositions;

	private	GameObject				m_pfbFairy;
	private	eCommonRoomMode			m_CommonRoomMode;
	private	int						m_DestinationIndex;
	
	//=====================================================
	
	void Awake()
	{
	}

	//=====================================================
	
	void Start()
	{
		m_CommonRoomMode = eCommonRoomMode.FadingIn;
		
		if( AudioManager.Instance != null )
		{
			AudioManager.Instance.PlayMusic( eLocation.COMMON_ROOM );
		}
		
		// Create fairy prefab
		m_pfbFairy = Instantiate( GameDataManager.Instance.GetPrefab( (eFairy)GameDataManager.Instance.PlayerCurrentFairy , true ) );
		m_pfbFairy.transform.localPosition = new Vector3( -0.4f , 0.05f , 20.0f );
	}

	//=====================================================
	
	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		ScreenManager.FadeInCompleteEvent -= OnFadeInCompleteEvent;
		ScreenManager.FadeOutCompleteEvent -= OnFadeOutCompleteEvent;
		GameManager.Instance.CommonRoomEvent -= OnCommonRoomEvent;
	}
	
	//=====================================================

	void OnEnable()
	{
		ScreenManager.FadeInCompleteEvent += OnFadeInCompleteEvent;
		ScreenManager.FadeOutCompleteEvent += OnFadeOutCompleteEvent;
		GameManager.Instance.CommonRoomEvent += OnCommonRoomEvent;
	}
	
	//=====================================================

	void OnFadeInCompleteEvent()
	{
		m_CommonRoomMode = eCommonRoomMode.Idle;
	}
	
	//=====================================================

	void OnFadeOutCompleteEvent()
	{
		m_CommonRoomMode = eCommonRoomMode.Exiting;

		GameManager.UploadPlayerDataToServer();
		
		switch( m_DestinationIndex )
		{
			case 0:
				GameManager.Instance.SetNextLocation( eLocation.HIGHSCORES_ROOM , false );
				Application.LoadLevel( "HighScoresRoom" );
				break;
			case 1:
				GameManager.Instance.SetNextLocation( eLocation.CLOTHING_ROOM , false );
				Application.LoadLevel( "ClothingRoom" );
				break;
			case 2:
				GameManager.Instance.SetNextLocation( eLocation.TRADING_CARD_ROOM , false );
				Application.LoadLevel( "TradingCardsRoom" );
				break;
			case 3:
				GameManager.Instance.SetNextLocation( eLocation.MAIN_HALL , false );
				Application.LoadLevel( "MainHall" );
				break;
		}
	}
	
	//=====================================================

	void Update()
	{
		//Time.timeScale = 0.2f;
		float fDeltaTime = Time.deltaTime;
		switch( m_CommonRoomMode )
		{
			case eCommonRoomMode.FadingIn:
				break;

			case eCommonRoomMode.Idle:
				break;

			case eCommonRoomMode.WalkingToDestination:
				
			
				Vector3 CurrentRot = m_pfbFairy.transform.eulerAngles;
				Vector3 DestRot = ( m_RoomDestinationPositions[ m_DestinationIndex ].transform.localPosition - m_pfbFairy.transform.localPosition ).normalized;
				float DestRotAngle = Mathf.Atan2( -DestRot.z , DestRot.x ) * Mathf.Rad2Deg;
				//Debug.Log( DestRotAngle );
				DestRotAngle += 90.0f;
				
				float DeltaRot = ( DestRotAngle - CurrentRot.y );
				if( DeltaRot < -180.0f )
					DeltaRot += 360.0f;
				CurrentRot.y += DeltaRot * ( fDeltaTime * 1.4f );
			
				m_pfbFairy.transform.eulerAngles = CurrentRot;
			
				Animator CurAnimator = m_pfbFairy.GetComponent<Animator>();
				CurAnimator.SetBool("IsWalking", true);                
				
				Vector3 CurrentPos = m_pfbFairy.transform.localPosition;
				Vector3 DestPos = m_RoomDestinationPositions[ m_DestinationIndex ].transform.localPosition;
				
				Vector3 DeltaVec = ( DestPos - CurrentPos );
				CurrentPos += m_pfbFairy.transform.forward * fDeltaTime * 1.6f;
			
				// Have we reached the destination position?
				if( DeltaVec.magnitude < 0.35f )
				{
					CurAnimator.SetBool("IsWalking", false);                
				}
				if( DeltaVec.magnitude < 0.15f )
				{
					m_CommonRoomMode = eCommonRoomMode.FadingOut;
					ScreenManager.FadeOut();
				}
			
				m_pfbFairy.transform.localPosition = CurrentPos;
				break;

			case eCommonRoomMode.FadingOut:
				break;

			case eCommonRoomMode.Exiting:
				break;
		}
	}

	//=====================================================

	public void OnButtonPressed_TradingCards()
	{
		if( m_CommonRoomMode == eCommonRoomMode.Idle )
		{
			m_CommonRoomMode = eCommonRoomMode.WalkingToDestination;
			m_DestinationIndex = 2;
			
			//Animator CurAnimator = m_pfbFairy.GetComponent<Animator>();
			//CurAnimator.Play( "Locomotion" , -1 , 0.0f );
		}
	}

	//=====================================================

	public void OnButtonPressed_Clothing()
	{
		if( m_CommonRoomMode == eCommonRoomMode.Idle )
		{
			m_CommonRoomMode = eCommonRoomMode.WalkingToDestination;
			m_DestinationIndex = 1;
		}
	}

	//=====================================================

	public void OnButtonPressed_HighScores()
	{
		if( m_CommonRoomMode == eCommonRoomMode.Idle )
		{
			m_CommonRoomMode = eCommonRoomMode.WalkingToDestination;
			m_DestinationIndex = 0;
		}
	}

	//=====================================================
	
	public void OnCommonRoomEvent()
	{
		m_CommonRoomMode = eCommonRoomMode.FadingOut;
		ScreenManager.FadeOut();
		m_DestinationIndex = 3;
	}
	
}
