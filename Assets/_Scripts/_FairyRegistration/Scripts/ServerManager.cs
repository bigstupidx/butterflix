using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tsumanga;

#if UNITY_EDITOR
using System.Reflection;
#endif


public class ServerManager : MonoBehaviour 
{
	public static event Action					RegistrationSuccessEvent;
	public static event Action<string>			RegistrationFailEvent;
	
	public static event Action					ReRegistrationSuccessEvent;
	public static event Action<string>			ReRegistrationFailEvent;
	
	public static event Action					DownloadPlayerDataSuccessEvent;
	public static event Action					DownloadPlayerDataFailEvent;

	public static event Action<List<string>>	SearchPreviousPlayersSuccessEvent;
	public static event Action<string>			SearchPreviousPlayersFailEvent;
	
	public static event Action<string>			SavePlayerDataSuccessEvent;
	public static event Action					SavePlayerDataFailEvent;
	
	public static event Action<double>			ServerTimeSuccessEvent;
	public static event Action					ServerTimeFailEvent;
	
	public static event Action<long, long>		RedeemCouponSuccessEvent;
	public static event Action					RedeemCouponFailEvent;
	
	public static ServerManager		instance				= null;
	
	private bool					isInitialised			= false;
	
	// Host selection
	private WebServices				PlayerWS				= null;
	private IRequest				SearchRequest			= null;
	
	// Status
	private bool					DataLoaded				= false;
    private bool Busy
    {
        set {
            Debug.Log( "Busy set m_busy = " +m_busy+" newval = " +value);
            m_busy = value; 
        }
        get {
            Debug.Log( "Busy get m_busy = " + m_busy);
            return m_busy; 
        }
    }

    private bool m_busy = false;
	
	// Registration
	private string					NewName					= string.Empty;

    public static bool Registered
    {
        get
        {
            return instance != null && instance.PlayerWS != null && instance.PlayerWS.Registered;
        }
    }
	//=============================================================================
	
	void Start()
	{
		
	}
	
	//=============================================================================

	void Awake()
	{
        Init();
		DontDestroyOnLoad( transform.gameObject );
	}
	
	//=============================================================================

	public void Init()
	{
		if(! isInitialised)
		{
			isInitialised = true;
			
			instance = this;
			
			NewName = "";
			Busy = false;
			
			if( ServerBase.WebServices == null )
				PlayerWS = ServerBase.Init( "https://multi.tsumanga.net" );
			else
				PlayerWS = ServerBase.WebServices;
						
			string prefix = "Player";
			string PlayerUid = PlayerPrefs.GetString(prefix + "Uid", "");
			
			if(PlayerUid != "")
			{
				string Nick = PlayerPrefs.GetString(prefix + "Nick", "");
				string SecretKey = PlayerPrefs.GetString(prefix + "Secret", "");
				Debug.Log( "Initialising server with log-in nickname: " + Nick );
				PlayerWS.Initialise(PlayerUid, SecretKey, Nick);
				NewName = Nick;
			}
			else
			{
				Debug.Log( "Initialising server - no log-in details" );
			}
		}
	}

	//=============================================================================
	
	public string GetPlayerName()
	{
		if( NewName.Length > 5 )
			return( NewName.Substring( 5 , NewName.Length - 5 ) );
		else
			return( "" );
	}

	//=============================================================================

	void LogCallbackCounters()
	{
		#if UNITY_EDITOR
		// Used for debugging to check event counters haven't exceeded 1 callback each
		//Debug.Log( "Callback counters for server manager:" );
		
		FieldInfo[] Fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		string[] FieldNames = Array.ConvertAll<FieldInfo, string>(Fields,delegate(FieldInfo field) { return field.Name; });					
		
		int NumFields = Fields.Length;
		for( int FieldIdx = 0 ; FieldIdx < NumFields ; FieldIdx++ )
		{
			if( FieldNames[ FieldIdx ].Contains( "Success" ) || FieldNames[ FieldIdx ].Contains( "Fail" ) )
			{
				MulticastDelegate CurDelegate = (MulticastDelegate )( Fields[ FieldIdx ].GetValue( this ) );
				if( CurDelegate != null )
				{
					int NumCallbacks = CurDelegate.GetInvocationList().Length;
					
					// If Number of callbacks is greater than 1 then something has gone wrong!
					if( NumCallbacks > 1 )
					{
						Debug.LogError( FieldNames[ FieldIdx ] + ": " + NumCallbacks );
					}
				}
			}
		}
		#endif
	}
	
