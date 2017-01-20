using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prime31;

public class AchievementsManager : MonoBehaviour
{
	public enum eScoreTime
	{
		Newest,
		Today,
		Last7Days,
		Last30Days,
		AllTime
	}

	enum eManagerState
	{
		Idle,
		Login,
		Logout,
		AddAchievement,
		ReportScore
	}
	
	// Event callbacks
	public static event Action								LoginSuccessEvent;
	public static event Action<string>						LoginFailEvent;

	public static event Action								LogoutSuccessEvent;
	public static event Action<string>						LogoutFailEvent;

	public static event Action								AddAchievementSuccessEvent;
	public static event Action<string>						AddAchievementFailEvent;

	public static event Action								ReportScoreSuccessEvent;
	public static event Action<string>						ReportScoreFailEvent;
	
	public static event Action<int,List< PlayerScore >>		GetScoreboardSuccessEvent;
	public static event Action								GetScoreboardFailEvent;

	string ScoreboardID = "populationtable_final";
	string ScoreboardIDAndroid = "CgkI0f6Qr6EPEAIQAQ";
	
	#if UNITY_IOS || UNITY_ANDROID
	private bool											m_bVerbose = false;
	#else
	private bool											m_bVerbose = true;
	#endif

    public 	static AchievementsManager						m_Instance = null;
	private bool											m_bInitialised = false;
	private bool											m_bIsLoggedIn = false;
	private bool											m_bIsUploadingAchievement = false;
	private int												m_UploadingAchievementID = 0;
	private float											m_UploadingAchievementTimeout = 10.0f;	
	private eManagerState									m_ManagerState = eManagerState.Idle;
	
	// Mapping from achievement ID to google play IDs
	string[] GPAchievementLookup = new string[ 30 ]
	{
		"CgkI0f6Qr6EPEAIQAg",
		"CgkI0f6Qr6EPEAIQAw",
		"CgkI0f6Qr6EPEAIQBA",
		"CgkI0f6Qr6EPEAIQBQ",
		"CgkI0f6Qr6EPEAIQBg",
		"CgkI0f6Qr6EPEAIQBw",
		"CgkI0f6Qr6EPEAIQCA",
		"CgkI0f6Qr6EPEAIQCQ",
		"CgkI0f6Qr6EPEAIQCg",
		"CgkI0f6Qr6EPEAIQCw",
		"CgkI0f6Qr6EPEAIQDA",
		"CgkI0f6Qr6EPEAIQDQ",
		"CgkI0f6Qr6EPEAIQDg",
		"CgkI0f6Qr6EPEAIQDw",
		"CgkI0f6Qr6EPEAIQEA",
		"CgkI0f6Qr6EPEAIQEQ",
		"CgkI0f6Qr6EPEAIQEg",
		"CgkI0f6Qr6EPEAIQEw",
		"CgkI0f6Qr6EPEAIQFA",
		"CgkI0f6Qr6EPEAIQFQ",
		"CgkI0f6Qr6EPEAIQFg",
		"CgkI0f6Qr6EPEAIQFw",
		"CgkI0f6Qr6EPEAIQGA",
		"CgkI0f6Qr6EPEAIQGQ",
		"CgkI0f6Qr6EPEAIQGg",
		"CgkI0f6Qr6EPEAIQGw",
		"CgkI0f6Qr6EPEAIQHA",
		"CgkI0f6Qr6EPEAIQHQ",
		"CgkI0f6Qr6EPEAIQHg",
		"CgkI0f6Qr6EPEAIQHw"
	};

	//=============================================================================

