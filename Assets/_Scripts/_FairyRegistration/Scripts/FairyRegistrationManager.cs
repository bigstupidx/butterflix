using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;

public class FairyRegistrationManager : MonoBehaviour 
{
	enum eRegistrationStage
	{
		ChooseRegistrationMethod = 0,
		ChooseFairyName,
		FacebookRegistering,
		FacebookRegisteringSuccess,
		FacebookRegisteringFailure,
		RegisteringFairy,
		RegisteringFairySuccess,
		RegisteringFairyFailure,
		FacebookRegisteringGetUserInfo,
		FindingPreviousFairies,
		PickPreviousFairy,
		RestoringPreviousFairy,
		RestoringPreviousFairySuccess,
		RestoringPreviousFairyFailure,
		FacebookConfirmation,
		FacebookLoginFailure
	}
	
	static	public	FairyRegistrationManager		instance;

	public	GameObject								m_MainPanel;
	public	GameObject[]							m_Pages;
	public	GraphicRaycaster						m_PageRaycaster;
	public	InputField								m_txtFairyNameInputField;
	public	Text									m_txtFairyName;
	public	Text									m_txtFacebookName;
	public	Text									m_txtFacebookID;
	public	Text									m_txtRegistrationErrorMessage;
	public	Text									m_txtFacebookErrorMessage;
	public	Text									m_txtRestoreFairyErrorMessage;
	