	//=============================================================================
	
	void Update()
	{
		#if UNITY_EDITOR
		//LogCallbackCounters();
		#endif
	}

	//=============================================================================

	public bool IsLoggedIn()
	{
		return PlayerWS.LoggedIn;
	}

	//=============================================================================

	public void RegisterPlayerName(string playerName)
	{
		NewName = playerName;
		
		if(!Busy)
		{
			if(!PlayerWS.Registered)
			{
				Debug.Log( "PlayerWS is not registered - calling RegisterPlayer()");
				StartRequest(RegisterPlayer(), "Registering ...");
			}
			else
			{
				Debug.Log( "PlayerWS is already registered - calling RegisterPlayer()");
				StartRequest(RegisterPlayer(), "Registering ...");
			}
		}
	}
	
	//=============================================================================
	
	public void SavePlayerData(string key,string data)
	{
		if(PlayerWS.Registered)
		{
			Debug.Log( "PlayerWS is registered - calling SaveData()");
			StartRequest(SaveData(key,data), "Saving...");
		}
		else
		{
			Debug.Log( "PlayerWS is not registered - cant call SaveData()");
		}
	}
	
	//=============================================================================
	
	public void SavePlayerData(string key,object data)
	{
		if(PlayerWS.Registered)
		{
			Debug.Log( "PlayerWS is registered - calling SaveData()");
			StartRequest(SaveData(key,data), "Saving...");
		}
		else
		{
			Debug.Log( "PlayerWS is not registered - cant call SaveData()");
		}
	}
	
	//=============================================================================

	// Reloads player data files from server and saves them locally
	public void LoadPlayerData()
	{
		if( PlayerWS.Registered )
		{
			Debug.Log( "PlayerWS is registered - calling LoadPlayerData()");
			
			string playerManagerDataKey = "playerData";
			string playerManagerDataFilename = "Player";

			StartCoroutine( LoadPlayerDataMain(	playerManagerDataKey , playerManagerDataFilename ) );
		}
		else
		{
			Debug.Log( "PlayerWS is not registered - cant call LoadPlayerData()");
			
			if(null != DownloadPlayerDataFailEvent)
				DownloadPlayerDataFailEvent();
		}
	}
		
	//=============================================================================

	IEnumerator LoadPlayerDataMain( string playerManagerDataKey , string playerManagerDataFilename )
	{
		// Use null uid to retrieve data for the player
		string uid = null;
		
		// Load player manager (matters if we dont get this file)
		yield return StartCoroutine(LoadDataAndSaveToFile(playerManagerDataKey, playerManagerDataFilename, uid));
		if( DataLoaded )
		{
			if(null != DownloadPlayerDataSuccessEvent)
				DownloadPlayerDataSuccessEvent();
		}
		else
		{
			if(null != DownloadPlayerDataFailEvent)
				DownloadPlayerDataFailEvent();
		}
	}
	
	//=============================================================================

    public void Unregister()
    {
        PlayerPrefs.DeleteKey("PlayerUid");
        PlayerPrefs.DeleteKey("PlayerNick");
        PlayerPrefs.DeleteKey("PlayerSecret");
        PlayerPrefs.Save();
        PlayerWS.Initialise("", "", "");
    }

	//=============================================================================
	
	private bool RequestOK(IRequest req, string info)
	{
		bool ok = (req.Status == RequestStatus.STATUS_OK);
		Busy = false;
		return ok;
	}
	
	//=============================================================================
	
	private void StartRequest(IEnumerator routine, string info)
	{
		Busy = true;
		StartCoroutine(routine);
	}
	
	//=============================================================================
	