    void Start()
    {
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: Start");		
		
        if( m_bInitialised == false )
        {
            m_bInitialised = true;
			m_bIsLoggedIn = false;
            m_Instance = this;
			m_bIsUploadingAchievement = false;
			m_UploadingAchievementTimeout = 10.0f;
			m_UploadingAchievementID = -1;
			
			m_ManagerState = eManagerState.Idle;
			
			if( m_bVerbose )	
				Debug.Log("AchievementsManager: Init");		
        }
		
		// Connect to Game Center / Google Play
		#if UNITY_IPHONE
		GameCenterManager.playerAuthenticatedEvent += GCOnPlayerAuthenticated;
		GameCenterManager.playerFailedToAuthenticateEvent += GCOnPlayerFailedToAuthenticate;
		GameCenterManager.reportAchievementFailedEvent += GCOnReportAchievementFailed;
		GameCenterManager.reportAchievementFinishedEvent += GCOnReportAchievementFinished;
		GameCenterManager.reportScoreFailedEvent += GCOnReportScoreFailed;
		GameCenterManager.reportScoreFinishedEvent += GCOnReportScoreFinished;
		
		// Auto login if we already logged in a previous time
		//if( PlayerPrefs.GetInt( "GCGPAutoLogin" , 0 ) > 0 )
		{
			Login();
		}
		#endif

		#if UNITY_ANDROID
		GPGManager.authenticationSucceededEvent += GPOnPlayerAuthenticated;
		GPGManager.authenticationFailedEvent += GPOnPlayerFailedToAuthenticate;
		GPGManager.unlockAchievementFailedEvent += GPOnReportAchievementFailed;
		GPGManager.unlockAchievementSucceededEvent += GPOnReportAchievementFinished;
		GPGManager.submitScoreFailedEvent += GPOnReportScoreFailed;
		GPGManager.submitScoreSucceededEvent += GPOnReportScoreFinished;
		
		// Auto login if we already logged in a previous time
		//if( PlayerPrefs.GetInt( "GCGPAutoLogin" , 0 ) > 0 )
		{
			Login();
		}
		#endif

		// Playtomic init (scoreboards)
		Playtomic.Initialize( "4297CC23E31BC29545FC2A9577AD4", "14A8D1497B7B68DBF43D97128CB88", "http://shreditscoreboard.herokuapp.com/" );
		
		// Connect to Facebook/Twitter
		#if UNITY_IPHONE || UNITY_ANDROID
		FacebookCombo.init();		
		TwitterCombo.init( "GPC3ImeHkYHf0Bdhr9gNC39SW", "dty3PjzDLQTnlQ9jjqMJUM2oy4apJhqtPpaFCX0oVcLvTw2kPg" );
		#endif
		
		#if UNITY_IPHONE && !UNITY_EDITOR
		FacebookBinding.setSessionLoginBehavior( FacebookSessionLoginBehavior.ForcingWebView );
		#endif

		// For debug builds register a random player
		RandomiseName();
    }

    //=============================================================================

	public void RandomiseName()
	{
	}
	
    //=============================================================================

    void Awake()
    {
        DontDestroyOnLoad( transform.gameObject );
    }

    //=============================================================================

