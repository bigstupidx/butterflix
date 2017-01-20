using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;
using System.IO;

public class HighScoresManager : MonoBehaviour 
{
	static	public	HighScoresManager				instance;

	public	GameObject								m_MainPanel;
	public	GameObject								m_pfbHighScoreItem;
	public	GameObject								m_ScrollingList;
	public	GraphicRaycaster						m_PageRaycaster;

	//public	GameObject								m_btnFacebook;
	//public	GameObject								m_btnTwitter;

	public	Text									m_txtPlayerName;
	public	Text									m_txtPlayerRank;
	public	Text									m_txtPlayerScore;

	// Button presses
	private	bool									m_bButtonPressed;
	private	int										m_ButtonIndex;

	private	bool									m_bIsWaitingForTwitterShare;
	private	bool									m_bIsWaitingForTwitterLogin;
	private	bool									m_bIsWaitingForFacebookLogin;
	private	bool									m_bScoresDownloaded;
	private float									m_Timer;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
		m_bScoresDownloaded = false;
		m_Timer = 0.0f;
		
		m_bIsWaitingForFacebookLogin = false;
		m_bIsWaitingForTwitterLogin = false;
		m_bIsWaitingForTwitterShare = false;
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
			m_MainPanel.SetActive( true );
			m_bScoresDownloaded = false;
			m_Timer = 0.0f;
			
			// Set up player name
			if( ServerManager.Registered )
			{
				m_txtPlayerName.text = ServerManager.instance.GetPlayerName();
			}
			