	IEnumerator RegisterPlayer()
	{
		IRequest req = PlayerWS.RegisterPlayer(NewName, "en_GB");
		yield return StartCoroutine(req);
		if(RequestOK(req, "Registered") && !PlayerWS.PlayerBanned)
		{
			string prefix = "Player";
			PlayerPrefs.SetString(prefix + "Uid", PlayerWS.PlayerUid);
			PlayerPrefs.SetString(prefix + "Nick", PlayerWS.PlayerNick);
			PlayerPrefs.SetString(prefix + "Secret", PlayerWS.PlayerSecret);
			PlayerPrefs.Save();
			
			if(null != RegistrationSuccessEvent)
				RegistrationSuccessEvent();
		}
		else
		{
			Debug.Log( "Registration failed with error: " + req.Status + " " + req.Error);
			
			string error = string.Empty;
			
			if(req.Error.Contains("409"))
			{
				error = "POPUP_NAME_UNAVAILABLE";
			}
			else
			{
				switch(req.Status)
				{
				case RequestStatus.FAIL_CONFLICT:
					error = "POPUP_NAME_UNAVAILABLE";
					break;
				
				case RequestStatus.FAIL_FORBIDDEN:
					error = "FORBIDDEN";
					RegistrationFallback();
					break;
				
				default:
					error = "POPUP_NAME_ERROR";
					RegistrationFallback();
					break;
				}
			}

			if(null != RegistrationFailEvent && (error != "FORBIDDEN") && (error != "POPUP_NAME_ERROR"))
				RegistrationFailEvent(error);
			
			/*
			public enum RequestStatus
			{
			PENDING = 0,
			NOT_REGISTERED = 1,
			STATUS_OK = 200,
			FAIL_BAD = 400,
			FAIL_AUTH = 401,
			FAIL_FORBIDDEN = 403,
			FAIL_NOT_FOUND = 404,
			FAIL_CONFLICT = 409,
			FAIL_UNKNOWN = 500
			}
			*/
		}
	}

	public void RegistrationFallback()
	{
		string prefix = "Player";
		PlayerPrefs.SetString(prefix + "Uid", PlayerWS.PlayerUid);
		PlayerPrefs.SetString(prefix + "Nick", NewName);
		PlayerPrefs.SetString(prefix + "Secret", PlayerWS.PlayerSecret);
		PlayerPrefs.Save();

		this.IsOffline = true;

		PlayerWS.PlayerNick = NewName;
		if(null != RegistrationSuccessEvent)
			RegistrationSuccessEvent();
	}

	public bool IsOffline
	{
		get
		{
			int isOffInt = PlayerPrefs.GetInt("IsOffline", 0);
			return (isOffInt == 0);
		}
		set
		{
			bool isOff = value;
			PlayerPrefs.SetInt("IsOffline", (isOff) ? 0 : 1);
		}
	}

	//=============================================================================
	
	public void ReRegisterPlayerName( string playerName )
	{
		NewName = playerName;
		
		if( !Busy )
		{
			Debug.Log( "PlayerWS is reregistering - calling ReRegisterPlayer()" );
			StartRequest(ReRegisterPlayer(), "Reregistering ...");
		}
	}
	
	//=============================================================================
	
	IEnumerator ReRegisterPlayer()
	{
		IRequest req = PlayerWS.ReRegisterPlayer(NewName, "en_GB");
		yield return StartCoroutine(req);
		if (RequestOK(req, "ReRegistered"))
		{
			string prefix = "Player";
			PlayerPrefs.SetString(prefix + "Uid", PlayerWS.PlayerUid);
			PlayerPrefs.SetString(prefix + "Nick", PlayerWS.PlayerNick);
			PlayerPrefs.SetString(prefix + "Secret", PlayerWS.PlayerSecret);
			PlayerPrefs.Save();
			
			req = PlayerWS.RestorePlayerData("Badge");
			yield return StartCoroutine(req);
			if (RequestOK(req, "Got badge"))
			{
				try {
					long badge = (long) (req.Result["badge"]);
					string key = "PlayerBadge";
					PlayerPrefs.SetInt(key, (int) badge);
					PlayerPrefs.Save();				
				} catch (KeyNotFoundException) {
					// nothing
				}
			}
			
			if(null != ReRegistrationSuccessEvent)
			{
				ReRegistrationSuccessEvent();
			}
		}
		else
		{
			Debug.Log( "ReRegistration failed with error: " + req.Status);
			
			if(null != ReRegistrationFailEvent)
				ReRegistrationFailEvent( req.Status.ToString() );
		}
	}
	
	//=============================================================================
	