	private	bool									m_bIsLoggingIn = false;
	private	eRegistrationStage						m_CurrentStage;
	private	bool									m_bButtonPressed;
	private	int										m_ButtonIndex;
	private string									m_InProgressPlayerID = null;
	private string									m_InProgressPlayerName = null;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
		m_CurrentStage = eRegistrationStage.ChooseRegistrationMethod;
	}

	//=====================================================

	public void Reset()
	{
		m_bButtonPressed = false;
		m_ButtonIndex = 0;
	}
	
	//=============================================================================

	public int GetButtonPressed()
	{
		return( m_ButtonIndex );
	}
	
	//=============================================================================

	public bool WasButtonPressed()
	{
		return( m_bButtonPressed );
	}
	
	//=============================================================================

	public void ShowPanel( bool bActive )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_bIsLoggingIn = false;
			m_MainPanel.SetActive( true );
			m_CurrentStage = eRegistrationStage.ChooseRegistrationMethod;
			
			#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
			FacebookManager.loginFailedEvent += OnFacebookLoginFailedEvent;
			#endif
			
			Update();
		}
		else
		{
			m_MainPanel.SetActive( false );

			#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
			FacebookManager.loginFailedEvent -= OnFacebookLoginFailedEvent;
			#endif
		}
	}

	//=====================================================
	
	public void Update()
	{
		// Show current page
		foreach( eRegistrationStage Stage in Enum.GetValues( typeof( eRegistrationStage ) ) )
		{
			if( m_Pages[ (int)Stage ] != null )
			{
				if( Stage == m_CurrentStage )
					m_Pages[ (int)Stage ].SetActive( true );
				else
					m_Pages[ (int)Stage ].SetActive( false );
			}
		}
		
		// Run logic for current page
		switch( m_CurrentStage )
		{
			case eRegistrationStage.FacebookConfirmation:
			case eRegistrationStage.ChooseRegistrationMethod:
				{
					#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
					// Session now valid?
					bool isSessionValid = FacebookCombo.isSessionValid();

					if( ( isSessionValid == true ) && ( m_bIsLoggingIn == true ) )
					{
						//string token = FacebookCombo.getAccessToken();
						
						Debug.Log( "FairyRegistrationManager: Update->SessionValid");		
						m_CurrentStage = eRegistrationStage.FacebookRegisteringGetUserInfo;
						m_bIsLoggingIn = false;
						GetFacebookUserInfo();
					}				
					#endif
				}
				break;	
				
			case eRegistrationStage.ChooseFairyName:
				{
				}
				break;	
				
			case eRegistrationStage.FacebookRegistering:
				{
					#if UNITY_EDITOR
					if( UnityEngine.Random.Range( 0 , 200 ) < 2 )
					{
						m_CurrentStage = eRegistrationStage.FacebookRegisteringGetUserInfo;
						GetFacebookUserInfo();
					}
					#endif
				}
				break;	
				
			case eRegistrationStage.FacebookRegisteringSuccess:
				{
					m_txtFacebookName.text = m_InProgressPlayerName;
					m_txtFacebookID.text = m_InProgressPlayerID;
				}
				break;	
				
			case eRegistrationStage.FacebookRegisteringFailure:
				break;	
				
			case eRegistrationStage.RegisteringFairy:
				break;	
				
			case eRegistrationStage.RegisteringFairySuccess:
				break;	

			case eRegistrationStage.RegisteringFairyFailure:
				break;	

			case eRegistrationStage.FacebookRegisteringGetUserInfo:
				break;	
				
			case eRegistrationStage.FindingPreviousFairies:
				break;	

			case eRegistrationStage.PickPreviousFairy:
				break;	

			case eRegistrationStage.RestoringPreviousFairy:
				break;	

			case eRegistrationStage.RestoringPreviousFairySuccess:
				break;	

			case eRegistrationStage.RestoringPreviousFairyFailure:
				break;	

			case eRegistrationStage.FacebookLoginFailure:
				break;	
		}
	}

	//=====================================================

	public void OnButtonPressed_Cancel()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 0;
		
		ScreenManager.FadeOut();
		m_PageRaycaster.enabled = false;
		//ShowPanel( false );
		//GameObject.Destroy( this.gameObject );
	}

	//=====================================================

	#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
	private void OnFacebookLoginFailedEvent( Prime31.P31Error error )
	{
		Debug.Log( "FairyRegistrationManager: OnFacebookLoginFailedEvent - " + error );
		m_CurrentStage = eRegistrationStage.FacebookRegisteringFailure;
	}	
	#endif	
	
	//=============================================================================

	public void OnButtonPressed_RegisterWithFacebook()
	{
		#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
		// Attempt to register
		bool isSessionValid = FacebookCombo.isSessionValid();

		if( isSessionValid == false )
		{
			// If no internet is present go to error message
			if( Application.internetReachability == NetworkReachability.NotReachable )
			{
				m_txtFacebookErrorMessage.text = TextManager.GetText("ERROR_NO_INTERNET");
				m_CurrentStage = eRegistrationStage.FacebookLoginFailure;
			}
			else
			{
				string[] permissions = new string[] { "email" };
				FacebookCombo.loginWithReadPermissions( permissions );
				m_bIsLoggingIn = true;
				//m_CurrentStage = eRegistrationStage.FacebookRegistering;
				Debug.Log("FairyRegistrationManager: Register->Log Into FB");		
			}
		}
		else
		{
			// Already logged in, get user info and friend info
			m_CurrentStage = eRegistrationStage.FacebookRegisteringGetUserInfo;
			GetFacebookUserInfo();
			Debug.Log("FairyRegistrationManager: Register->Session Valid,success");		
		}
		#else
		if( UnityEngine.Random.Range( 0 , 100 ) < 25 )
		{
			m_CurrentStage = eRegistrationStage.FacebookRegisteringGetUserInfo;
			GetFacebookUserInfo();
		}
		else
		{
			m_CurrentStage = eRegistrationStage.FacebookRegistering;
		}
		#endif
	}

	//=====================================================
	
	void GetFacebookUserInfo()
	{
		#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
		StartCoroutine( GetFacebookUserInfoWithDelay() );
		#else
		// Have a fake delay for testing then randomly succeed/fail
		StartCoroutine( EditorFakeUserInfo() );
		#endif
	}

	//=====================================================

	public IEnumerator GetFacebookUserInfoWithDelay()
	{
		yield return new WaitForSeconds( 0.75f );

		Facebook.instance.getMe( ( error, result ) =>
		{
			// if we have an error we dont proceed any further
			if( ( error != null ) || ( result == null ) )
			{
				Debug.Log( "DLog3" + error + " " + result );
				Debug.Log("FairyRegistrationManager: Update->Error on Me graph request - " + error + " " + result );		
				m_CurrentStage = eRegistrationStage.FacebookRegisteringFailure;
			}
			else
			{
				Debug.Log( "DLog4" + error + " " + result );

				// Grab the userId and persist it for later use
				if( result != null )
				{
					Debug.Log( "DLog5" );
					m_InProgressPlayerID = result.id;
					m_InProgressPlayerName = result.name;
				}

				Debug.Log( "Me Graph Request finished: " );
				Debug.Log( result );
				
				m_CurrentStage = eRegistrationStage.FacebookRegisteringSuccess;
				
				// Search for previous fairies
				OnButtonPressed_FacebookSuccessRestorePreviousFairy();
			}
		});
	}

	//=====================================================

	public IEnumerator EditorFakeUserInfo()
	{
		yield return new WaitForSeconds( 1.5f );

		if( UnityEngine.Random.Range( 0 , 100 ) < 75 )
		{
			m_InProgressPlayerName = "KeyTest1708";
			m_InProgressPlayerID = "2345185483865971";
			m_CurrentStage = eRegistrationStage.FacebookRegisteringSuccess;

			// Search for previous fairies
			OnButtonPressed_FacebookSuccessRestorePreviousFairy();
		}
		else
		{
			m_CurrentStage = eRegistrationStage.FacebookRegisteringFailure;
		}
	}

	//=====================================================

	public void OnButtonPressed_RegisterWithoutFacebook()
	{
		m_CurrentStage = eRegistrationStage.FacebookConfirmation;
	}

	//=====================================================

	public void OnButtonPressed_RegisterWithoutFacebookConfirm()
	{
		m_CurrentStage = eRegistrationStage.ChooseFairyName;
		PlayerPrefs.SetInt( "FacebookRegistered" , 0 );
	}

	//=====================================================

	public void OnButtonPressed_FacebookFailureOK()
	{
		m_CurrentStage = eRegistrationStage.ChooseRegistrationMethod;
	}

	//=====================================================

	public void OnButtonPressed_FacebookSuccessOK()
	{
		m_CurrentStage = eRegistrationStage.ChooseFairyName;
		m_txtFairyNameInputField.text = m_InProgressPlayerName;
		
		// Save facebook login info
		PlayerPrefs.SetString( "FacebookName" , m_InProgressPlayerName );
		PlayerPrefs.SetString( "FacebookID" , m_InProgressPlayerID );
		PlayerPrefs.SetInt( "FacebookRegistered" , 1 );
	}

	//=====================================================

	public void OnButtonPressed_RestoringPreviousFairyFailureOK()
	{
		m_CurrentStage = eRegistrationStage.FacebookRegisteringSuccess;
	}

	//=====================================================

	public void OnButtonPressed_RestoringPreviousFairySuccessOK()
	{
		OnButtonPressed_Cancel();
	}

	//=====================================================

	public void OnButtonPressed_RegisteringFairySuccess()
	{
		OnButtonPressed_Cancel();
	}

	//=====================================================

	public void OnButtonPressed_RegisteringFairyFailure()
	{
		m_CurrentStage = eRegistrationStage.ChooseFairyName;
	}

	//=====================================================

	public void OnButtonPressed_FacebookLoginError()
	{
		m_CurrentStage = eRegistrationStage.ChooseRegistrationMethod;
	}

	//=====================================================

	private void OnReRegistrationSuccessEvent()
	{		
		ServerManager.ReRegistrationSuccessEvent -= OnReRegistrationSuccessEvent;
		ServerManager.ReRegistrationFailEvent -= OnReRegistrationFailEvent;
		
		// Now load the player data
		ServerManager.DownloadPlayerDataSuccessEvent += OnDownloadPlayerDataSuccessEvent;
		ServerManager.DownloadPlayerDataFailEvent += OnDownloadPlayerDataFailEvent;
		ServerManager.instance.LoadPlayerData();
	}
	
	//=============================================================================
	
	private void OnReRegistrationFailEvent( string error )
	{
		ServerManager.ReRegistrationSuccessEvent -= OnReRegistrationSuccessEvent;
		ServerManager.ReRegistrationFailEvent -= OnReRegistrationFailEvent;
		
		m_txtRestoreFairyErrorMessage.text = "Failed to restore Fairy!";
		m_CurrentStage = eRegistrationStage.RestoringPreviousFairyFailure;
		Debug.Log( "OnReRegistrationFailEvent: Fail: " + error );
	}
	
	//=============================================================================
	
	private void OnDownloadPlayerDataSuccessEvent()
	{		
		ServerManager.DownloadPlayerDataSuccessEvent -= OnDownloadPlayerDataSuccessEvent;
		ServerManager.DownloadPlayerDataFailEvent -= OnDownloadPlayerDataFailEvent;
		
		// Reload playerData into manager class
		if( GameDataManager.Instance != null )
		{
			GameDataManager.Instance.LoadPlayer();
			PlayerPrefsWrapper.SetInt( "IsTutorialCompleted", 1 );
		}

		m_CurrentStage = eRegistrationStage.RestoringPreviousFairySuccess;
	}
	
	//=============================================================================
	
	private void OnDownloadPlayerDataFailEvent()
	{
		ServerManager.DownloadPlayerDataSuccessEvent -= OnDownloadPlayerDataSuccessEvent;
		ServerManager.DownloadPlayerDataFailEvent -= OnDownloadPlayerDataFailEvent;
		
		m_txtRestoreFairyErrorMessage.text = "Failed to restore Fairy!";
		m_CurrentStage = eRegistrationStage.RestoringPreviousFairyFailure;
	}
	
	//=============================================================================

	public void OnButtonPressed_FacebookSuccessRestorePreviousFairy()
	{
		// Save facebook login info
		PlayerPrefs.SetString( "FacebookName" , m_InProgressPlayerName );
		PlayerPrefs.SetString( "FacebookID" , m_InProgressPlayerID );
		PlayerPrefs.SetInt( "FacebookRegistered" , 1 );
		
		// Attempt to find fairy data with this facebook ID
		m_CurrentStage = eRegistrationStage.FindingPreviousFairies;
		
		ServerManager.SearchPreviousPlayersSuccessEvent += OnSearchPreviousPlayersSuccessEvent;
		ServerManager.SearchPreviousPlayersFailEvent += OnSearchPreviousPlayersFailEvent;
		ServerManager.instance.SearchForPreviousPlayers();
	}

	//=====================================================

	private void OnSearchPreviousPlayersSuccessEvent( List< string > FairyNames )
	{		
		ServerManager.SearchPreviousPlayersSuccessEvent -= OnSearchPreviousPlayersSuccessEvent;
		ServerManager.SearchPreviousPlayersFailEvent -= OnSearchPreviousPlayersFailEvent;
		
		if( FairyNames.Count > 0 )
		{
			Debug.Log( "OnSearchPreviousPlayersSuccessEvent: " + FairyNames.Count );
			foreach( string Name in FairyNames )
			{
				Debug.Log( Name );
			}
			
			// Restore first fairy in the list (reregister the player first)
			ServerManager.ReRegistrationSuccessEvent += OnReRegistrationSuccessEvent;
			ServerManager.ReRegistrationFailEvent += OnReRegistrationFailEvent;
			ServerManager.instance.ReRegisterPlayerName( FairyNames[ 0 ] );
			m_CurrentStage = eRegistrationStage.RestoringPreviousFairy;
		}
		else
		{
			// Success but no fairies found - register as normal
			
			m_txtRestoreFairyErrorMessage.text = "No fairies found for this Facebook account!";
			//m_CurrentStage = eRegistrationStage.RestoringPreviousFairyFailure;
			m_CurrentStage = eRegistrationStage.FacebookRegisteringSuccess;
		}
	}
	
	//=============================================================================
	
	private void OnSearchPreviousPlayersFailEvent( string error )
	{
		ServerManager.SearchPreviousPlayersSuccessEvent -= OnSearchPreviousPlayersSuccessEvent;
		ServerManager.SearchPreviousPlayersFailEvent -= OnSearchPreviousPlayersFailEvent;
		
		Debug.Log( "OnSearchPreviousPlayersFailEvent: " + error );

		m_txtRestoreFairyErrorMessage.text = "No fairies found for this Facebook account!";
		m_CurrentStage = eRegistrationStage.RestoringPreviousFairyFailure;
	}
	
	//=============================================================================

	public void OnButtonPressed_RegisterFairy()
	{
		// Register player name
		ServerManager.RegistrationSuccessEvent += OnRegistrationSuccessEvent;
		ServerManager.RegistrationFailEvent += OnRegistrationFailEvent;
		
		string FairyName = "WFS2_" + m_txtFairyName.text;
		ServerManager.instance.RegisterPlayerName( FairyName );
		
		m_CurrentStage = eRegistrationStage.RegisteringFairy;
	}

	//=====================================================
	
	private void OnRegistrationSuccessEvent()
	{		
		ServerManager.RegistrationSuccessEvent -= OnRegistrationSuccessEvent;
		ServerManager.RegistrationFailEvent -= OnRegistrationFailEvent;
		
		m_CurrentStage = eRegistrationStage.RegisteringFairySuccess;
	}
	
	//=============================================================================
	
	private void OnRegistrationFailEvent(string error)
	{
		// Reset texts
		m_txtRegistrationErrorMessage.text = "";
		
		if(error == "FORBIDDEN")
			m_txtRegistrationErrorMessage.text = TextManager.GetText("ERROR_NO_INTERNET");
		else if(error.Contains("UNAVAILABLE"))
			m_txtRegistrationErrorMessage.text = TextManager.GetText("POPUP_NAME_ERROR");
		else
		{
			m_txtRegistrationErrorMessage.text = TextManager.GetText("ERROR_NO_INTERNET");
		}
		
		ServerManager.RegistrationSuccessEvent -= OnRegistrationSuccessEvent;
		ServerManager.RegistrationFailEvent -= OnRegistrationFailEvent;
		
		m_CurrentStage = eRegistrationStage.RegisteringFairyFailure;
	}
	
	//=============================================================================
}