			m_txtPlayerScore.text = GameDataManager.Instance.PlayerHighestEverPopulation.ToString("#,##0");
			m_txtPlayerRank.text = "----";
			
		}
		else
		{
			m_MainPanel.SetActive( false );
		}
	}

	//=====================================================
	
	public void Update()
	{
		m_Timer += Time.deltaTime;
		
		if( m_Timer > 0.2f )
		{
			if( ( AchievementsManager.m_Instance != null ) && ( m_bScoresDownloaded == false ) )
			{
				AchievementsManager.m_Instance.GetScoreboard( AchievementsManager.eScoreTime.AllTime , 1 , 100 );
				m_bScoresDownloaded = true;
			}
		}
		
		#if UNITY_IPHONE || UNITY_ANDROID
		if( m_bIsWaitingForFacebookLogin )
		{
			if( FacebookCombo.isSessionValid() )
			{
				m_bIsWaitingForFacebookLogin = false;
				ShareWithFacebook();
			}
		}
		#endif
	}

	//=====================================================

	public void Start()
	{
		ShowPanel( true );
	}

	//=====================================================

	void OnGetScoreboardSuccessEvent( int TotalScores , List< PlayerScore > Scores )
	{
		float fHeight = 0.0f;
		foreach( Transform Child in m_ScrollingList.transform )
		{
			Destroy( Child.gameObject );
		}
		
		int PlayerRank = -1;
		string CurPlayerID = GetPlayerID();
		for( int Idx = 0 ; Idx < Scores.Count ; Idx++ )
		{
			GameObject pfbListItem = Instantiate( m_pfbHighScoreItem ) as GameObject;
			pfbListItem.transform.parent = m_ScrollingList.transform;
			pfbListItem.transform.localPosition = new Vector3( 0.0f , -265.0f + ( (float)Idx * -65.0f ) , 0.0f );
			pfbListItem.transform.localScale = new Vector3( 1.0f , 1.0f , 1.0f );
			
			// Setup text
			Text txtRank = pfbListItem.transform.GetChild( 1 ).GetChild( 0 ).GetComponent<Text>();
			Text txtScore = pfbListItem.transform.GetChild( 2 ).GetChild( 0 ).GetComponent<Text>();
			Text txtName = pfbListItem.transform.GetChild( 3 ).GetComponent<Text>();

			PlayerScore CurScore = Scores[ Idx ];
			string Country = CurScore.fields.GetString( "country" );
			txtRank.text = CurScore.rank.ToString("#,##0");
			txtName.text = CurScore.playername + "  (" + Country + ")";
			txtScore.text = CurScore.points.ToString("#,##0");
			
			// Is this the player?
			if( CurScore.playerid == CurPlayerID )
				PlayerRank = (int)CurScore.rank;
			
			fHeight += 65.0f;
		}
		fHeight += 200.0f;
	
		m_ScrollingList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical , -fHeight );
		
		if( PlayerRank == -1 )
			m_txtPlayerRank.text = "> 100";
		else
			m_txtPlayerRank.text = PlayerRank.ToString();
	}
	
	//=====================================================

	string GetPlayerID()
	{
		string prefix = "Player";
		string PlayerUid = PlayerPrefs.GetString(prefix + "Uid", "");
		return( PlayerUid );
	}
	
	//=============================================================================

	void OnGetScoreboardFailEvent()
	{
		Debug.Log( "OnGetScoreboardFailEvent" );
	}
			
	//=====================================================

	public void OnButtonPressed_Cancel()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 0;
		
		ScreenManager.FadeOut();
		m_PageRaycaster.enabled = false;
	}

	//=====================================================

	void OnDisable()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		FacebookManager.loginFailedEvent -= OnFacebookLoginFailedEvent;
		TwitterManager.loginSucceededEvent -= OnTwitterLoginSucceeded;
		TwitterManager.loginFailedEvent -= OnTwitterLoginFailed;
		#endif
		AchievementsManager.GetScoreboardSuccessEvent -= OnGetScoreboardSuccessEvent;
		AchievementsManager.GetScoreboardFailEvent -= OnGetScoreboardFailEvent;
	}

	//=============================================================================
	
	void OnEnable()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		FacebookManager.loginFailedEvent += OnFacebookLoginFailedEvent;
		TwitterManager.loginSucceededEvent += OnTwitterLoginSucceeded;
		TwitterManager.loginFailedEvent += OnTwitterLoginFailed;
		#endif
		AchievementsManager.GetScoreboardSuccessEvent += OnGetScoreboardSuccessEvent;
		AchievementsManager.GetScoreboardFailEvent += OnGetScoreboardFailEvent;
	}

	public void OnButtonPressed_Facebook()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( m_bIsWaitingForFacebookLogin || m_bIsWaitingForTwitterLogin || m_bIsWaitingForTwitterShare )
			return;
		
		//If it's not valid you can't post - try logging in using this code and then checking session valid again.
		bool isLoggedIn = FacebookCombo.isSessionValid();
		
		if( !isLoggedIn )
		{
			m_bIsWaitingForFacebookLogin = true;
			string[] permissions = new string[] { "email" };
			FacebookCombo.loginWithReadPermissions( permissions );
		}
		else
		{
			ShareWithFacebook();
		}
		#endif
	}

	//=====================================================

	public void OnButtonPressed_Twitter()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( m_bIsWaitingForFacebookLogin || m_bIsWaitingForTwitterLogin || m_bIsWaitingForTwitterShare )
			return;
		
		bool isLoggedIn = TwitterCombo.isLoggedIn();
		
		if( !isLoggedIn )
		{
			m_bIsWaitingForTwitterLogin = true;
			TwitterCombo.showLoginDialog();
		}
		else
		{
			ShareWithTwitter();
			m_bIsWaitingForTwitterShare = true;
		}
		#endif
	}

	//=====================================================

	#if UNITY_IPHONE || UNITY_ANDROID
	private void OnFacebookLoginFailedEvent(Prime31.P31Error error)
	{
		m_bIsWaitingForFacebookLogin = false;
	}

	//=============================================================================
	
	private void OnTwitterLoginSucceeded(string msg)
	{
		m_bIsWaitingForTwitterLogin = false;
		ShareWithTwitter();
		//m_bIsWaitingForTwitterShare = true;
	}

	//=============================================================================
	
	private void OnTwitterLoginFailed(string error)
	{
		m_bIsWaitingForTwitterLogin = false;
	}

	//=============================================================================

	private void ShareWithTwitter()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		//TwitterManager.requestDidFinishEvent += OnTwitterRequestDidFinishEvent;
		//TwitterManager.requestDidFailEvent += OnTwitterRequestDidFailEvent;

		//string pathToImage = string.Empty;
		m_bIsWaitingForTwitterShare = false;
		
		// If not already done, save twitter image to local directory before tweeting
		string outPath = CopyResourcesFileToLocal( "Sharing/Textures/TwitterImage" , "twitterimage.png" );

		string desc = TextManager.GetText("POPUP_SHARE_TWITTER");
		desc = desc.Replace( "(population)" , GameDataManager.Instance.PlayerHighestEverPopulation.ToString("#,##0") );
		desc = desc.Replace( "(fairy)" , ServerManager.instance.GetPlayerName() );

		TwitterCombo.postStatusUpdate( desc, outPath );
		PopupGenericMessage.instance.Show( "TWITTER_SHARE_SUCCESS" );
		#endif
	}

	//=============================================================================
	
	private void ShareWithFacebook()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		//FacebookManager.dialogCompletedWithUrlEvent += OnDialogCompletedWithUrlEvent;
		//FacebookManager.dialogFailedEvent += OnDialogFailedEvent;

		string desc = TextManager.GetText("POPUP_SHARE_FACEBOOK");
		desc = desc.Replace( "(population)" , GameDataManager.Instance.PlayerHighestEverPopulation.ToString("#,##0") );
		desc = desc.Replace( "(fairy)" , ServerManager.instance.GetPlayerName() );

		Dictionary<string,object> parameters = new Dictionary<string,object>
		{
			{ "link", "https://www.facebook.com/playwinxclub" },
			{ "name", TextManager.GetText("POPUP_SHARE_FACEBOOK_LINK_NAME") },
			{ "picture", "https://s3-eu-west-1.amazonaws.com/butterflix-liveupdates/FBImage.png" },
			{ "caption", TextManager.GetText("POPUP_SHARE_FACEBOOK_CAPTION") },
			{ "description", desc }
		};
		FacebookCombo.showFacebookShareDialog( parameters );
		#endif
	}

	//=============================================================================

	private string CopyResourcesFileToLocal( string ResourceName , string OutputName )
	{
		string OutPath = System.IO.Path.Combine( Application.persistentDataPath, OutputName );
		if( System.IO.File.Exists( OutPath ) == false )
		{
			TextAsset textAsset = (TextAsset)Resources.Load( ResourceName , typeof(TextAsset) ) as TextAsset;
			
			if( textAsset != null )
			{
				Stream fs = new MemoryStream(textAsset.bytes);
				BinaryReader r = new BinaryReader(fs);						
					
				byte[] SourceDataPtr;

				SourceDataPtr = r.ReadBytes( (int)fs.Length );
				System.IO.File.WriteAllBytes( OutPath , SourceDataPtr );
				
				r.Close();
				fs.Close();	
			}
		}
		
		return( OutPath );
	}
	#endif
}