	IEnumerator RenamePlayer()
	{
		IRequest req = PlayerWS.ChangeNickname(NewName);
		yield return StartCoroutine(req);
		if (RequestOK(req, "Renamed"))
		{
			string prefix = "Player";
			PlayerPrefs.SetString(prefix + "Nick", PlayerWS.PlayerNick);			
			PlayerPrefs.Save();
		}
		else
		{
			Debug.Log( "Renaming failed with error: " + req.Status);
			
			string error = string.Empty;
			switch(req.Status)
			{
			case RequestStatus.FAIL_CONFLICT:
				error = TextManager.GetText("POPUP_NAME_UNAVAILABLE");
				break;
				
			default:
				error = TextManager.GetText("POPUP_NAME_ERROR");
				break;
			}
			
			if(null != RegistrationFailEvent)
				RegistrationFailEvent(error);
		}
	}
	
	//=============================================================================
	
	IEnumerator DummyAction()
	{
		IRequest req = new FailedRequest(RequestStatus.FAIL_BAD, "Supposed to fail");
		yield return new WaitForSeconds(2);
		yield return StartCoroutine(req);
		RequestOK(req, ""); // always false
	}
	
	//=============================================================================
	
	IEnumerator LoadDataAndSaveToFile(string dataKey,string dataFilename,string uid)
	{
		// Save data as a file
		string DocPath = PreHelpers.GetFileFolderPath();
		string FilePath = DocPath + dataFilename + ".txt";
		
		IRequest req = null;
		
		if( uid != null )
			req = PlayerWS.GetPlayerData(dataKey, uid);
		else
			req = PlayerWS.RestorePlayerData(dataKey);
		
		yield return StartCoroutine(req);
		if (req.Status != RequestStatus.STATUS_OK)
		{
			Debug.Log( "Error downloading friend data for key: " + dataKey + " - " + req.Error );
			DataLoaded = false;

			// Remove existing file
			File.Delete( FilePath );
		}
		else
		{
			// Retrieve data as JSON
			Dictionary<string,object> OutDict = req.Result as Dictionary<string,object>;
			
			JSON DataOut = new JSON();
			DataOut.fields = OutDict;
			string OutString = DataOut.serialized;

			// Is it a valid file? (contains JSON data and not just {})
			if( OutString.Length > 8 )
				DataLoaded = true;
			else
				DataLoaded = false;
			

			if( DataLoaded )
			{
				Debug.Log( "Data downloaded ok for key: " + dataKey + " (" + OutString.Length + ")" );
			
				// Open/create file
				StreamWriter CurFile = File.CreateText( FilePath );
				
				CurFile.Write( OutString );
				
				// Close file
				CurFile.Close();
			}
			else
			{
				// Remove existing file
				File.Delete( FilePath );
				Debug.Log( "Data for key is empty: " + dataKey + " (" + OutString.Length + ")" );
			}
		}
	}

	//=============================================================================

    IEnumerator LoadDataAndSaveToPref(string dataKey, string dataFilename, string uid)
    {
        // Save data as a file
        string DocPath = PreHelpers.GetFileFolderPath();
        string FilePath = DocPath + dataFilename + ".txt";

        IRequest req = null;

        if (uid != null)
            req = PlayerWS.GetPlayerData(dataKey, uid);
        else
            req = PlayerWS.RestorePlayerData(dataKey);

        yield return StartCoroutine(req);
        if (req.Status != RequestStatus.STATUS_OK)
        {
            Debug.Log( "Error downloading friend data for key: " + dataKey + " - " + req.Error);
            DataLoaded = false;

            // Remove existing file
            File.Delete(FilePath);
        }
        else
        {
            // Retrieve data as JSON
            Dictionary<string, object> OutDict = req.Result as Dictionary<string, object>;

            JSON DataOut = new JSON();
            DataOut.fields = OutDict;
            string OutString = DataOut.serialized;

            // Is it a valid file? (contains JSON data and not just {})
            if (OutString.Length > 8)
                DataLoaded = true;
            else
                DataLoaded = false;


            if (DataLoaded)
            {
                Debug.Log( "Data downloaded ok for key: " + dataKey + " (" + OutString.Length + ")");

                // Open/create file
                PlayerPrefs.SetString(dataFilename, OutString);
            }
            else
            {
                // Remove existing file
                PlayerPrefs.DeleteKey(dataFilename);
                Debug.Log( "Data for key is empty: " + dataKey + " (" + OutString.Length + ")");
            }
        }
    }


	//=============================================================================