	#if UNITY_IPHONE
	// Game Center
	private void GCOnPlayerFailedToAuthenticate( string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnPlayerFailedToAuthenticate");		

		m_bIsLoggedIn = false;

		if( LoginFailEvent != null )
			LoginFailEvent( "Error - GC login failed: " + Error );
		
		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GCOnPlayerAuthenticated()
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnPlayerAuthenticated");		
		
		m_bIsLoggedIn = true;

		// Auto log-in from now on
		PlayerPrefs.SetInt( "GCGPAutoLogin" , 1 );

		if( LoginSuccessEvent != null )
			LoginSuccessEvent();
		
		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================
	
	private void GCOnReportAchievementFailed( string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnReportAchievementFailed");		

		if( AddAchievementFailEvent != null )
			AddAchievementFailEvent( "Error - GC failed to add achievement: " + Error );
		
		m_bIsUploadingAchievement = false;
		m_UploadingAchievementTimeout = 30.0f;

		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GCOnReportAchievementFinished( string Msg )
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnReportAchievementFinished");		
		
		if( AddAchievementSuccessEvent != null )
			AddAchievementSuccessEvent();
		
		m_bIsUploadingAchievement = false;
		if( GameDataManager.Instance != null )
		{
			GameDataManager.Instance.MarkAchievementUploaded( m_UploadingAchievementID );
		}

		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================

	private void GCOnReportScoreFailed( string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnReportScoreFailed");		

		if( ReportScoreFailEvent != null )
			ReportScoreFailEvent( "Error - GC failed to report score: " + Error );
		
		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GCOnReportScoreFinished( string Msg )
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GCOnReportScoreFinished");		
		
		if( ReportScoreSuccessEvent != null )
			ReportScoreSuccessEvent();
		
		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================
	#endif

	#if UNITY_ANDROID
	// Google Play
	private void GPOnPlayerFailedToAuthenticate( string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnPlayerFailedToAuthenticate");		

		m_bIsLoggedIn = false;

		if( LoginFailEvent != null )
			LoginFailEvent( "Error - GP login failed: " + Error );
		
		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GPOnPlayerAuthenticated( string UserID )
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnPlayerAuthenticated: " + UserID );		
		
		m_bIsLoggedIn = true;

		// Auto log-in from now on
		PlayerPrefs.SetInt( "GCGPAutoLogin" , 1 );

		if( LoginSuccessEvent != null )
			LoginSuccessEvent();
		
		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================
	
	private void GPOnReportAchievementFailed( string ID , string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnReportAchievementFailed");		

		if( AddAchievementFailEvent != null )
			AddAchievementFailEvent( "Error - GP failed to add achievement " + ID + " : " + Error );
		
		m_bIsUploadingAchievement = false;
		m_UploadingAchievementTimeout = 30.0f;

		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GPOnReportAchievementFinished( string ID , bool NewlyUnlocked )
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnReportAchievementFinished " + ID + " : New? " + NewlyUnlocked );		
		
		if( AddAchievementSuccessEvent != null )
			AddAchievementSuccessEvent();
		
		m_bIsUploadingAchievement = false;
		if( GameDataManager.Instance != null )
		{
			GameDataManager.Instance.MarkAchievementUploaded( m_UploadingAchievementID );
		}
		
		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================
	
	private void GPOnReportScoreFailed( string ID , string Error )
	{
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnReportScoreFailed");		

		if( ReportScoreFailEvent != null )
			ReportScoreFailEvent( "Error - GP failed to report score" + ID + " : " + Error );
		
		m_ManagerState = eManagerState.Idle;
	}	
	
	//=============================================================================

	private void GPOnReportScoreFinished( string ID , Dictionary<string,object> Dict )
	{		
		if( m_bVerbose )	
			Debug.Log("AchievementsManager: GPOnReportScoreFinished " + ID );		
		
		if( ReportScoreSuccessEvent != null )
			ReportScoreSuccessEvent();
		
		m_ManagerState = eManagerState.Idle;
	}
	
	//=============================================================================

	#endif

	void Update()
	{
		// Upload achievements if required
		m_UploadingAchievementTimeout -= Time.deltaTime;
		if( m_UploadingAchievementTimeout > 0.0f )
			return;
		
		if( GameDataManager.Instance == null )
			return;
		
		if( m_bIsUploadingAchievement == false )
		{
			m_UploadingAchievementID = GameDataManager.Instance.GetAchievementToUpload();
			if( m_UploadingAchievementID != -1 )
			{
				m_bIsUploadingAchievement = true;
				m_UploadingAchievementTimeout = 0.0f;
				AddAchievement( m_UploadingAchievementID );
				Debug.Log( "Uploading achievement: " + m_UploadingAchievementID );
			}
		}
	}
	
	//=============================================================================
	
	public void Login()
	{
		#if UNITY_IPHONE
		if( ( IsLoggedIn() == false ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			GameCenterBinding.authenticateLocalPlayer();
			m_ManagerState = eManagerState.Login;
		}
		else
		{
			if( LoginFailEvent != null )
				LoginFailEvent( "Error - Already logged in or manager busy" );
		}
		#endif

		#if UNITY_ANDROID
		if( ( IsLoggedIn() == false ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			if( PlayerPrefs.GetInt( "GCGPAutoLogin" , 0 ) > 0 )
				PlayGameServices.attemptSilentAuthentication();
			else
				PlayGameServices.authenticate();
			
			m_ManagerState = eManagerState.Login;
		}
		else
		{
			if( LoginFailEvent != null )
				LoginFailEvent( "Error - Already logged in or manager busy" );
		}
		#endif
	}

	//=============================================================================
	
	public void Logout()
	{
		#if UNITY_IPHONE
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			// Stop auto log-in from now on
			PlayerPrefs.SetInt( "GCGPAutoLogin" , 0 );
			m_bIsLoggedIn = false;
			m_ManagerState = eManagerState.Idle;
			
			if( LogoutSuccessEvent != null )
				LogoutSuccessEvent();
		}
		#endif

		#if UNITY_ANDROID
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			// Stop auto log-in from now on
			PlayerPrefs.SetInt( "GCGPAutoLogin" , 0 );
			m_bIsLoggedIn = false;
			m_ManagerState = eManagerState.Idle;
			
			PlayGameServices.signOut();
			
			if( LogoutSuccessEvent != null )
				LogoutSuccessEvent();
		}
		#endif
	}

	//=============================================================================

	public void AddAchievement( int ID )
	{
		#if UNITY_IPHONE
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			GameCenterBinding.reportAchievement( ID.ToString() , 100.0f );
			m_ManagerState = eManagerState.AddAchievement;
		}
		else
		{
			if( AddAchievementFailEvent != null )
				AddAchievementFailEvent( "Error - not logged in or manager busy" );
			
			m_bIsUploadingAchievement = false;
			m_UploadingAchievementTimeout = 20.0f;
		}
		#endif

		#if UNITY_ANDROID
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			string GPAchievementID = GPAchievementLookup[ ID ];
			PlayGameServices.unlockAchievement( GPAchievementID , true );
			m_ManagerState = eManagerState.AddAchievement;
		}
		else
		{
			if( AddAchievementFailEvent != null )
				AddAchievementFailEvent( "Error - not logged in or manager busy" );

			m_bIsUploadingAchievement = false;
			m_UploadingAchievementTimeout = 20.0f;
		}
		#endif
		
		#if UNITY_EDITOR
		m_bIsUploadingAchievement = false;
		m_UploadingAchievementTimeout = 5.0f;
		#endif
	}

	//=============================================================================

	public void ReportScore( System.Int64 Score )
	{
		// Report score to GC/GP first
		#if UNITY_IPHONE
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			GameCenterBinding.reportScore( Score , ScoreboardID );
			m_ManagerState = eManagerState.ReportScore;
		}
		else
		{
			if( ReportScoreFailEvent != null )
				ReportScoreFailEvent( "Error - not logged in or manager busy" );
		}
		#endif

		#if UNITY_ANDROID
		if( ( IsLoggedIn() == true ) && ( m_ManagerState == eManagerState.Idle ) )
		{
			PlayGameServices.submitScore( ScoreboardIDAndroid , Score );
			m_ManagerState = eManagerState.ReportScore;
		}
		else
		{
			if( ReportScoreFailEvent != null )
				ReportScoreFailEvent( "Error - not logged in or manager busy" );
		}
		#endif
		
		// Now report score to playtomic system along with any custom parameters
		if( IsPlayerRegistered() == false )
			return;

		#if UNITY_EDITOR
		string sourceType = "Editor";
		#elif UNITY_IPHONE
		string sourceType = "iOS";
		#elif UNITY_ANDROID
		string sourceType = "iOS"; //"Android";
		#else
		string sourceType = "Unknown";
		#endif
		
		string CurCountry = PreHelpers.GetCountryCode();
		
		PlayerScore NewScore = new PlayerScore 
		{
			playername = GetPlayerName(),
			playerid = GetPlayerID(),
			points = Score,
			table = ScoreboardID,
			allowduplicates = false,
			source = sourceType,
			fields = new PDictionary 
			{
				{ "country", CurCountry }
			}
		};
		
		Playtomic.Leaderboards.Save( NewScore , ScoreSubmitComplete );
	}

	//=============================================================================

	public void ReportScoreTest( System.Int64 Score , string PlayerName , string PlayerID )
	{
		#if UNITY_EDITOR
		string sourceType = "Editor";
		#elif UNITY_IPHONE
		string sourceType = "iOS";
		#elif UNITY_ANDROID
		string sourceType = "iOS"; //"Android";
		#else
		string sourceType = "Unknown";
		#endif
		
		string CurCountry = PreHelpers.GetCountryCode();
		
		PlayerScore NewScore = new PlayerScore 
		{
			playername = PlayerName,
			playerid = PlayerID,
			points = Score,
			table = "testboard",
			allowduplicates = false,
			source = sourceType,
			fields = new PDictionary 
			{
				{ "country", CurCountry }
			},
			filters = new PDictionary
			{
				{"perpage", 5 },
				{"highest", true },
				{"mode", eScoreTime.AllTime.ToString().ToLower() },
				{"allowduplicates", false }
			}
		};
		
		Playtomic.Leaderboards.SaveAndList( NewScore , ScoreListComplete );
	}

	//=============================================================================

	void ScoreSubmitComplete( PResponse Response )
	{
		if( Response.success )
		{
			Debug.Log("Score saved!");		
		}
		else
		{
			// Submission failed because of response.errormessage with response.errorcode
			Debug.Log("Score failed to save: " + Response.errormessage );		
		}
	}	

	//=============================================================================

	public bool IsLoggedIn()
	{
		return( m_bIsLoggedIn );
	}
	
	//=============================================================================
	
	public void DisplayAchievements()
	{
		#if UNITY_IPHONE
		GameCenterBinding.showAchievements();
		#endif
		
		#if UNITY_ANDROID
		PlayGameServices.showAchievements();
		#endif
	}
	
	//=============================================================================

	public void DisplayScoreboards()
	{
		#if UNITY_IPHONE
		GameCenterBinding.showLeaderboardWithTimeScope( GameCenterLeaderboardTimeScope.AllTime );
		#endif
		
		#if UNITY_ANDROID
		PlayGameServices.showLeaderboards();
		#endif
	}
	
	//=============================================================================

	public void GetScoreboard( eScoreTime TimeDuration = eScoreTime.AllTime , int Page = 1 , int ScoresPerPage = 10 )
	{
		/*
		if( bFriendsAndPlayerOnly )
		{
			// Only for registered users
			if( IsPlayerRegistered() == false )
			{
				isGettingScoreboard = false;
				if( GetScoreboardFailEvent != null )
					GetScoreboardFailEvent();
				
				return;
			}
			
			PLeaderboardOptions Table = new PLeaderboardOptions 
			{
				{"table", string.Format( "scoreboard{0}" , GameMode ) },
				{"friendslist" , GetPlayerFriendList() },
				{"page", Page },
				{"perpage", ScoresPerPage },
				{"highest", true },
				{"mode", TimeDuration.ToString().ToLower() },
				{"allowduplicates", false }
			};

			Playtomic.Leaderboards.List( Table, ScoreListComplete );		
			}
			else
			{
				// Filter by stage
				PDictionary filters = new PDictionary
				{
					{ "stage" , StageFilter }
				};
				
				PLeaderboardOptions Table = new PLeaderboardOptions 
				{
					{"table", string.Format( "scoreboard{0}" , GameMode ) },
					//{"playerid" , GetPlayerID() },
					{"page", Page },
					{"perpage", ScoresPerPage },
					{"highest", true },
					{"filters" , filters },
					{"mode", TimeDuration.ToString().ToLower() },
					{"allowduplicates", false }
				};

				Playtomic.Leaderboards.List( Table, ScoreListComplete );		
			}
		}
		*/

		// No filter
		PLeaderboardOptions Table = new PLeaderboardOptions 
		{
			{"table", ScoreboardID },
			//{"playerid" , GetPlayerID() },
			{"page", Page },
			{"perpage", ScoresPerPage },
			{"highest", true },
			{"mode", TimeDuration.ToString().ToLower() },
			{"allowduplicates", false }
		};

		Playtomic.Leaderboards.List( Table, ScoreListComplete );		
	}
	
	//=============================================================================

	void ScoreListComplete( List< PlayerScore > Scores , int TotalScores , PResponse Response )
	{
		bool bVerbose = true;
		if( Response.success )
		{
			if( bVerbose )	Debug.Log("Score list ok: " + Scores.Count + "/" + TotalScores );
			if( bVerbose )
			{
				foreach( PlayerScore CurScore in Scores )
				{
					try
					{
						string Country = CurScore.fields.GetString( "country" );
						//int Stage = CurScore.fields.GetInt( "stage" );
						
						#if UNITY_EDITOR
						Debug.Log( CurScore.rank + ") " + CurScore.playername + "(" + Country + ") - " + CurScore.points + " " + CurScore.perpage );
						#endif
					}
					catch
					{
						Debug.Log( "Error reading score data" );
					}
				}
			}
			
			if( GetScoreboardSuccessEvent != null )
				GetScoreboardSuccessEvent( TotalScores , Scores );
		}
		else
		{
			Debug.Log("Failed to retrieve scorelist: " + Response.errormessage );		

			if( GetScoreboardFailEvent != null )
				GetScoreboardFailEvent();
		}
	}	

	//=============================================================================

	string GetRandomName()
	{
		string[] Names = {
				"Adelaide Chevere",
				"Herman Collie",
				"Margarete Deininger",
				"Adriana Bruner",
				"Milford Bernhardt",
				"Fredrick Leahy",
				"Jerry Chancellor",
				"Nicki Dupont",
				"Sherryl Coady",
				"Sofia Takahashi",
				"Cristin Lotts",
				"Man Putman",
				"Edmond Cluff",
				"Rasheeda Reichard",
				"Isela Sabia",
				"Sheridan Shkreli",
				"Simon Brickley",
				"Laure Pasquariello",
				"Maude Gardella",
				"Shana Gibbs",
				"Cathrine Melendez",
				"Jason Ehret",
				"Christel Calabrese",
				"Niesha Kreamer",
				"Lesia Kinder",
				"Denae Cadogan",
				"Adah Sack",
				"Ricky Varnado",
				"Ai Preciado",
				"Rudolf Mojica",
				"Sandra Beus",
				"Erick Vasko",
				"Brandie Oddo",
				"Jess Yamasaki",
				"Yelena Fenley",
				"Sheldon Cabiness",
				"Mary Shropshire",
				"Earle Bonds",
				"Nicolasa Tozier",
				"Kiara Bachman",
				"Mindy Brawn",
				"Rina Folson",
				"Detra Lairsey",
				"Lasonya Baize",
				"Cornelia Chou",
				"Josef Printz",
				"Bernardine Stetson",
				"Donte Mizell",
				"Debby Hawes",
				"Alexia Demas",
				};

		int Idx = UnityEngine.Random.Range( 0 , Names.Length - 1 );
		return( Names[ Idx ] );
	}

	//=============================================================================

	public bool IsPlayerRegistered()
	{
		if( ServerManager.Registered ) //&& ServerManager.instance.IsLoggedIn() )
       	{
			return( true );
		}
		else
		{
			return( false );
		}
	}
	
	//=============================================================================
	
	public string GetPlayerName()
	{
		//string prefix = "Player";
		//string Nick = PlayerPrefs.GetString(prefix + "Nick", "");
		
		return( ServerManager.instance.GetPlayerName() );
	}
	
	//=============================================================================

	string GetPlayerID()
	{
		string prefix = "Player";
		string PlayerUid = PlayerPrefs.GetString(prefix + "Uid", "");
		return( PlayerUid );
	}
	
	//=============================================================================
}