	IEnumerator SaveData( string dataKey , string data )
	{
		IRequest req = PlayerWS.StorePlayerData(dataKey, data);
		yield return StartCoroutine(req);
		if(req.Status != RequestStatus.STATUS_OK)
		{
			Debug.LogWarning( "Error uploading data for key: " + dataKey +" "+req.Status);

			if(null != SavePlayerDataFailEvent)
				SavePlayerDataFailEvent();
			//LastStatus = currentRequest.Status.ToString() + " " + currentRequest.Error;
		}
		else
		{
			Debug.Log( "Data uploaded ok for key: " + dataKey );

			if(null != SavePlayerDataSuccessEvent)
				SavePlayerDataSuccessEvent(dataKey);
		}

		Busy = false;
	}
	
	//=============================================================================
	
	IEnumerator SaveData( string dataKey , object data )
	{
		IRequest req = PlayerWS.StorePlayerData(dataKey, data);
		yield return StartCoroutine(req);
		if(req.Status != RequestStatus.STATUS_OK)
		{
			Debug.Log( "Error uploading data for key: " + dataKey );

			if(null != SavePlayerDataFailEvent)
				SavePlayerDataFailEvent();
		}
		else
		{
			Debug.Log( "Data uploaded ok for key: " + dataKey );
			
			if(null != SavePlayerDataSuccessEvent)
				SavePlayerDataSuccessEvent(dataKey);
		}

		Busy = false;
	}
	
	//=============================================================================

	public void SearchForPreviousPlayers()
	{
        if( Busy == false )
        {
            Debug.Log( "Searching for previous players." );
            StartRequest( SearchPreviousPlayers() , "Searching for previous players ..." );
        }
        else 
		{
            Debug.Log( "SearchForPreviousPlayers faild as Busy" );
        }
	}
	
	//=============================================================================

	// Used to find nicknames of previous registrations from the same device
	IEnumerator SearchPreviousPlayers()
	{
		SearchRequest = PlayerWS.SearchPreviousPlayers();
		yield return StartCoroutine(SearchRequest);
		if( RequestOK(SearchRequest, "Previous Players Found") )
		{
			if( (SearchRequest != null) && (SearchRequest.Status == RequestStatus.STATUS_OK) )
			{
				List<object> foundPlayers = SearchRequest.Result["niks"] as List<object>;
				
				if(foundPlayers.Count > 0)
				{
					List<string> foundNiks = new List<string>();
					for(int i = 0; i < foundPlayers.Count; i++)
						foundNiks.Add( foundPlayers[i] as string );
					
					if(null != SearchPreviousPlayersSuccessEvent)
						SearchPreviousPlayersSuccessEvent(foundNiks);
				}
				else
				{
					if(null != SearchPreviousPlayersFailEvent)
						SearchPreviousPlayersFailEvent(SearchRequest.Status.ToString());
				}
			}
		}
		else
		{
			// Assume there's an internet issue
			if(null != SearchPreviousPlayersFailEvent)
				SearchPreviousPlayersFailEvent("ERROR_NO_INTERNET");
		}
	}
	
	//=============================================================================
	
	public void RedeemCouponCode(string couponCode)
	{
		if(PlayerWS.Registered)
		{
			Debug.Log("PlayerWS is registered - calling RedeemCoupon()");
			
			StartCoroutine( RedeemCoupon(couponCode) );
		}
		else
		{
			Debug.Log("PlayerWS is not registered - cant call RedeemCoupon()");
			
			if(null != RedeemCouponFailEvent)
				RedeemCouponFailEvent();
		}
	}
	
	//=============================================================================
	
	IEnumerator RedeemCoupon(string couponCode)
	{
		IRequest req = PlayerWS.RedeemCoupon(couponCode);
		yield return StartCoroutine(req);
		long couponGems = 0;
		long couponHearts = 0;
		
		if (RequestOK (req, "Redeemed"))
		{
			IDictionary<string, object> result = req.Result;
			couponGems = result.Get<long>("gems", 0);
			couponHearts = result.Get<long>("hearts", 0);
			if (null != RedeemCouponSuccessEvent)
			   RedeemCouponSuccessEvent(couponGems, couponHearts);
		}
		else
		{
			Debug.Log( "Redeem coupon code failed" );

			if(null != RedeemCouponFailEvent)
				RedeemCouponFailEvent();
		}
	}
	
	//=============================================================================
}
